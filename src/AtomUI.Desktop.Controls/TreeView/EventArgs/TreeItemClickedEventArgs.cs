namespace AtomUI.Desktop.Controls;

public class TreeItemClickedEventArgs : EventArgs
{
    public TreeViewItem Item { get; }

    public TreeItemClickedEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}