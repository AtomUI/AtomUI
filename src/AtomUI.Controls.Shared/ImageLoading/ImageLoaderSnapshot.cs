namespace AtomUI.Controls;

/// <summary>
/// A sample of the scheduler queues, the memory caches and the load counters of a loader, taken when
/// <see cref="IImageLoader.Snapshot" /> is read.
/// </summary>
/// <param name="ActiveReads">
/// The downloads and local reads that are running.
/// </param>
/// <param name="QueuedReads">
/// The downloads and local reads that wait for a free slot.
/// </param>
/// <param name="ActiveDecodes">
/// The decodes that are running.
/// </param>
/// <param name="QueuedDecodes">
/// The decodes that wait for a free slot.
/// </param>
/// <param name="EncodedCacheEntries">
/// The number of entries in the memory cache of encoded bytes.
/// </param>
/// <param name="EncodedCacheBytes">
/// The bytes held by the memory cache of encoded bytes.
/// </param>
/// <param name="DecodedCacheEntries">
/// The number of entries in the memory cache of decoded images.
/// </param>
/// <param name="DecodedCacheBytes">
/// The cost accounted to the memory cache of decoded images; a raster image counts as
/// <c>width * height * 4</c> bytes.
/// </param>
/// <param name="CacheHits">
/// The loads so far that were served from a cache or whose cached bytes the server revalidated.
/// </param>
/// <param name="CacheMisses">
/// The loads so far that did not use the cache, including borrowed images.
/// </param>
/// <param name="CanceledLoads">
/// The loads so far that ended in cancellation.
/// </param>
/// <param name="FailedLoads">
/// The loads so far that ended with a failed result, including timeouts.
/// </param>
public readonly record struct ImageLoaderSnapshot(
    int ActiveReads,
    int QueuedReads,
    int ActiveDecodes,
    int QueuedDecodes,
    int EncodedCacheEntries,
    long EncodedCacheBytes,
    int DecodedCacheEntries,
    long DecodedCacheBytes,
    long CacheHits,
    long CacheMisses,
    long CanceledLoads,
    long FailedLoads);
