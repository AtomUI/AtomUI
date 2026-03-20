using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class TreeItemContextMenuEventArgs : RoutedEventArgs
{
    public TreeViewItem ViewItem { get; }
    
    public TreeItemContextMenuEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}