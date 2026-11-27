namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="TreeView.ItemClicked" /> event: the node that was clicked.
/// </summary>
public class TreeItemClickedEventArgs : EventArgs
{
    /// <summary>
    /// The node that was clicked.
    /// </summary>
    public TreeViewItem ViewItem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeItemClickedEventArgs" /> class.
    /// </summary>
    /// <param name="viewItem">
    /// The node that was clicked.
    /// </param>
    public TreeItemClickedEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}