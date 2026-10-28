using Avalonia;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public readonly struct MotionContext
{
    public Size TargetSize { get; }
    public double RenderScaling { get; }
    public TimeSpan Duration { get; }
    public TimeSpan Delay { get; }
    public Easing Easing { get; }
    public double BaseOpacity { get; }
    public Direction LayoutDirection { get; }

    internal MotionValues InitialValues { get; }
    internal TimeSpan DefaultDuration { get; }
    internal bool HasExplicitDuration { get; }
    internal MotionPreparation Preparation { get; }
    internal IReadOnlyDictionary<AvaloniaProperty, object?>? InitialProperties { get; }

    internal MotionContext(Size targetSize, double renderScaling, TimeSpan duration,
        TimeSpan delay, Easing easing, MotionValues initialValues, double baseOpacity,
        TimeSpan defaultDuration, bool hasExplicitDuration, MotionPreparation preparation,
        IReadOnlyDictionary<AvaloniaProperty, object?>? initialProperties = null, Direction layoutDirection = Direction.Bottom)
    {
        TargetSize = targetSize;
        RenderScaling = renderScaling;
        Duration = duration;
        Delay = delay;
        Easing = easing;
        InitialValues = initialValues;
        BaseOpacity = baseOpacity;
        DefaultDuration = defaultDuration;
        HasExplicitDuration = hasExplicitDuration;
        Preparation = preparation;
        InitialProperties = initialProperties;
        LayoutDirection = layoutDirection;
    }
}
