using System.Runtime.CompilerServices;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Animations;

/// <summary>
/// Provides attached motion configuration and target-level operations.
/// </summary>
public sealed class Motion
{
    /// <summary>
    /// The motion played when the element becomes visible through <see cref="IsVisibleProperty" />.
    /// </summary>
    /// <remarks>
    /// Only an element that has a value for <see cref="IsVisibleProperty" /> uses it, and only the element itself. The
    /// value is read when that value changes to <see langword="true" />. <see langword="null" />, the default, shows
    /// the element without a motion.
    /// </remarks>
    public static readonly AttachedProperty<AbstractMotion?> EnterProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, AbstractMotion?>("Enter");

    /// <summary>
    /// The motion played when the element is hidden through <see cref="IsVisibleProperty" />.
    /// </summary>
    /// <remarks>
    /// Only an element that has a value for <see cref="IsVisibleProperty" /> uses it, and only the element itself. The
    /// value is read when that value changes to <see langword="false" />. <see langword="null" />, the default, hides
    /// the element at once.
    /// </remarks>
    public static readonly AttachedProperty<AbstractMotion?> LeaveProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, AbstractMotion?>("Leave");

    /// <summary>
    /// The visibility that the attached motion owner applies to the element.
    /// </summary>
    /// <remarks>
    /// Setting it hands the <c>IsVisible</c> of the element over to the owner. <see langword="true" /> shows the
    /// element and plays <see cref="EnterProperty" /> on it. <see langword="false" /> plays
    /// <see cref="LeaveProperty" /> and hides the element when that motion has ended. Without a motion, or with
    /// animation disabled, the element switches at once. <see cref="GetVisibilityTask" /> reports when the switch has
    /// finished. The value applies to the element only. An element without a value for it is not managed by the owner.
    /// </remarks>
    public static readonly AttachedProperty<bool> IsVisibleProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, bool>("IsVisible", true);

    /// <summary>
    /// A duration that replaces the <see cref="AbstractMotion.Duration" /> of every motion played on the element.
    /// </summary>
    /// <remarks>
    /// <see langword="null" />, the default, lets each motion use its own duration. The value applies to the element
    /// only, and to the whole timeline of a sequence or parallel motion. A negative value throws an
    /// <see cref="ArgumentOutOfRangeException" /> when a motion is played with animation enabled on the element.
    /// </remarks>
    public static readonly AttachedProperty<TimeSpan?> DurationOverrideProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, TimeSpan?>("DurationOverride");

    /// <summary>
    /// Whether motions may animate the element.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="true" />. When it is <see langword="false" />, a motion played on the element
    /// applies its end state at once, also one that is playing when the value changes. The <c>EnableMotion</c> token
    /// found in the resources of the element or of an ancestor disables animation in the same way. The value applies to
    /// the element only.
    /// </remarks>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, bool>("IsEnabled", true);

    /// <summary>
    /// The side of the element where a layout motion adds or removes space when the motion declares no direction.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="Direction.Bottom" />. A motion reads it from
    /// <see cref="MotionContext.LayoutDirection" />. Changing it while a motion plays through
    /// <see cref="IsVisibleProperty" /> ends that motion and snaps the element to the visibility it requests. The value
    /// applies to the element only.
    /// </remarks>
    public static readonly AttachedProperty<Direction> LayoutDirectionProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, Direction>("LayoutDirection", Direction.Bottom);

    /// <summary>
    /// Gets the enter motion of <paramref name="target" />. <see langword="null" /> shows the element without a motion.
    /// </summary>
    public static AbstractMotion? GetEnter(Control target) => target.GetValue(EnterProperty);
    /// <summary>
    /// Sets the enter motion of <paramref name="target" />. The owner uses it when the element next becomes visible.
    /// </summary>
    public static void SetEnter(Control target, AbstractMotion? value) => target.SetValue(EnterProperty, value);
    /// <summary>
    /// Gets the leave motion of <paramref name="target" />. <see langword="null" /> hides the element at once.
    /// </summary>
    public static AbstractMotion? GetLeave(Control target) => target.GetValue(LeaveProperty);
    /// <summary>
    /// Sets the leave motion of <paramref name="target" />. The owner uses it when the element is next hidden.
    /// </summary>
    public static void SetLeave(Control target, AbstractMotion? value) => target.SetValue(LeaveProperty, value);
    /// <summary>
    /// Gets the visibility requested for <paramref name="target" />, which is <see langword="true" /> when the property
    /// is not set.
    /// </summary>
    public static bool GetIsVisible(Control target) => target.GetValue(IsVisibleProperty);
    /// <summary>
    /// Requests a visibility for <paramref name="target" /> and, the first time, registers the element with the owner.
    /// </summary>
    public static void SetIsVisible(Control target, bool value)
    {
        target.SetValue(IsVisibleProperty, value);
        // Setting the initial true source can equal the attached default and
        // produce no change notification, but still opts into visibility ownership.
        VisibilityOwners.GetValue(target, static control => new MotionVisibility(control)).Initialize();
    }
    /// <summary>
    /// Gets the duration override of <paramref name="target" />, or <see langword="null" /> when it has none.
    /// </summary>
    public static TimeSpan? GetDurationOverride(Control target) => target.GetValue(DurationOverrideProperty);
    /// <summary>
    /// Sets the duration override of <paramref name="target" />. <see langword="null" /> removes it.
    /// </summary>
    public static void SetDurationOverride(Control target, TimeSpan? value) => target.SetValue(DurationOverrideProperty, value);
    /// <summary>
    /// Gets whether motions may animate <paramref name="target" />.
    /// </summary>
    public static bool GetIsEnabled(Control target) => target.GetValue(IsEnabledProperty);
    /// <summary>
    /// Sets whether motions may animate <paramref name="target" />. Changing it to <see langword="false" /> ends a
    /// motion that is playing on the element at its end state.
    /// </summary>
    public static void SetIsEnabled(Control target, bool value) => target.SetValue(IsEnabledProperty, value);
    /// <summary>
    /// Gets the layout direction of <paramref name="target" />.
    /// </summary>
    public static Direction GetLayoutDirection(Control target) => target.GetValue(LayoutDirectionProperty);
    /// <summary>
    /// Sets the layout direction of <paramref name="target" />. A change ends a motion that plays through
    /// <see cref="IsVisibleProperty" /> and snaps the element to the visibility it requests.
    /// </summary>
    public static void SetLayoutDirection(Control target, Direction value) => target.SetValue(LayoutDirectionProperty, value);

