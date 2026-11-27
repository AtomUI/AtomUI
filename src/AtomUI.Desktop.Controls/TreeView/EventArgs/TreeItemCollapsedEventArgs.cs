namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="TreeView.ItemCollapsed" /> event: the node that was collapsed.
/// </summary>
public class TreeItemCollapsedEventArgs : EventArgs
{
    /// <summary>
    /// The node that was collapsed.
    /// </summary>
    public TreeViewItem ViewItem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeItemCollapsedEventArgs" /> class.
    /// </summary>
    /// <param name="viewItem">
    /// The node that was collapsed.
    /// </param>
    public TreeItemCollapsedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}