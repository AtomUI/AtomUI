namespace AtomUI.Controls;

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

public sealed class ImageLoadRequest
{
    private int _decodePixelWidth;
    private int _decodePixelHeight;

    public ImageLoadRequest(ImageSource source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public ImageSource Source { get; }

    public ImageRequestOptions? Options { get; init; }

    public int DecodePixelWidth
    {
        get => _decodePixelWidth;
        init => _decodePixelWidth = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

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

    public ImageRequestPriority Priority { get; init; } = ImageRequestPriority.Normal;

    public IProgress<ImageLoadProgress>? Progress { get; init; }

    internal ImageRequestPriorityState? PriorityState { get; init; }
}
