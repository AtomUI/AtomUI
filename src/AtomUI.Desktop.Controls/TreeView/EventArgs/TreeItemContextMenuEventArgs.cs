using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class TreeItemContextMenuEventArgs : RoutedEventArgs
{
    public TreeItem Item { get; }
    
    public TreeItemContextMenuEventArgs(TreeItem item)
    {
        Item = item;
    }
}