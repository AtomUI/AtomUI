namespace AtomUI.Controls;

/// <summary>
/// Carries the details of one <see cref="IImageLoader.LoadEvent" />.
/// </summary>
public sealed class ImageLoaderEventArgs : EventArgs
{
    internal ImageLoaderEventArgs(
        ImageLoaderEventKind kind,
        ImageSourceKind sourceKind,
        ImageLoadOrigin? origin,
        ImageLoadErrorCode? errorCode,
        TimeSpan elapsed)
    {
        Kind = kind;
        SourceKind = sourceKind;
        Origin = origin;
        ErrorCode = errorCode;
        Elapsed = elapsed;
    }

    /// <summary>
    /// The point of the load that the event reports.
    /// </summary>
    public ImageLoaderEventKind Kind { get; }

    /// <summary>
    /// The kind of <see cref="ImageSource" /> that is being loaded.
    /// </summary>
    public ImageSourceKind SourceKind { get; }

    /// <summary>
    /// Where the image came from. It is set only for <see cref="ImageLoaderEventKind.CacheHit" /> and
    /// <see cref="ImageLoaderEventKind.Completed" />.
    /// </summary>
    public ImageLoadOrigin? Origin { get; }

    /// <summary>
    /// The code of the failure. It is set only for <see cref="ImageLoaderEventKind.Failed" />.
    /// </summary>
    public ImageLoadErrorCode? ErrorCode { get; }

    /// <summary>
    /// The time that passed since the load started.
    /// </summary>
    public TimeSpan Elapsed { get; }
}
