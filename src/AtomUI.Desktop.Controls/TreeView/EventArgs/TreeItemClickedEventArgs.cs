namespace AtomUI.Desktop.Controls;

public class TreeItemClickedEventArgs : EventArgs
{
    public TreeItem Item { get; }

    public TreeItemClickedEventArgs(TreeItem item)
    {
        Item = item;
    }
}