using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class TransferItemDeletedEventArgs : EventArgs
{
    public IItemKey? Item { get; }
    public TransferItemDeletedEventArgs(IItemKey? item)
    {
        Item = item;
    }
}