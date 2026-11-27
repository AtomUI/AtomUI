namespace AtomUI.Desktop.Controls;

/// <summary>
/// Provides the old and the new value for the <see cref="Calendar.ValueChanged" /> event.
/// The event is raised after the new value is committed, and only when the value changed.
/// </summary>
public sealed class CalendarValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the arguments with the value before and the value after the change.
    /// </summary>
    public CalendarValueChangedEventArgs(DateTime oldValue, DateTime newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>
    /// The value of the calendar before the change.
    /// </summary>
    public DateTime OldValue { get; }
    /// <summary>
    /// The value of the calendar after the change. It is already committed when the event is raised.
    /// </summary>
    public DateTime NewValue { get; }
}

/// <summary>
/// Provides the selected date and its origin for the <see cref="Calendar.Selected" /> event.
/// The event is raised after the selection is committed, every time the user selects a date, even when
/// the value did not change.
/// </summary>
public sealed class CalendarSelectedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the arguments for one selection.
    /// </summary>
    public CalendarSelectedEventArgs(DateTime value, CalendarSelectSource source)
    {
        Value = value;
        Source = source;
    }

    /// <summary>
    /// The selected date. It is already committed as the value of the calendar when the event is raised.
    /// </summary>
    public DateTime Value { get; }
    /// <summary>
    /// Where the selection came from: the date grid, a Select of the default header or a custom header.
    /// </summary>
    public CalendarSelectSource Source { get; }
}

/// <summary>
/// Provides the date shown by the panel and the mode for the <see cref="Calendar.PanelChanged" /> event.
/// It is raised after a selection that changes the displayed month or year, depending on the mode, and after
/// a mode switch. The new value or mode is already committed when the event is raised.
/// </summary>
public sealed class CalendarPanelChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the arguments for one panel change.
    /// </summary>
    public CalendarPanelChangedEventArgs(DateTime value, CalendarMode mode)
    {
        Value = value;
        Mode = mode;
    }

    /// <summary>
    /// The date that the panel shows after the change: the selected date after a selection, or the unchanged
    /// value after a mode switch.
    /// </summary>
    public DateTime Value { get; }
    /// <summary>
    /// The mode after the change: the unchanged mode after a selection, or the newly selected mode after a mode
    /// switch.
    /// </summary>
    public CalendarMode Mode { get; }
}
