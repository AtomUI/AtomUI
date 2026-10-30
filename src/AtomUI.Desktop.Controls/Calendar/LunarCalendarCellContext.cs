namespace AtomUI.Desktop.Controls;

public sealed record LunarCalendarCellContext : CalendarCellContext
{
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

    public LunarCalendarDateInfo? LunarDateInfo { get; }
    public IReadOnlyList<LunarCalendarMonthInfo> LunarMonths { get; }
    public LunarCalendarHoliday? Holiday { get; }
    public string SecondaryText { get; }
    public LunarCalendarSecondaryContentKind SecondaryContentKind { get; }
    public bool IsWeekend { get; }
    public bool IsHoliday { get; }
    public bool IsAdjustedWorkday { get; }
}
