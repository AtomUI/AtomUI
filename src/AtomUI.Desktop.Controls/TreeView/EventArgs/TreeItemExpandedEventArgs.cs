namespace AtomUI.Desktop.Controls;

public class TreeItemExpandedEventArgs : EventArgs
{
    public TreeItem Item { get; }

    public TreeItemExpandedEventArgs(TreeItem item)
    {
        Item = item;
    }
}