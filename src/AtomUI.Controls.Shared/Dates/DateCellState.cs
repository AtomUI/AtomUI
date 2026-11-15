namespace AtomUI.Controls;

internal readonly record struct DateMonthLayout(int LeadingDays, int DaysInMonth, int Rows);

internal readonly record struct DateCellStateInput(
    DateOnly Today,
    DayOfWeek FirstDayOfWeek,
    DateOnly? MinDate = null,
    DateOnly? MaxDate = null,
    DateOnly? SelectedDate = null,
    DateOnly? RangeStart = null,
    DateOnly? RangeEnd = null,
    Func<DateOnly, bool>? DisabledDate = null);

[Flags]
internal enum DateCellState
{
    None = 0,
    Today = 1 << 0,
    Disabled = 1 << 1,
    Selected = 1 << 2,
    RangeStart = 1 << 3,
    RangeMiddle = 1 << 4,
    RangeEnd = 1 << 5,
    RowStart = 1 << 6,
    RowEnd = 1 << 7
}
