using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class CascaderItemContextMenuEventArgs : RoutedEventArgs
{
    public TreeItem Item { get; }
    
    public CascaderItemContextMenuEventArgs(TreeItem item)
    {
        Item = item;
    }
}