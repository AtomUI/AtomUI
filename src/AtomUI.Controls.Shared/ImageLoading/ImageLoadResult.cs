using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// The outcome of one <see cref="IImageLoader.LoadAsync" /> call: a decoded image or an error. A successful
/// result holds a lease on the image; dispose it when the image is no longer needed.
/// </summary>
public sealed class ImageLoadResult : IDisposable
{
    private IDisposable? _lease;

    internal ImageLoadResult(
        IImage image,
        IDisposable lease,
        int originalPixelWidth,
        int originalPixelHeight,
        int decodedPixelWidth,
        int decodedPixelHeight,
        string? mediaType,
        ImageLoadOrigin origin,
        ImageSourceValidation sourceValidation,
        string? contentId,
        IReadOnlyDictionary<ImageLoadStage, TimeSpan>? stageDurations = null)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        _lease = lease ?? throw new ArgumentNullException(nameof(lease));
        OriginalPixelWidth = originalPixelWidth;
        OriginalPixelHeight = originalPixelHeight;
        DecodedPixelWidth = decodedPixelWidth;
        DecodedPixelHeight = decodedPixelHeight;
        MediaType = mediaType;
        Origin = origin;
        SourceValidation = sourceValidation;
        ContentId = contentId;
        StageDurations = stageDurations ?? EmptyDurations;
    }

    internal ImageLoadResult(
        ImageLoadError error,
        IReadOnlyDictionary<ImageLoadStage, TimeSpan>? stageDurations = null)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        StageDurations = stageDurations ?? EmptyDurations;
    }

    /// <summary>
    /// <see langword="true" /> when <see cref="Image" /> is available; <see langword="false" /> when the load
    /// failed and <see cref="Error" /> says why.
    /// </summary>
    public bool IsSuccess => Image is not null;

    /// <summary>
    /// The decoded image, or <see langword="null" /> when the load failed. The image is valid until this result
    /// is disposed. Release it by disposing this result, not the image.
    /// </summary>
    public IImage? Image { get; }

    /// <summary>
    /// Why the load failed, or <see langword="null" /> when it succeeded.
    /// </summary>
    public ImageLoadError? Error { get; }

    /// <summary>
    /// The width of the source image in pixels, before the decode size was applied. It is 0 when the load failed.
    /// </summary>
    public int OriginalPixelWidth { get; }

    /// <summary>
    /// The height of the source image in pixels, before the decode size was applied. It is 0 when the load failed.
    /// </summary>
    public int OriginalPixelHeight { get; }

    /// <summary>
    /// The width of <see cref="Image" /> in pixels. It is 0 when the load failed.
    /// </summary>
    public int DecodedPixelWidth { get; }

    /// <summary>
    /// The height of <see cref="Image" /> in pixels. It is 0 when the load failed.
    /// </summary>
    public int DecodedPixelHeight { get; }

    /// <summary>
    /// The media type derived from the format of the content, such as <c>image/png</c> or
    /// <c>image/svg+xml</c>, or <see langword="null" /> for a borrowed image or a failed load.
    /// </summary>
    public string? MediaType { get; }

    /// <summary>
    /// Where the bytes or the image of this load came from. Read it together with
    /// <see cref="SourceValidation" />: a result can come from a cache and still be confirmed by the server.
    /// </summary>
    public ImageLoadOrigin Origin { get; }

    /// <summary>
    /// How the load established that the content is what the source currently serves.
    /// </summary>
    public ImageSourceValidation SourceValidation { get; }

    /// <summary>
    /// The SHA-256 digest of the encoded content as a lowercase hexadecimal string, or <see langword="null" />
    /// for a borrowed image or a failed load.
    /// </summary>
    public string? ContentId { get; }

    /// <summary>
    /// The time spent in each <see cref="ImageLoadStage" /> the load entered. A stage that was entered several
    /// times is summed up, and a stage that was not entered has no entry.
    /// </summary>
    public IReadOnlyDictionary<ImageLoadStage, TimeSpan> StageDurations { get; }

    /// <summary>
    /// Releases the lease on <see cref="Image" />. The image is destroyed when no result and no cache entry uses
    /// it anymore. Calling this method again has no effect.
    /// </summary>
    public void Dispose()
    {
        Interlocked.Exchange(ref _lease, null)?.Dispose();
    }

    private static IReadOnlyDictionary<ImageLoadStage, TimeSpan> EmptyDurations { get; } =
        new Dictionary<ImageLoadStage, TimeSpan>();
}
