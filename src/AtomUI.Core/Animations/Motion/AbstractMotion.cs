using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Animations;

/// <summary>
/// Defines a finite motion and its per-request preparation.
/// </summary>
public abstract class AbstractMotion
{
    /// <summary>
    /// How long the motion runs. A <see cref="Motion.DurationOverrideProperty" /> value on the target takes precedence.
    /// </summary>
    /// <remarks>
    /// <see langword="null" /> uses the <c>MotionDurationMid</c> token resolved on the target, or 200 milliseconds when
    /// the token is absent. Zero commits the end state at once, held or released as
    /// <see cref="AbstractMotion.FillMode" /> specifies. A negative value throws an
    /// <see cref="ArgumentOutOfRangeException" /> when the motion is played with animation enabled on the target;
    /// while animation is disabled the motion lasts no time instead. As a child of a sequence or parallel motion,
    /// <see langword="null" /> selects the token default, not the duration of the parent.
    /// </remarks>
    public TimeSpan? Duration { get; set; }
    /// <summary>
    /// How long the motion waits before it starts. During the wait the target shows the start value when
    /// <see cref="AbstractMotion.FillMode" /> holds the start, and is left untouched otherwise.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="TimeSpan.Zero" />. A negative value throws an
    /// <see cref="ArgumentOutOfRangeException" /> when the motion is played.
    /// </remarks>
    public TimeSpan Delay { get; set; }
    /// <summary>
    /// The easing of every keyframe segment that does not name one of its own. <see langword="null" /> uses
    /// <see cref="DefaultEasing" />.
    /// </summary>
    /// <remarks>
    /// A sequence or parallel motion composes its children only with a <see cref="LinearEasing" />. Any other value
    /// here or in <see cref="DefaultEasing" /> throws an <see cref="InvalidOperationException" /> when the motion is
    /// played.
    /// </remarks>
    public Easing? Easing { get; set; }
    /// <summary>
    /// The pivot of scale and rotation, as a point in the bounds of the target. <see langword="null" /> keeps the
    /// origin that the motion assigns to <see cref="MotionBuilder.Origin" />, which is the center of the target unless
    /// the motion changes it.
    /// </summary>
    public RelativePoint? Origin { get; set; }
    /// <summary>
    /// Which ends of the motion are held on the target. The default is <c>Both</c>.
    /// </summary>
    /// <remarks>
    /// <c>Backward</c> holds the start value during <see cref="Delay" />. <c>Forward</c> holds the end value after the
    /// motion completes, until another motion starts on the target, the target leaves the visual tree or
    /// <see cref="Motion.Reset" /> releases it. <c>None</c> holds neither end, so the target returns to its own values.
    /// A child of a sequence or parallel motion must keep the default, because the parent decides what is held.
    /// </remarks>
    public FillMode FillMode { get; set; } = FillMode.Both;

    protected AbstractMotion()
    {
    }

    /// <summary>
    /// Creates a motion and sets its duration, easing and fill mode.
    /// </summary>
    /// <param name="duration">
    /// The initial <see cref="Duration" />.
    /// </param>
    /// <param name="easing">
    /// The initial <see cref="AbstractMotion.Easing" />. <see langword="null" /> leaves <see cref="DefaultEasing" />
    /// in charge.
    /// </param>
    /// <param name="fillMode">
    /// The initial <see cref="AbstractMotion.FillMode" />.
    /// </param>
    protected AbstractMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
    {
        Duration = duration;
        Easing = easing;
        FillMode = fillMode;
    }

    /// <summary>
    /// Plays this motion on <paramref name="target" />. The task completes when playback has ended and the target is in
    /// its final state.
    /// </summary>
    /// <param name="target">
    /// The control to animate. It must be attached to the visual tree and have a valid arrangement. Otherwise nothing
    /// is started and the task completes at once with <see cref="MotionOutcome.NotReady" />.
    /// </param>
    /// <param name="cancellationToken">
    /// Stops the motion early. The task then completes with <see cref="MotionOutcome.Cancelled" />, and the target
    /// returns to the values it has without the motion.
    /// </param>
    /// <returns>
    /// How the playback ended, the id of the playback on the target, and whether frames were played.
    /// <see cref="MotionResult.IsCompleted" /> is also <see langword="true" /> when the end state was applied at once.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="target" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Called from a thread other than the UI thread, or the motion declares invalid keyframes or compositions.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="Duration" />, <see cref="Delay" />, <see cref="AbstractMotion.FillMode" /> or
    /// <see cref="Origin" /> is invalid.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The motion declares a layout transition and <paramref name="target" /> does not implement
    /// <see cref="IMotionLayout" />.
    /// </exception>
    /// <remarks>
    /// Only one motion plays on a target at a time. A request that starts while another one plays replaces it, and the
    /// replaced motion completes with <see cref="MotionOutcome.Superseded" />. A target that leaves the visual tree
    /// during playback ends the motion with <see cref="MotionOutcome.Detached" />. When animation is disabled on the
    /// target by <see cref="Motion.IsEnabledProperty" /> or by the <c>EnableMotion</c> token, the end state is applied
    /// at once and the outcome is <see cref="MotionOutcome.Completed" />.
    /// </remarks>
    public Task<MotionResult> PlayAsync(Control target, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);
        Dispatcher.UIThread.VerifyAccess();
        return MotionPlayback.PlayAsync(this, target, null, cancellationToken);
    }

    /// <summary>
    /// The easing used when <see cref="AbstractMotion.Easing" /> is <see langword="null" />. It is read each time the
    /// motion is prepared, and is a <see cref="LinearEasing" /> unless an override changes it.
    /// </summary>
    protected virtual Easing DefaultEasing => MotionCurves.Linear;

    /// <summary>
    /// Declares the keyframes of the motion on <paramref name="builder" />. It is called once for every request, before
    /// anything is written to the target.
    /// </summary>
    /// <param name="builder">
    /// Receives the channels, the layout transition or the child motions of this definition. It rejects every write
    /// after this method returns, so a motion must not keep it.
    /// </param>
    /// <param name="context">
    /// The resolved settings and the size of the target for this request. It does not give access to the target.
    /// </param>
    /// <remarks>
    /// Keep this method synchronous and free of writes to the target. A definition is prepared again for each request
    /// and can serve several targets. Invalid declarations throw when the builder is frozen right after this method
    /// returns, before the target changes. A definition that composes itself, directly or through its children, is
    /// rejected as a cycle.
    /// </remarks>
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
        var requestedDuration = durationOverride ?? Motion.GetDurationOverride(target);
        var duration = requestedDuration ?? Duration ?? defaultDuration;
        var disabled = !Motion.IsAnimationEnabled(target);
        if (disabled)
        {
            duration = TimeSpan.Zero;
        }
        var scaling = TopLevel.GetTopLevel(target)?.RenderScaling ?? 1;
        var opacity = target.GetBaseValue(Visual.OpacityProperty);
        var baseOpacity = opacity.HasValue ? opacity.Value : Visual.OpacityProperty.GetDefaultValue(target);
        var context = new MotionContext(target.Bounds.Size, scaling, duration, Delay, Easing ?? DefaultEasing,
            initialValues, baseOpacity, defaultDuration, requestedDuration.HasValue || Duration.HasValue || disabled,
            new MotionPreparation(target), layoutDirection: Motion.GetLayoutDirection(target));
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
