using Avalonia;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// The settings and target geometry that a motion uses while it is built.
/// </summary>
/// <remarks>
/// A new context is created for every request and passed to <see cref="AbstractMotion.Build" />. It holds no
/// target, so a build cannot change the target.
/// </remarks>
public readonly struct MotionContext
{
    /// <summary>
    /// The size of the bounds of the target in DIPs, read when the motion is prepared.
    /// </summary>
    public Size TargetSize { get; }
    /// <summary>
    /// The render scaling of the top level that hosts the target, or 1 when the target has none.
    /// </summary>
    public double RenderScaling { get; }
    /// <summary>
    /// The duration the motion is built for, or zero when animation is disabled on the target. A child of a composition
    /// gets its own duration or the default duration.
    /// </summary>
    public TimeSpan Duration { get; }
    /// <summary>
    /// The delay of the motion that is built. The delay of a child offsets that child inside a composition.
    /// </summary>
    public TimeSpan Delay { get; }
    /// <summary>
    /// The easing of the motion that is built: its own easing, or the default easing of its definition.
    /// </summary>
    public Easing Easing { get; }
    /// <summary>
    /// The opacity of the target beneath any animation, or the default opacity of 1 when none is set.
    /// </summary>
    public double BaseOpacity { get; }
    /// <summary>
    /// The side of the target where a layout motion adds or removes space, taken from
    /// <see cref="Motion.LayoutDirectionProperty" /> on the target.
    /// </summary>
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
