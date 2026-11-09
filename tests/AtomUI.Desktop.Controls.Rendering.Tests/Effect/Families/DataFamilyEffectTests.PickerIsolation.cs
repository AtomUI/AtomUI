using AtomUI.Controls.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the calendar and picker members: a <c>Controls[X]</c> override reaches X only and the root Token
/// reaches X; the Calendar forwards its owner to its date grid; a picker forwards its owner to its popup presenter and the
/// time view to its cells, while the popup's DateViewer, the header selects of the Calendar and the popup frame keep their
/// own identity. Borrowed Own Tokens (read through another member's identity) are pinned here as well.
/// </summary>
public partial class DataFamilyEffectTests
{
    private static Calendar MiniCalendar(bool lunar)
    {
        var calendar = CreateCalendar(fullscreen: false, lunar: lunar);
        calendar.Width = 260;
        return calendar;
    }

    [Fact]
    public void Controls_X_Colours_Change_Only_X_For_Calendars_Viewers_And_Pickers()
    {
        var calendar   = MiniCalendar(lunar: false);
        var lunar      = MiniCalendar(lunar: true);
        var viewer     = CreateDateViewer();
        var datePicker = CreatePicker(PickerKind.Date);
        var timePicker = CreatePicker(PickerKind.Time);
        var range      = CreatePicker(PickerKind.RangeDate);
        var top     = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Children = { calendar, lunar, viewer } };
        var pickers = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Children = { datePicker, timePicker } };
        var root = new StackPanel { Spacing = 12, Children = { top, pickers, range } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            AssertReachesOnly(provider, capture,
            [
                (CalendarTokens.Identity, "ColorPrimary", calendar),
                (LunarCalendarTokens.Identity, "ColorPrimary", lunar),
                (DateViewerTokens.Identity, "ColorPrimary", viewer),
                (DatePickerTokens.Identity, "ColorText", datePicker),
                (TimePickerTokens.Identity, "ColorText", timePicker),
                (RangeDatePickerTokens.Identity, "ColorText", range)
            ]);
        });
    }

    [Fact]
    public void Calendar_Grid_Follows_The_Calendar_And_Its_Header_Selects_Keep_Their_Identity()
    {
        var calendar = MiniCalendar(lunar: false);
        var lunar    = MiniCalendar(lunar: true);
        var viewer   = CreateDateViewer();
        var root     = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Children = { calendar, lunar, viewer } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            var grid      = Descendant<DateViewer>(calendar);
            var lunarGrid = Descendant<DateViewer>(lunar);
            var cell      = SelectedCalendarCell(calendar);
            var select    = Descendant<ComboBox>(calendar);
            ThemeScope.GetTokenOwner(grid).ShouldBe(CalendarTokens.Identity);
            ThemeScope.GetTokenOwner(cell).ShouldBe(CalendarTokens.Identity);
            ThemeScope.GetTokenOwner(lunarGrid).ShouldBe(LunarCalendarTokens.Identity);
            ThemeScope.GetTokenOwner(viewer).ShouldBe(DateViewerTokens.Identity);
            ThemeScope.GetTokenOwner(select).ShouldBe(ComboBoxTokens.Identity);
            using var before = capture();

            // The Calendar's grid is the Calendar's: Controls[DateViewer] globals do not reach it, Controls[Calendar] does.
            provider.Config = IsolationConfig(control: (DateViewerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byViewer = capture();
            ChangedRatioOf(before, byViewer, calendar).ShouldBe(0d, "Controls[DateViewer].ColorPrimary reached the Calendar grid.");
            ChangedRatioOf(before, byViewer, viewer).ShouldBeGreaterThan(0d, "Controls[DateViewer] did not reach the DateViewer.");
            provider.Config = IsolationConfig(control: (CalendarTokens.Identity, "ColorPrimary", IsolationColor));
            using var byCalendar = capture();
            ChangedRatioOf(before, byCalendar, cell).ShouldBeGreaterThan(0d, "Controls[Calendar] did not reach its selected cell.");
            ChangedRatioOf(before, byCalendar, lunar).ShouldBe(0d, "Controls[Calendar].ColorPrimary reached the LunarCalendar.");

            // The header select is a registered ComboBox: it keeps its own identity.
            provider.Config = IsolationConfig(control: (CalendarTokens.Identity, "ColorBorder", IsolationColor));
            using var byCalendarBorder = capture();
            ChangedRatioOf(before, byCalendarBorder, select).ShouldBe(0d, "Controls[Calendar].ColorBorder reached the header select.");
            provider.Config = IsolationConfig(control: (ComboBoxTokens.Identity, "ColorBorder", IsolationColor));
            using var byComboBox = capture();
            ChangedRatioOf(before, byComboBox, select).ShouldBeGreaterThan(0d, "Controls[ComboBox] did not reach the header select.");
        });
    }

    [Fact]
    public void Picker_Popup_Follows_The_Picker_And_Its_DateViewer_Keeps_Its_Identity()
    {
        var picker = (DatePicker)CreatePicker(PickerKind.Date);
        picker.IsShowTime        = true;
        picker.IsPopupPinnedOpen = true;
        var host = PickerOverlayHost(picker);
        WithIsolationWindow(host, (provider, capture) =>
        {
            WaitForPickerPopup(picker);
            var presenter = PresenterOf(picker);
            var timeView  = Descendant<TimeView>(presenter);
            var timeCell  = SelectedTimeCell(picker);
            var viewer    = Descendant<DateViewer>(presenter);
            var dateCell  = viewer.GetVisualDescendants().OfType<DateViewerCell>().First(static cell => cell.Model?.IsSelected == true);
            var frame     = PopupRootOf(picker);
            ThemeScope.GetTokenOwner(presenter).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(timeView).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(timeCell).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(viewer).ShouldBe(DateViewerTokens.Identity);
            ThemeScope.GetTokenOwner(frame).ShouldBe(ArrowDecoratedBoxTokens.Identity);
            using var before = capture();

            // Controls[DatePicker] reaches the time view and its cells (the presenter takes the picker's owner).
            provider.Config = IsolationConfig(control: (DatePickerTokens.Identity, "ControlItemBgActive", IsolationColor));
            using var byPicker = capture();
            ChangedRatioOf(before, byPicker, timeCell).ShouldBeGreaterThan(0d, "Controls[DatePicker] did not reach the time cells.");

            // The popup's DateViewer keeps its identity: Controls[DatePicker].ColorPrimary does not reach its selected cell,
            // Controls[DateViewer] does, and Controls[Calendar] reaches nothing in the picker.
            provider.Config = IsolationConfig(control: (DatePickerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byPickerPrimary = capture();
            ChangedRatioOf(before, byPickerPrimary, dateCell).ShouldBe(0d, "Controls[DatePicker].ColorPrimary reached the popup DateViewer.");
            provider.Config = IsolationConfig(control: (DateViewerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byViewer = capture();
            ChangedRatioOf(before, byViewer, dateCell).ShouldBeGreaterThan(0d, "Controls[DateViewer] did not reach the popup cell.");
            provider.Config = IsolationConfig(control: (CalendarTokens.Identity, "ColorPrimary", IsolationColor));
            using var byCalendar = capture();
            ChangedRatioOf(before, byCalendar, host).ShouldBe(0d, "Controls[Calendar] reached the DatePicker.");

            // The popup frame keeps the ArrowDecoratedBox identity.
            provider.Config = IsolationConfig(control: (DatePickerTokens.Identity, "ColorBgElevated", IsolationColor));
            using var byPickerElevated = capture();
            ChangedRatioOf(before, byPickerElevated, timeView).ShouldBeGreaterThan(0d, "Controls[DatePicker] did not reach the time view.");
        });
    }

    [Fact]
    public void Time_Cells_Follow_The_Picker_Motion_Tokens()
    {
        // The cells sit in registered ScrollViewers; the time view forwards the picker's owner, so their TokenTransitions
        // take the picker's durations.
        var picker = CreatePicker(PickerKind.Time);
        picker.IsPopupPinnedOpen = true;
        WithProvider(PickerOverlayHost(picker), provider =>
        {
            WaitForPickerPopup(picker);
            var cell = SelectedTimeCell(picker);
            ThemeScope.GetTokenOwner(cell).ShouldBe(TimePickerTokens.Identity);
            provider.Config = new ThemeConfigBuilder()
                              .WithControl(TimePickerTokens.Identity, new ControlThemeConfigBuilder()
                                                                       .WithToken("MotionDurationMid", "00:00:00.4000000").Build())
                              .Build();
            WaitFor(() => cell.Transitions?.OfType<Avalonia.Animation.TransitionBase>()
                              .All(static transition => transition.Duration == TimeSpan.FromMilliseconds(400)) == true);
            cell.Transitions.ShouldNotBeNull();
            cell.Transitions!.OfType<Avalonia.Animation.TransitionBase>()
                .ShouldAllBe(static transition => transition.Duration == TimeSpan.FromMilliseconds(400));
        });
    }

    [Fact]
    public void Borrowed_Own_Tokens_Reach_Their_Readers()
    {
        // DatePicker.RangePanelSpacing spaces the two panels of the RangeDatePicker popup, and TimePicker.HeaderMargin /
        // ItemHeight shape the time view of the date-time picker: these readers borrow another member's Own Tokens (recorded
        // for the abstract-layer follow-up).
        foreach (var (kind, identity, token, value) in new[]
                 {
                     (PickerKind.RangeDate, DatePickerTokens.Identity, "RangePanelSpacing", "64"),
                     (PickerKind.Date, TimePickerTokens.Identity, "HeaderMargin", "0,12,0,12"),
                     (PickerKind.Date, TimePickerTokens.Identity, "ItemHeight", "40")
                 })
        {
            var picker = CreatePicker(kind);
            if (picker is DatePicker date)
            {
                date.IsShowTime = true;
            }

            picker.IsPopupPinnedOpen = true;
            var host = PickerOverlayHost(picker);
            WithIsolationWindow(host, (provider, capture) =>
            {
                WaitForPickerPopup(picker);
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, token, value));
                using var after = capture();
                ChangedRatioOf(before, after, host).ShouldBeGreaterThan(0.001, $"Controls[{identity}].{token}");
            });
        }
    }
}
