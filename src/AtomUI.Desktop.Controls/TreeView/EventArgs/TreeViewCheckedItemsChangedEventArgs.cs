using System.Collections;

namespace AtomUI.Desktop.Controls;

public class TreeViewCheckedItemsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewCheckedItemsChangedEventArgs"/> class.
    /// </summary>
    /// <param name="removedItems">The items removed from the check.</param>
    /// <param name="addedItems">The items added to the check.</param>
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