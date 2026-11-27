namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="Select.SelectionChanged" /> event. The select raises the event after a selection
/// change is committed: when <see cref="Select.SelectedOption" /> or <see cref="Select.SelectedOptions" /> is
/// replaced, or when an item is added to or removed from the <see cref="Select.SelectedOptions" /> list itself.
/// </summary>
public class SelectSelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// The selection before the change. In <see cref="SelectMode.Single" /> mode it is the previous
    /// <see cref="Select.SelectedOption" />; in the other modes it is the previous
    /// <see cref="Select.SelectedOptions" /> list, or a snapshot of it when an item was added to it or removed
    /// from it.
    /// </summary>
    public object? OldValue { get; }
    /// <summary>
    /// The selection after the change. In <see cref="SelectMode.Single" /> mode it is the new
    /// <see cref="Select.SelectedOption" />; in the other modes it is the new
    /// <see cref="Select.SelectedOptions" /> list, or a snapshot of it when an item was added to it or removed
    /// from it.
    /// </summary>
    public object? NewValue { get; }

    /// <summary>
    /// The <see cref="SelectMode" /> of the select at the time of the change. It tells whether the values are a
    /// single option or a list of options.
    /// </summary>
    public SelectMode Mode { get; }

    /// <summary>
    /// Creates the event data for a selection change.
    /// </summary>
    /// <param name="mode">
    /// The <see cref="SelectMode" /> of the select at the time of the change.
    /// </param>
    /// <param name="oldValue">
    /// The selection before the change.
    /// </param>
    /// <param name="newValue">
    /// The selection after the change.
    /// </param>
    public SelectSelectionChangedEventArgs(SelectMode mode, object? oldValue, object? newValue)
    {
        Mode     = mode;
        OldValue = oldValue;
        NewValue = newValue;
    }
}