// CachingService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        // Generic Property Setter Cache
        private static class PropertySetterCache
        {
            private static readonly Dictionary<string, Delegate> _setters =
                new Dictionary<string, Delegate>();
            private static readonly object _lock = new object();

            public static Action<TTarget, TValue> GetSetter<TTarget, TValue>(string propertyName)
            {
                var cacheKey =
                    $"{typeof(TTarget).FullName}.{propertyName}<{typeof(TValue).FullName}>";

                lock (_lock)
                {
                    if (!_setters.TryGetValue(cacheKey, out var setter))
                    {
                        // Create compiled expression for high-performance property setting
                        var targetParam = Expression.Parameter(typeof(TTarget), "target");
                        var valueParam = Expression.Parameter(typeof(TValue), "value");
                        var property = Expression.Property(targetParam, propertyName);
                        var assign = Expression.Assign(property, valueParam);
                        var lambda = Expression.Lambda<Action<TTarget, TValue>>(
                            assign,
                            targetParam,
                            valueParam
                        );
                        setter = lambda.Compile();
                        _setters[cacheKey] = setter;

                        LogApi.Message(
                            $"[PropertySetterCache] Created setter for {cacheKey}",
                            LoggingLevels.DEBUG
                        );
                    }

                    return (Action<TTarget, TValue>)setter;
                }
            }

            public static Func<TTarget, TValue> GetGetter<TTarget, TValue>(string propertyName)
            {
                var cacheKey =
                    $"GET_{typeof(TTarget).FullName}.{propertyName}<{typeof(TValue).FullName}>";

                lock (_lock)
                {
                    if (!_setters.TryGetValue(cacheKey, out var getter))
                    {
                        // Create compiled expression for high-performance property getting
                        var targetParam = Expression.Parameter(typeof(TTarget), "target");
                        var property = Expression.Property(targetParam, propertyName);
                        var lambda = Expression.Lambda<Func<TTarget, TValue>>(
                            property,
                            targetParam
                        );
                        getter = lambda.Compile();
                        _setters[cacheKey] = getter;

                        LogApi.Message(
                            $"[PropertySetterCache] Created getter for {cacheKey}",
                            LoggingLevels.DEBUG
                        );
                    }

                    return (Func<TTarget, TValue>)getter;
                }
            }

            public static void Clear()
            {
                lock (_lock)
                {
                    int count = _setters.Count;
                    _setters.Clear();
                    LogApi.Message(
                        $"[PropertySetterCache] Cleared {count} compiled delegates",
                        LoggingLevels.DEBUG
                    );
                }
            }

            public static int Count => _setters.Count;
        }

        // Main Cache Section
        private readonly Dictionary<string, CacheEntry> _cache =
            new Dictionary<string, CacheEntry>();
        private readonly object _cacheLock = new object();
        private bool _disposed = false;
        private bool _enabled = true; // Enabled by default

        public bool IsEnabled() => _enabled;

        // Statistics
        private int _hits = 0;
        private int _misses = 0;
        private DateTime _lastCleanup = DateTime.UtcNow;
        private int _lastCleanupTick;
        private int _compiledDelegateHits = 0;
        private int _compiledDelegateMisses = 0;
        private readonly RIMAPI_Settings _settings;

        public CachingService(RIMAPI_Settings settings)
        {
            LogApi.Info("[CachingService] Initialized");
            if (Current.ProgramState == ProgramState.Playing)
            {
                _lastCleanupTick = Find.TickManager.TicksGame;
            }
            else
            {
                _lastCleanupTick = 0;
            }
            _settings = settings;

            _enabled = _settings.EnableCaching;
        }

        // Public Compiled Delegate Cache API (Generic)
        public Action<TTarget, TValue> GetPropertySetter<TTarget, TValue>(string propertyName)
        {
            try
            {
                var setter = PropertySetterCache.GetSetter<TTarget, TValue>(propertyName);
                _compiledDelegateHits++;
                return setter;
            }
            catch (Exception ex)
            {
                _compiledDelegateMisses++;
                LogApi.Error(
                    $"[PropertySetterCache] Failed to get setter for {typeof(TTarget).Name}.{propertyName}<{typeof(TValue).Name}>: {ex}"
                );
                throw;
            }
        }

        public Func<TTarget, TValue> GetPropertyGetter<TTarget, TValue>(string propertyName)
        {
            try
            {
                var getter = PropertySetterCache.GetGetter<TTarget, TValue>(propertyName);
                _compiledDelegateHits++;
                return getter;
            }
            catch (Exception ex)
            {
                _compiledDelegateMisses++;
                LogApi.Error(
                    $"[PropertySetterCache] Failed to get getter for {typeof(TTarget).Name}.{propertyName}<{typeof(TValue).Name}>: {ex}"
                );
                throw;
            }
        }

        public void ClearCompiledDelegates() => PropertySetterCache.Clear();

        public int CompiledDelegateCount => PropertySetterCache.Count;

        public (int hits, int misses) GetCompiledDelegateStats() =>
            (_compiledDelegateHits, _compiledDelegateMisses);

        // Main Cache Methods
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
            }
        }

        public void SetWithExpirationType<T>(
            string key,
            T value,
            CacheExpirationType expirationType = CacheExpirationType.Absolute,
            TimeSpan? expiration = null,
            int? gameTicksExpiration = null,
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
                    ExpirationType = expirationType,
                    GameTickAdded = Find.TickManager?.TicksGame ?? 0,
                };

                switch (expirationType)
                {
                    case CacheExpirationType.Absolute:
                        if (expiration.HasValue)
                            entry.AbsoluteExpiration = now.Add(expiration.Value);
                        break;

                    case CacheExpirationType.Sliding:
                        if (expiration.HasValue)
                            entry.SlidingExpiration = expiration.Value;
                        break;

                    case CacheExpirationType.GameTick:
                        if (gameTicksExpiration.HasValue)
                            entry.GameTickExpiration = gameTicksExpiration.Value;
                        break;

                    case CacheExpirationType.Never:
                        // No expiration
                        break;
                }

                _cache[key] = entry;
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
                int count = _cache.Count;
                _cache.Clear();
                LogApi.Message($"[Cache] Cleared {count} entries", LoggingLevels.DEBUG);
            }
        }

        public async Task CacheAwareResponseAsync<T>(
            HttpListenerContext context,
            string cacheKey,
            Func<Task<ApiResult<T>>> dataFactory,
            TimeSpan? expiration = null,
            CachePriority priority = CachePriority.Normal,
            CacheExpirationType expirationType = CacheExpirationType.Absolute,
            int? gameTicksExpiration = null
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
                    SetWithExpirationType(cacheKey, result, expirationType, expiration, gameTicksExpiration, priority);
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
                    CompiledDelegateCount = CompiledDelegateCount,
                    CompiledDelegateHits = _compiledDelegateHits,
                    CompiledDelegateMisses = _compiledDelegateMisses,
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
                    Clear();
                    ClearCompiledDelegates();
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
            }
        }

        private bool IsExpired(CacheEntry entry)
        {
            var now = DateTime.UtcNow;
            var currentTick = Find.TickManager?.TicksGame ?? 0;
            return IsExpired(entry, now, currentTick);
        }

        private long CalculateMemoryUsage()
        {
            // Rough estimation
            return _cache.Sum(kvp =>
                System.Text.Encoding.UTF8.GetByteCount(kvp.Key) + EstimateObjectSize(kvp.Value)
            );
        }

        private long EstimateObjectSize(object obj)
        {
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
            ClearCompiledDelegates();
            LogApi.Info("[CachingService] Disposed");
        }
    }
}
