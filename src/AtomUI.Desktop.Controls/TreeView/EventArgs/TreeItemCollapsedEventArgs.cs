namespace AtomUI.Desktop.Controls;

public class TreeItemCollapsedEventArgs : EventArgs
{
    public TreeViewItem ViewItem { get; }

    public TreeItemCollapsedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}