using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for a context menu request on an item of a cascader view. No event of AtomUI uses this type.
/// </summary>
public class CascaderItemContextMenuEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The <see cref="TreeViewItem" /> on which the context menu was requested.
    /// </summary>
    public TreeViewItem ViewItem { get; }
    
    /// <summary>
    /// Creates the event data that reports a context menu request on <paramref name="viewItem" />.
    /// </summary>
    public CascaderItemContextMenuEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}