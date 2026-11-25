using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.AdornerLayerTests;

public class FocusOutlineAdornerTests
{
    static FocusOutlineAdornerTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Keyboard_Focus_Shows_Unclipped_Ant_Design_Outline()
    {
        var button = new Desktop.Controls.Button { Content = "Button" };

        ShowInWindow(button, () =>
        {
            Focus(button, NavigationMethod.Tab);

            var adorner = GetFocusOutline(button);
            AdornerLayer.GetAdornedElement(adorner).ShouldBeSameAs(button);
            AdornerLayer.GetIsClipEnabled(adorner).ShouldBeFalse();
            // Ant Design genFocusOutline: lineWidthFocus = lineWidth * 3, outline-offset = 1.
            adorner.OutlineThickness.ShouldBe(3);
            adorner.OutlineOffset.ShouldBe(1);
            adorner.OutlineBrush.ShouldNotBeNull();
        });
    }

    [Fact]
    public void Pointer_Focus_Does_Not_Show_Outline()
    {
        var button = new Desktop.Controls.Button { Content = "Button" };

        ShowInWindow(button, () =>
        {
            Focus(button, NavigationMethod.Pointer);

            FindFocusOutlines(button).ShouldBeEmpty();
        });
    }

    [Fact]
    public void Circle_Button_Outline_Follows_Effective_Corner_Radius()
    {
        var button = new Desktop.Controls.Button { Content = "A", Shape = ButtonShape.Circle };

        ShowInWindow(button, () =>
        {
            Focus(button, NavigationMethod.Tab);

            // A radius of at least half the size renders the outline as a circle.
            var radius = ((IFocusOutlineShapeProvider)button).FocusOutlineCornerRadius;
            radius.TopLeft.ShouldBeGreaterThanOrEqualTo(button.Bounds.Height / 2);
        });
    }

    [Fact]
    public void CheckBox_Outline_Wraps_Indicator_Only()
    {
        var checkBox = new Desktop.Controls.CheckBox { Content = "Check" };

        ShowInWindow(checkBox, () =>
        {
            Focus(checkBox, NavigationMethod.Tab);

            var adorned = AdornerLayer.GetAdornedElement(GetFocusOutline(checkBox));
            adorned.ShouldNotBeSameAs(checkBox);
            ((Control)adorned!).Name.ShouldBe("Indicator");
        });
    }

    [Fact]
    public void Radio_Outline_Wraps_Circular_Indicator()
    {
        var radio = new Desktop.Controls.RadioButton { Content = "Radio" };

        ShowInWindow(radio, () =>
        {
            Focus(radio, NavigationMethod.Tab);

            var adorned = (Control)AdornerLayer.GetAdornedElement(GetFocusOutline(radio))!;
            adorned.Name.ShouldBe("Indicator");
            var radius = ((IFocusOutlineShapeProvider)adorned).FocusOutlineCornerRadius;
            radius.TopLeft.ShouldBe(Math.Min(adorned.Bounds.Width, adorned.Bounds.Height) / 2, 0.01);
        });
    }

    [Fact]
    public void Switch_Outline_Follows_Pill_Shape()
    {
        var toggleSwitch = new Desktop.Controls.ToggleSwitch();

        ShowInWindow(toggleSwitch, () =>
        {
            Focus(toggleSwitch, NavigationMethod.Tab);

            AdornerLayer.GetAdornedElement(GetFocusOutline(toggleSwitch)).ShouldBeSameAs(toggleSwitch);
            var radius = ((IFocusOutlineShapeProvider)toggleSwitch).FocusOutlineCornerRadius;
            radius.TopLeft.ShouldBe(Math.Min(toggleSwitch.Bounds.Width, toggleSwitch.Bounds.Height) / 2, 0.01);
        });
    }

    [Fact]
    public void Collapse_Item_Outline_Wraps_Header_Only()
    {
        var item     = new CollapseItem { Header = "Header", Content = "Body" };
        var collapse = new Collapse();
        collapse.Items.Add(item);

        ShowInWindow(collapse, () =>
        {
            Focus(item, NavigationMethod.Tab);

            var adorned = (Control)AdornerLayer.GetAdornedElement(GetFocusOutline(item))!;
            adorned.Name.ShouldBe("PART_HeaderDecorator");
        });
    }

    [Fact]
    public void Mentions_Host_Is_Not_A_Tab_Stop()
    {
        new Desktop.Controls.Mentions().IsTabStop.ShouldBeFalse();
    }

    public static TheoryData<string> InputFamilyHosts => new()
    {
        nameof(Desktop.Controls.DatePicker),
        nameof(Desktop.Controls.TimePicker),
        nameof(Desktop.Controls.RangeDatePicker),
        nameof(Desktop.Controls.RangeTimePicker),
        nameof(Desktop.Controls.TextArea),
        nameof(Desktop.Controls.ComboBox),
        nameof(Desktop.Controls.LineEdit)
    };

    // The input frame expresses focus (border + outline shadow); no part of the host may add an adorner frame.
    [Theory]
    [MemberData(nameof(InputFamilyHosts))]
    public void Input_Family_Keyboard_Focus_Adds_No_Focus_Frame(string hostName)
    {
        Control host = hostName switch
        {
            nameof(Desktop.Controls.DatePicker) => new Desktop.Controls.DatePicker(),
            nameof(Desktop.Controls.TimePicker) => new Desktop.Controls.TimePicker(),
            nameof(Desktop.Controls.RangeDatePicker) => new Desktop.Controls.RangeDatePicker(),
            nameof(Desktop.Controls.RangeTimePicker) => new Desktop.Controls.RangeTimePicker(),
            nameof(Desktop.Controls.TextArea) => new Desktop.Controls.TextArea(),
            nameof(Desktop.Controls.ComboBox) => new Desktop.Controls.ComboBox { IsEditable = true },
            _ => new Desktop.Controls.LineEdit()
        };
        host.Width = 260;

        ShowInWindow(host, () =>
        {
            var stops = new[] { host }.Concat(host.GetVisualDescendants().OfType<Control>())
                                     .Where(c => c.Focusable && c.IsTabStop && c.IsEffectivelyVisible)
                                     .ToList();
            stops.ShouldNotBeEmpty();
            var reached = 0;
            foreach (var stop in stops)
            {
                // Tab navigation only lands on elements that can actually take focus.
                if (!stop.Focus(NavigationMethod.Tab))
                {
                    continue;
                }
                Dispatcher.UIThread.RunJobs();
                reached++;
                FindFocusOutlines(host).ShouldBeEmpty($"{hostName}: {stop.GetType().Name}#{stop.Name}");
            }
            reached.ShouldBeGreaterThan(0);
        });
    }

    private static FocusOutlineAdorner GetFocusOutline(Visual target)
    {
        var outlines = FindFocusOutlines(target);
        outlines.Count.ShouldBe(1);
        return outlines[0];
    }

    private static List<FocusOutlineAdorner> FindFocusOutlines(Visual target)
    {
        var layer = AdornerLayer.GetAdornerLayer(target);
        layer.ShouldNotBeNull();
        return layer!.Children.OfType<FocusOutlineAdorner>().ToList();
    }

    private static void Focus(InputElement element, NavigationMethod method)
    {
        element.Focus(method).ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var host = new VisualLayerManager
        {
            Child = new StackPanel { Margin = new Thickness(20), Children = { content } }
        };
        var window = new AvaloniaWindow
        {
            Width   = 400,
            Height  = 300,
            Content = host
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
