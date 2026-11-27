namespace AtomUI.Controls;

/// <summary>
/// Identifies the kind of <see cref="ImageSource" /> behind a load or an event.
/// </summary>
public enum ImageSourceKind
{
    /// <summary>
    /// An image that is downloaded over HTTP or HTTPS.
    /// </summary>
    Http,
    /// <summary>
    /// An image that is read from a file on disk.
    /// </summary>
    File,
    /// <summary>
    /// An image that comes from an application asset.
    /// </summary>
    Asset,
    /// <summary>
    /// An image that is read through a file of a storage provider.
    /// </summary>
    StorageFile,
    /// <summary>
    /// An image that is held in a byte array.
    /// </summary>
    Bytes,
    /// <summary>
    /// An image that is read from a stream which a factory opens.
    /// </summary>
    Stream,
    /// <summary>
    /// An image that is already decoded and is supplied by the caller.
    /// </summary>
    Borrowed
}

/// <summary>
/// How a <see cref="FileImageSource" /> detects that the file changed since its content was cached.
/// </summary>
public enum ImageFileValidationMode
{
    /// <summary>
    /// Compares the creation time, the length and the last write time of the file with the cached values. The
    /// file is not read again while they match. This is the default.
    /// </summary>
    Metadata,
    /// <summary>
    /// Reads the file on every load and identifies its content by a hash, so a change that leaves the file times
    /// unchanged is detected.
    /// </summary>
    ContentHash
}

/// <summary>
/// How strictly the conformance of an SVG document is checked. The mode is set once for the application on
/// <see cref="ImageLoadingOptionsBuilder.Svg" />. The security checks are the same in both modes.
/// </summary>
public enum SvgConformanceMode
{
    /// <summary>
    /// Accepts an SVG document with duplicate <c>id</c> attributes.
    /// </summary>
    Compatible,
    /// <summary>
    /// Rejects an SVG document with duplicate <c>id</c> attributes with
    /// <see cref="ImageLoadErrorCode.InvalidImageData" />.
    /// </summary>
    Strict
}

/// <summary>
/// The state of the image that a control shows.
/// </summary>
public enum ImageLoadState
{
    /// <summary>
    /// No load is running and no image is shown, for example because no source is set.
    /// </summary>
    Idle,
    /// <summary>
    /// A load is running.
    /// </summary>
    Loading,
    /// <summary>
    /// An image is shown. It is the fallback image when the main source failed.
    /// </summary>
    Loaded,
    /// <summary>
    /// The load ended with an error and no image is shown.
    /// </summary>
    Failed
}

/// <summary>
/// How a load decides between cached content and the source.
/// </summary>
public enum ImageCacheReadPolicy
{
    /// <summary>
    /// Reuses cached content only while the source confirms that it is current: by HTTP freshness rules or a
    /// conditional request, by the validation mode of a file, or by the revision of a storage file or stream.
    /// This is the default.
    /// </summary>
    ValidateSource,
    /// <summary>
    /// Reads the source on every load. For HTTP the cached validators are sent, so the server can answer
    /// 304 Not Modified and the cached bytes are reused.
    /// </summary>
    RefreshSource,
    /// <summary>
    /// Uses cached content without checking the source, and reads the source only when nothing usable is cached.
    /// </summary>
    PreferCache,
    /// <summary>
    /// Never reads the source. A load that finds nothing usable in the cache fails with
    /// <see cref="ImageLoadErrorCode.CacheMiss" />.
    /// </summary>
    CacheOnly
}

/// <summary>
/// The caches that may store the content a load reads.
/// </summary>
public enum ImageCacheStoragePolicy
{
    /// <summary>
    /// Stores nothing. Cached content may still be read.
    /// </summary>
    None,
    /// <summary>
    /// Stores the encoded bytes and the decoded image in memory.
    /// </summary>
    Memory,
    /// <summary>
    /// Stores in memory and, for a source with a stable identity, in the disk cache when the application enabled
    /// it. This is the default.
    /// </summary>
    MemoryAndDisk
}

