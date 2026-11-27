namespace AtomUI.Controls;

/// <summary>
/// Holds the priority of one image load and lets its owner raise that priority while the load is queued or
/// running; <c>ImagePreviewEntry</c> in AtomUI.Controls owns one per thumbnail and full image load.
/// </summary>
/// <remarks>
/// <c>Promote</c> only lowers the numeric value, so a load is never moved back to a lower priority, and it raises
/// <c>PriorityChanged</c> once per successful change. The normalized request exposes the value as its
/// <c>Priority</c>.
/// </remarks>
internal sealed class ImageRequestPriorityState
{
    private int _priority;

    internal ImageRequestPriorityState(ImageRequestPriority priority)
    {
        _priority = (int)priority;
    }

    internal event Action? PriorityChanged;

    internal ImageRequestPriority Priority => (ImageRequestPriority)Volatile.Read(ref _priority);

    internal void Promote(ImageRequestPriority priority)
    {
        var requested = (int)priority;
        var current = Volatile.Read(ref _priority);
        while (requested < current)
        {
            var observed = Interlocked.CompareExchange(ref _priority, requested, current);
            if (observed == current)
            {
                PriorityChanged?.Invoke();
                return;
            }
            current = observed;
        }
    }
}

/// <summary>
/// Describes one image load: which <see cref="ImageSource" /> to load, how to cache it and the pixel size to
/// decode it to.
/// </summary>
public sealed class ImageLoadRequest
{
    private int _decodePixelWidth;
    private int _decodePixelHeight;

    /// <summary>
    /// Creates a request that loads <paramref name="source" />.
    /// </summary>
    /// <param name="source">
    /// The image to load.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source" /> is <see langword="null" />.
    /// </exception>
    public ImageLoadRequest(ImageSource source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// The image to load.
    /// </summary>
    public ImageSource Source { get; }

    /// <summary>
    /// The cache and network options of this load, or <see langword="null" /> to use the defaults of
    /// <see cref="ImageRequestOptions" />.
    /// </summary>
    public ImageRequestOptions? Options { get; init; }

    /// <summary>
    /// The largest width to decode the image to, in physical pixels. The aspect ratio is kept and the image is
    /// never scaled up.
    /// </summary>
    /// <remarks>
    /// 0 leaves the width unbounded; 0 for both dimensions decodes at the original size. Only a codec that decodes
    /// to a pixel size applies the limit; the SVG codec ignores it. A negative value throws
    /// <see cref="ArgumentOutOfRangeException" />.
    /// </remarks>
    public int DecodePixelWidth
    {
        get => _decodePixelWidth;
        init => _decodePixelWidth = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

    /// <summary>
    /// The largest height to decode the image to, in physical pixels. The aspect ratio is kept and the image is
    /// never scaled up.
    /// </summary>
    /// <remarks>
    /// 0 leaves the height unbounded; 0 for both dimensions decodes at the original size. Only a codec that decodes
    /// to a pixel size applies the limit; the SVG codec ignores it. A negative value throws
    /// <see cref="ArgumentOutOfRangeException" />.
    /// </remarks>
    public int DecodePixelHeight
    {
        get => _decodePixelHeight;
        init => _decodePixelHeight = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

    // When both decode sizes are set, decode so the result covers the box instead of fitting inside it
    // (Fill/UniformToFill presentation). Has no effect when either size is zero.
    internal bool DecodeToCover { get; init; }

    /// <summary>
    /// The priority this load has while it is queued. It does not interrupt work that has already started. Work
    /// shared by several loads runs at the highest priority among them.
    /// </summary>
    public ImageRequestPriority Priority { get; init; } = ImageRequestPriority.Normal;

    /// <summary>
    /// Receives <see cref="ImageLoadProgress" /> reports while the load runs, or <see langword="null" /> for
    /// none. Reports arrive on the thread that reaches each stage, not on the UI thread, and an exception thrown
    /// by the reporter does not fail the load.
    /// </summary>
    public IProgress<ImageLoadProgress>? Progress { get; init; }

    internal ImageRequestPriorityState? PriorityState { get; init; }
}
