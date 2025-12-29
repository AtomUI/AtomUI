using System.Collections;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class TreeViewCheckedItemsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewCheckedItemsChangedEventArgs"/> class.
    /// </summary>
    /// <param name="routedEvent">The event being raised.</param>
    /// <param name="removedItems">The items removed from the check.</param>
    /// <param name="addedItems">The items added to the check.</param>
    public TreeViewCheckedItemsChangedEventArgs(IList<ITreeViewItemData> removedItems, IList<ITreeViewItemData> addedItems)
    {
        RemovedItems = removedItems;
        AddedItems   = addedItems;
    }

    /// <summary>
    /// Gets the items that were added to the selection.
    /// </summary>
    public IList<ITreeViewItemData> AddedItems { get; }

    /// <summary>
    /// Gets the items that were removed from the selection.
    /// </summary>
    public IList<ITreeViewItemData> RemovedItems { get; }
}