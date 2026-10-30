using System.Globalization;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class CalendarViewCellBuilderTests
{
    // 2026-07 锚点,周日为周首:7/1 是周三,网格起点应为 6/28(周日)
    [Fact]
    public void BuildDateCells_Returns42Cells_StartingAtGridStart()
    {
        var anchor = new DateTime(2026, 7, 15);
        var cells = BuildDateCells(
            anchor, new DateTime(2026, 7, 30), DayOfWeek.Sunday, null, null, null);

        cells.Count.ShouldBe(42);
        cells[0].Value.ShouldBe(new DateTime(2026, 6, 28));
        cells[^1].Value.ShouldBe(new DateTime(2026, 8, 8));
    }

    [Fact]
    public void BuildDateCells_MarksInViewAndDisplayText()
    {
        var anchor = new DateTime(2026, 7, 15);
        var cells = BuildDateCells(
            anchor, new DateTime(2026, 7, 30), DayOfWeek.Sunday, null, null, null);

        var jul1 = cells.First(c => c.Value == new DateTime(2026, 7, 1));
        jul1.IsInView.ShouldBeTrue();
        jul1.DisplayText.ShouldBe("01");
        cells.First(c => c.Value == new DateTime(2026, 6, 28)).IsInView.ShouldBeFalse();
    }

    [Fact]
    public void BuildDateCells_MarksTodayAndSelected()
    {
        var anchor = new DateTime(2026, 7, 30);
        var cells = BuildDateCells(
            anchor, new DateTime(2026, 7, 30), DayOfWeek.Sunday, null, null, null);

        var cell = cells.First(c => c.Value == new DateTime(2026, 7, 30));
        cell.IsToday.ShouldBeTrue();
        cell.IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void BuildDateCells_DisabledByValidRangeAndDisabledDate()
    {
        var anchor = new DateTime(2026, 7, 15);
        var cells = BuildDateCells(
            anchor, new DateTime(2026, 7, 30), DayOfWeek.Sunday,
            new DateTime(2026, 7, 10), new DateTime(2026, 7, 20),
            d => d == new DateTime(2026, 7, 15));

        cells.First(c => c.Value == new DateTime(2026, 7, 5)).IsDisabled.ShouldBeTrue();   // 范围外
        cells.First(c => c.Value == new DateTime(2026, 7, 10)).IsDisabled.ShouldBeFalse(); // 边界包含
        cells.First(c => c.Value == new DateTime(2026, 7, 20)).IsDisabled.ShouldBeFalse(); // 边界包含
        cells.First(c => c.Value == new DateTime(2026, 7, 15)).IsDisabled.ShouldBeTrue();  // DisabledDate
    }

    [Fact]
    public void BuildDateCells_MondayFirst_ShiftsGridStart()
    {
        var anchor = new DateTime(2026, 7, 15); // 7/1 周三,周一为首 → 起点 6/29(周一)
        var cells = BuildDateCells(
            anchor, new DateTime(2026, 7, 30), DayOfWeek.Monday, null, null, null);
        cells[0].Value.ShouldBe(new DateTime(2026, 6, 29));
    }

    [Fact]
    public void BuildWeekNumberCells_ReturnsSixNonFocusableCells()
    {
        var anchor = new DateTime(2026, 7, 15);
        var dateCells = BuildDateCells(
            anchor, new DateTime(2026, 7, 30), DayOfWeek.Monday, null, null, null);

        var weeks = BuildWeekNumberCells(
            dateCells, CultureInfo.InvariantCulture,
            CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        weeks.Count.ShouldBe(6);
        weeks.ShouldAllBe(w => w.Kind == DateViewerCellType.Week);
        weeks.ShouldAllBe(w => !w.IsFocusable && !w.IsSelected && !w.IsDisabled);
        // 第一行起点 2026-06-29,ISO 周序号 27
        weeks[0].DisplayText.ShouldBe("27");
    }

    [Fact]
    public void BuildWeekNumberCells_CopiesRowStartDisabledState()
    {
        var dateCells = BuildDateCells(
            new DateTime(2026, 7, 15), new DateTime(2026, 7, 30), DayOfWeek.Monday,
            new DateTime(2026, 6, 30), null, null);

        var weeks = BuildWeekNumberCells(
            dateCells, CultureInfo.InvariantCulture,
            CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        weeks[0].Value.ShouldBe(new DateTime(2026, 6, 29));
        weeks[0].IsDisabled.ShouldBeTrue();
        weeks[1].IsDisabled.ShouldBeFalse();
    }

    [Fact]
    public void BuildMonthCells_Returns12Cells_WithDayTruncation()
    {
        // anchor 1/31 → February 截断到当年 2 月末
        var anchor = new DateTime(2028, 1, 31); // 闰年
        var cells = BuildMonthCells(
            anchor, new DateTime(2028, 1, 31), CultureInfo.InvariantCulture, null, null, null);

        cells.Count.ShouldBe(12);
        cells[1].Value.ShouldBe(new DateTime(2028, 2, 1));
        AssertFebruaryActivation(anchor, new DateTime(2028, 2, 29)); // 闰年 2 月末
        cells[0].IsSelected.ShouldBeTrue();                 // anchor 所在 1 月
    }

    [Fact]
    public void BuildMonthCells_NonLeapFebruaryTruncatesTo28()
    {
        var anchor = new DateTime(2027, 1, 31);
        var cells = BuildMonthCells(
            anchor, new DateTime(2027, 1, 31), CultureInfo.InvariantCulture, null, null, null);
        cells[1].Value.ShouldBe(new DateTime(2027, 2, 1));
        AssertFebruaryActivation(anchor, new DateTime(2027, 2, 28));
    }

    [Fact]
    public void BuildMonthCells_DisabledWhenMonthOutsideValidRange()
    {
        var anchor = new DateTime(2026, 6, 15);
        // 有效范围只覆盖 2026 年 5~7 月
        var cells = BuildMonthCells(
            anchor, new DateTime(2026, 6, 15), CultureInfo.InvariantCulture,
            new DateTime(2026, 5, 1), new DateTime(2026, 7, 31), null);

        cells[0].IsDisabled.ShouldBeTrue();  // 1 月无交集
        cells[4].IsDisabled.ShouldBeFalse(); // 5 月相交
        cells[11].IsDisabled.ShouldBeTrue(); // 12 月无交集
    }

    [Fact]
    public void BuildMonthCells_DisabledDateOnAnchorDay_DoesNotDisableEntireMonth()
    {
        var anchor = new DateTime(2026, 6, 15);
        var cells = BuildMonthCells(
            anchor, new DateTime(2026, 6, 15), CultureInfo.InvariantCulture,
            null, null, d => d.Day == 15);

        cells.ShouldAllBe(cell => !cell.IsDisabled);
    }

    [Fact]
    public void BuildDateCells_JanuaryAnchor_IncludesPreviousDecemberCells()
    {
        // 锚点 2026-01-05,周日为首:2026-01-01 是周四,网格起点回退到 2025-12-28(周日)
        var anchor = new DateTime(2026, 1, 5);
        var cells = BuildDateCells(
            anchor, new DateTime(2026, 1, 5), DayOfWeek.Sunday, null, null, null);

        cells.Count.ShouldBe(42);
        cells[0].Value.ShouldBe(new DateTime(2025, 12, 28));
        cells[0].IsInView.ShouldBeFalse();                              // 上年 12 月补位
        cells.First(c => c.Value == new DateTime(2026, 1, 1)).IsInView.ShouldBeTrue();
    }

    [Fact]
    public void BuildDateCells_NullValidRange_DoesNotDisableFarDates()
    {
        var anchor = new DateTime(2026, 7, 15);
        var cells = BuildDateCells(
            anchor, new DateTime(2026, 7, 30), DayOfWeek.Sunday, null, null, null);

        cells.ShouldAllBe(c => !c.IsDisabled); // ValidRange=null 且无 DisabledDate 时全部可用
    }

    [Fact]
    public void BuildDateCells_DisabledDateException_Propagates()
    {
        var anchor = new DateTime(2026, 7, 15);
        Should.Throw<InvalidOperationException>(() =>
            BuildDateCells(
                anchor, new DateTime(2026, 7, 30), DayOfWeek.Sunday, null, null,
                _ => throw new InvalidOperationException("boom")));
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(9999, 12, 31)]
    public void BuildDateCells_DateTimeBoundary_Returns42ValidCells(int year, int month, int day)
    {
        var anchor = new DateTime(year, month, day);

        var cells = BuildDateCells(
            anchor, anchor, DayOfWeek.Sunday, null, null, null);

        cells.Count.ShouldBe(42);
        cells.Where(cell => cell.Value.HasValue).Select(cell => cell.Value).Distinct().Count()
            .ShouldBe(cells.Count(cell => cell.Value.HasValue));
        cells.Where(cell => cell.Value is null).ShouldAllBe(cell => cell.IsDisabled && !cell.IsFocusable);
        cells.Any(cell => cell.Value == anchor).ShouldBeTrue();
    }
    private static IReadOnlyList<DateViewerCellModel> BuildDateCells(DateTime anchor, DateTime today,
        DayOfWeek firstDay, DateTime? min, DateTime? max, Func<DateTime, bool>? disabled) =>
        DatePanelAlgorithms.Build(new DatePanelInput
        {
            DisplayDate = anchor, SelectedDate = anchor, Today = today, FirstDayOfWeek = firstDay,
            MinDate = min, MaxDate = max, DisabledDate = disabled, DateNumberFormat = "D2",
            ConstraintMode = DatePanelConstraintMode.Calendar, Culture = CultureInfo.InvariantCulture
        }, 0).Cells;

    private static IReadOnlyList<DateViewerCellModel> BuildMonthCells(DateTime anchor, DateTime today,
        CultureInfo culture, DateTime? min, DateTime? max, Func<DateTime, bool>? disabled) =>
        DatePanelAlgorithms.Build(new DatePanelInput
        {
            DisplayDate = anchor, SelectedDate = anchor, Today = today, Culture = culture,
            PanelKind = DateViewerPanelKind.Month, SelectionUnit = DateViewerSelectionUnit.Month,
            MinDate = min, MaxDate = max, DisabledDate = disabled, ConstraintMode = DatePanelConstraintMode.Calendar
        }, 0).Cells;

    private static IReadOnlyList<DateViewerCellModel> BuildWeekNumberCells(IReadOnlyList<DateViewerCellModel> dates,
        CultureInfo culture, CalendarWeekRule rule, DayOfWeek firstDay)
    {
        culture = (CultureInfo)culture.Clone();
        culture.DateTimeFormat.CalendarWeekRule = rule;
        return DatePanelAlgorithms.Build(new DatePanelInput
        {
            DisplayDate = dates.First(cell => cell.IsInView).Value!.Value, Culture = culture,
            FirstDayOfWeek = firstDay, ShowWeek = true, WeekNumbering = DateWeekNumbering.Culture,
            ConstraintMode = DatePanelConstraintMode.Calendar,
            DisabledDate = value => dates.Any(cell => cell.Value == value && cell.IsDisabled)
        }, 0).Cells.Where(cell => cell.Kind == DateViewerCellType.Week).ToArray();
    }

    private static void AssertFebruaryActivation(DateTime anchor, DateTime expected)
    {
        AvaloniaTestApp.EnsureInitialized();
        var calendar = new AtomUI.Desktop.Controls.Calendar { Value = anchor, Mode = CalendarMode.Year };
        new DatePanelSession(calendar).Apply(new DatePanelAction.Activate(new DateTime(anchor.Year, 2, 1), DateViewerCellType.Month));
        calendar.Value.ShouldBe(expected);
    }

}
