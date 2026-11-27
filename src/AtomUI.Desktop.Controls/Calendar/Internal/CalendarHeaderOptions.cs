namespace AtomUI.Desktop.Controls.Internal.Calendar;

/// <summary>
/// Computes the year and month options of the default <see cref="CalendarHeader" />.
/// It is pure logic and depends on no control.
/// </summary>
internal static class CalendarHeaderOptions
{
    /// <summary>
    /// Builds the years offered by the year Select of the default header, ascending and inclusive at both ends.
    /// With both range years the list runs from <c>rangeStartYear</c> to <c>rangeEndYear</c>; otherwise from
    /// <c>currentYear - 10</c> to <c>currentYear + 9</c>, clamped to the years that <see cref="DateTime" /> supports.
    /// </summary>
    public static IReadOnlyList<int> BuildYearOptions(int currentYear, int? rangeStartYear, int? rangeEndYear)
    {
        int first, last;
        if (rangeStartYear is { } s && rangeEndYear is { } e)
        {
            first = s;
            last = e;
        }
        else
        {
            first = Math.Max(DateTime.MinValue.Year, currentYear - 10);
            last = Math.Min(DateTime.MaxValue.Year, currentYear + 9);
        }

        var years = new List<int>(Math.Max(0, last - first + 1));
        for (var y = first; y <= last; y++)
        {
            years.Add(y);
        }

        return years;
    }

    /// <summary>
    /// Builds the months (1..12) offered by the month Select of the default header for one year, ascending and
    /// inclusive at both ends. In the first or the last year of the valid range, only the months inside the range
    /// are returned; a year outside the range yields an empty list.
    /// </summary>
    public static IReadOnlyList<int> BuildMonthOptions(int year, DateTime? rangeStart, DateTime? rangeEnd)
    {
        var firstMonth = 1;
        var lastMonth = 12;

        if (rangeStart is { } s && s.Year == year)
        {
            firstMonth = Math.Max(firstMonth, s.Month);
        }

        if (rangeStart is { } s2 && year < s2.Year)
        {
            return Array.Empty<int>();
        }

        if (rangeEnd is { } e && e.Year == year)
        {
            lastMonth = Math.Min(lastMonth, e.Month);
        }

        if (rangeEnd is { } e2 && year > e2.Year)
        {
            return Array.Empty<int>();
        }

        if (lastMonth < firstMonth)
        {
            return Array.Empty<int>();
        }

        var months = new List<int>(lastMonth - firstMonth + 1);
        for (var m = firstMonth; m <= lastMonth; m++)
        {
            months.Add(m);
        }

        return months;
    }

    /// <summary>
    /// Clamps a month to the months that <see cref="BuildMonthOptions" /> offers for one year, so that switching
    /// the year keeps the current month whenever that year allows it.
    /// Returns <c>desiredMonth</c> unchanged when the year offers no month.
    /// </summary>
    public static int ClampMonthToYear(int desiredMonth, int year, DateTime? rangeStart, DateTime? rangeEnd)
    {
        var months = BuildMonthOptions(year, rangeStart, rangeEnd);
        if (months.Count == 0)
        {
            return desiredMonth;
        }

        if (desiredMonth < months[0])
        {
            return months[0];
        }

        if (desiredMonth > months[^1])
        {
            return months[^1];
        }

        return desiredMonth;
    }
}
