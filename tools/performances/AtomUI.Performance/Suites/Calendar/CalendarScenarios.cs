using Avalonia.Controls;
using AtomCalendar = AtomUI.Desktop.Controls.Calendar;
using AtomCalendarDateRange = AtomUI.Desktop.Controls.CalendarDateRange;
using AtomCalendarMode = AtomUI.Desktop.Controls.CalendarMode;

namespace AtomUI.Performance;

internal static partial class Program
{
    // Calendar API 演进：DisplayDate/DisplayMode/SelectionMode/SelectedDates/BlackoutDates 已移除，
    // 由 Value/Mode/ValidRange/DisabledDate/RangeBars 取代；CalendarMode 仅剩 Month/Year。
    private static IReadOnlyList<PerfScenario> CreateCalendarScenarios()
    {
        return
        [
            new PerfScenario("Calendar.Default", _ => CreateCalendar()),
            new PerfScenario("Calendar.SingleDate.Selected", _ => CreateCalendar(
                selectedDate: new DateTime(2024, 1, 20))),
            new PerfScenario("Calendar.ShowWeek", _ => CreateCalendar(showWeek: true)),
            new PerfScenario("Calendar.YearMode", _ => CreateCalendar(mode: AtomCalendarMode.Year)),
            new PerfScenario("Calendar.DisabledDate", _ => CreateCalendarWithDisabledDates()),
            new PerfScenario("Calendar.RangeRestricted", _ => CreateCalendar(
                validRange: new AtomCalendarDateRange(new DateTime(2024, 1, 10), new DateTime(2024, 3, 20)))),
            new PerfScenario("Calendar.RangeBars", _ => CreateCalendarWithRangeBars()),
            new PerfScenario("Calendar.Mini", _ => CreateCalendar(fullscreen: false)),
            new PerfScenario("Calendar.Batch4", _ => CreateCalendarBatch())
        ];
    }

    private static AtomCalendar CreateCalendar(
        AtomCalendarMode mode = AtomCalendarMode.Month,
        DateTime? selectedDate = null,
        bool showWeek = false,
        bool fullscreen = true,
        AtomCalendarDateRange? validRange = null)
    {
        var calendar = new AtomCalendar
        {
            Mode        = mode,
            ShowWeek    = showWeek,
            Fullscreen  = fullscreen,
            ValidRange  = validRange
        };

        if (selectedDate.HasValue)
        {
            calendar.Value = selectedDate.Value;
        }

        return calendar;
    }

    private static AtomCalendar CreateCalendarWithDisabledDates()
    {
        var calendar = CreateCalendar();
        calendar.DisabledDate = date => date.Day is 5 or 6 or 7;
        return calendar;
    }

    private static AtomCalendar CreateCalendarWithRangeBars()
    {
        var calendar = CreateCalendar();
        calendar.RangeBars.Add(new AtomUI.Desktop.Controls.CalendarRangeBar
        {
            StartDate = new DateTime(2024, 1, 12),
            EndDate   = new DateTime(2024, 1, 20),
            Label     = "Range"
        });
        return calendar;
    }

    private static Control CreateCalendarBatch()
    {
        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                CreateCalendar(),
                CreateCalendar(selectedDate: new DateTime(2024, 1, 20)),
                CreateCalendar(mode: AtomCalendarMode.Year),
                CreateCalendar(showWeek: true)
            }
        };
    }
}
