using System.Reflection;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
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

    // Ant Design renders the clear icon as a named button in the Tab order; Enter / Space clear and focus
    // returns to the select.
    [Fact]
    public void NonEditable_Clear_Button_Is_A_Named_Tab_Stop_And_Enter_Clears()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 240,
            ItemsSource     = new[] { "Alpha", "Beta" },
            SelectedIndex   = 0,
            IsAllowClear    = true,
            IsMotionEnabled = false
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.Focus(NavigationMethod.Tab).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
            var clearButton = FindClearButton(comboBox);
            clearButton.IsEffectivelyVisible.ShouldBeTrue();

            PressKey(window, Key.Tab, PhysicalKey.Tab);
            Dispatcher.UIThread.RunJobs();
            FocusedElement(comboBox).ShouldBeSameAs(clearButton);

            var localizer = global::AtomUI.ApplicationExtensions.GetLocalizer(Application.Current!);
            Avalonia.Automation.AutomationProperties.GetName(clearButton)
                    .ShouldBe(localizer!.Get(global::AtomUI.Controls.Localization.CommonLangResourceKind.Clear));

            PressKey(window, Key.Enter, PhysicalKey.Enter);
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(-1);
            comboBox.IsDropDownOpen.ShouldBeFalse();
            FocusedElement(comboBox).ShouldBeSameAs(comboBox);
            comboBox.Classes.ShouldContain(":focus-visible");
        });
    }

    [Fact]
    public void Editable_Clear_Button_Keeps_Keyboard_Focus_And_Space_Clears()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 240,
            ItemsSource     = new[] { "Alpha", "Beta" },
            IsEditable      = true,
            SelectedIndex   = 0,
            IsAllowClear    = true,
            IsMotionEnabled = false
        };

        ShowInWindow(comboBox, window =>
        {
            // Focus on an editable ComboBox is forwarded to its text box.
            comboBox.Focus(NavigationMethod.Tab);
            Dispatcher.UIThread.RunJobs();
            var editableTextBox = FocusedElement(comboBox).ShouldBeAssignableTo<Avalonia.Controls.TextBox>()!;
            var clearButton = FindClearButton(comboBox);

            // Focus reaching the clear button must not be forwarded back to the editable text box.
            PressKey(window, Key.Tab, PhysicalKey.Tab);
            Dispatcher.UIThread.RunJobs();
            FocusedElement(comboBox).ShouldBeSameAs(clearButton);

            // A button activates on Space release.
            PressKey(window, Key.Space, PhysicalKey.Space);
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeFalse();
            comboBox.SelectedIndex.ShouldBe(-1);
            string.IsNullOrEmpty(comboBox.Text).ShouldBeTrue();
            FocusedElement(comboBox).ShouldBeSameAs(editableTextBox);
        });
    }

    // Forward: previous stop > input > clear > next stop. The clear button exists only while focus is in the
    // control (or the pointer is over it), so Shift+Tab from outside lands on the input, and from the clear
    // button returns to the input.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Tab_Order_Is_Input_Then_Clear_Then_Out(bool isEditable)
    {
        var before = new Avalonia.Controls.Button { Content = "Before" };
        var after  = new Avalonia.Controls.Button { Content = "After" };
        var comboBox = new AtomUIComboBox
        {
            Width           = 240,
            ItemsSource     = new[] { "Alpha", "Beta" },
            IsEditable      = isEditable,
            SelectedIndex   = 0,
            IsAllowClear    = true,
            IsMotionEnabled = false
        };

        ShowInWindow(new StackPanel { Children = { before, comboBox, after } }, window =>
        {
            before.Focus(NavigationMethod.Tab).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
            var clearButton = FindClearButton(comboBox);
            IInputElement input = isEditable
                ? comboBox.GetVisualDescendants().OfType<Avalonia.Controls.TextBox>().Single(t => t.Name == "PART_EditableTextBox")
                : comboBox;

            Press(window, Key.Tab).ShouldBeSameAs(input);
            Press(window, Key.Tab).ShouldBeSameAs(clearButton);
            Press(window, Key.Tab).ShouldBeSameAs(after);

            Press(window, Key.Tab, RawInputModifiers.Shift).ShouldBeSameAs(input);
            Press(window, Key.Tab, RawInputModifiers.Shift).ShouldBeSameAs(before);

            Press(window, Key.Tab).ShouldBeSameAs(input);
            Press(window, Key.Tab).ShouldBeSameAs(clearButton);
            Press(window, Key.Tab, RawInputModifiers.Shift).ShouldBeSameAs(input);
        });

        IInputElement? Press(AvaloniaWindow window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
        {
            window.KeyPress(key, modifiers, PhysicalKey.Tab, null);
            Dispatcher.UIThread.RunJobs();
            return FocusedElement(comboBox);
        }
    }

    [Fact]
    public void Closing_Without_A_Pending_Query_Does_Not_Run_The_Filter_Again()
    {
        var filter = new CountingFilter();
        var comboBox = new AtomUIComboBox
        {
            Width           = 240,
            ItemsSource     = new[] { "Alpha", "Avocado", "Beta" },
            IsEditable      = true,
            IsFilterEnabled = true,
            Filter          = filter,
            FilterValue     = "A",
            IsMotionEnabled = false
        };

        ShowInWindow(comboBox, _ =>
        {
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            filter.Calls = 0;

            comboBox.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            filter.Calls.ShouldBe(0);
        });
    }

    private sealed class CountingFilter : global::AtomUI.Controls.Utils.IValueFilter
    {
        public int Calls { get; set; }

        public bool Filter(object? value, object? filterValue)
        {
            Calls++;
            return value?.ToString()?.StartsWith(filterValue?.ToString() ?? string.Empty) == true;
        }
    }

    private static IconButton FindClearButton(AtomUIComboBox comboBox)
    {
        return comboBox.GetVisualDescendants().OfType<IconButton>().Single(button => button.Name == "PART_ClearButton");
    }

    private static IInputElement? FocusedElement(Visual visual)
    {
        return TopLevel.GetTopLevel(visual)?.FocusManager?.GetFocusedElement();
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
