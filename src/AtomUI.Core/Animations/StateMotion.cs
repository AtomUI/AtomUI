using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.VisualTree;

namespace AtomUI.Animations;

/// <summary>Template-build registration for state-animation targets owned by the templated control.</summary>
internal sealed class StateMotion
{
    #region 公共属性定义

    public static readonly AttachedProperty<StateMotionInitialization> InitializationProperty =
        AvaloniaProperty.RegisterAttached<StateMotion, Control, StateMotionInitialization>("Initialization");

    public static StateMotionInitialization GetInitialization(Control control)
    {
        return control.GetValue(InitializationProperty);
    }

    public static void SetInitialization(Control control, StateMotionInitialization value)
    {
        control.SetValue(InitializationProperty, value);
    }

    #endregion

    private static readonly AttachedProperty<StateMotionSession?> RegistrationProperty =
        AvaloniaProperty.RegisterAttached<StateMotion, Control, StateMotionSession?>("Registration");

    static StateMotion()
    {
        InitializationProperty.Changed.AddClassHandler<Control>(static (target, change) =>
        {
            if (target.GetValue(RegistrationProperty) is { } previous)
            {
                target.AttachedToVisualTree -= HandleAttached;
                target.DetachedFromVisualTree -= HandleDetached;
                target.Loaded -= HandleLoaded;
                previous.Dispose();
            }
            if (change.NewValue is StateMotionInitialization.Owner)
            {
                // Constructed before attachment enables transitions. Shared static event
                // callbacks avoid a wrapper and three delegate allocations per target.
                target.SetValue(RegistrationProperty, new StateMotionSession(target));
                target.AttachedToVisualTree += HandleAttached;
                target.DetachedFromVisualTree += HandleDetached;
                target.Loaded += HandleLoaded;
                if (target.IsAttachedToVisualTree())
                {
                    AssociateOwner(target);
                }
            }
            else
            {
                target.ClearValue(RegistrationProperty);
            }
        });
    }

    private StateMotion()
    {
    }

    private static void HandleAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AssociateOwner((Control)sender!);
    }

    private static void AssociateOwner(Control target)
    {
        if (target.TemplatedParent is IStateMotionOwner owner)
        {
            owner.StateMotionSession.RegisterTarget(target.GetValue(RegistrationProperty)!);
        }
        else
        {
            Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(target,
                "State motion initialization target has no participating template owner.");
        }
    }

    private static void HandleLoaded(object? sender, RoutedEventArgs e)
    {
        var target = (Control)sender!;
        if (target.TemplatedParent is IStateMotionOwner owner && owner.StateMotionSession.IsReady)
        {
            target.GetValue(RegistrationProperty)?.CompleteInitialization();
        }
    }

    private static void HandleDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var target = (Control)sender!;
        if (target.GetValue(RegistrationProperty) is { } session)
        {
            session.Reset();
            session.UnregisterFromOwner();
        }
    }
}

internal enum StateMotionInitialization
{
    None,
    Owner
}
