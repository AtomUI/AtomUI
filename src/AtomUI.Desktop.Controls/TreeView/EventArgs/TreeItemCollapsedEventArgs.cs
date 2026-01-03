namespace AtomUI.Desktop.Controls;

public class TreeItemCollapsedEventArgs : EventArgs
{
    public TreeViewItem Item { get; }

    public TreeItemCollapsedEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}