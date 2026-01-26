namespace AtomUI.Desktop.Controls;

public class ListBoxItemClickedEventArgs : EventArgs
{
    public ListBoxItem Item { get; }

    public ListBoxItemClickedEventArgs(ListBoxItem item)
    {
        Item = item;
    }
}