    /// <summary>
    /// Returns a task that completes when the latest visibility change requested for <paramref name="target" /> through
    /// <see cref="IsVisibleProperty" /> has finished.
    /// </summary>
    /// <param name="target">
    /// The element to wait for.
    /// </param>
    /// <returns>
    /// The result of the latest visibility request. It carries <see cref="MotionOutcome.Superseded" /> when a newer
    /// request replaces the one awaited, and <see cref="MotionOutcome.Detached" /> when the element leaves the visual
    /// tree. For an element that <see cref="IsVisibleProperty" /> does not manage, the task is already complete with
    /// <see cref="MotionOutcome.Completed" />. It faults when a motion fails.
    /// </returns>
    public static Task<MotionResult> GetVisibilityTask(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return VisibilityOwners.TryGetValue(target, out var owner) ? owner.Completion : StableVisibility;
    }

    /// <summary>
    /// Releases the end values that a completed motion holds on <paramref name="target" />, so that the element shows
    /// the values set on it directly. It does nothing when the target has no motion state.
    /// </summary>
    /// <param name="target">
    /// The element to release.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Called from a thread other than the UI thread, or a motion is still playing on <paramref name="target" />.
    /// Cancel it and wait for its task first.
    /// </exception>
    public static void Reset(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        Dispatcher.UIThread.VerifyAccess();
        MotionPlayback.Reset(target);
    }

    /// <summary>
    /// Tells whether the completed motion behind <paramref name="result" /> is still the latest playback on
    /// <paramref name="target" />.
    /// </summary>
    /// <param name="target">
    /// The element the motion played on.
    /// </param>
    /// <param name="result">
    /// The result returned for that element.
    /// </param>
    /// <returns>
    /// <see langword="true" /> only when the motion completed, the element is attached to the visual tree and no later
    /// request has started on it. A result that did not start a playback is never current.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Called from a thread other than the UI thread.
    /// </exception>
    public static bool IsCurrent(Control target, MotionResult result)
    {
        ArgumentNullException.ThrowIfNull(target);
        Dispatcher.UIThread.VerifyAccess();
        return MotionPlayback.IsCurrent(target, result);
    }

    internal static void SnapVisibility(Control target)
    {
        VisibilityOwners.GetValue(target, static control => new MotionVisibility(control)).SnapCurrent();
    }

    internal static bool IsAnimationEnabled(Control target) =>
        GetIsEnabled(target) &&
        (!target.TryFindResource(EnableMotionResourceKey, target.ActualThemeVariant, out var enabled) || enabled is not false);

    // Resource dictionaries use object keys. Reuse the boxed enum for repeated
    // preparation/policy reads instead of boxing once per guard.
    private static readonly object EnableMotionResourceKey = SharedTokenKind.EnableMotion;
    private static readonly ConditionalWeakTable<Control, MotionVisibility> VisibilityOwners = new();
    private static readonly Task<MotionResult> StableVisibility = Task.FromResult(new MotionResult(MotionOutcome.Completed, 0, false));

    private Motion()
    {
    }

    static Motion()
    {
        IsVisibleProperty.Changed.AddClassHandler<Control>(static (target, _) =>
            VisibilityOwners.GetValue(target, static control => new MotionVisibility(control)).Request());
        EnterProperty.Changed.AddClassHandler<Control>(static (target, _) => ConfigureVisibility(target));
        LeaveProperty.Changed.AddClassHandler<Control>(static (target, _) => ConfigureVisibility(target));
        IsEnabledProperty.Changed.AddClassHandler<Control>(static (target, change) =>
        {
            if (!change.GetNewValue<bool>())
            {
                if (VisibilityOwners.TryGetValue(target, out var owner))
                {
                    owner.SnapCurrent();
                }
                MotionPlayback.CompleteCurrent(target);
            }
        });
        LayoutDirectionProperty.Changed.AddClassHandler<Control>(static (target, _) =>
        {
            if (VisibilityOwners.TryGetValue(target, out var owner))
            {
                owner.SnapCurrent();
            }
        });
    }

    private static void ConfigureVisibility(Control target)
    {
        // Enter/Leave may also be read by a control's own lifecycle. Only a declared
        // IsVisible source opts into this attached visibility owner.
        if (target.IsSet(IsVisibleProperty))
        {
            VisibilityOwners.GetValue(target, static control => new MotionVisibility(control)).Initialize();
        }
    }
}
