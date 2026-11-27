using System.Collections.Concurrent;

namespace AtomUI.Controls;

internal static class IconProviderCache
{
    /// <summary>
    /// Maximum number of enum types to cache. When exceeded, oldest entries are removed.
    /// </summary>
    private const int MaxCacheSize = 256;
    
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, Func<Icon>>> CreatorCache = 
        new();
    
    /// <summary>
    /// Tracks insertion order for FIFO eviction when cache exceeds MaxCacheSize.
    /// </summary>
    private static readonly Queue<Type> CacheInsertionOrder = new();
    
    private static readonly object _lockObject = new();
    
    public static Func<Icon> GetOrAddCreator(Type enumType, object enumValue, 
        Func<object, Func<Icon>> creatorFactory)
    {
        EnsureCacheSize();
        
        var cache = CreatorCache.GetOrAdd(enumType, _ =>
        {
            lock (_lockObject)
            {
                if (!CacheInsertionOrder.Contains(enumType))
                {
                    CacheInsertionOrder.Enqueue(enumType);
                }
            }
            return new ConcurrentDictionary<object, Func<Icon>>();
        });
        
        return cache.GetOrAdd(enumValue, creatorFactory);
    }
    
    /// <summary>
    /// Ensures cache size doesn't exceed MaxCacheSize by removing oldest entries.
    /// </summary>
    private static void EnsureCacheSize()
    {
        lock (_lockObject)
        {
            while (CreatorCache.Count > MaxCacheSize && CacheInsertionOrder.Count > 0)
            {
                var oldestType = CacheInsertionOrder.Dequeue();
                CreatorCache.TryRemove(oldestType, out _);
            }
        }
    }
    
    public static void ClearCache(Type enumType)
    {
        lock (_lockObject)
        {
            CreatorCache.TryRemove(enumType, out _);
            // The type stays in CacheInsertionOrder because a queue cannot drop one entry. EnsureCacheSize
            // dequeues it later, and removing a type that is no longer cached does nothing.
        }
    }
    
    public static void ClearAllCache()
    {
        lock (_lockObject)
        {
            CreatorCache.Clear();
            CacheInsertionOrder.Clear();
        }
    }
}
