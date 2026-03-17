namespace AtomUI.Desktop.Controls;

public class ListViewItemClickedEventArgs : EventArgs
{
    public ListViewItem Item { get; }

    public ListViewItemClickedEventArgs(ListViewItem item)
    {
        Item = item;
    }
}