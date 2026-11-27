using System.Globalization;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The source of the holidays and adjusted workdays that a <see cref="LunarCalendar" /> marks on its date cells.
/// Assign an implementation to <see cref="LunarCalendar.HolidayProvider" />; the calendar asks for data only while
/// <see cref="LunarCalendar.ShowHolidays" /> is <see langword="true" />.
/// </summary>
public interface ILunarCalendarHolidayProvider
{
    /// <summary>
    /// Gets the holidays and adjusted workdays to mark on the dates of one date panel.
    /// </summary>
    /// <param name="visibleRange">
    /// The inclusive range from the first to the last date of the panel that lies inside
    /// <see cref="LunarCalendar.SupportedRange" />.
    /// </param>
    /// <param name="culture">
    /// The culture of the calendar. Use it to localize the names.
    /// </param>
    /// <param name="holidays">
    /// The entries to mark when this method returns <see langword="true" />; it must not be <see langword="null" />.
    /// Entries outside <paramref name="visibleRange" /> are ignored. Of several entries for one date, only the last
    /// one is used. When this method returns <see langword="false" />, this argument is not read.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the entries of <paramref name="holidays" /> are marked;
    /// <see langword="false" /> when no date of the panel is marked.
    /// </returns>
    /// <remarks>
    /// The calendar keeps the result for the panel it shows. It asks again when the panel, the culture, the provider
    /// instance or the options of the calendar change. When the data of a provider changes while it stays assigned,
    /// call <see cref="LunarCalendar.RefreshHolidayData" /> to make the calendar ask again.
    /// </remarks>
    bool TryGetHolidays(
        CalendarDateRange visibleRange,
        CultureInfo culture,
        out IReadOnlyList<LunarCalendarHoliday> holidays);
}

/// <summary>
/// A holiday or an adjusted workday on one date, as an <see cref="ILunarCalendarHolidayProvider" /> reports it.
/// </summary>
public sealed record LunarCalendarHoliday
{
    /// <summary>
    /// Creates an entry that marks a date as a holiday or as an adjusted workday.
    /// </summary>
    /// <param name="date">
    /// The date the entry belongs to. Its time of day is dropped.
    /// </param>
    /// <param name="name">
    /// The name of the holiday or adjusted workday. <see langword="null" /> becomes an empty string.
    /// </param>
    /// <param name="kind">
    /// Whether the date is a holiday or an adjusted workday.
    /// </param>
    public LunarCalendarHoliday(DateTime date, string name, LunarCalendarHolidayKind kind)
    {
        Date = date.Date;
        Name = name ?? string.Empty;
        Kind = kind;
    }

    /// <summary>
    /// The date the entry belongs to, without a time of day.
    /// </summary>
    public DateTime Date { get; }
    /// <summary>
    /// The name of the holiday or adjusted workday, or an empty string when the entry was created with
    /// <see langword="null" />. While holidays are shown, a date cell uses it as its second line.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Whether the date is a holiday or an adjusted workday. It selects the marker color of the date cell:
    /// <see cref="LunarCalendarToken.HolidayMarkerColor" /> or <see cref="LunarCalendarToken.WorkdayMarkerColor" />.
    /// A Saturday or Sunday marked as a workday is not highlighted as a weekend.
    /// </summary>
    public LunarCalendarHolidayKind Kind { get; }
}
