using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class CascaderItemContextMenuEventArgs : RoutedEventArgs
{
    public TreeViewItem Item { get; }
    
    public CascaderItemContextMenuEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}