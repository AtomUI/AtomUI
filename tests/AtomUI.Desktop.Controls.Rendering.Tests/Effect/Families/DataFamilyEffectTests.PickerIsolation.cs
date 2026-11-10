using AtomUI.Controls.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the calendar and picker members: a <c>Controls[X]</c> override reaches X only and the root Token
/// reaches X; the Calendar forwards its owner to its date grid; a picker forwards its owner to its popup presenter, the date
/// presenter to its DateViewer and the time view to its cells, while the header selects of the Calendar and the popup frame
/// keep their own identity. Borrowed Own Tokens (read through another member's identity) are pinned here as well.
/// </summary>
public partial class DataFamilyEffectTests
{
    private static Calendar MiniCalendar(bool lunar)
    {
        var calendar = CreateCalendar(fullscreen: false, lunar: lunar);
        calendar.Width = 260;
        return calendar;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Controls_X_Colours_Change_Only_X_For_Calendars_Viewers_And_Pickers(bool rangeMembers)
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
        (ControlTokenIdentity Identity, string Token, Control Control)[] members =
        [
            (CalendarTokens.Identity, "ColorPrimary", calendar),
            (LunarCalendarTokens.Identity, "ColorPrimary", lunar),
            (DateViewerTokens.Identity, "ColorPrimary", viewer),
            (DatePickerTokens.Identity, "ColorText", datePicker),
            (TimePickerTokens.Identity, "ColorText", timePicker),
            (RangeDatePickerTokens.Identity, "ColorText", range)
        ];
        if (rangeMembers)
        {
            // The range viewer keeps both panels at their natural dimensions in a separate finite arrangement.
            // These two members are unrelated siblings for each other's control-only override.
            var rangeViewer = CreateRangeDateViewer();
            var rangeTime   = CreatePicker(PickerKind.RangeTime);
            root.Children.Clear();
            root.Children.Add(rangeViewer);
            root.Children.Add(rangeTime);
            members =
            [
                (RangeDateViewerTokens.Identity, "ColorPrimary", rangeViewer),
                (RangeTimePickerTokens.Identity, "ColorText", rangeTime)
            ];
            members.ShouldContain(member => member.Identity == RangeDateViewerTokens.Identity,
                "The range arrangement must exercise RangeDateViewer isolation.");
            members.ShouldContain(member => member.Identity == RangeTimePickerTokens.Identity,
                "The range arrangement must exercise RangeTimePicker isolation.");
        }

        WithIsolationWindow(root, (provider, capture) => AssertReachesOnly(provider, capture, members));
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
    public void Picker_Popup_Follows_The_Picker_Including_Its_Date_Panel()
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
            var dateCell  = SelectedPopupDateCell(picker);
            var hoverCell = viewer.GetVisualDescendants().OfType<DateViewerCell>()
                                  .First(static cell => cell.Model is { IsSelected: false, IsInView: true, Kind: DateViewerCellType.Date });
            var frame     = PopupRootOf(picker);
            ThemeScope.GetTokenOwner(presenter).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(timeView).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(timeCell).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(viewer).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(dateCell).ShouldBe(DatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(frame).ShouldBe(ArrowDecoratedBoxTokens.Identity);
            using var before = capture();

            // Controls[DatePicker] reaches the time view and its cells (the presenter takes the picker's owner).
            provider.Config = IsolationConfig(control: (DatePickerTokens.Identity, "ControlItemBgActive", IsolationColor));
            using var byPicker = capture();
            ChangedRatioOf(before, byPicker, timeCell).ShouldBeGreaterThan(0d, "Controls[DatePicker] did not reach the time cells.");

            // The presenter forwards the picker's owner to its DateViewer (as the Calendar does): antd
            // components.DatePicker.colorPrimary recolours the selected date cell, and Controls[DateViewer] globals no longer
            // reach the popup.
            provider.Config = IsolationConfig(control: (DatePickerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byPickerPrimary = capture();
            ChangedRatioOf(before, byPickerPrimary, dateCell).ShouldBeGreaterThan(0d, "Controls[DatePicker].ColorPrimary did not reach the popup date cell.");
            provider.Config = IsolationConfig(control: (DateViewerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byViewer = capture();
            ChangedRatioOf(before, byViewer, presenter).ShouldBe(0d, "Controls[DateViewer].ColorPrimary reached the picker popup.");
            provider.Config = IsolationConfig(control: (CalendarTokens.Identity, "ColorPrimary", IsolationColor));
            using var byCalendar = capture();
            ChangedRatioOf(before, byCalendar, host).ShouldBe(0d, "Controls[Calendar] reached the DatePicker.");

            // The DateViewer Own Tokens are still read from the DateViewer identity (borrowed): its hover background shows on
            // the picker's popup cells.
            Pseudo(hoverCell, ":pointerover");
            provider.Config = IsolationConfig();
            using var hovered = capture();
            provider.Config = IsolationConfig(control: (DateViewerTokens.Identity, "CellHoverBg", IsolationColor));
            using var byViewerHover = capture();
            ChangedRatioOf(hovered, byViewerHover, hoverCell).ShouldBeGreaterThan(0d, "Controls[DateViewer].CellHoverBg did not reach the popup cell.");
            ((Avalonia.Controls.IPseudoClasses)hoverCell.Classes).Remove(":pointerover");

            // The popup frame keeps its own identity: Controls[DatePicker].ColorBgElevated reaches the time view inside the
            // frame, never the frame's own pixels around the presenter; the root ColorBgElevated does (the ArrowDecoratedBox
            // theme still reads it as a shared Token).
            provider.Config = IsolationConfig();
            using var plain = capture();
            provider.Config = IsolationConfig(control: (DatePickerTokens.Identity, "ColorBgElevated", IsolationColor));
            using var byPickerElevated = capture();
            ChangedRatioOf(plain, byPickerElevated, timeView).ShouldBeGreaterThan(0d, "Controls[DatePicker] did not reach the time view.");
            FrameOnlyChangedPixels(plain, byPickerElevated, frame, presenter).ShouldBe(0, "Controls[DatePicker].ColorBgElevated reached the popup frame.");
            provider.Config = IsolationConfig(root: ("ColorBgElevated", IsolationColor));
            using var byRoot = capture();
            FrameOnlyChangedPixels(plain, byRoot, frame, presenter).ShouldBeGreaterThan(0, "The root ColorBgElevated did not reach the popup frame.");
        });
    }

    [Fact]
    public void Range_Picker_Popup_Date_Panel_Follows_The_Range_Picker()
    {
        var picker = CreatePicker(PickerKind.RangeDate);
        picker.IsPopupPinnedOpen = true;
        var host = PickerOverlayHost(picker);
        WithIsolationWindow(host, (provider, capture) =>
        {
            WaitForPickerPopup(picker);
            var presenter = PresenterOf(picker);
            var viewer    = Descendant<RangeDateViewer>(presenter);
            var dateCell  = SelectedPopupDateCell(picker);
            ThemeScope.GetTokenOwner(viewer).ShouldBe(RangeDatePickerTokens.Identity);
            ThemeScope.GetTokenOwner(dateCell).ShouldBe(RangeDatePickerTokens.Identity);
            using var before = capture();
            provider.Config = IsolationConfig(control: (RangeDatePickerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byPicker = capture();
            ChangedRatioOf(before, byPicker, dateCell).ShouldBeGreaterThan(0d, "Controls[RangeDatePicker].ColorPrimary did not reach the popup date cell.");
            provider.Config = IsolationConfig(control: (DateViewerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byViewer = capture();
            ChangedRatioOf(before, byViewer, presenter).ShouldBe(0d, "Controls[DateViewer].ColorPrimary reached the range picker popup.");
            provider.Config = IsolationConfig(control: (DatePickerTokens.Identity, "ColorPrimary", IsolationColor));
            using var byDatePicker = capture();
            ChangedRatioOf(before, byDatePicker, host).ShouldBe(0d, "Controls[DatePicker].ColorPrimary reached the RangeDatePicker.");
        });
    }

    [Theory]
    [InlineData(PickerKind.Date)]
    [InlineData(PickerKind.RangeDate)]
    [InlineData(PickerKind.Time)]
    [InlineData(PickerKind.RangeTime)]
    public void Picker_Input_Keeps_The_Text_Colour_While_Open_And_Previews_In_The_Placeholder_Colour(PickerKind kind)
    {
        // antd 6.7 (date-picker/style/index.ts): the input inherits colorText while the popup is open; only the hover
        // preview (`-placeholder` > input, AtomUI's :choosing state) shows colorTextPlaceholder, on the active input.
        var picker = CreatePicker(kind);
        picker.IsPopupPinnedOpen = true;
        if (picker is RangeInfoPickerInput range)
        {
            range.RangeActivatedPart = RangeActivatedPart.End;
        }

        var host = PickerOverlayHost(picker);
        var identity = IdentityOf(kind);
        WithIsolationWindow(host, (provider, capture) =>
        {
            WaitForPickerPopup(picker);
            var inputs = picker.GetVisualDescendants().OfType<InfoPickerTextBox>().ToList();
            var active = inputs[^1];
            using var before = capture();

            // Open, not previewing: the inputs show the text colour.
            provider.Config = IsolationConfig(control: (identity, "ColorText", IsolationColor));
            using var byText = capture();
            foreach (var input in inputs)
            {
                ChangedRatioOf(before, byText, input).ShouldBeGreaterThan(0d, $"{input.Name} of the open {kind} picker does not show ColorText.");
            }

            provider.Config = IsolationConfig(control: (identity, "ColorTextTertiary", IsolationColor));
            using var byTertiary = capture();
            ChangedRatioOf(before, byTertiary, picker).ShouldBe(0d, $"The open {kind} picker input reads ColorTextTertiary.");

            // Previewing: the active input shows the placeholder colour; the other range input keeps the text colour.
            Pseudo(picker, ":choosing");
            provider.Config = IsolationConfig();
            using var choosing = capture();
            provider.Config = IsolationConfig(control: (identity, "ColorTextPlaceholder", IsolationColor));
            using var byPlaceholder = capture();
            ChangedRatioOf(choosing, byPlaceholder, active).ShouldBeGreaterThan(0d, $"The {kind} hover preview does not show ColorTextPlaceholder.");
            if (inputs.Count > 1)
            {
                ChangedRatioOf(choosing, byPlaceholder, inputs[0]).ShouldBe(0d, $"The inactive {kind} input shows ColorTextPlaceholder.");
            }
        });
    }

    [Theory]
    [InlineData(PickerKind.Date)]
    [InlineData(PickerKind.RangeDate)]
    [InlineData(PickerKind.Time)]
    [InlineData(PickerKind.RangeTime)]
    public void Picker_Focus_Ring_Paints_Outside_The_Picker(PickerKind kind)
    {
        // The input frame's focus shadow (antd activeShadow, default strength) paints outside the picker bounds; the picker
        // used to clip it (ClipToBounds), which hid the ring entirely on the range pickers.
        var picker = CreatePicker(kind);
        WithIsolationWindow(picker, (_, capture) =>
        {
            picker.ClipToBounds.ShouldBeFalse();
            using var before = capture();
            picker.GetVisualDescendants().OfType<InputControlFrame>().First().IsInputFocusWithin = true;
            using var focused = capture();
            var bounds = RegionOf(picker);
            var above  = new SkiaSharp.SKRectI(bounds.Left, bounds.Top - 2, bounds.Right, bounds.Top);
            var below  = new SkiaSharp.SKRectI(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom + 2);
            FrameCapture.CountDifferentPixels(before, focused, above, TokenEffectHarness.DefaultChannelTolerance)
                        .ShouldBeGreaterThan(0, $"The focus ring of the {kind} picker does not show above the input.");
            FrameCapture.CountDifferentPixels(before, focused, below, TokenEffectHarness.DefaultChannelTolerance)
                        .ShouldBeGreaterThan(0, $"The focus ring of the {kind} picker does not show below the input.");
        });
    }

    private static ControlTokenIdentity IdentityOf(PickerKind kind) => kind switch
    {
        PickerKind.Date => DatePickerTokens.Identity,
        PickerKind.RangeDate => RangeDatePickerTokens.Identity,
        PickerKind.Time => TimePickerTokens.Identity,
        _ => RangeTimePickerTokens.Identity
    };

    /// <summary>Changed pixels of the popup frame outside the presenter it hosts (its padding, edge and arrow).</summary>
    private static int FrameOnlyChangedPixels(SkiaSharp.SKBitmap before, SkiaSharp.SKBitmap after, Control frame, Control presenter)
    {
        var frameRegion     = RegionOf(frame);
        var presenterRegion = RegionOf(presenter);
        (frameRegion.Width * frameRegion.Height).ShouldBeGreaterThan(presenterRegion.Width * presenterRegion.Height,
            "The popup frame shows no pixels of its own around the presenter.");
        return FrameCapture.CountDifferentPixels(before, after, frameRegion, TokenEffectHarness.DefaultChannelTolerance) -
               FrameCapture.CountDifferentPixels(before, after, presenterRegion, TokenEffectHarness.DefaultChannelTolerance);
    }

    private static SkiaSharp.SKRectI RegionOf(Control control)
    {
        var window = (Avalonia.Visual)TopLevel.GetTopLevel(control)!;
        var origin = control.TranslatePoint(new Avalonia.Point(), window).ShouldNotBeNull();
        return new SkiaSharp.SKRectI(
            (int)Math.Floor(origin.X),
            (int)Math.Floor(origin.Y),
            (int)Math.Ceiling(origin.X + control.Bounds.Width),
            (int)Math.Ceiling(origin.Y + control.Bounds.Height));
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
        // DatePicker.RangePanelSpacing / ButtonsPanelMargin spaces the RangeDatePicker popup, TimePicker.ItemMargin the
        // RangeTimePicker cells, and TimePicker.HeaderMargin / ItemHeight shape the time view of the date-time picker: these
        // readers borrow another member's Own Tokens (recorded for the abstract-layer follow-up).
        foreach (var (kind, identity, token, value) in new[]
                 {
                     (PickerKind.RangeDate, DatePickerTokens.Identity, "RangePanelSpacing", "64"),
                     (PickerKind.RangeDate, DatePickerTokens.Identity, "ButtonsPanelMargin", "24"),
                     (PickerKind.RangeTime, TimePickerTokens.Identity, "ItemMargin", "8"),
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

        // LunarCalendar reuses the Calendar cell theme, which reads the Calendar Own Tokens explicitly.
        var lunar = CreateCalendar(fullscreen: true, lunar: true);
        WithIsolationWindow(lunar, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = IsolationConfig(control: (CalendarTokens.Identity, "FullCellPadding", "16"));
            using var after = capture();
            ChangedRatioOf(before, after, lunar).ShouldBeGreaterThan(0.001, "Controls[Calendar].FullCellPadding (LunarCalendar)");
        });
    }
}
