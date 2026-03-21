using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class TransferItemsRemovedEventArgs : EventArgs
{
    public IList<IItemKey>? Items { get; }
    public TransferItemsRemovedEventArgs(IList<IItemKey>? items)
    {
        Items = items;
    }
}