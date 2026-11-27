namespace AtomUI.Controls;

/// <summary>
/// The cache and network options of one <see cref="ImageLoadRequest" />.
/// </summary>
/// <remarks>
/// A request whose headers include an authentication header and that has no
/// <see cref="CachePartition" /> is isolated for safety: it reads no shared cache, writes no cache, is not
/// merged with other requests, and its effective <see cref="CacheStorage" /> is
/// <see cref="ImageCacheStoragePolicy.None" />. Authentication headers are <c>Authorization</c>,
/// <c>Proxy-Authorization</c>, <c>Cookie</c> and the names added with
/// <see cref="ImageLoadingOptionsBuilder.AddAuthenticationHeaderName" />.
/// </remarks>
public sealed record ImageRequestOptions
{
    /// <summary>
    /// How the loader decides between cached content and the source.
    /// </summary>
    public ImageCacheReadPolicy CacheRead { get; init; } = ImageCacheReadPolicy.ValidateSource;

    /// <summary>
    /// The caches that may store the content of this request. It does not make the loader read the source; use
    /// <see cref="ImageCacheReadPolicy.RefreshSource" /> for that.
    /// </summary>
    public ImageCacheStoragePolicy CacheStorage { get; init; } = ImageCacheStoragePolicy.MemoryAndDisk;

    /// <summary>
    /// A name that keeps the cached content of this request apart from the content stored under other
    /// partitions. It must not be blank; <see langword="null" /> stores the content in the shared partition.
    /// </summary>
    public string? CachePartition { get; init; }

    /// <summary>
    /// A label that keeps a separately cached copy of the same source. Loads with different variants share no
    /// cached content. It must not be blank.
    /// </summary>
    public string? Variant { get; init; }

    /// <summary>
    /// How long the whole load may take, counted from the call and covering queueing, transfer, validation and
    /// decode. <see langword="null" /> uses
    /// <see cref="ImageLoadingOptionsBuilder.DefaultRequestTimeout" />. When the time passes, the load fails with
    /// <see cref="ImageLoadErrorCode.Timeout" />. It must be a finite positive duration.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// The headers sent with an HTTP request, or <see langword="null" /> for none. Names are compared without
    /// case. The headers are copied when the load starts, and they take part in the cache identity for every
    /// source kind.
    /// </summary>
    /// <remarks>
    /// <c>Host</c>, <c>Content-Length</c>, <c>Connection</c>, <c>Transfer-Encoding</c>, <c>Upgrade</c>,
    /// <c>Range</c> and <c>Proxy-Connection</c> are rejected, and so is a value that contains a line break.
    /// </remarks>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }
}
