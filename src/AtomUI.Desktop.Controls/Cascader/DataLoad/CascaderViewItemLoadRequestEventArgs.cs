using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class CascaderViewItemLoadRequestEventArgs : RoutedEventArgs
{
    public CascaderViewItem Item { get; }

    public CascaderViewItemLoadRequestEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}