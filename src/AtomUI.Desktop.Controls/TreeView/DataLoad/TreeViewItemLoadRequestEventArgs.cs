using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class TreeViewItemLoadRequestEventArgs : RoutedEventArgs
{
    public TreeViewItem Item { get; }

    public TreeViewItemLoadRequestEventArgs(TreeViewItem item)
    {
        Item = item;
    }
}