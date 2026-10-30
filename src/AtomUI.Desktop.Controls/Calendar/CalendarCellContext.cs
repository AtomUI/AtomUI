namespace AtomUI.Desktop.Controls;

/// <summary>
/// Cell 模板（<see cref="Calendar.CellTemplate"/> / <see cref="Calendar.FullCellTemplate"/>）的数据上下文。
/// </summary>
public record CalendarCellContext : DateViewerCellContext
{
    public CalendarCellContext(DateTime value, DateTime today, DateViewerCellType cellType, string displayValue,
        bool isToday, bool isInView, bool isSelected, bool isDisabled, bool isFocused = false)
        : base(value, today, cellType,
            displayValue, isToday, isInView, isSelected, isDisabled, isFocused, false, false, false, false, false, false)
    {
    }
}
