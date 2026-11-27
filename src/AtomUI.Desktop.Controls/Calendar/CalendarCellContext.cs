namespace AtomUI.Desktop.Controls;

/// <summary>
/// The data context of the cell templates <see cref="Calendar.CellTemplate" /> and
/// <see cref="Calendar.FullCellTemplate" />.
/// </summary>
public record CalendarCellContext : DateViewerCellContext
{
    /// <summary>
    /// Creates the context of one date or month cell. The range flags of <see cref="DateViewerCellContext" /> are
    /// <c>false</c>.
    /// </summary>
    public CalendarCellContext(DateTime value, DateTime today, DateViewerCellType cellType, string displayValue,
        bool isToday, bool isInView, bool isSelected, bool isDisabled, bool isFocused = false)
        : base(value, today, cellType,
            displayValue, isToday, isInView, isSelected, isDisabled, isFocused, false, false, false, false, false, false)
    {
    }
}
