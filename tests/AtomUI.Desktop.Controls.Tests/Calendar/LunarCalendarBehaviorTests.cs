using System.Reflection;
using System.Globalization;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Internal.Calendar;
using Shouldly;
using Xunit;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using LunarCalendarControl = AtomUI.Desktop.Controls.LunarCalendar;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class LunarCalendarBehaviorTests
{
    static LunarCalendarBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void DefaultsAndSupportedRange_MatchDesignContract()
    {
        var calendar = new LunarCalendarControl { Value = new DateTime(2024, 2, 10) };

        LunarCalendarControl.SupportedRange.Start.ShouldBe(new DateTime(1900, 1, 1));
        LunarCalendarControl.SupportedRange.End.ShouldBe(new DateTime(2100, 12, 31));
        calendar.ShowSolarTerms.ShouldBeTrue();
        calendar.ShowTraditionalFestivals.ShouldBeTrue();
        calendar.ShowHolidays.ShouldBeTrue();
        calendar.HighlightWeekends.ShouldBeTrue();
        calendar.HolidayProvider.ShouldBeNull();
        calendar.SelectedLunarDateInfo.LunarYear.ShouldBe(2024);
        calendar.SelectedLunarDateInfo.LunarMonth.ShouldBe(1);
        calendar.SelectedLunarDateInfo.LunarDay.ShouldBe(1);
    }

    [Theory]
    [InlineData(1800, 1, 1, 1900, 1, 1)]
    [InlineData(2200, 1, 1, 2100, 12, 31)]
    public void Value_IsNormalizedAndClampedToSupportedRange(
        int inputYear,
        int inputMonth,
        int inputDay,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(inputYear, inputMonth, inputDay, 18, 30, 0)
        };

        calendar.Value.ShouldBe(new DateTime(expectedYear, expectedMonth, expectedDay));
        calendar.SelectedLunarDateInfo.SolarDate.ShouldBe(calendar.Value);
    }

    [Fact]
    public void ProgrammaticValueChange_UpdatesProjectionWithoutRaisingUserEvents()
    {
        var calendar = new LunarCalendarControl { Value = new DateTime(2024, 2, 10) };
        var eventCount = 0;
        calendar.PanelChanged += (_, _) => eventCount++;
        calendar.ValueChanged += (_, _) => eventCount++;
        calendar.Selected += (_, _) => eventCount++;

        calendar.Value = new DateTime(2024, 9, 17);

        eventCount.ShouldBe(0);
        calendar.SelectedLunarDateInfo.LunarMonth.ShouldBe(8);
        calendar.SelectedLunarDateInfo.LunarDay.ShouldBe(15);
    }

    [Fact]
    public void EmptyValidRangeIntersection_DisablesAllBusinessCellsWithoutMutatingValidRange()
    {
        var requested = new CalendarDateRange(new DateTime(2200, 1, 1), new DateTime(2200, 12, 31));
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            ValidRange = requested
        };
        var session = new DatePanelSession(calendar);

        calendar.ValidRange.ShouldBeSameAs(requested);
        session.Models.SelectMany(panel => panel.Cells).Where(model => model.Kind != DateViewerCellType.Week)
            .ShouldAllBe(model => model.IsDisabled);
    }

    [Fact]
    public void HeaderContext_ExtendsCalendarCommandsWithLunarProjectionAndLabels()
    {
        var calendar = new LunarCalendarControl
        {
            Value = new DateTime(2024, 2, 10),
            Mode = CalendarMode.Month
        };

        var context = BuildHeaderContext(calendar).ShouldBeOfType<LunarCalendarHeaderContext>();

        context.Value.ShouldBe(calendar.Value);
        context.Mode.ShouldBe(calendar.Mode);
        context.LunarDateInfo.ShouldBe(calendar.SelectedLunarDateInfo);
        context.SupportedRange.ShouldBeSameAs(LunarCalendarControl.SupportedRange);
        context.LunarYearText.ShouldNotBeEmpty();
        context.LunarMonthRangeText.ShouldNotBeEmpty();
        context.ChangeValueCommand.CanExecute(new DateTime(2024, 9, 17)).ShouldBeTrue();
        context.ChangeModeCommand.CanExecute(CalendarMode.Year).ShouldBeTrue();
    }

    [Fact]
    public void HeaderOptions_LocalizeGregorianLabelsButKeepLunarTermsChinese()
    {
        var calendar = new LunarCalendarControl { Value = new DateTime(2024, 2, 10) };
        var adapter = GetAdapter(calendar);
        var culture = CultureInfo.GetCultureInfo("en-US");

        adapter.FormatYearOption(2024, culture).ShouldBe("2024 (甲辰龙年)");
        adapter.FormatMonthOption(2024, 2, culture).ShouldBe("Feb (腊月-正月)");
    }

    [Fact]
    public void PresentationMetrics_UseLunarMiniContentHeight()
    {
        var calendar = new LunarCalendarControl { Value = new DateTime(2024, 2, 10) };

        GetAdapter(calendar).Metrics.MiniContentHeightResourceKey
            .ShouldBe(LunarCalendarTokenKind.MiniContentHeight);
    }

    private static LunarCalendarPresentationAdapter GetAdapter(LunarCalendarControl calendar) =>
        (LunarCalendarPresentationAdapter)typeof(AtomUI.Desktop.Controls.Calendar)
            .GetField("_presentationAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(calendar)!;

    private static CalendarHeaderContext BuildHeaderContext(LunarCalendarControl calendar) =>
        (CalendarHeaderContext)typeof(AtomUI.Desktop.Controls.Calendar)
            .GetMethod("BuildHeaderContext", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(calendar, [])!;
}
