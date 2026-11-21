using System.Globalization;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Internal.Calendar;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Calendar and LunarCalendar effect rows. The date grid of a Calendar is a registered DateViewer whose panel and cell
/// themes the Calendar supplies; the Calendar forwards its Token owner to that DateViewer, so the identity-less header, panel
/// and cells read the hosting Calendar's (or LunarCalendar's) globals and <c>Controls[Calendar]</c> reaches the cells.
/// LunarCalendar reuses the Calendar theme, whose global reads resolve to the LunarCalendar (hand list
/// <see cref="s_calendarTheme"/>).
/// </summary>
public partial class DataFamilyEffectTests
{
    private const string CalendarThemes = "src/AtomUI.Desktop.Controls/Calendar/Themes/";

    /// <summary>The identity-less default header of Calendar and LunarCalendar.</summary>
    internal static readonly OwnerReadList s_calendarHeader = new(CalendarThemes + "CalendarHeaderTheme.axaml",
        ["UniformlyMarginXS", "UniformlyPaddingSM", "UniformlyPaddingXS"]);

    /// <summary>The Calendar cell theme (shared by Calendar and LunarCalendar) reads the hosting calendar's globals.</summary>
    internal static readonly OwnerReadList s_calendarDateCell = new(CalendarThemes + "CalendarDateCellTheme.axaml",
    [
        "BorderRadiusSM", "BorderThickness", "ColorBgContainerDisabled", "ColorFillSecondary", "ColorPrimary", "ColorText",
        "ColorTextDisabled", "ColorTextLightSolid", "ControlHeightSM", "ControlItemBgHover"
    ]);

    /// <summary>
    /// CalendarTheme reads its globals from the owner (owner: marker): the consumption map attributes them to Calendar, and on
    /// a LunarCalendar they resolve to the LunarCalendar.
    /// </summary>
    internal static readonly OwnerReadList s_calendarTheme = new(CalendarThemes + "CalendarTheme.axaml",
    [
        "BorderRadiusLG", "ColorBorderSecondary", "ColorSplit", "ColorTextLightSolid", "FontSizeSM",
        "LineWidth", "UniformlyMarginXXS", "UniformlyPaddingXS"
    ], "src/AtomUI.Desktop.Controls/Calendar/Internal/CalendarRangeBarPanel.cs");

    /// <summary>The Calendar date panel theme (shared by Calendar and LunarCalendar) reads the compact week header height.</summary>
    internal static readonly OwnerReadList s_calendarDatePanel = new(CalendarThemes + "CalendarDatePanelTheme.axaml",
        ["FontHeightSM"]);

    private static readonly DateTime s_calendarDate = new(2026, 7, 15);

    /// <summary>A date in the current month that is not today, so the today outline and the selection do not overlap.</summary>
    private static DateTime TodayMonthValue() =>
        DateTime.Today.Day > 15 ? DateTime.Today.AddDays(-7) : DateTime.Today.AddDays(7);

    internal static Calendar CreateCalendar(
        bool fullscreen = true,
        bool lunar = false,
        CalendarMode mode = CalendarMode.Month,
        DateTime? value = null,
        bool rangeBars = false)
    {
        Calendar calendar = lunar ? new LunarCalendar() : new Calendar();
        calendar.Fullscreen = fullscreen;
        calendar.Mode       = mode;
        calendar.Value      = value ?? s_calendarDate;
        calendar.Width      = fullscreen ? 620 : 300;
        if (rangeBars)
        {
            var date = calendar.Value.Date;
            // Two overlapping bars: the second one stacks below the first (RangeBarGap).
            calendar.RangeBars.Add(new CalendarRangeBar
            {
                StartDate  = date.AddDays(-2), EndDate = date.AddDays(1), Label = "Sprint review",
                Background = Brushes.SteelBlue
            });
            calendar.RangeBars.Add(new CalendarRangeBar
            {
                StartDate  = date.AddDays(-1), EndDate = date.AddDays(2), Label = "Release",
                Background = Brushes.IndianRed
            });
        }

        return calendar;
    }

