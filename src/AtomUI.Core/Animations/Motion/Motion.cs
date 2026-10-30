using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Animations;

/// <summary>Provides attached motion configuration and target-level operations.</summary>
public sealed class Motion
{
    public static readonly AttachedProperty<AbstractMotion?> EnterProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, AbstractMotion?>("Enter");

    public static readonly AttachedProperty<AbstractMotion?> LeaveProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, AbstractMotion?>("Leave");

    public static readonly AttachedProperty<bool> IsVisibleProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, bool>("IsVisible", true);

    public static readonly AttachedProperty<TimeSpan?> DurationOverrideProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, TimeSpan?>("DurationOverride");

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, bool>("IsEnabled", true);

    public static readonly AttachedProperty<Direction> LayoutDirectionProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, Direction>("LayoutDirection", Direction.Bottom);

    public static AbstractMotion? GetEnter(Control target) => target.GetValue(EnterProperty);
    public static void SetEnter(Control target, AbstractMotion? value) => target.SetValue(EnterProperty, value);
    public static AbstractMotion? GetLeave(Control target) => target.GetValue(LeaveProperty);
    public static void SetLeave(Control target, AbstractMotion? value) => target.SetValue(LeaveProperty, value);
    public static bool GetIsVisible(Control target) => target.GetValue(IsVisibleProperty);
    public static void SetIsVisible(Control target, bool value)
    {
        target.SetValue(IsVisibleProperty, value);
        // Setting the initial true source can equal the attached default and
        // produce no change notification, but still opts into visibility ownership.
        VisibilityOwners.GetValue(target, static control => new MotionVisibility(control)).Initialize();
    }
    public static TimeSpan? GetDurationOverride(Control target) => target.GetValue(DurationOverrideProperty);
    public static void SetDurationOverride(Control target, TimeSpan? value) => target.SetValue(DurationOverrideProperty, value);
    public static bool GetIsEnabled(Control target) => target.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Control target, bool value) => target.SetValue(IsEnabledProperty, value);
    public static Direction GetLayoutDirection(Control target) => target.GetValue(LayoutDirectionProperty);
    public static void SetLayoutDirection(Control target, Direction value) => target.SetValue(LayoutDirectionProperty, value);

    public static Task<MotionResult> GetVisibilityTask(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return VisibilityOwners.TryGetValue(target, out var owner) ? owner.Completion : StableVisibility;
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

    internal static void SnapVisibility(Control target)
    {
        VisibilityOwners.GetValue(target, static control => new MotionVisibility(control)).SnapCurrent();
    }

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
            if (!change.GetNewValue<bool>() && VisibilityOwners.TryGetValue(target, out var owner))
            {
                owner.SnapCurrent();
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
