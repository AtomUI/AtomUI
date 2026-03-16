using System.Collections;

namespace AtomUI.Desktop.Controls;

internal class ListSyncSelectionEventArgs : EventArgs
{
    public ListSyncSelectionEventArgs(IList removedItems, IList addedItems)
    {
        RemovedItems = removedItems;
        AddedItems   = addedItems;
    }
    
    public IList AddedItems { get; }
    
    public IList RemovedItems { get; }
}