using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Controls.Primitives;
using Shouldly;
using Xunit;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomFloatButton = AtomUI.Desktop.Controls.FloatButton;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests;

public class Issue489KeyboardActivationTests
{
    static Issue489KeyboardActivationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void SplitButton_Tab_Focused_Secondary_Enter_Toggles_Flyout()
    {
        var splitButton = new SplitButton
        {
            Content         = "More",
            IsMotionEnabled = false,
            Flyout          = new Flyout
            {
                Content         = new TextBlock { Text = "Actions" },
                IsMotionEnabled = false
            }
        };
        var primaryClicks = 0;
        splitButton.Click += (_, _) => primaryClicks++;

        var window = CreateWindowWithLeadingTextBox(splitButton, out var textBox);
        try
        {
            var secondary = splitButton.GetVisualDescendants()
                                       .OfType<AtomButton>()
                                       .Single(button => button.Name == "PART_SecondaryButton");

            FocusByTab(window, textBox, secondary);

            PressEnter(window);
            splitButton.Flyout!.IsOpen.ShouldBeTrue();
            primaryClicks.ShouldBe(0);

            FocusControl(secondary);
            PressEnter(window);
            splitButton.Flyout!.IsOpen.ShouldBeFalse();
            primaryClicks.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void FloatButtonGroup_Tab_Focused_Trigger_Enter_Toggles_Open()
    {
        var group = new FloatButtonGroup
        {
            Trigger         = FloatButtonGroupTrigger.Click,
            IsMotionEnabled = false,
            Children =
            {
                new AtomFloatButton(),
                new AtomFloatButton()
            }
        };

        var window = CreateWindowWithLeadingTextBox(group, out var textBox);
        try
        {
            var trigger = group.GetVisualDescendants()
                               .OfType<AtomFloatButton>()
                               .Single(button => button.Classes.Contains("semantic-trigger"));

            FocusByTab(window, textBox, trigger);

            PressEnter(window);
            group.IsOpen.ShouldBeTrue();

            FocusControl(trigger);
            PressEnter(window);
            group.IsOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void DropdownButton_Tab_Focused_Enter_Toggles_Flyout()
    {
        var dropdownButton = new DropdownButton
        {
            Content        = "Menu",
            DropdownFlyout = new MenuFlyout
            {
                IsMotionEnabled = false,
                Items =
                {
                    new MenuItem { Header = "One" }
                }
            }
        };

        var window = CreateWindowWithLeadingTextBox(dropdownButton, out var textBox);
        try
        {
            FocusByTab(window, textBox, dropdownButton);

            PressEnter(window);
            dropdownButton.DropdownFlyout!.IsOpen.ShouldBeTrue();

            FocusControl(dropdownButton);
            PressEnter(window);
            dropdownButton.DropdownFlyout!.IsOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void FlyoutHost_Tab_Focused_Anchor_Enter_Toggles_Flyout()
    {
        var anchor = new AtomButton { Content = "Anchor" };
        var host = new FlyoutHost
        {
            Content         = anchor,
            IsMotionEnabled = false,
            Flyout          = new Flyout
            {
                Content         = new TextBlock { Text = "Popup" },
                IsMotionEnabled = false
            }
        };

        var window = CreateWindowWithLeadingTextBox(host, out var textBox);
        try
        {
            FocusByTab(window, textBox, anchor);

            PressEnter(window);
            host.Flyout!.IsOpen.ShouldBeTrue();

            FocusControl(anchor);
            PressEnter(window);
            host.Flyout!.IsOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static AvaloniaWindow CreateWindowWithLeadingTextBox(Control target, out TextBox textBox)
    {
        textBox = new TextBox { Width = 120 };
        var panel = new StackPanel();
        panel.Children.Add(textBox);
        panel.Children.Add(target);
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 360,
            Height = 260
        };
        overlayPanel.Children.Add(panel);
        var visualLayerManager = new Avalonia.Controls.Primitives.VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = visualLayerManager
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        target.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void EnablePopupOverlayLayer(Avalonia.Controls.Primitives.VisualLayerManager visualLayerManager)
    {
        var property = typeof(Avalonia.Controls.Primitives.VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }

    private static void FocusByTab(AvaloniaWindow window, TextBox start, Control expected)
    {
        start.Focus().ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();

        for (var i = 0; i < 8 && !ReferenceEquals(GetFocusedElement(window), expected); i++)
        {
            window.KeyPress(Key.Tab, RawInputModifiers.None, PhysicalKey.Tab, null);
            Dispatcher.UIThread.RunJobs();
        }

        GetFocusedElement(window).ShouldBeSameAs(expected);
    }

    private static object? GetFocusedElement(AvaloniaWindow window)
    {
        return TopLevel.GetTopLevel(window)?.FocusManager?.GetFocusedElement();
    }

    private static void FocusControl(Control control)
    {
        control.Focus(NavigationMethod.Tab).ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();
    }

    private static void PressEnter(AvaloniaWindow window)
    {
        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        Dispatcher.UIThread.RunJobs();
    }
}
