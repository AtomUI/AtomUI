namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the selection of the grid before and after it changed, for the
/// <see cref="DataGrid.SelectionChanged" /> event, which is raised after the new selection has been applied.
/// </summary>
public sealed class DataGridSelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes an instance with the selection before and after a change.
    /// </summary>
    /// <param name="oldSelection">
    /// The selection before the change.
    /// </param>
    /// <param name="newSelection">
    /// The selection after the change.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="oldSelection" /> or <paramref name="newSelection" /> is null.
    /// </exception>
    public DataGridSelectionChangedEventArgs(
        DataGridSelectionState oldSelection,
        DataGridSelectionState newSelection)
    {
        ArgumentNullException.ThrowIfNull(oldSelection);
        ArgumentNullException.ThrowIfNull(newSelection);
        OldSelection = oldSelection;
        NewSelection = newSelection;
    }

    /// <summary>
    /// Gets the selection before the change.
    /// </summary>
    public DataGridSelectionState OldSelection { get; }

    /// <summary>
    /// Gets the selection after the change.
    /// </summary>
    public DataGridSelectionState NewSelection { get; }
}
