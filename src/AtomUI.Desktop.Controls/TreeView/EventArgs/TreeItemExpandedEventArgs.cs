namespace AtomUI.Desktop.Controls;

public class TreeItemExpandedEventArgs : EventArgs
{
    public TreeViewItem Item { get; }

    public TreeItemExpandedEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}