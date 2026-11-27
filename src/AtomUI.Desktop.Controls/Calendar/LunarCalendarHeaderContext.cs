using System.Windows.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The data context that a <see cref="LunarCalendar" /> provides to the custom header template set through
/// <see cref="Calendar.HeaderTemplate" />. It adds the lunar information of the value, the supported range and the
/// lunar year and month texts to <see cref="CalendarHeaderContext" />.
/// </summary>
public sealed class LunarCalendarHeaderContext : CalendarHeaderContext
{
    /// <summary>
    /// Creates the context for the current state of the calendar and the two commands of its custom header.
    /// </summary>
    /// <param name="value">
    /// The calendar value that the header shows.
    /// </param>
    /// <param name="mode">
    /// The calendar mode that the header shows.
    /// </param>
    /// <param name="changeValueCommand">
    /// The command that becomes <see cref="CalendarHeaderContext.ChangeValueCommand" />.
    /// </param>
    /// <param name="changeModeCommand">
    /// The command that becomes <see cref="CalendarHeaderContext.ChangeModeCommand" />.
    /// </param>
    /// <param name="lunarDateInfo">
    /// The lunar information of the date of <paramref name="value" />. It becomes <see cref="LunarDateInfo" />.
    /// </param>
    /// <param name="supportedRange">
    /// The dates the lunar calendar supports. It becomes <see cref="SupportedRange" />.
    /// </param>
    /// <param name="lunarYearText">
    /// The lunar year text of the year of <paramref name="value" />. It becomes <see cref="LunarYearText" />.
    /// </param>
    /// <param name="lunarMonthRangeText">
    /// The text of the lunar months of the month of <paramref name="value" />. It becomes
    /// <see cref="LunarMonthRangeText" />.
    /// </param>
    public LunarCalendarHeaderContext(
        DateTime value,
        CalendarMode mode,
        ICommand changeValueCommand,
        ICommand changeModeCommand,
        LunarCalendarDateInfo lunarDateInfo,
        CalendarDateRange supportedRange,
        string lunarYearText,
        string lunarMonthRangeText)
        : base(value, mode, changeValueCommand, changeModeCommand)
    {
        LunarDateInfo = lunarDateInfo;
        SupportedRange = supportedRange;
        LunarYearText = lunarYearText;
        LunarMonthRangeText = lunarMonthRangeText;
    }

    /// <summary>
    /// The lunar information of the date of <see cref="CalendarHeaderContext.Value" />.
    /// </summary>
    public LunarCalendarDateInfo LunarDateInfo { get; }
    /// <summary>
    /// The dates the lunar calendar supports. The calendar passes <see cref="LunarCalendar.SupportedRange" />.
    /// </summary>
    public CalendarDateRange SupportedRange { get; }
    /// <summary>
    /// The text of the year of <see cref="CalendarHeaderContext.Value" />: the year number and, in parentheses, the
    /// stem-branch and the zodiac of that lunar year.
    /// </summary>
    public string LunarYearText { get; }
    /// <summary>
    /// The text of the lunar months that intersect the month of <see cref="CalendarHeaderContext.Value" />. One
    /// month is named alone; several are given as the first and the last one, joined with a hyphen.
    /// </summary>
    public string LunarMonthRangeText { get; }
}
