namespace AtomUI.Desktop.Controls;

public class CascaderItemExpandedEventArgs : EventArgs
{
    public CascaderViewItem Item { get; }

    public CascaderItemExpandedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}