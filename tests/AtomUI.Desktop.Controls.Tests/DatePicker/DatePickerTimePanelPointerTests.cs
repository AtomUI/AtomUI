using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

// Pointer interaction with the time panel of the date and time pickers, as in Ant Design: a time click shows the new
// candidate at once, hovering only previews it in a dimmed color, and leaving the cells shows the candidate again.
// One test per host, because each host wires the candidate text and the preview state on its own.
public class DatePickerTimePanelPointerTests
{
    static DatePickerTimePanelPointerTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void DatePicker_Confirms_The_Clicked_Time_And_Dims_Only_The_Preview()
    {
        // Gitee IKUUCL: 06:07:08 replaced 07:04:08 in the panel, yet the confirm button committed 07:04:08.
        var picker = new DatePicker
        {
            SelectedDateTime = new DateTime(2026, 11, 11, 7, 4, 8), IsShowTime = true, IsNeedConfirm = true,
            IsMotionEnabled = false
        };

        Show(picker, () =>
        {
            var presenter = Presenter(picker);
            var candidateForeground = TextForeground(picker);
            Tap(TimeCell(presenter, DateTimePickerPanelType.Hour, 6));
            Tap(TimeCell(presenter, DateTimePickerPanelType.Minute, 7));
            Tap(TimeCell(presenter, DateTimePickerPanelType.Second, 8));
            Input(picker).Text.ShouldBe("2026-11-11 06:07:08");
            IsPreviewing(picker).ShouldBeFalse();

            Hover(TimeCell(presenter, DateTimePickerPanelType.Hour, 7));
            Input(picker).Text.ShouldBe("2026-11-11 07:07:08");
            IsPreviewing(picker).ShouldBeTrue();
            TextForeground(picker).ShouldNotBe(candidateForeground);

            // Leaving the cells ends the preview instead of showing the bare date at 00:00:00.
            Hover(TimeHeader(presenter));
            Input(picker).Text.ShouldBe("2026-11-11 06:07:08");
            IsPreviewing(picker).ShouldBeFalse();
            TextForeground(picker).ShouldBe(candidateForeground);

            // The confirm button previews the candidate itself, which is not a dimmed hover preview.
            Hover(ConfirmButton(presenter));
            IsPreviewing(picker).ShouldBeFalse();
            Click(ConfirmButton(presenter));
            picker.IsPickerOpen.ShouldBeFalse();
            picker.SelectedDateTime.ShouldBe(new DateTime(2026, 11, 11, 6, 7, 8));
        });
    }

    [Fact]
    public void RangeDatePicker_Shows_The_Clicked_Time_And_Dims_Only_The_Preview()
    {
        var picker = new RangeDatePicker
        {
            IsShowTime = true, IsMotionEnabled = false,
            RangeStartSelectedDate = new DateTime(2026, 11, 11), RangeEndSelectedDate = new DateTime(2026, 11, 15)
        };

        Show(picker, () =>
        {
            var presenter = Presenter(picker);
            var candidateForeground = TextForeground(picker);
            Tap(TimeCell(presenter, DateTimePickerPanelType.Hour, 2));
            Input(picker).Text.ShouldBe("2026-11-11 02:00:00");

            Hover(TimeCell(presenter, DateTimePickerPanelType.Hour, 3));
            Input(picker).Text.ShouldBe("2026-11-11 03:00:00");
            IsPreviewing(picker).ShouldBeTrue();
            TextForeground(picker).ShouldNotBe(candidateForeground);

            Hover(TimeHeader(presenter));
            Input(picker).Text.ShouldBe("2026-11-11 02:00:00");
            IsPreviewing(picker).ShouldBeFalse();
            TextForeground(picker).ShouldBe(candidateForeground);
        });
    }

