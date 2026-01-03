using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class TreeItemContextMenuEventArgs : RoutedEventArgs
{
    public TreeViewItem Item { get; }
    
    public TreeItemContextMenuEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}