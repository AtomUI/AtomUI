using System.Collections;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="TreeView.CheckedItemsChanged" /> event: the items that one update of the check state
/// removed from and added to the checked items.
/// </summary>
public class TreeViewCheckedItemsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewCheckedItemsChangedEventArgs" /> class.
    /// </summary>
    /// <param name="removedItems">
    /// The items removed from the check.
    /// </param>
    /// <param name="addedItems">
    /// The items added to the check.
    /// </param>
    public TreeViewCheckedItemsChangedEventArgs(IList removedItems, IList addedItems)
    {
        RemovedItems = removedItems;
        AddedItems   = addedItems;
    }

    /// <summary>
    /// Gets the items that were added to the selection.
    /// </summary>
    public IList AddedItems { get; }

    /// <summary>
    /// Gets the items that were removed from the selection.
    /// </summary>
    public IList RemovedItems { get; }
}