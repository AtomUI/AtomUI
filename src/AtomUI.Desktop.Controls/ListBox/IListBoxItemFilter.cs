namespace AtomUI.Desktop.Controls;

public interface IListBoxItemFilter
{
    bool Filter(ListBox listBox, ListBoxItem listBoxItem, object? filterValue);
}