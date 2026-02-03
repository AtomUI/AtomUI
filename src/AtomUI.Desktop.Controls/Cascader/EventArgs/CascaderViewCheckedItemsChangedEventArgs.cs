namespace AtomUI.Desktop.Controls;

public class CascaderViewCheckedItemsChangedEventArgs : EventArgs
{
    public IList<ICascaderViewOption>? OldItems { get; }
    public IList<ICascaderViewOption>? NewItems { get; }
    
    public CascaderViewCheckedItemsChangedEventArgs(IList<ICascaderViewOption>? oldItems, IList<ICascaderViewOption>? newItems)
    {
        OldItems = oldItems;
        NewItems = newItems;
    }
}