    [Fact]
    public void TimePicker_Dims_Only_The_Hover_Preview()
    {
        var picker = new TimePicker { SelectedTime = TimeSpan.Zero, IsNeedConfirm = true, IsMotionEnabled = false };

        Show(picker, () =>
        {
            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<TimePickerPresenter>()!;
            var candidateForeground = TextForeground(picker);

            Hover(TimeCell(presenter, DateTimePickerPanelType.Hour, 3));
            Input(picker).Text.ShouldBe("03:00:00");
            IsPreviewing(picker).ShouldBeTrue();
            TextForeground(picker).ShouldNotBe(candidateForeground);

            // The time picker shows no clock header; the middle of the button bar is outside every cell.
            Hover(presenter.GetVisualDescendants().OfType<Control>().First(c => c.Name == "PART_ButtonsFrame"));
            Input(picker).Text.ShouldBe("00:00:00");
            IsPreviewing(picker).ShouldBeFalse();
            TextForeground(picker).ShouldBe(candidateForeground);
        });
    }

    private static void Show(InfoPickerInput picker, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 960, Height = 800,
            Content = new StackPanel { Children = { picker } }
        };
        try
        {
            window.Show();
            Drain();
            picker.IsPickerOpen = true;
            Drain();
            assertion();
        }
        finally
        {
            window.Close();
            Drain();
        }
    }

    private static DatePickerPresenter Presenter(InfoPickerInput picker) =>
        picker.PickerPresenter.ShouldBeAssignableTo<DatePickerPresenter>()!;

    private static TextBox Input(Control picker) =>
        picker.GetVisualDescendants().OfType<TextBox>().First(control => control.Name == "PART_InfoInputBox");

    // The brush the text is drawn with, which is what the user sees.
    private static IBrush? TextForeground(Control picker) =>
        Input(picker).GetVisualDescendants().OfType<TextPresenter>().Single(p => p.Name == "PART_TextPresenter").Foreground;

    private static bool IsPreviewing(Control picker) => picker.Classes.Contains(InfoPickerPseudoClass.Previewing);

    private static Control ConfirmButton(DatePickerPresenter presenter) =>
        presenter.GetVisualDescendants().OfType<Control>().First(control => control.Name == "PART_ConfirmButton");

    // The clock header above the columns: inside the time view but outside every cell.
    private static Control TimeHeader(Control presenter) =>
        presenter.GetVisualDescendants().OfType<TimeView>().Single()
                 .GetVisualDescendants().OfType<TextBlock>().First(text => text.IsEffectivelyVisible && text.Bounds.Width > 0);

    // The looping column keeps off-screen cells realized; only a cell inside the viewport can be hit.
    private static TimeViewCell TimeCell(Control presenter, DateTimePickerPanelType type, int value)
    {
        var panel    = presenter.GetVisualDescendants().OfType<DateTimePickerPanel>().First(p => p.PanelType == type);
        var viewport = (Control)panel.GetVisualParent()!;
        return panel.GetVisualDescendants().OfType<TimeViewCell>().First(cell =>
            cell.Tag is int tag && tag == value &&
            cell.TranslatePoint(new Point(0, cell.Bounds.Height / 2), viewport) is { } center &&
            center.Y >= 0 && center.Y <= viewport.Bounds.Height);
    }

    // A tap routed straight to the cell, with no pointer movement and so no hover.
    private static void Tap(Control control)
    {
        var root    = TopLevel.GetTopLevel(control)!;
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
        var release = new PointerEventArgs(InputElement.PointerReleasedEvent, control, pointer, root, default, 0,
            PointerPointProperties.None, KeyModifiers.None);
        control.RaiseEvent(new TappedEventArgs(InputElement.TappedEvent, release));
        Drain();
    }

    private static void Hover(Control control)
    {
        var (root, point) = Locate(control);
        root.MouseMove(point);
        Drain();
    }

    private static void Click(Control control)
    {
        var (root, point) = Locate(control);
        root.MouseMove(point);
        root.MouseDown(point, MouseButton.Left);
        root.MouseUp(point, MouseButton.Left);
        Drain();
    }

    // The popup is its own top level, so pointer input goes to the top level that hosts the control.
    private static (TopLevel Root, Point Point) Locate(Control control)
    {
        var root  = TopLevel.GetTopLevel(control)!;
        var point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), root)!.Value;
        return (root, point);
    }

    private static void Drain() => Dispatcher.UIThread.RunJobs();
}