/// <summary>
/// How an image control chooses the pixel size that it asks the loader to decode to.
/// </summary>
public enum ImageDecodeMode
{
    /// <summary>
    /// Decodes to the size at which the image is displayed, in physical pixels. For a fill or uniform-to-fill
    /// stretch the decoded image covers the box. This is the default.
    /// </summary>
    Auto,
    /// <summary>
    /// Decodes to the original pixel size of the image.
    /// </summary>
    Original,
    /// <summary>
    /// Decodes to the <c>DecodePixelWidth</c> and <c>DecodePixelHeight</c> of the control. The load fails when
    /// both are zero.
    /// </summary>
    Explicit
}

/// <summary>
/// The order in which queued loads are started. A priority never interrupts a load that has already started.
/// </summary>
/// <remarks>
/// A queued <see cref="Normal" /> or <see cref="Low" /> load moves up one level for every two seconds it
/// waits, up to <see cref="High" />. <see cref="Preload" /> never moves up.
/// </remarks>
public enum ImageRequestPriority
{
    /// <summary>
    /// The image that an image previewer shows at full size.
    /// </summary>
    Critical,
    /// <summary>
    /// An image that is about to be shown, such as a thumbnail or a cover image.
    /// </summary>
    High,
    /// <summary>
    /// An ordinary visible image. This is the default.
    /// </summary>
    Normal,
    /// <summary>
    /// An image that is not urgent.
    /// </summary>
    Low,
    /// <summary>
    /// An image that is fetched for later use. It starts only when no load of a higher priority is queued.
    /// </summary>
    Preload
}

/// <summary>
/// Where the image that a load delivers came from.
/// </summary>
public enum ImageLoadOrigin
{
    /// <summary>
    /// The <see cref="BorrowedImageSource.Image" /> that the caller supplied.
    /// </summary>
    Borrowed,
    /// <summary>
    /// The memory cache of decoded images.
    /// </summary>
    DecodedMemory,
    /// <summary>
    /// The memory cache of encoded bytes, which were decoded for this load.
    /// </summary>
    EncodedMemory,
    /// <summary>
    /// The disk cache.
    /// </summary>
    Persistent,
    /// <summary>
    /// An HTTP or HTTPS download.
    /// </summary>
    Network,
    /// <summary>
    /// A local source: a file, an application asset, a storage file, a byte array or a stream.
    /// </summary>
    Local
}

/// <summary>
/// How a load established that its content is what the source currently serves.
/// </summary>
public enum ImageSourceValidation
{
    /// <summary>
    /// No validation applies, because the content is held in memory or is the image of the caller.
    /// </summary>
    NotRequired,
    /// <summary>
    /// The content was read from the source, or the source confirmed that the cached copy is unchanged.
    /// </summary>
    Current,
    /// <summary>
    /// The server answered a conditional HTTP request for cached bytes with 304 Not Modified.
    /// </summary>
    Revalidated,
    /// <summary>
    /// Cached content was used without asking the source whether it is still current.
    /// </summary>
    Unverified
}

/// <summary>
/// The steps a load goes through, reported as <see cref="ImageLoadProgress.Stage" />.
/// </summary>
public enum ImageLoadStage
{
    /// <summary>
    /// The load has entered the pipeline and is looking for work in flight to join.
    /// </summary>
    Resolving,
    /// <summary>
    /// A cached copy of the source or of its decoded image is being looked up.
    /// </summary>
    CacheLookup,
    /// <summary>
    /// The load waits in a scheduler queue for a free download, read or decode slot.
    /// </summary>
    Queued,
    /// <summary>
    /// The bytes of a local source are being read.
    /// </summary>
    Reading,
    /// <summary>
    /// The bytes of an HTTP response are being received.
    /// </summary>
    Downloading,
    /// <summary>
    /// The format, the dimensions, the resource limits and, for SVG, the content safety are being checked.
    /// </summary>
    Validating,
    /// <summary>
    /// The drawable image is being built from the validated content.
    /// </summary>
    Decoding
}

