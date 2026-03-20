namespace AtomUI.Desktop.Controls;

public class TreeItemExpandedEventArgs : EventArgs
{
    public TreeViewItem ViewItem { get; }

    public TreeItemExpandedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}