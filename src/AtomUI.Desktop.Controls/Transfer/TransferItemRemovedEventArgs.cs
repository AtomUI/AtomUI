using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class TransferItemRemovedEventArgs : EventArgs
{
    public IItemKey? Item { get; }
    public TransferItemRemovedEventArgs(IItemKey? item)
    {
        Item = item;
    }
}