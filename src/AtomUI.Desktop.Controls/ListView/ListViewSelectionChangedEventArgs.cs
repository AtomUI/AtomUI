using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the items that entered and that left the selection in one selection change of a list, for the
/// <see cref="ListView.SelectionChanged" /> event. The list raises the event after the selection, the
/// containers and <see cref="ListView.SelectedValue" /> were updated.
/// </summary>
public sealed class ListViewSelectionChangedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The indexes of the items that left the selection in this change.
    /// </summary>
    public IReadOnlyList<int> DeselectedIndexes { get; }
    /// <summary>
    /// The items that left the selection in this change.
    /// </summary>
    public IReadOnlyList<object?> DeselectedItems { get; }
    /// <summary>
    /// The indexes of the items that entered the selection in this change.
    /// </summary>
    public IReadOnlyList<int> SelectedIndexes { get; }
    /// <summary>
    /// The items that entered the selection in this change.
    /// </summary>
    public IReadOnlyList<object?> SelectedItems { get; }

    /// <summary>
    /// Creates the arguments of one selection change.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise.
    /// </param>
    /// <param name="deselectedIndexes">
    /// The indexes of the items that left the selection.
    /// </param>
    /// <param name="deselectedItems">
    /// The items that left the selection.
    /// </param>
    /// <param name="selectedIndexes">
    /// The indexes of the items that entered the selection.
    /// </param>
    /// <param name="selectedItems">
    /// The items that entered the selection.
    /// </param>
    public ListViewSelectionChangedEventArgs(
        RoutedEvent routedEvent,
        IReadOnlyList<int> deselectedIndexes,
        IReadOnlyList<object?> deselectedItems,
        IReadOnlyList<int> selectedIndexes,
        IReadOnlyList<object?> selectedItems)
        : base(routedEvent)
    {
        DeselectedIndexes = deselectedIndexes;
        DeselectedItems   = deselectedItems;
        SelectedIndexes   = selectedIndexes;
        SelectedItems     = selectedItems;
    }
}
