namespace AtomUI.Desktop.Controls;

public sealed class DateViewerValueChangedEventArgs(DateTime? oldValue, DateTime? value) : EventArgs
{
    public DateTime? OldValue { get; } = oldValue;
    public DateTime? Value { get; } = value;
}

public sealed class RangeDateViewerValueChangedEventArgs(DateViewerRange? oldValue, DateViewerRange? value) : EventArgs
{
    public DateViewerRange? OldValue { get; } = oldValue;
    public DateViewerRange? Value { get; } = value;
}

public sealed class DateViewerSelectedEventArgs(DateTime value, DateViewerSelectionUnit unit) : EventArgs
{
    public DateTime Value { get; } = value;
    public DateViewerSelectionUnit SelectionUnit { get; } = unit;
}

public sealed class RangeDateViewerSelectedEventArgs(DateViewerRange value, DateTime selectedValue, DateViewerSelectionUnit unit) : EventArgs
{
    public DateViewerRange Value { get; } = value;
    public DateTime SelectedValue { get; } = selectedValue;
    public DateViewerSelectionUnit SelectionUnit { get; } = unit;
}

public sealed class DateViewerPanelChangedEventArgs(DateTime displayDate, DateViewerPanelKind panelKind) : EventArgs
{
    public DateTime DisplayDate { get; } = displayDate;
    public DateViewerPanelKind PanelKind { get; } = panelKind;
}

public sealed class DateViewerHoveredValueChangedEventArgs(DateTime? value) : EventArgs
{
    public DateTime? Value { get; } = value;
}
