using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Verse;

namespace RIMAPI.Core
{
    public class CachingService : ICachingService, IDisposable
    {
        private class CacheEntry
        {
            public object Value { get; set; }
            public DateTime Created { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime? AbsoluteExpiration { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
            public CachePriority Priority { get; set; }
            public CacheExpirationType ExpirationType { get; set; }
            public int GameTickAdded { get; set; }
            public int? GameTickExpiration { get; set; }
        }

        private readonly Dictionary<string, CacheEntry> _cache =
            new Dictionary<string, CacheEntry>();
        private readonly object _cacheLock = new object();
        private readonly List<string> _expirationQueue = new List<string>();
        private bool _disposed = false;
        private bool _enabled;

        public bool IsEnabled() => _enabled;

        // Statistics
        private int _hits = 0;
        private int _misses = 0;
        private DateTime _lastCleanup = DateTime.UtcNow;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromSeconds(30);

        private int _lastCleanupTick;

        public CachingService()
        {
            LogApi.Info("Injected caching service...");
            _lastCleanupTick = Find.TickManager?.TicksGame ?? 0;
        }

        // Call this regularly from the main game tick
        public void Update()
        {
            var currentTick = Find.TickManager?.TicksGame ?? 0;

            // Cleanup every 10 seconds (600 ticks at 60 TPS)
            if (currentTick - _lastCleanupTick >= 600)
            {
                _lastCleanupTick = currentTick;
                CleanupExpiredEntries();
            }
        }

        private void CleanupExpiredEntries()
        {
            lock (_cacheLock)
            {
                var expiredKeys = new List<string>();
                var now = DateTime.UtcNow;
                var currentTick = Find.TickManager?.TicksGame ?? 0;

                foreach (var kvp in _cache)
                {
                    if (IsExpired(kvp.Value, now, currentTick))
                    {
                        expiredKeys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredKeys)
                {
                    _cache.Remove(key);
                }

                if (expiredKeys.Count > 0)
                {
                    LogApi.Message(
                        $"[Cache] Cleaned up {expiredKeys.Count} expired entries",
                        LoggingLevels.DEBUG
                    );
                }

                _lastCleanup = now;
                UpdateExpirationQueue();
            }
        }

        private bool IsExpired(CacheEntry entry, DateTime now, int currentTick)
        {
            switch (entry.ExpirationType)
            {
                case CacheExpirationType.Absolute:
                    return entry.AbsoluteExpiration.HasValue
                        && now >= entry.AbsoluteExpiration.Value;

                case CacheExpirationType.Sliding:
                    if (entry.SlidingExpiration.HasValue)
                    {
                        return now >= entry.LastAccessed.Add(entry.SlidingExpiration.Value);
                    }
                    return false;

                case CacheExpirationType.GameTick:
                    if (entry.GameTickExpiration.HasValue)
                    {
                        return currentTick >= entry.GameTickAdded + entry.GameTickExpiration.Value;
                    }
                    return false;

                case CacheExpirationType.Never:
                    return false;

                default:
                    return false;
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (!_enabled)
            {
                value = default;
                return false;
            }

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    // Check expiration
                    if (IsExpired(entry))
                    {
                        _cache.Remove(key);
                        _misses++;
                        value = default;
                        return false;
                    }

                    // Update sliding expiration
                    if (
                        entry.ExpirationType == CacheExpirationType.Sliding
                        && entry.SlidingExpiration.HasValue
                    )
                    {
                        entry.LastAccessed = DateTime.UtcNow;
                    }

                    _hits++;
                    value = (T)entry.Value;
                    return true;
                }

                _misses++;
                value = default;
                return false;
            }
        }

        public void Set<T>(
            string key,
            T value,
            TimeSpan? expiration = null,
            CachePriority priority = CachePriority.Normal
        )
        {
            if (!_enabled)
                return;

            lock (_cacheLock)
            {
                var now = DateTime.UtcNow;
                var entry = new CacheEntry
                {
                    Value = value,
                    Created = now,
                    LastAccessed = now,
                    Priority = priority,
                    ExpirationType = CacheExpirationType.Absolute,
                    GameTickAdded = Find.TickManager?.TicksGame ?? 0,
                };

                if (expiration.HasValue)
                {
                    entry.AbsoluteExpiration = now.Add(expiration.Value);
                }

                _cache[key] = entry;
                UpdateExpirationQueue();
            }
        }

        public void Remove(string key)
        {
            lock (_cacheLock)
            {
                _cache.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
                _expirationQueue.Clear();
            }
        }

        public async Task CacheAwareResponseAsync<T>(
            HttpListenerContext context,
            string cacheKey,
            Func<Task<ApiResult<T>>> dataFactory,
            TimeSpan? expiration = null,
            CachePriority priority = CachePriority.Normal,
            CacheExpirationType expirationType = CacheExpirationType.Absolute
        )
        {
            try
            {
                // Try to get from cache
                if (TryGet(cacheKey, out ApiResult<T> cachedResult))
                {
                    // Cache hit - return the cached ApiResult
                    await ResponseBuilder.SendApiResult(context.Response, cachedResult);
                    LogApi.Message($"[Cache] Hit for key: {cacheKey}", LoggingLevels.DEBUG);
                    return;
                }

                // Cache miss - generate data
                LogApi.Message(
                    $"[Cache] Miss for key: {cacheKey}, generating...",
                    LoggingLevels.DEBUG
                );
                var result = await dataFactory();

                // Only cache successful responses
                if (result.Success)
                {
                    Set(cacheKey, result, expiration, priority);
                }

                // Return response
                await ResponseBuilder.SendApiResult(context.Response, result);
            }
            catch (Exception ex)
            {
                LogApi.Error($"[Cache] Error in CacheAwareResponseAsync: {ex}");
                var errorResult = ApiResult<T>.Fail($"Cache error: {ex.Message}");
                await ResponseBuilder.SendApiResult(context.Response, errorResult);
            }
        }

        public void InvalidateByPattern(string pattern)
        {
            lock (_cacheLock)
            {
                var regex = new Regex(pattern);
                var keysToRemove = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }

                LogApi.Info(
                    $"[Cache] Invalidated {keysToRemove.Count} entries by pattern: {pattern}"
                );
            }
        }

