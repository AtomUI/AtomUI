namespace AtomUI.Desktop.Controls;

public interface IListViewItemFilter
{
    bool Filter(ListView listBox, ListViewItem listBoxItem, object? filterValue);
}