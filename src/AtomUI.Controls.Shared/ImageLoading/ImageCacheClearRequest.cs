namespace AtomUI.Controls;

/// <summary>
/// Tells <see cref="IImageLoader.ClearCacheAsync" /> which caches to clear and whether to cancel loads that
/// are in flight. Every flag defaults to <see langword="true" />, so an empty request clears every cache in
/// every partition and cancels every load in flight.
/// </summary>
public sealed record ImageCacheClearRequest
{
    /// <summary>
    /// The name of the cache partition to clear, as given in
    /// <see cref="ImageRequestOptions.CachePartition" />, or <see langword="null" /> to clear every partition.
    /// </summary>
    public string? CachePartition { get; init; }

    /// <summary>
    /// <see langword="true" /> to evict the decoded images from memory. An evicted image stays alive while a
    /// result returned earlier still holds it.
    /// </summary>
    public bool ClearDecodedMemory { get; init; } = true;

    /// <summary>
    /// <see langword="true" /> to drop the encoded image bytes cached in memory.
    /// </summary>
    public bool ClearEncodedMemory { get; init; } = true;

    /// <summary>
    /// <see langword="true" /> to delete the disk cache. Has no effect when the disk cache is disabled.
    /// </summary>
    public bool ClearPersistent { get; init; } = true;

    /// <summary>
    /// <see langword="true" /> to cancel the queued and running loads in the scope, whichever caller started
    /// them. Their <see cref="IImageLoader.LoadAsync" /> calls end with
    /// <see cref="OperationCanceledException" />.
    /// </summary>
    public bool CancelInFlight { get; init; } = true;
}
