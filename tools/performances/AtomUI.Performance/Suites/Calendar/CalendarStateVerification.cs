using System.Reflection;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AtomCalendar = AtomUI.Desktop.Controls.Calendar;
using AtomCalendarMode = AtomUI.Desktop.Controls.CalendarMode;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunCalendarStateVerification()
    {
        var failures = new List<string>();
        VerifyCalendarMonthGridShape(failures);
        VerifyCalendarModeSwitching(failures);
        VerifyCalendarValueSelection(failures);
        VerifyCalendarDisabledDates(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Calendar state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Calendar state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyCalendarMonthGridShape(ICollection<string> failures)
    {
        var calendar = CreateCalendar(selectedDate: new DateTime(2024, 1, 20));

        using var realized = RealizeControl(calendar);
        ExpectCalendarShape(calendar, dateCells: 42, monthCells: 0, weekCells: 0, "Month Calendar", failures);

        var showWeekCalendar = CreateCalendar(selectedDate: new DateTime(2024, 1, 20), showWeek: true);
        using var weekRealized = RealizeControl(showWeekCalendar);
        ExpectCalendarShape(showWeekCalendar, dateCells: 42, monthCells: 0, weekCells: 6, "ShowWeek Calendar", failures);
    }

    private static void VerifyCalendarModeSwitching(ICollection<string> failures)
    {
        var selectedDate = new DateTime(2024, 1, 20);
        var calendar = CreateCalendar(selectedDate: selectedDate);

        using var realized = RealizeControl(calendar);
        ExpectCalendarShape(calendar, dateCells: 42, monthCells: 0, weekCells: 0, "Default Month Calendar", failures);

        calendar.Mode = AtomCalendarMode.Year;
        RefreshLayout(realized.Window);
        ExpectCalendarShape(calendar, dateCells: 0, monthCells: 12, weekCells: 0, "Month -> Year Calendar", failures);

        calendar.Mode = AtomCalendarMode.Month;
        RefreshLayout(realized.Window);
        ExpectCalendarShape(calendar, dateCells: 42, monthCells: 0, weekCells: 0, "Year -> Month Calendar", failures);
        Expect(calendar.Value == selectedDate,
            $"Calendar Value should survive mode switches, actual {calendar.Value:yyyy-MM-dd}.",
            failures);
    }

    private static void VerifyCalendarValueSelection(ICollection<string> failures)
    {
        var selectedDate = new DateTime(2024, 1, 20);
        var calendar = CreateCalendar(selectedDate: selectedDate);

        using var realized = RealizeControl(calendar);
        var selectedCells = FindCalendarCells(calendar, ":selected");
        Expect(selectedCells.Count == 1,
            $"Calendar should mark exactly one selected cell, actual {selectedCells.Count}.",
            failures);
        var selectedCellDate = GetCalendarCellValue(selectedCells.FirstOrDefault());
        Expect(selectedCellDate == selectedDate,
            $"Selected cell should carry {selectedDate:yyyy-MM-dd}, actual {selectedCellDate:yyyy-MM-dd}.",
            failures);

        calendar.Value = new DateTime(2024, 1, 25);
        RefreshLayout(realized.Window);
        selectedCells = FindCalendarCells(calendar, ":selected");
        Expect(selectedCells.Count == 1 &&
               GetCalendarCellValue(selectedCells.FirstOrDefault()) == new DateTime(2024, 1, 25),
            "Calendar Value change should move the selected cell.",
            failures);
    }

    private static void VerifyCalendarDisabledDates(ICollection<string> failures)
    {
        var calendar = CreateCalendarWithDisabledDates();

        using var realized = RealizeControl(calendar);
        var disabledCells = FindCalendarCells(calendar, ":disabled");
        Expect(disabledCells.Count == 3,
            $"Calendar should mark the three DisabledDate days as disabled, actual {disabledCells.Count}.",
            failures);
        Expect(disabledCells.All(cell => GetCalendarCellValue(cell).Day is 5 or 6 or 7),
            "Disabled cells should carry the DisabledDate days.",
            failures);
    }

    private static void ExpectCalendarShape(
        AtomCalendar calendar,
        int dateCells,
        int monthCells,
        int weekCells,
        string label,
        ICollection<string> failures)
    {
        var actualDateCells   = CountCalendarCells(calendar, ":date");
        var actualMonthCells  = CountCalendarCells(calendar, ":month");
        var actualWeekCells   = CountCalendarCells(calendar, ":week");

        Expect(actualDateCells == dateCells,
            $"{label} should have {dateCells} date cells, actual {actualDateCells}.",
            failures);
        Expect(actualMonthCells == monthCells,
            $"{label} should have {monthCells} month cells, actual {actualMonthCells}.",
            failures);
        Expect(actualWeekCells == weekCells,
            $"{label} should have {weekCells} week cells, actual {actualWeekCells}.",
            failures);
    }

    private static int CountCalendarCells(Control root, string pseudoClass)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<Control>()
                   .Count(control => control.GetType().Name == "CalendarViewCell" &&
                                     control.Classes.Contains(pseudoClass));
    }

    private static List<Control> FindCalendarCells(Control root, string pseudoClass)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<Control>()
                   .Where(control => control.GetType().Name == "CalendarViewCell" &&
                                     control.Classes.Contains(pseudoClass))
                   .ToList();
    }

    private static DateTime GetCalendarCellValue(Control? cell)
    {
        if (cell is null)
        {
            return default;
        }
        var model = cell.GetType()
                        .GetProperty("Model", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?.GetValue(cell);
        var value = model?.GetType()
                          .GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)
                          ?.GetValue(model);
        return value is DateTime dateTime ? dateTime : default;
    }
}
