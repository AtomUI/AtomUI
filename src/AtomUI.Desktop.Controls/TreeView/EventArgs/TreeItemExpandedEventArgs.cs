namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="TreeView.ItemExpanded" /> event: the node that was expanded.
/// </summary>
public class TreeItemExpandedEventArgs : EventArgs
{
    /// <summary>
    /// The node that was expanded.
    /// </summary>
    public TreeViewItem ViewItem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeItemExpandedEventArgs" /> class.
    /// </summary>
    /// <param name="viewItem">
    /// The node that was expanded.
    /// </param>
    public TreeItemExpandedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}