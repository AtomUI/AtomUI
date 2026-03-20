using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class CascaderItemContextMenuEventArgs : RoutedEventArgs
{
    public TreeViewItem ViewItem { get; }
    
    public CascaderItemContextMenuEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}