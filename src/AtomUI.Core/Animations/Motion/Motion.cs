using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Animations;

public abstract partial class Motion
{
    public TimeSpan? Duration { get; set; }
    public TimeSpan Delay { get; set; }
    public Easing? Easing { get; set; }
    public RelativePoint? Origin { get; set; }
    public FillMode FillMode { get; set; } = FillMode.Both;

    protected Motion()
    {
    }

    protected Motion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
    {
        Duration = duration;
        Easing = easing;
        FillMode = fillMode;
    }

    public Task<MotionResult> PlayAsync(Control target, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);
        Dispatcher.UIThread.VerifyAccess();
        return MotionPlayback.PlayAsync(this, target, null, cancellationToken);
    }

    public static void Reset(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        Dispatcher.UIThread.VerifyAccess();
        MotionPlayback.Reset(target);
    }

    public static bool IsCurrent(Control target, MotionResult result)
    {
        ArgumentNullException.ThrowIfNull(target);
        Dispatcher.UIThread.VerifyAccess();
        return MotionPlayback.IsCurrent(target, result);
    }

    protected virtual Easing DefaultEasing => MotionCurves.Linear;

    protected abstract void Build(MotionBuilder builder, in MotionContext context);

    internal Task<MotionResult> PlayAsync(Control target, TimeSpan duration, CancellationToken cancellationToken)
    {
        Dispatcher.UIThread.VerifyAccess();
        return MotionPlayback.PlayAsync(this, target, duration, cancellationToken);
    }

    internal MotionPlan Prepare(Control target, MotionValues initialValues, TimeSpan? durationOverride)
    {
        var defaultDuration = TokenResourceUtils.FindTokenResource(target, SharedTokenKind.MotionDurationMid) is TimeSpan tokenDuration
            ? tokenDuration : TimeSpan.FromMilliseconds(200);
        var requestedDuration = durationOverride ?? GetDurationOverride(target);
        var duration = requestedDuration ?? Duration ?? defaultDuration;
        var disabled = !GetIsEnabled(target) || TokenResourceUtils.FindTokenResource(target, SharedTokenKind.EnableMotion) is false;
        if (disabled)
        {
            duration = TimeSpan.Zero;
        }
        var scaling = TopLevel.GetTopLevel(target)?.RenderScaling ?? 1;
        var opacity = target.GetBaseValue(Visual.OpacityProperty);
        var baseOpacity = opacity.HasValue ? opacity.Value : Visual.OpacityProperty.GetDefaultValue(target);
        var context = new MotionContext(target.Bounds.Size, scaling, duration, Delay, Easing ?? DefaultEasing,
            initialValues, baseOpacity, defaultDuration, requestedDuration.HasValue || Duration.HasValue || disabled,
            new MotionPreparation(target), layoutDirection: GetLayoutDirection(target));
        return PrepareDefinition(context);
    }

    internal MotionPlan PrepareChild(in MotionContext parent, MotionValues initialValues,
        IReadOnlyDictionary<AvaloniaProperty, object?>? initialProperties = null)
    {
        var context = new MotionContext(parent.TargetSize, parent.RenderScaling, Duration ?? parent.DefaultDuration,
            Delay, Easing ?? DefaultEasing, initialValues, parent.BaseOpacity, parent.DefaultDuration,
            Duration.HasValue, parent.Preparation, initialProperties, parent.LayoutDirection);
        return PrepareDefinition(context);
    }

    private MotionPlan PrepareDefinition(in MotionContext context)
    {
        var fillMode = FillMode;
        var origin = Origin;
        if (context.Duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(Duration));
        }
        if (context.Delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(Delay));
        }
        if (!Enum.IsDefined(fillMode))
        {
            throw new ArgumentOutOfRangeException(nameof(FillMode));
        }
        if (origin is { } point && (!double.IsFinite(point.Point.X) || !double.IsFinite(point.Point.Y)))
        {
            throw new ArgumentOutOfRangeException(nameof(Origin));
        }
        context.Preparation.Enter(this);
        var builder = new MotionBuilder(context);
        try
        {
            Build(builder, context);
            var plan = builder.Freeze(fillMode, origin);
            _ = plan.Duration + plan.Delay;
            return plan;
        }
        finally
        {
            builder.Close();
            context.Preparation.Leave(this);
        }
    }
}
