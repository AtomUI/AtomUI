using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUIColorPicker = AtomUI.Desktop.Controls.ColorPicker;
using AtomUIFlyout = AtomUI.Desktop.Controls.Flyout;
using AtomUINavMenu = AtomUI.Desktop.Controls.NavMenu;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests;

// Follow-ups to issue #517: controls must not create long-lived subscriptions (IInputManager.Process,
// shared Flyout events) while detached, or the subscription source roots the control and its page.
public class DetachedGlobalSubscriptionTests
{
    static DetachedGlobalSubscriptionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void NavMenu_Mode_Change_After_Detach_Does_Not_Mount_Interaction_Handler()
    {
        var menu = new AtomUINavMenu
        {
            Mode            = NavMenuMode.Horizontal,
            IsMotionEnabled = false
        };
        var host   = new ContentControl { Content = menu };
        var window = CreateWindow(host);
        try
        {
            menu.InteractionHandler.ShouldBeOfType<DefaultNavMenuInteractionHandler>();

            host.Content = null;
            Dispatcher.UIThread.RunJobs();
            menu.InteractionHandler.ShouldBeNull();

            menu.Mode = NavMenuMode.Vertical;
            Dispatcher.UIThread.RunJobs();
            menu.InteractionHandler.ShouldBeNull();

            host.Content = menu;
            Dispatcher.UIThread.RunJobs();
            menu.InteractionHandler.ShouldBeOfType<DefaultNavMenuInteractionHandler>();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void NavMenu_Never_Attached_Does_Not_Mount_Interaction_Handler()
    {
        var menu = new AtomUINavMenu
        {
            Mode = NavMenuMode.Horizontal
        };
        menu.Mode = NavMenuMode.Vertical;

        menu.InteractionHandler.ShouldBeNull();
    }

    [Fact]
    public void Shared_Flyout_Does_Not_Reference_Detached_SplitButton_Helper()
    {
        var sharedFlyout = new AtomUIFlyout
        {
            Content         = new TextBlock { Text = "Actions" },
            IsMotionEnabled = false
        };
        var splitButton = new SplitButton
        {
            Content         = "More",
            Flyout          = sharedFlyout,
            IsMotionEnabled = false
        };
        var host   = new ContentControl { Content = splitButton };
        var window = CreateWindow(host);
        try
        {
            var helper = GetPrivateField<FlyoutStateHelper>(splitButton, "_flyoutStateHelper");
            GetPrivateField<AtomUIFlyout?>(helper, "_registeredFlyout").ShouldBeSameAs(sharedFlyout);

            host.Content = null;
            Dispatcher.UIThread.RunJobs();
            GetPrivateField<AtomUIFlyout?>(helper, "_registeredFlyout").ShouldBeNull();

            host.Content = splitButton;
            Dispatcher.UIThread.RunJobs();
            GetPrivateField<AtomUIFlyout?>(helper, "_registeredFlyout").ShouldBeSameAs(sharedFlyout);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void ColorPicker_Hover_Trigger_While_Detached_And_Open_Does_Not_Subscribe_Global_Input()
    {
        var colorPicker = new AtomUIColorPicker
        {
            Width           = 240,
            IsMotionEnabled = false
        };
        var host   = new ContentControl { Content = colorPicker };
        var window = CreateWindow(host);
        try
        {
            host.Content = null;
            Dispatcher.UIThread.RunJobs();

            colorPicker.IsPickerOpen = true;
            colorPicker.TriggerType  = FlyoutTriggerType.Hover;
            Dispatcher.UIThread.RunJobs();

            GetPrivateField<IDisposable?>(colorPicker, "_popupPointerSubscription").ShouldBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        for (var type = instance.GetType(); type is not null; type = type.BaseType)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null)
            {
                return (T)field.GetValue(instance)!;
            }
        }

        throw new InvalidOperationException($"Field {fieldName} not found on {instance.GetType()}.");
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 320,
            Height  = 240,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
