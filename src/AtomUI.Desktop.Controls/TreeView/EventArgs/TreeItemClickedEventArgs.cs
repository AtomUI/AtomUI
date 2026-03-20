namespace AtomUI.Desktop.Controls;

public class TreeItemClickedEventArgs : EventArgs
{
    public TreeViewItem ViewItem { get; }

    public TreeItemClickedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}