/// <summary>
/// The reason an image load failed, reported as <see cref="ImageLoadError.Code" />.
/// </summary>
public enum ImageLoadErrorCode
{
    /// <summary>
    /// The source cannot be used or read, for example an invalid HTTP URI, an unreadable file or an unexpected
    /// failure.
    /// </summary>
    InvalidSource,
    /// <summary>
    /// The scheme of the source is not supported. No built-in source reports this code.
    /// </summary>
    UnsupportedScheme,
    /// <summary>
    /// Access to a file or storage file was denied, or direct file access is unavailable on the platform.
    /// </summary>
    AccessDenied,
    /// <summary>
    /// The file or the application asset does not exist.
    /// </summary>
    NotFound,
    /// <summary>
    /// The HTTP request could not be sent or its response could not be read.
    /// </summary>
    NetworkFailure,
    /// <summary>
    /// The load exceeded its timeout.
    /// </summary>
    Timeout,
    /// <summary>
    /// The server answered with an error status, which <see cref="ImageLoadError.HttpStatus" /> reports.
    /// </summary>
    HttpStatus,
    /// <summary>
    /// The download followed more redirects than <see cref="ImageLoadingOptionsBuilder.MaxRedirects" /> allows.
    /// </summary>
    TooManyRedirects,
    /// <summary>
    /// A load with <see cref="ImageCacheReadPolicy.CacheOnly" /> found nothing usable in the cache.
    /// </summary>
    CacheMiss,
    /// <summary>
    /// The encoded image or the SVG document is larger than the configured byte limit.
    /// </summary>
    ResponseTooLarge,
    /// <summary>
    /// An HTTPS request was redirected to HTTP.
    /// </summary>
    RedirectBlocked,
    /// <summary>
    /// The origin of the request or of a redirect target is not on the HTTP origin allow list of the application.
    /// </summary>
    OriginNotAllowed,
    /// <summary>
    /// The declared media type does not match the actual format of the content.
    /// </summary>
    ContentTypeMismatch,
    /// <summary>
    /// The content is not a recognized image format, or no registered codec can decode it.
    /// </summary>
    UnsupportedFormat,
    /// <summary>
    /// Vector content is not safe to render: markup was served with an HTML content type, or the SVG contains
    /// scripts, event handlers, a DTD or references to external resources.
    /// </summary>
    UnsafeVectorContent,
    /// <summary>
    /// The image data is malformed, for example a truncated header or invalid SVG XML. In strict SVG mode a
    /// duplicate <c>id</c> also has this code.
    /// </summary>
    InvalidImageData,
    /// <summary>
    /// The SVG exceeds a structural limit such as the XML characters, the elements, the depth or the path data,
    /// or its references form a cycle.
    /// </summary>
    VectorComplexityLimitExceeded,
    /// <summary>
    /// The images embedded in an SVG exceed the configured count or byte limit.
    /// </summary>
    EmbeddedResourceLimitExceeded,
    /// <summary>
    /// The image is wider or taller than the configured maximum.
    /// </summary>
    DimensionLimitExceeded,
    /// <summary>
    /// The image has more pixels than the configured maximum.
    /// </summary>
    PixelLimitExceeded,
    /// <summary>
    /// The decoded image, including the images embedded in an SVG, needs more bytes than the configured maximum.
    /// </summary>
    DecodedByteLimitExceeded,
    /// <summary>
    /// The image is animated and has more than one frame.
    /// </summary>
    AnimationNotSupported,
    /// <summary>
    /// The codec could not decode the validated content.
    /// </summary>
    DecodeFailed
}

/// <summary>
/// The point of a load that an <see cref="IImageLoader.LoadEvent" /> reports.
/// </summary>
public enum ImageLoaderEventKind
{
    /// <summary>
    /// The load was accepted and is about to begin.
    /// </summary>
    Started,
    /// <summary>
    /// The load succeeded from a cache, or the server revalidated the cached bytes with 304 Not Modified.
    /// </summary>
    CacheHit,
    /// <summary>
    /// The load succeeded without using the cache.
    /// </summary>
    Completed,
    /// <summary>
    /// The load ended with a failed result, including a timeout.
    /// </summary>
    Failed,
    /// <summary>
    /// The load was canceled and ends with an <see cref="OperationCanceledException" />.
    /// </summary>
    Canceled
}
