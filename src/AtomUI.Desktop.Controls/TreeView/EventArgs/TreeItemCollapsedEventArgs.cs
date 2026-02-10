namespace AtomUI.Desktop.Controls;

public class TreeItemCollapsedEventArgs : EventArgs
{
    public TreeItem Item { get; }

    public TreeItemCollapsedEventArgs(TreeItem item)
    {
        Item = item;
    }
}