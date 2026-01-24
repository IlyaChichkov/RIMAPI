// ICachingService.cs
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Models;

namespace RIMAPI.Core
{
    public enum CachePriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3,
    }

    public enum CacheExpirationType
    {
        Absolute, // Expire after fixed time
        Sliding, // Reset timer on access
        Never, // Manual invalidation only
        GameTick, // Expire after N game ticks
    }

    public interface ICachingService
    {
        bool IsEnabled();

        // Basic caching
        bool TryGet<T>(string key, out T value);
        void Set<T>(
            string key,
            T value,
            TimeSpan? expiration = null,
            CachePriority priority = CachePriority.Normal
        );
        void Remove(string key);
        ApiResult<ServerCacheResponseDto> Clear();
        ApiResult<ServerCacheResponseDto> SetEnabled(bool value);

        // Property setter cache methods
        Action<TTarget, TValue> GetPropertySetter<TTarget, TValue>(string propertyName);
        Func<TTarget, TValue> GetPropertyGetter<TTarget, TValue>(string propertyName);

        // Cache-aware response generation
        Task CacheAwareResponseAsync<T>(
            HttpListenerContext context,
            string cacheKey,
            Func<Task<ApiResult<T>>> dataFactory,
            TimeSpan? expiration = null,
            CachePriority priority = CachePriority.Normal,
            CacheExpirationType expirationType = CacheExpirationType.Absolute,
            int? gameTicksExpiration = null
        );

        // Cache invalidation patterns
        void InvalidateByPattern(string pattern);
        void InvalidateByPrefix(string prefix);
        void InvalidateBySuffix(string suffix);

        // Cache statistics
        CacheStatistics GetStatistics(bool includeDetails = false);
        void Trim(CachePriority? priority = null);
    }

    public class CacheEntryDetail
    {
        public string Key { get; set; }
        public string Type { get; set; } // Type of cached value
        public DateTime Created { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime? AbsoluteExpiration { get; set; }
        public double RemainingSeconds { get; set; } // Time until expiration
        public int Hits { get; set; } // Track hits per entry if possible, or just global hits
        public long EstimatedSize { get; set; }
        public string Priority { get; set; }
    }

    public class RecentHitDetail
    {
        public string Key { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsHit { get; set; } // True = Hit, False = Miss/Generated
    }

    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }
        public long MemoryUsageBytes { get; set; }
        public DateTime LastCleanup { get; set; }
        public double HitRatio => TotalEntries > 0 ? (double)Hits / (Hits + Misses) : 0;
        public int CompiledDelegateCount { get; set; }
        public int CompiledDelegateHits { get; set; }
        public int CompiledDelegateMisses { get; set; }

        // New detailed data
        public List<CacheEntryDetail> Entries { get; set; }
        public List<RecentHitDetail> RecentActivity { get; set; }
    }
}
