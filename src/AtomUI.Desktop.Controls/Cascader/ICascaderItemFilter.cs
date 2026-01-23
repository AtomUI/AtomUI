namespace AtomUI.Desktop.Controls;

public interface ICascaderItemFilter
{
    bool Filter(CascaderView cascaderView, ICascaderItemInfo cascaderItemInfo, object? filterValue);
}