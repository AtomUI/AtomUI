using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class TreeViewItemLoadRequestEventArgs : RoutedEventArgs
{
    public TreeViewItem Node { get; }

    public TreeViewItemLoadRequestEventArgs(TreeViewItem item)
    {
        Node = item;
    }
}