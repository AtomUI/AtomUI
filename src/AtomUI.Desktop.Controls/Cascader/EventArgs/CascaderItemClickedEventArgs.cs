namespace AtomUI.Desktop.Controls;

public class CascaderItemClickedEventArgs : EventArgs
{
    public CascaderViewItem Item { get; }

    public CascaderItemClickedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}

public class CascaderItemDoubleClickedEventArgs : EventArgs
{
    public CascaderViewItem Item { get; }

    public CascaderItemDoubleClickedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}