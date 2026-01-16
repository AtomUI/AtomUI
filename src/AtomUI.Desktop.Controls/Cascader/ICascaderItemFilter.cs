namespace AtomUI.Desktop.Controls;

public interface ICascaderItemFilter
{
    bool Filter(CascaderView cascaderView, CascaderViewItem cascaderViewItem, object? filterValue);
}