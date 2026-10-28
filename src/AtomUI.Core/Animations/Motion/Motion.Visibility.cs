using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Animations;

public abstract partial class Motion
{
    public static readonly AttachedProperty<Motion?> EnterProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, Motion?>("Enter");

    public static readonly AttachedProperty<Motion?> LeaveProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, Motion?>("Leave");

    public static readonly AttachedProperty<bool> IsVisibleProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, bool>("IsVisible", true);

    public static readonly AttachedProperty<TimeSpan?> DurationOverrideProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, TimeSpan?>("DurationOverride");

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, bool>("IsEnabled", true);

    public static readonly AttachedProperty<Direction> LayoutDirectionProperty =
        AvaloniaProperty.RegisterAttached<Motion, Control, Direction>("LayoutDirection", Direction.Bottom);

    public static Motion? GetEnter(Control target) => target.GetValue(EnterProperty);
    public static void SetEnter(Control target, Motion? value) => target.SetValue(EnterProperty, value);
    public static Motion? GetLeave(Control target) => target.GetValue(LeaveProperty);
    public static void SetLeave(Control target, Motion? value) => target.SetValue(LeaveProperty, value);
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

    internal static void SnapVisibility(Control target)
    {
        VisibilityOwners.GetValue(target, static control => new MotionVisibility(control)).SnapCurrent();
    }

    private static readonly ConditionalWeakTable<Control, MotionVisibility> VisibilityOwners = new();
    private static readonly Task<MotionResult> StableVisibility = Task.FromResult(new MotionResult(MotionOutcome.Completed, 0, false));

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
