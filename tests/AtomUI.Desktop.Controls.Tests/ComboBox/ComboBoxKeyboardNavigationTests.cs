using System.Reflection;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;

namespace AtomUI.Desktop.Controls.Tests.ComboBox;

public class ComboBoxKeyboardNavigationTests
{
    static ComboBoxKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void NonEditable_ComboBox_Allows_Consecutive_Keyboard_Selections()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 240,
            ItemsSource     = new[] { "Alpha", "Beta", "Gamma" },
            IsMotionEnabled = false
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.Focus(NavigationMethod.Tab).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            SelectNextItem(window, comboBox, 0);

            comboBox.Focus(NavigationMethod.Tab).ShouldBeTrue();
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            TopLevel.GetTopLevel(comboBox)?.FocusManager.GetFocusedElement()
                    .ShouldBeSameAs(comboBox);

            PressKey(window, Key.Down, PhysicalKey.ArrowDown);
            Dispatcher.UIThread.RunJobs();
            PressKey(window, Key.Enter, PhysicalKey.Enter);
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(1);
            comboBox.IsDropDownOpen.ShouldBeFalse();
        });
    }

    private static void SelectNextItem(AvaloniaWindow window, AtomUIComboBox comboBox, int expectedIndex)
    {
        comboBox.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        PressKey(window, Key.Down, PhysicalKey.ArrowDown);
        Dispatcher.UIThread.RunJobs();
        PressKey(window, Key.Enter, PhysicalKey.Enter);
        Dispatcher.UIThread.RunJobs();

        comboBox.SelectedIndex.ShouldBe(expectedIndex);
        comboBox.IsDropDownOpen.ShouldBeFalse();
    }

    private static void PressKey(AvaloniaWindow window, Key key, PhysicalKey physicalKey)
    {
        window.KeyPress(key, RawInputModifiers.None, physicalKey, null);
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 220,
            Content = CreatePopupOverlayHost(content)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            content.Measure(Size.Infinity);
            content.Arrange(new Rect(content.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static VisualLayerManager CreatePopupOverlayHost(Control content)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 360,
            Height = 220
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);
        return visualLayerManager;
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
