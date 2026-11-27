namespace AtomUI.Controls;

/// <summary>
/// Event data for the change of the number of items in a list box, a list view or a transfer tree view.
/// </summary>
public class ItemCountChangedEventArgs : EventArgs
{
    /// <summary>
    /// The number of items the view holds after the change. The count covers all pages, and in a tree view it counts
    /// nodes at every level.
    /// </summary>
    public int ItemCount { get; }
    
    /// <summary>
    /// Initializes the event data with an item count.
    /// </summary>
    /// <param name="itemCount">
    /// The number of items the view holds after the change.
    /// </param>
    public ItemCountChangedEventArgs(int itemCount)
    {
        ItemCount = itemCount;
    }
}