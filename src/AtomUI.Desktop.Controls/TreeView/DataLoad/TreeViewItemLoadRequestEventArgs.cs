using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class TreeViewItemLoadRequestEventArgs : RoutedEventArgs
{
    public TreeViewItem ViewItem { get; }

    public TreeViewItemLoadRequestEventArgs(TreeViewItem viewItem)
    {
        ViewItem = viewItem;
    }
}