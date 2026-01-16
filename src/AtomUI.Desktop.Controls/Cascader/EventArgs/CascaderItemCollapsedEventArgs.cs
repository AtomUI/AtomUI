namespace AtomUI.Desktop.Controls;

public class CascaderItemCollapsedEventArgs : EventArgs
{
    public CascaderViewItem Item { get; }

    public CascaderItemCollapsedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}