    private static DateViewerCell CalendarCell(Control calendar, DateTime date) =>
        calendar.GetVisualDescendants().OfType<DateViewerCell>()
                .First(cell => cell.Model is { Kind: DateViewerCellType.Date or DateViewerCellType.Month } model &&
                               model.Value?.Date == date.Date && model.IsInView);

    /// <summary>
    /// The painted block of a Calendar cell. Comparisons use it rather than the cell: the cell bounds do not locate the
    /// rendered block of the fullscreen grid.
    /// </summary>
    private static Control CellInnerOf(Control cell) => Descendant<Avalonia.Controls.Border>(cell, "PART_CellInner");

    private static Control CalendarRangeBarsOf(Control calendar) =>
        calendar.GetVisualDescendants().OfType<CalendarRangeBarPanel>().First();

    private static Control FirstRangeBarOf(Control calendar) =>
        CalendarRangeBarsOf(calendar).GetVisualChildren().OfType<Control>().First();

    private static Control FirstRangeBarLabelOf(Control calendar) =>
        Descendant<Avalonia.Controls.TextBlock>(FirstRangeBarOf(calendar));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Range_Bar_Label_Follows_ColorTextLightSolid(bool lunar)
    {
        // The range bar label colour was a hard-coded Brushes.White; the panel now applies the owner's ColorTextLightSolid
        // (the ColorTextLightSolid row of the member proves the selected cell's text, this row the bar label).
        Run(() => CreateCalendar(fullscreen: true, lunar: lunar, rangeBars: true), null,
            lunar ? LunarCalendarTokens.Identity : CalendarTokens.Identity, "ColorTextLightSolid",
            new EffectOptions { Region = static c => FirstRangeBarLabelOf(c) });
    }

    private static Control CalendarDividerOf(Control calendar) =>
        Descendant<DockPanel>(calendar, "PART_BodyPresenter").Children[0];

    private static Control CalendarHeaderOf(Control calendar) => Descendant<CalendarHeader>(calendar);

    private static Control SelectedCalendarCell(Control calendar) =>
        CellInnerOf(CalendarCell(calendar, ((Calendar)calendar).Value));

    private static Control OutsideCalendarCell(Control calendar) =>
        CellInnerOf(calendar.GetVisualDescendants().OfType<DateViewerCell>()
                            .First(static cell => cell.Model is { Kind: DateViewerCellType.Date, IsInView: false }));

    /// <summary>A mini calendar on a grey surface, so its rounded frame shows (radius 48, as the corners are small).</summary>
    private static Scenario CalendarOnSurface(bool lunar) =>
        new(() => new Avalonia.Controls.Border
            {
                Background = Brushes.LightGray, Padding = new Thickness(8),
                Child      = CreateCalendar(fullscreen: false, lunar: lunar)
            }, null,
            new EffectOptions { Target = static host => Descendant<Calendar>(host), Perturb = static _ => new CornerRadius(48) });

    public static IEnumerable<object[]> CalendarRows() =>
        HostRows("Calendar", [], [s_calendarHeader, s_calendarDateCell, s_calendarDatePanel, s_datePanel]);

