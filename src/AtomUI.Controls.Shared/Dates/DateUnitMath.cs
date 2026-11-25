using System.Globalization;
using AtomUI.Desktop.Controls;

namespace AtomUI.Controls;

/// <summary>Gregorian unit boundaries and culture-aware week numbering.</summary>
internal static class DateUnitMath
{
    internal static int DaysInMonth(int year, int month) => DateTime.DaysInMonth(year, month);

    internal static DateTime GetMonthStart(DateTime value) => new(value.Year, value.Month, 1);

    internal static int GetQuarter(DateTime value) => (value.Month - 1) / 3 + 1;

    internal static DateTime GetQuarterStart(DateTime value) => new(value.Year, (GetQuarter(value) - 1) * 3 + 1, 1);

    internal static DayOfWeek GetWeekFirstDay(CultureInfo culture) => IsMainlandChinese(culture)
        ? DayOfWeek.Monday
        : UsesSundayFirstWeek(culture) ? DayOfWeek.Sunday : culture.DateTimeFormat.FirstDayOfWeek;

    internal static int DayOffset(DayOfWeek date, DayOfWeek firstDayOfWeek) => ((int)date - (int)firstDayOfWeek + 7) % 7;

    internal static DateTime GetWeekStart(DateTime value, DayOfWeek firstDayOfWeek) => GetWeekRange(value, firstDayOfWeek).Start;

    internal static (int Year, int Week) GetIsoWeek(DateTime value) => (ISOWeek.GetYear(value), ISOWeek.GetWeekOfYear(value));

    internal static (int Year, int Week) GetWeekIdentity(DateTime value, CultureInfo culture, DayOfWeek firstDay)
    {
        var rule = IsMainlandChinese(culture) ? CalendarWeekRule.FirstFourDayWeek :
            UsesSundayFirstWeek(culture) ? CalendarWeekRule.FirstDay : culture.DateTimeFormat.CalendarWeekRule;
        var referenceDay = rule == CalendarWeekRule.FirstFourDayWeek ? 4 : rule == CalendarWeekRule.FirstFullWeek ? 7 : 1;
        DateTime FirstWeek(int year) => GetWeekStart(new DateTime(year, 1, referenceDay), firstDay);
        var start = GetWeekStart(value, firstDay);
        var year = value.Year;
        if (year < 9999 && start >= FirstWeek(year + 1))
        {
            year++;
        }
        else if (year > 1 && start < FirstWeek(year))
        {
            year--;
        }

        return (year, (int)((start - FirstWeek(year)).TotalDays / 7) + 1);
    }

    internal static DateTime Normalize(DateTime value, DateViewerSelectionUnit unit, DayOfWeek firstDayOfWeek) =>
        GetUnitRange(value, unit, firstDayOfWeek).Start;

    internal static bool IsSameUnit(DateTime first, DateTime second, DateViewerSelectionUnit unit, DayOfWeek firstDayOfWeek) =>
        Normalize(first, unit, firstDayOfWeek) == Normalize(second, unit, firstDayOfWeek);

    internal static DateUnitRange GetUnitRange(DateTime value, DateViewerSelectionUnit unit, DayOfWeek firstDayOfWeek)
    {
        value = value.Date;
        return unit switch
        {
            DateViewerSelectionUnit.Date => new DateUnitRange(value, value),
            DateViewerSelectionUnit.Week => GetWeekRange(value, firstDayOfWeek),
            DateViewerSelectionUnit.Month => new DateUnitRange(GetMonthStart(value), new DateTime(value.Year, value.Month, DaysInMonth(value.Year, value.Month))),
            DateViewerSelectionUnit.Quarter => GetQuarterRange(value),
            DateViewerSelectionUnit.Year => new DateUnitRange(new DateTime(value.Year, 1, 1), new DateTime(value.Year, 12, 31)),
            _ => throw new ArgumentOutOfRangeException(nameof(unit))
        };
    }

    internal static DateUnitRange GetWeekRange(DateTime value, DayOfWeek firstDayOfWeek)
    {
        var offset = DayOffset(value.DayOfWeek, firstDayOfWeek);
        var start = value.Date.Ticks - offset * TimeSpan.TicksPerDay;
        return new DateUnitRange(new DateTime(Math.Max(0, start)),
            new DateTime(Math.Min(DateTime.MaxValue.Date.Ticks, start + 6 * TimeSpan.TicksPerDay)));
    }

    private static DateUnitRange GetQuarterRange(DateTime value)
    {
        var start = GetQuarterStart(value);
        var lastMonth = start.Month + 2;
        return new DateUnitRange(start, new DateTime(value.Year, lastMonth, DaysInMonth(value.Year, lastMonth)));
    }

    private static bool IsMainlandChinese(CultureInfo culture) => culture.TwoLetterISOLanguageName == "zh" &&
        (culture.Name.EndsWith("-CN", StringComparison.OrdinalIgnoreCase) || culture.Name.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase));

    private static bool UsesSundayFirstWeek(CultureInfo culture) =>
        culture.Name.EndsWith("-US", StringComparison.OrdinalIgnoreCase) ||
        culture.Name.EndsWith("-TW", StringComparison.OrdinalIgnoreCase) ||
        culture.Name.EndsWith("-BR", StringComparison.OrdinalIgnoreCase);
}

internal readonly record struct DateUnitRange(DateTime Start, DateTime End)
{
    public bool Contains(DateTime value) => value.Date >= Start && value.Date <= End;
}
