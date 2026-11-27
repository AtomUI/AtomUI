namespace AtomUI.Desktop.Controls;

/// <summary>
/// An inclusive range of dates: both the first and the last day belong to it.
/// <see cref="Calendar.ValidRange" /> limits the selectable dates with it, and
/// <see cref="LunarCalendar.SupportedRange" /> states the dates a LunarCalendar supports.
/// </summary>
public sealed class CalendarDateRange
{
    /// <summary>
    /// Creates a range from the first to the last day, both included. Any time of day is dropped from both dates.
    /// </summary>
    /// <param name="start">
    /// The first day of the range.
    /// </param>
    /// <param name="end">
    /// The last day of the range. It must not be earlier than <paramref name="start" />.
    /// </param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// <paramref name="end" /> is earlier than <paramref name="start" />.
    /// </exception>
    public CalendarDateRange(DateTime start, DateTime end)
    {
        var s = start.Date;
        var e = end.Date;
        if (e < s)
        {
            throw new ArgumentOutOfRangeException(nameof(end), "range end must not be earlier than start");
        }

        Start = s;
        End = e;
    }

    /// <summary>
    /// The first day of the range, normalized to <see cref="DateTime.Date" />.
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// The last day of the range, inclusive, normalized to <see cref="DateTime.Date" />.
    /// </summary>
    public DateTime End { get; }
}
