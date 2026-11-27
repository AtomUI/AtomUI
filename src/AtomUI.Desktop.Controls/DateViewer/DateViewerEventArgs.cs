namespace AtomUI.Desktop.Controls;

/// <summary>
/// Provides data for the <see cref="DateViewer.ValueChanged" /> event, which is raised after a cell pick
/// changed <see cref="DateViewer.Value" />.
/// </summary>
public sealed class DateViewerValueChangedEventArgs(DateTime? oldValue, DateTime? value) : EventArgs
{
    /// <summary>
    /// The value of <see cref="DateViewer.Value" /> before the pick. It is <see langword="null" /> when the
    /// viewer had no value.
    /// </summary>
    public DateTime? OldValue { get; } = oldValue;
    /// <summary>
    /// The value of <see cref="DateViewer.Value" /> after the pick.
    /// </summary>
    public DateTime? Value { get; } = value;
}

/// <summary>
/// Provides data for the <see cref="RangeDateViewer.ValueChanged" /> event, which is raised after a cell pick
/// changed <see cref="RangeDateViewer.Value" />.
/// </summary>
public sealed class RangeDateViewerValueChangedEventArgs(DateViewerRange? oldValue, DateViewerRange? value) : EventArgs
{
    /// <summary>
    /// The value of <see cref="RangeDateViewer.Value" /> before the pick. It is <see langword="null" /> when the
    /// viewer had no value.
    /// </summary>
    public DateViewerRange? OldValue { get; } = oldValue;
    /// <summary>
    /// The value of <see cref="RangeDateViewer.Value" /> after the pick. After the first pick of a new range,
    /// only its start is set.
    /// </summary>
    public DateViewerRange? Value { get; } = value;
}

/// <summary>
/// Provides data for the <see cref="DateViewer.Selected" /> event, which is raised after a cell pick was
/// applied to <see cref="DateViewer.Value" />, whether or not the value changed.
/// </summary>
public sealed class DateViewerSelectedEventArgs(DateTime value, DateViewerSelectionUnit unit) : EventArgs
{
    /// <summary>
    /// The value of <see cref="DateViewer.Value" /> after the pick.
    /// </summary>
    public DateTime Value { get; } = value;
    /// <summary>
    /// The <see cref="DateViewer.SelectionUnit" /> in effect when the cell was picked.
    /// </summary>
    public DateViewerSelectionUnit SelectionUnit { get; } = unit;
}

/// <summary>
/// Provides data for the <see cref="RangeDateViewer.Selected" /> event, which is raised after a cell pick was
/// applied to <see cref="RangeDateViewer.Value" />, whether or not the value changed.
/// </summary>
public sealed class RangeDateViewerSelectedEventArgs(DateViewerRange value, DateTime selectedValue, DateViewerSelectionUnit unit) : EventArgs
{
    /// <summary>
    /// The value of <see cref="RangeDateViewer.Value" /> after the pick. After the first pick of a new range,
    /// only its start is set.
    /// </summary>
    public DateViewerRange Value { get; } = value;
    /// <summary>
    /// The date that was picked, normalized for the selection unit. It becomes the start or the end of
    /// <see cref="Value" />, whichever keeps the range ascending.
    /// </summary>
    public DateTime SelectedValue { get; } = selectedValue;
    /// <summary>
    /// The <see cref="RangeDateViewer.SelectionUnit" /> in effect when the cell was picked.
    /// </summary>
    public DateViewerSelectionUnit SelectionUnit { get; } = unit;
}

/// <summary>
/// Provides data for the <see cref="DateViewer.PanelChanged" /> event of <see cref="DateViewer" /> and
/// <see cref="RangeDateViewer" />, which is raised after the viewer switched to another panel or to another
/// page of the same panel.
/// </summary>
public sealed class DateViewerPanelChangedEventArgs(DateTime displayDate, DateViewerPanelKind panelKind) : EventArgs
{
    /// <summary>
    /// The <see cref="DateViewer.DisplayDate" /> value after the change.
    /// </summary>
    public DateTime DisplayDate { get; } = displayDate;
    /// <summary>
    /// The <see cref="DateViewer.PanelKind" /> value after the change.
    /// </summary>
    public DateViewerPanelKind PanelKind { get; } = panelKind;
}

/// <summary>
/// Provides data for the <see cref="DateViewer.HoveredValueChanged" /> event of <see cref="DateViewer" /> and
/// <see cref="RangeDateViewer" />, which is raised after the hovered cell changed.
/// </summary>
public sealed class DateViewerHoveredValueChangedEventArgs(DateTime? value) : EventArgs
{
    /// <summary>
    /// The <see cref="DateViewer.HoveredValue" /> after the change: the date of the hovered cell, or
    /// <see langword="null" /> when no cell or a disabled cell is hovered.
    /// </summary>
    public DateTime? Value { get; } = value;
}
