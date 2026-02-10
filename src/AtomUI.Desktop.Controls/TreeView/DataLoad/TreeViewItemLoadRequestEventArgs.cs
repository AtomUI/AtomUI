using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class TreeViewItemLoadRequestEventArgs : RoutedEventArgs
{
    public TreeItem Item { get; }

    public TreeViewItemLoadRequestEventArgs(TreeItem item)
    {
        Item = item;
    }
}