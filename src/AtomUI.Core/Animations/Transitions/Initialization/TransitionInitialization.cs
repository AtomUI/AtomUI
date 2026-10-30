using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.VisualTree;

namespace AtomUI.Animations;

/// <summary>Template-build registration for state-animation targets owned by the templated control.</summary>
internal sealed class TransitionInitialization
{
    #region 公共属性定义

    public static readonly AttachedProperty<TransitionInitializationMode> ModeProperty =
        AvaloniaProperty.RegisterAttached<TransitionInitialization, Control, TransitionInitializationMode>("Mode");

    public static TransitionInitializationMode GetMode(Control control)
    {
        return control.GetValue(ModeProperty);
    }

    public static void SetMode(Control control, TransitionInitializationMode value)
    {
        control.SetValue(ModeProperty, value);
    }

    #endregion

    private static readonly AttachedProperty<TransitionInitializationSession?> RegistrationProperty =
        AvaloniaProperty.RegisterAttached<TransitionInitialization, Control, TransitionInitializationSession?>("Registration");

    static TransitionInitialization()
    {
        ModeProperty.Changed.AddClassHandler<Control>(static (target, _) =>
        {
            if (target.GetValue(RegistrationProperty) is { } previous)
            {
                target.AttachedToVisualTree -= HandleAttached;
                target.DetachedFromVisualTree -= HandleDetached;
                target.Loaded -= HandleLoaded;
                // Releasing suppression can publish Transitions synchronously and
                // install a newer registration. Retire this identity before disposal.
                target.ClearValue(RegistrationProperty);
                previous.Dispose();
            }
            if (GetMode(target) != TransitionInitializationMode.Owner ||
                target.GetValue(RegistrationProperty) is not null)
            {
                return;
            }

            var session = new TransitionInitializationSession(target);
            if (GetMode(target) != TransitionInitializationMode.Owner ||
                target.GetValue(RegistrationProperty) is not null)
            {
                session.Dispose();
                return;
            }
            target.SetValue(RegistrationProperty, session);
            if (!ReferenceEquals(target.GetValue(RegistrationProperty), session) ||
                GetMode(target) != TransitionInitializationMode.Owner)
            {
                session.Dispose();
                return;
            }

            target.AttachedToVisualTree += HandleAttached;
            target.DetachedFromVisualTree += HandleDetached;
            target.Loaded += HandleLoaded;
            if (target.IsAttachedToVisualTree())
            {
                AssociateOwner(target);
            }
        });
    }

    private TransitionInitialization()
    {
    }

    private static void HandleAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AssociateOwner((Control)sender!);
    }

    private static void AssociateOwner(Control target)
    {
        if (target.GetValue(RegistrationProperty) is not { } session)
        {
            return;
        }
        if (target.TemplatedParent is ITransitionInitializationOwner owner)
        {
            owner.TransitionInitialization.RegisterTarget(session);
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
        if (target.TemplatedParent is ITransitionInitializationOwner owner && owner.TransitionInitialization.IsReady)
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

internal enum TransitionInitializationMode
{
    None,
    Owner
}
