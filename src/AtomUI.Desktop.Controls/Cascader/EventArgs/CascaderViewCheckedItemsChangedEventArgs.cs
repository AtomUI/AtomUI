namespace AtomUI.Desktop.Controls;

public class CascaderViewCheckedItemsChangedEventArgs : EventArgs
{
    public IList<ICascaderOption>? OldItems { get; }
    public IList<ICascaderOption>? NewItems { get; }
    
    public CascaderViewCheckedItemsChangedEventArgs(IList<ICascaderOption>? oldItems, IList<ICascaderOption>? newItems)
    {
        OldItems = oldItems;
        NewItems = newItems;
    }
}