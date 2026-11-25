using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

// Keyboard behavior of the date and time pickers: Ant Design stops and keys, plus the AtomUI forwarding of
// navigation keys from the input to the popup panel.
public class PickerKeyboardTests
{
    static PickerKeyboardTests() => AvaloniaTestApp.EnsureInitialized();

    private static readonly DateTime July15 = new(2026, 7, 15);

    [Fact]
    public void DatePicker_Stops_Are_The_Input_Then_The_Clear_Button_And_Focus_Does_Not_Open()
    {
        var picker = new DatePicker { SelectedDateTime = July15, IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Focused(window).ShouldBeSameAs(Input(picker));
            picker.IsPickerOpen.ShouldBeFalse();

            Tab(window);
            Focused(window).ShouldBeOfType<InputClearIconButton>();

            Tab(window, shift: true);
            Focused(window).ShouldBeSameAs(Input(picker));
        });
    }

    [Fact]
    public void RangeDatePicker_Stops_Are_Start_End_Then_Clear_And_Focus_Activates_The_Part()
    {
        var picker = new RangeDatePicker
        {
            RangeStartSelectedDate = July15, RangeEndSelectedDate = July15.AddDays(3), IsMotionEnabled = false
        };

        Show(picker, window =>
        {
            Tab(window);
            Focused(window).ShouldBeSameAs(Input(picker));
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.Start);

            Tab(window);
            Focused(window).ShouldBeSameAs(SecondaryInput(picker));
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.End);

            Tab(window);
            Focused(window).ShouldBeOfType<InputClearIconButton>();
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Enter_Opens_A_Closed_Picker_And_Escape_Closes_It()
    {
        var picker = new DatePicker { SelectedDateTime = July15, IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Press(window, Key.Enter);
            picker.IsPickerOpen.ShouldBeTrue();

            Press(window, Key.Escape);
            picker.IsPickerOpen.ShouldBeFalse();
            picker.SelectedDateTime.ShouldBe(July15);
        });
    }

    [Fact]
    public void Typing_Opens_And_Previews_Then_Enter_Commits()
    {
        var picker = new DatePicker { SelectedDateTime = July15, IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-08-20");

            picker.IsPickerOpen.ShouldBeTrue();
            picker.SelectedDateTime.ShouldBe(July15, "typed text is a candidate until it is submitted");
            Presenter(picker).SelectedDateTime.ShouldBe(new DateTime(2026, 8, 20));
            Viewer(picker).PanelSession.Input.DisplayDate.Month.ShouldBe(8, "the panel reveals the typed date");

            Press(window, Key.Enter);
            picker.SelectedDateTime.ShouldBe(new DateTime(2026, 8, 20));
            picker.IsPickerOpen.ShouldBeFalse();
            Input(picker).Text.ShouldBe("2026-08-20");
        });
    }

    [Fact]
    public void Invalid_Text_Keeps_The_Popup_Open_On_Enter_And_Escape_Reverts_It()
    {
        var picker = new DatePicker { SelectedDateTime = July15, IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-13-40");
            Press(window, Key.Enter);
            picker.IsPickerOpen.ShouldBeTrue();
            picker.SelectedDateTime.ShouldBe(July15);

            Press(window, Key.Escape);
            picker.IsPickerOpen.ShouldBeFalse();
            picker.SelectedDateTime.ShouldBe(July15);
            Input(picker).Text.ShouldBe("2026-07-15");
        });
    }

    [Fact]
    public void Escape_Reverts_A_Valid_Typed_Candidate()
    {
        var picker = new DatePicker { SelectedDateTime = July15, IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-08-20");
            Press(window, Key.Escape);

            picker.SelectedDateTime.ShouldBe(July15);
            Input(picker).Text.ShouldBe("2026-07-15");
            picker.IsPickerOpen = true;
            Drain();
            Presenter(picker).SelectedDateTime.ShouldBe(July15, "the reverted candidate is not kept for the next open");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Focus_Leaving_Commits_Without_Confirmation_And_Discards_With_It(bool isNeedConfirm)
    {
        var picker = new DatePicker { SelectedDateTime = July15, IsNeedConfirm = isNeedConfirm, IsMotionEnabled = false };
        var next = new Avalonia.Controls.Button { Content = "next" };

        Show(new StackPanel { Children = { picker, next } }, window =>
        {
            Tab(window);
            Retype(window, "2026-08-20");
            next.Focus(NavigationMethod.Tab);
            Drain();

            picker.IsPickerOpen.ShouldBeFalse();
            picker.SelectedDateTime.ShouldBe(isNeedConfirm ? July15 : new DateTime(2026, 8, 20));
            Input(picker).Text.ShouldBe(isNeedConfirm ? "2026-07-15" : "2026-08-20");
        });
    }

    [Fact]
    public void Enter_Commits_Without_The_Confirm_Button()
    {
        var picker = new DatePicker { SelectedDateTime = July15, IsNeedConfirm = true, IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-08-20");
            Press(window, Key.Enter);

            picker.SelectedDateTime.ShouldBe(new DateTime(2026, 8, 20));
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Range_Enter_On_The_Start_Hands_Over_To_The_End_Then_Commits()
    {
        var picker = new RangeDatePicker { IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-07-10");
            Press(window, Key.Enter);

            picker.IsPickerOpen.ShouldBeTrue();
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.End);
            Focused(window).ShouldBeSameAs(SecondaryInput(picker), "focus follows the activated part");

            window.KeyTextInput("2026-07-20");
            Drain();
            Press(window, Key.Enter);

            picker.IsPickerOpen.ShouldBeFalse();
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 7, 10));
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 7, 20));
        });
    }

    [Fact]
    public void Range_Tab_Submits_The_Typed_Start_And_Moves_To_The_End()
    {
        var picker = new RangeDatePicker { IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-07-10");
            Tab(window);

            Focused(window).ShouldBeSameAs(SecondaryInput(picker));
            picker.IsPickerOpen.ShouldBeTrue();
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.End);
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 7, 10));
        });
    }

    [Fact]
    public void Navigation_Keys_Move_The_Panel_Focus_Across_Pages_And_Enter_Commits_It()
    {
        var picker = new DatePicker { SelectedDateTime = new DateTime(2026, 7, 31), IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Press(window, Key.Enter);

            Press(window, Key.Right);
            var session = Viewer(picker).PanelSession;
            session.FocusedValue.ShouldBe(new DateTime(2026, 8, 1));
            session.Input.DisplayDate.Month.ShouldBe(8, "the page follows the keyboard focus");
            Input(picker).Text.ShouldBe("2026-08-01", "the input previews the focused date");
            Focused(window).ShouldBeSameAs(Input(picker), "keyboard focus stays in the input");
            Cell(picker, new DateTime(2026, 8, 1)).Classes.Contains(":hovered").ShouldBeTrue();

            Press(window, Key.PageDown);
            session.FocusedValue.ShouldBe(new DateTime(2026, 9, 1));
            Press(window, Key.End);
            session.FocusedValue.ShouldBe(new DateTime(2026, 9, 30));
            Press(window, Key.Home);
            session.FocusedValue.ShouldBe(new DateTime(2026, 9, 1));
            Press(window, Key.Up);
            session.FocusedValue.ShouldBe(new DateTime(2026, 8, 25));

            Press(window, Key.Enter);
            picker.SelectedDateTime.ShouldBe(new DateTime(2026, 8, 25));
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Navigation_Keys_Do_Not_Leave_The_Selectable_Range()
    {
        var picker = new DatePicker
        {
            SelectedDateTime = July15, MaxDate = new DateTime(2026, 7, 16), IsMotionEnabled = false
        };

        Show(picker, window =>
        {
            Tab(window);
            Press(window, Key.Enter);
            Press(window, Key.Right);
            Press(window, Key.Right);
            Press(window, Key.Right);

            Viewer(picker).PanelSession.FocusedValue.ShouldBe(new DateTime(2026, 7, 16));
        });
    }

    [Fact]
    public void Range_Navigation_Previews_The_Range_And_Enter_Hands_Over()
    {
        var picker = new RangeDatePicker { IsMotionEnabled = false };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-07-10");
            Press(window, Key.Enter);
            Press(window, Key.Right);
            Press(window, Key.Right);

            SecondaryInput(picker).Text.ShouldBe("2026-07-12");
            Press(window, Key.Enter);
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 7, 10));
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 7, 12));
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Theory]
    [InlineData(DatePickerMode.Date)]
    [InlineData(DatePickerMode.Week)]
    [InlineData(DatePickerMode.Month)]
    [InlineData(DatePickerMode.Quarter)]
    [InlineData(DatePickerMode.Year)]
    public void Formatted_Text_Parses_Back_To_The_Picker_Unit(DatePickerMode mode)
    {
        var text = DatePickerFormattingHelper.FormatDateTime(July15, null, mode, false, ClockIdentifierType.HourClock24,
            null);
        DatePickerFormattingHelper.TryParseDateTime(text, null, mode, false, ClockIdentifierType.HourClock24, null,
            out var value).ShouldBeTrue(text);
        value.ShouldBe(DatePickerFormattingHelper.NormalizeDateTime(July15, mode));
    }

    [Theory]
    [InlineData("2026-7-15")]
    [InlineData("2026-07-32")]
    [InlineData("15/07/2026")]
    [InlineData("")]
    public void Text_That_Does_Not_Match_The_Format_Is_Rejected(string text)
    {
        DatePickerFormattingHelper.TryParseDateTime(text, null, DatePickerMode.Date, false,
            ClockIdentifierType.HourClock24, null, out _).ShouldBeFalse();
    }

    [Fact]
    public void Week_Text_May_Omit_The_Localized_Suffix_And_Quarter_Text_Is_Strict()
    {
        var weekText = DatePickerFormattingHelper.FormatDateTime(July15, null, DatePickerMode.Week, false,
            ClockIdentifierType.HourClock24, null);
        var bareWeek = new string(weekText.TakeWhile(c => char.IsAsciiDigit(c) || c == '-').ToArray());
        DatePickerFormattingHelper.TryParseDateTime(bareWeek, null, DatePickerMode.Week, false,
            ClockIdentifierType.HourClock24, null, out var week).ShouldBeTrue(bareWeek);
        week.ShouldBe(DatePickerFormattingHelper.NormalizeDateTime(July15, DatePickerMode.Week));

        DatePickerFormattingHelper.TryParseDateTime("2026-Q5", null, DatePickerMode.Quarter, false,
            ClockIdentifierType.HourClock24, null, out _).ShouldBeFalse();
    }

    [Fact]
    public void ShowTime_Text_Carries_The_Clock()
    {
        var picker = new DatePicker
        {
            SelectedDateTime = July15, IsShowTime = true, IsNeedConfirm = true, IsMotionEnabled = false
        };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "2026-08-20 10:20:30");
            Press(window, Key.Enter);

            picker.SelectedDateTime.ShouldBe(new DateTime(2026, 8, 20, 10, 20, 30));
        });
    }

    [Fact]
    public void TimePicker_Keys_Step_The_Active_Column_And_Enter_Commits()
    {
        var picker = new TimePicker
        {
            SelectedTime = new TimeSpan(10, 20, 30), IsNeedConfirm = true, IsMotionEnabled = false
        };

        Show(picker, window =>
        {
            Tab(window);
            Press(window, Key.Enter);
            picker.IsPickerOpen.ShouldBeTrue();

            Press(window, Key.Down);
            Input(picker).Text.ShouldBe("11:20:30");
            Press(window, Key.Right);
            Press(window, Key.Up);
            Input(picker).Text.ShouldBe("11:19:30");
            picker.PickerPresenter!.GetVisualDescendants().OfType<TimeViewCell>()
                  .Count(cell => cell.Classes.Contains(":keyboard-active")).ShouldBe(1);
            Focused(window).ShouldBeSameAs(Input(picker));

            Press(window, Key.Enter);
            picker.SelectedTime.ShouldBe(new TimeSpan(11, 19, 30));
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void TimePicker_Escape_Discards_Even_Without_Confirmation()
    {
        var picker = new TimePicker
        {
            SelectedTime = new TimeSpan(10, 20, 30), IsNeedConfirm = false, IsMotionEnabled = false
        };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "08:00:00");
            Press(window, Key.Escape);

            picker.IsPickerOpen.ShouldBeFalse();
            picker.SelectedTime.ShouldBe(new TimeSpan(10, 20, 30));
            Input(picker).Text.ShouldBe("10:20:30");
        });
    }

    [Fact]
    public void RangeTimePicker_Enter_On_A_Complete_Range_Start_Hands_Over_To_The_End()
    {
        var picker = new RangeTimePicker
        {
            RangeStartSelectedTime = new TimeSpan(9, 0, 0), RangeEndSelectedTime = new TimeSpan(18, 0, 0),
            IsMotionEnabled = false
        };

        Show(picker, window =>
        {
            Tab(window);
            Retype(window, "08:00:00");
            Press(window, Key.Enter);

            picker.IsPickerOpen.ShouldBeTrue();
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.End);
            picker.RangeStartSelectedTime.ShouldBe(new TimeSpan(8, 0, 0));
            Focused(window).ShouldBeSameAs(SecondaryInput(picker));

            Press(window, Key.Enter);
            picker.IsPickerOpen.ShouldBeFalse();
            picker.RangeEndSelectedTime.ShouldBe(new TimeSpan(18, 0, 0));
        });
    }

    private static void Show(Control content, Action<AvaloniaWindow> assertion)
    {
        var before = new Avalonia.Controls.Button { Content = "before" };
        var window = new AvaloniaWindow
        {
            Width = 960, Height = 800,
            Content = new StackPanel { Children = { before, content } }
        };
        try
        {
            window.Show();
            Drain();
            before.Focus(NavigationMethod.Tab);
            Drain();
            assertion(window);
        }
        finally
        {
            window.Close();
            Drain();
        }
    }

    private static void Tab(AvaloniaWindow window, bool shift = false) => Press(window, Key.Tab,
        shift ? RawInputModifiers.Shift : RawInputModifiers.None);

    private static void Press(AvaloniaWindow window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPress(key, modifiers, PhysicalKey.None, null);
        window.KeyRelease(key, modifiers, PhysicalKey.None, null);
        Drain();
    }

    // Selects the focused input's text and types over it, as a user retyping a value would.
    private static void Retype(AvaloniaWindow window, string text)
    {
        var input = (TextBox)Focused(window)!;
        input.SelectAll();
        window.KeyTextInput(text);
        Drain();
    }

    private static IInputElement? Focused(AvaloniaWindow window) => window.FocusManager?.GetFocusedElement();

    private static TextBox Input(Control picker) => Named<TextBox>(picker, "PART_InfoInputBox");
    private static TextBox SecondaryInput(Control picker) => Named<TextBox>(picker, "PART_SecondaryInfoInputBox");

    private static T Named<T>(Control picker, string name) where T : Control =>
        picker.GetVisualDescendants().OfType<T>().First(control => control.Name == name);

    private static DatePickerPresenter Presenter(InfoPickerInput picker) =>
        picker.PickerPresenter.ShouldBeAssignableTo<DatePickerPresenter>()!;

    private static DateViewer Viewer(InfoPickerInput picker) =>
        Presenter(picker).GetVisualDescendants().OfType<DateViewer>().Single();

    private static DateViewerCell Cell(InfoPickerInput picker, DateTime value) =>
        Viewer(picker).GetVisualDescendants().OfType<DateViewerCell>()
                      .First(cell => cell.Model is { IsInView: true } model && model.Value == value);

    private static void Drain() => Dispatcher.UIThread.RunJobs();
}