    [Theory]
    [MemberData(nameof(CalendarRows))]
    public void Calendar_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = CalendarScenario(token, lunar: false);
        Run(scenario.Create, scenario.Arrange, CalendarTokens.Identity, token, scenario.Options);
    }

    // ItemActiveColor (shared by AbstractCalendarToken) colours the fullscreen selected cell of the Gregorian calendar;
    // the lunar cells draw their selected text through LunarCalendarCellContent (ColorTextLightSolid), so the lunar row
    // has no visible state for it.
    public static IEnumerable<object[]> LunarCalendarRows() =>
        HostRows("LunarCalendar", [], [s_calendarTheme, s_calendarHeader, s_calendarDateCell, s_calendarDatePanel, s_datePanel])
            .Where(static row => (string)row[0] != "ItemActiveColor");

    [Theory]
    [MemberData(nameof(LunarCalendarRows))]
    public void LunarCalendar_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = LunarCalendarScenario(token) ?? CalendarScenario(token, lunar: true);
        Run(scenario.Create, scenario.Arrange, LunarCalendarTokens.Identity, token, scenario.Options);
    }

    [Theory]
    [InlineData(CalendarMode.Year, DateViewerCellType.Month, "#ffe6f4ff")]
    [InlineData(CalendarMode.Month, DateViewerCellType.Date, "#ff1677ff")]
    public void Lunar_Fullscreen_Selection_Isolates_ItemActiveBg_And_Preserves_Mode_Colours(
        CalendarMode mode, DateViewerCellType kind, string defaultColour)
    {
        var lunar = CreateCalendar(fullscreen: true, lunar: true, mode: mode, value: new DateTime(2026, 7, 15));
        WithIsolationWindow(lunar, (provider, capture) =>
        {
            using var before = capture();
            var cell = lunar.GetVisualDescendants().OfType<DateViewerCell>()
                .Single(cell => cell.Model is { IsInView: true } model && model.Kind == kind && cell.Classes.Contains(":selected"));
            var inner = (Avalonia.Controls.Border)CellInnerOf(cell);
            var expected = Color.Parse(defaultColour);
            inner.Background.ShouldBeAssignableTo<ISolidColorBrush>()!.Color.ShouldBe(expected);

            provider.Config = IsolationConfig(control: (CalendarTokens.Identity, "ItemActiveBg", "#ffff0000"));
            using var sourceOverride = capture();
            inner.Background.ShouldBeAssignableTo<ISolidColorBrush>()!.Color.ShouldBe(expected,
                "Calendar Own must not paint the actual selected Lunar cell.");
            ChangedRatioOf(before, sourceOverride, inner).ShouldBe(0d);

            provider.Config = IsolationConfig(control: (LunarCalendarTokens.Identity, "ItemActiveBg", "#ff00b96b"));
            using var lunarOverride = capture();
            inner.Background.ShouldBeAssignableTo<ISolidColorBrush>()!.Color.ShouldBe(
                mode == CalendarMode.Year ? Color.Parse("#ff00b96b") : expected);
            if (mode == CalendarMode.Year)
            {
                ChangedRatioOf(before, lunarOverride, inner).ShouldBeGreaterThan(0.005);
            }
            else
            {
                ChangedRatioOf(before, lunarOverride, inner).ShouldBe(0d,
                    "The existing lunar date-primary selector must remain distinct from the selected month surface.");
            }
        });
    }

    [Fact]
    public void Lunar_Fullscreen_Year_ItemActiveBg_Has_Own_Effect()
    {
        Run(() => CreateCalendar(fullscreen: true, lunar: true, mode: CalendarMode.Year, value: new DateTime(2026, 7, 15)),
            null, LunarCalendarTokens.Identity, "ItemActiveBg",
            new EffectOptions { Region = SelectedLunarMonthSurface });
    }

    private static Control SelectedLunarMonthSurface(Control lunar) =>
        CellInnerOf(lunar.GetVisualDescendants().OfType<DateViewerCell>()
            .Single(static cell => cell.Model is { Kind: DateViewerCellType.Month, IsInView: true } && cell.Classes.Contains(":selected")));

    private static Scenario CalendarScenario(string token, bool lunar)
    {
        Scenario Mini(Action<Control>? arrange = null, CalendarMode mode = CalendarMode.Month, DateTime? value = null,
                      EffectOptions? options = null) =>
            new(() => CreateCalendar(fullscreen: false, lunar: lunar, mode: mode, value: value), arrange, options);

        Scenario Full(bool rangeBars = false, Action<Control>? arrange = null, EffectOptions? options = null) =>
            new(() => CreateCalendar(lunar: lunar, rangeBars: rangeBars), arrange, options);

        switch (token)
        {
            // ---- mini calendar ----
            case "MiniContentHeight" or "ControlHeightSM" or "FontHeightSM":
                return Mini();
            case "YearMonthCellWidth" or "ControlHeightLG":
                // Month cells of the year view (lunar: MiniMonthCellWidth = ControlHeightLG * 2).
                return Mini(mode: CalendarMode.Year);
            case "ColorPrimary":
                // The selected cell is filled with the primary colour.
                return Mini();
            case "ColorTextLightSolid":
                // The selected cell's light text.
                return Mini(options: new EffectOptions { Region = static c => SelectedCalendarCell(c) });
            case "ItemActiveColor":
                // The fullscreen selected cell's text on ItemActiveBg.
                return Full(options: new EffectOptions { Region = static c => SelectedCalendarCell(c) });
            case "BorderRadiusSM":
                // The selected cell's rounded corners (radius 12, as the cell is small).
                return Mini(options: new EffectOptions
                {
                    Region = static c => SelectedCalendarCell(c), Perturb = static _ => new CornerRadius(12)
                });
            case "BorderThickness":
                // The today outline (the selection is elsewhere in the month).
                return Mini(value: TodayMonthValue());
            case "ControlItemBgHover":
                return Mini(static c => Pseudo(CalendarCell(c, s_calendarDate.AddDays(1)), ":pointerover"));
            case "ColorBgContainerDisabled":
                return new Scenario(() =>
                {
                    var calendar = CreateCalendar(fullscreen: false, lunar: lunar);
                    calendar.DisabledDate = static date => date.Day > 20;
                    return calendar;
                });
            case "ColorSplit" or "LineWidth":
                // The divider under the header is drawn in the mini calendar only.
                return Mini(options: new EffectOptions { Region = static c => CalendarDividerOf(c) });
            case "ColorBorderSecondary":
                // The frame border shows once the application gives the calendar a border thickness.
                return Mini(static c => ((Calendar)c).BorderThickness = new Thickness(1));
            case "BorderRadiusLG":
                return CalendarOnSurface(lunar);

            // ---- fullscreen calendar ----
            case "FullCellMinHeight":
                // Give the calendar enough space for its natural row sizing to take effect.
                return new Scenario(() => new Avalonia.Controls.Window
                {
                    Width = 720,
                    Height = 1030,
                    Content = CreateCalendar(lunar: lunar)
                }, Options: new EffectOptions
                {
                    Target = static root => Descendant<Calendar>(root),
                    Perturb = static _ => 72d
                });
            case "RangeBarGap":
                return Full(rangeBars: true, options: new EffectOptions
                {
                    Region = static c => CalendarRangeBarsOf(c).GetVisualChildren().OfType<Control>().ElementAt(1)
                });
            case "RangeBarHeight" or "UniformlyMarginXXS":
                return Full(rangeBars: true, options: new EffectOptions { Region = static c => CalendarRangeBarsOf(c) });
            case "RangeBarLabelPadding" or "FontSizeSM":
                // The label inside the first bar.
                return Full(rangeBars: true, options: new EffectOptions { Region = static c => FirstRangeBarOf(c) });
            case "YearControlWidth" or "MonthControlWidth":
                return Full(options: new EffectOptions { Region = static c => CalendarHeaderOf(c) });
            case "ColorTextDisabled":
                // The text of the cells outside the month.
                return Full(options: new EffectOptions { Region = static c => OutsideCalendarCell(c) });
            default:
                // FullBg / ColorBgContainer, FullPanelBg, ItemActiveBg / ControlItemBgActive (selected cell), the cell
                // geometry (FullCellMargin / UniformlyMarginXS, FullCellPadding / UniformlyPaddingXS,
                // FullCellBorderThickness / LineWidthBold, ColorFillSecondary), the week header (WeekHeaderHeight,
                // WeekHeaderPadding, UniformlyPaddingSM / UniformlyPaddingXXS), the header selects (YearControlWidth,
                // MonthControlWidth), ColorText and ColorTextDisabled (outside cells).
                return Full();
        }
    }

    /// <summary>Lunar rows that need their own state; null for rows shared with the Gregorian calendar.</summary>
    private static Scenario? LunarCalendarScenario(string token)
    {
        static LunarCalendar Lunar(bool fullscreen = true, CalendarMode mode = CalendarMode.Month, bool holidays = false)
        {
            var calendar = (LunarCalendar)CreateCalendar(fullscreen, lunar: true, mode: mode);
            if (holidays)
            {
                calendar.HolidayProvider = new FixedHolidayProvider(
                [
                    new LunarCalendarHoliday(s_calendarDate.AddDays(2), "Holiday", LunarCalendarHolidayKind.Holiday),
                    new LunarCalendarHoliday(s_calendarDate.AddDays(3), "Workday", LunarCalendarHolidayKind.Workday)
                ]);
            }

            return calendar;
        }

        Control Cell(Control c, int days) => CellInnerOf(CalendarCell(c, s_calendarDate.AddDays(days)));

        switch (token)
        {
            case "ItemActiveBg" or "ControlItemBgActive":
                // The full-screen year view selects a month; date cells retain the lunar primary-colour override.
                return new Scenario(static () => Lunar(fullscreen: true, mode: CalendarMode.Year), null,
                    new EffectOptions { Region = SelectedLunarMonthSurface });
            case "MiniDateCellSize" or "MiniContentHeight":
                return new Scenario(static () => Lunar(fullscreen: false));
            case "MiniMonthCellWidth":
                return new Scenario(static () => Lunar(fullscreen: false, mode: CalendarMode.Year));
            // Holiday and adjusted workday markers (the WorkdayMarkerColor row was masked before Task 18), compared on the
            // marker line of the cell.
            case "HolidayMarkerColor" or "ColorError" or "MarkerSize" or "MarkerMargin":
                return new Scenario(static () => Lunar(holidays: true), null,
                    new EffectOptions { Region = c => MarkerLineOf(Cell(c, 2)) });
            case "MarkerCornerRadius":
                // The marker is a dot (radius = half its size); a square marker shows the radius.
                return new Scenario(static () => Lunar(holidays: true), null,
                    new EffectOptions
                    {
                        Region  = c => Descendant<Avalonia.Controls.Border>(Cell(c, 2), "PART_Marker"),
                        Perturb = static _ => new CornerRadius(0)
                    });
            case "ColorTextDisabled":
                return new Scenario(static () => Lunar(), null, new EffectOptions { Region = static c => OutsideCalendarCell(c) });
            case "WorkdayMarkerColor":
                return new Scenario(static () => Lunar(holidays: true), null,
                    new EffectOptions { Region = c => MarkerLineOf(Cell(c, 3)) });
            case "WeekendTextColor":
                // 2026-07-18 is a Saturday.
                return new Scenario(static () => Lunar(), null, new EffectOptions { Region = c => Cell(c, 3) });
            case "SecondaryTextColor" or "SecondaryTextFontSize" or "SecondaryTextLineHeight" or "ColorTextTertiary":
                return new Scenario(static () => Lunar(), null, new EffectOptions { Region = c => Cell(c, 1) });
            case "RangeBarTopOffset":
                return new Scenario(static () => (LunarCalendar)CreateCalendar(lunar: true, rangeBars: true), null,
                    new EffectOptions { Region = static c => CalendarRangeBarsOf(c) });
            case "BorderRadiusLG":
                // The selected fullscreen lunar cell is a rounded primary block.
                return new Scenario(static () => Lunar(), null, new EffectOptions { Region = c => Cell(c, 0) });
            default:
                return null;
        }
    }

    private static Control MarkerLineOf(Control cell) => Descendant<Grid>(cell, "PART_SecondaryPresenter");

    private sealed class FixedHolidayProvider(IReadOnlyList<LunarCalendarHoliday> holidays) : ILunarCalendarHolidayProvider
    {
        public bool TryGetHolidays(CalendarDateRange visibleRange, CultureInfo culture,
                                   out IReadOnlyList<LunarCalendarHoliday> result)
        {
            result = holidays;
            return true;
        }
    }
}
