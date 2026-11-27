using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="TreeView.ItemContextMenuRequest" /> event: the node for which a context menu was
/// requested.
/// </summary>
public class TreeItemContextMenuEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The node for which the context menu was requested.
    /// </summary>
    public TreeViewItem ViewItem { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeItemContextMenuEventArgs" /> class.
    /// </summary>
    /// <param name="viewItem">
    /// The node for which the context menu was requested.
    /// </param>
    public TreeItemContextMenuEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}