        public void InvalidateByPrefix(string prefix)
        {
            lock (_cacheLock)
            {
                var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }

                LogApi.Info(
                    $"[Cache] Invalidated {keysToRemove.Count} entries by prefix: {prefix}"
                );
            }
        }

        public void InvalidateBySuffix(string suffix)
        {
            lock (_cacheLock)
            {
                var keysToRemove = _cache.Keys.Where(k => k.EndsWith(suffix)).ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }

                LogApi.Info(
                    $"[Cache] Invalidated {keysToRemove.Count} entries by suffix: {suffix}"
                );
            }
        }

        public CacheStatistics GetStatistics()
        {
            lock (_cacheLock)
            {
                return new CacheStatistics
                {
                    TotalEntries = _cache.Count,
                    Hits = _hits,
                    Misses = _misses,
                    MemoryUsageBytes = CalculateMemoryUsage(),
                    LastCleanup = _lastCleanup,
                };
            }
        }

        public void SetEnabled(bool enabled)
        {
            if (_enabled != enabled)
            {
                _enabled = enabled;
                if (!enabled)
                {
                    Clear(); // Clear cache when disabling
                }
                LogApi.Info($"[Cache] Caching {(enabled ? "enabled" : "disabled")}");
            }
        }

        public void Trim(CachePriority? priority = null)
        {
            lock (_cacheLock)
            {
                if (priority.HasValue)
                {
                    // Remove entries with priority lower than specified
                    var keysToRemove = _cache
                        .Where(kvp => kvp.Value.Priority < priority.Value)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        _cache.Remove(key);
                    }

                    LogApi.Info(
                        $"[Cache] Trimmed {keysToRemove.Count} entries with priority < {priority}"
                    );
                }
                else
                {
                    // Simple LRU-like cleanup: remove 20% of oldest entries
                    var entries = _cache.OrderBy(kvp => kvp.Value.LastAccessed).ToList();
                    var toRemove = entries.Take(entries.Count / 5).Select(kvp => kvp.Key).ToList();

                    foreach (var key in toRemove)
                    {
                        _cache.Remove(key);
                    }

                    LogApi.Info($"[Cache] Trimmed {toRemove.Count} oldest entries");
                }

                UpdateExpirationQueue();
            }
        }

        private bool IsExpired(CacheEntry entry)
        {
            var now = DateTime.UtcNow;

            switch (entry.ExpirationType)
            {
                case CacheExpirationType.Absolute:
                    return entry.AbsoluteExpiration.HasValue
                        && now >= entry.AbsoluteExpiration.Value;

                case CacheExpirationType.Sliding:
                    if (entry.SlidingExpiration.HasValue)
                    {
                        return now >= entry.LastAccessed.Add(entry.SlidingExpiration.Value);
                    }
                    return false;

                case CacheExpirationType.GameTick:
                    if (entry.GameTickExpiration.HasValue && Find.TickManager != null)
                    {
                        return Find.TickManager.TicksGame
                            >= entry.GameTickAdded + entry.GameTickExpiration.Value;
                    }
                    return false;

                case CacheExpirationType.Never:
                    return false;

                default:
                    return false;
            }
        }

        private void UpdateExpirationQueue()
        {
            _expirationQueue.Clear();
            _expirationQueue.AddRange(_cache.Keys);
        }

        private long CalculateMemoryUsage()
        {
            // Rough estimation - in reality you'd need more sophisticated calculation
            return _cache.Sum(kvp =>
                System.Text.Encoding.UTF8.GetByteCount(kvp.Key) + EstimateObjectSize(kvp.Value)
            );
        }

        private long EstimateObjectSize(object obj)
        {
            // Very rough estimation
            if (obj == null)
                return 0;

            try
            {
                var json = JsonConvert.SerializeObject(obj);
                return System.Text.Encoding.UTF8.GetByteCount(json);
            }
            catch
            {
                return 1024; // Default 1KB estimate
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Clear();
        }
    }
}
