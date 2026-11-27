namespace AtomUI.Desktop.Controls;

/// <summary>
/// The data context of the cells of a <see cref="LunarCalendar" />: the <see cref="CalendarCellContext" /> of
/// the cell plus the lunar text and the holiday state it shows.
/// </summary>
public sealed record LunarCalendarCellContext : CalendarCellContext
{
    /// <summary>
    /// Creates the context of one cell of a lunar calendar. The first eight arguments are passed to
    /// <see cref="CalendarCellContext" /> unchanged. <paramref name="lunarMonths" /> is copied into a read-only
    /// list, and a <see langword="null" /> <paramref name="secondaryText" /> is stored as
    /// <see cref="string.Empty" />.
    /// </summary>
    public LunarCalendarCellContext(
        DateTime value,
        DateTime today,
        DateViewerCellType cellType,
        string displayValue,
        bool isToday,
        bool isInView,
        bool isSelected,
        bool isDisabled,
        LunarCalendarDateInfo? lunarDateInfo,
        IReadOnlyList<LunarCalendarMonthInfo> lunarMonths,
        LunarCalendarHoliday? holiday,
        string secondaryText,
        LunarCalendarSecondaryContentKind secondaryContentKind,
        bool isWeekend,
        bool isHoliday,
        bool isAdjustedWorkday)
        : base(value, today, cellType, displayValue, isToday, isInView, isSelected, isDisabled)
    {
        LunarDateInfo = lunarDateInfo;
        LunarMonths = Array.AsReadOnly(lunarMonths.ToArray());
        Holiday = holiday;
        SecondaryText = secondaryText ?? string.Empty;
        SecondaryContentKind = secondaryContentKind;
        IsWeekend = isWeekend;
        IsHoliday = isHoliday;
        IsAdjustedWorkday = isAdjustedWorkday;
    }

    /// <summary>
    /// The lunar information of the date of a date cell, or <see langword="null" /> for a month cell or a date
    /// outside <see cref="LunarCalendar.SupportedRange" />.
    /// </summary>
    public LunarCalendarDateInfo? LunarDateInfo { get; }
    /// <summary>
    /// The lunar months that intersect the solar month a month cell stands for, as a read-only copy; empty for a
    /// date cell.
    /// </summary>
    public IReadOnlyList<LunarCalendarMonthInfo> LunarMonths { get; }
    /// <summary>
    /// The holiday or adjusted workday that <see cref="LunarCalendar.HolidayProvider" /> reported for the date of a
    /// date cell, or <see langword="null" />. It is set only while <see cref="LunarCalendar.ShowHolidays" /> is
    /// <see langword="true" />, and only for dates inside <see cref="LunarCalendar.SupportedRange" />.
    /// </summary>
    public LunarCalendarHoliday? Holiday { get; }
    /// <summary>
    /// The second line of the cell. A month cell shows the range of its lunar months. A date cell shows the first
    /// of these that applies and is enabled: the holiday or adjusted-workday name, the first traditional festival,
    /// the solar term, the lunar day name. The value is never <see langword="null" />; it is empty for a date
    /// outside <see cref="LunarCalendar.SupportedRange" />.
    /// </summary>
    public string SecondaryText { get; }
    /// <summary>
    /// What <see cref="SecondaryText" /> names, for styling each kind differently. For a month cell and for the
    /// lunar day name it is <see cref="LunarCalendarSecondaryContentKind.LunarDay" />.
    /// </summary>
    public LunarCalendarSecondaryContentKind SecondaryContentKind { get; }
    /// <summary>
    /// Whether the cell is a date cell on a Saturday or Sunday that is not an adjusted workday. It drives the
    /// weekend state of the cell content while <see cref="LunarCalendar.HighlightWeekends" /> is
    /// <see langword="true" />.
    /// </summary>
    public bool IsWeekend { get; }
    /// <summary>
    /// Whether <see cref="Holiday" /> is a day off. The cell content then shows the holiday marker and gets the
    /// <c>:holiday</c> state.
    /// </summary>
    public bool IsHoliday { get; }
    /// <summary>
    /// Whether <see cref="Holiday" /> marks the date as an adjusted workday. The cell content then shows the
    /// holiday marker, gets the <c>:workday</c> state, and <see cref="IsWeekend" /> is <see langword="false" />.
    /// </summary>
    public bool IsAdjustedWorkday { get; }
}
