using AtomUI.Controls.Utils;

namespace AtomUI.Desktop.Controls;

public class DefaultListBoxItemFilter : IListBoxItemFilter
{
    private IValueFilter _valueFilter;
    
    public DefaultListBoxItemFilter(ValueFilterMode filterMode = ValueFilterMode.Contains)
    {
        if (filterMode == ValueFilterMode.None ||
            filterMode == ValueFilterMode.Custom)
        {
            throw new ArgumentException("ValueFilterMode must not be None or Custom.");
        }
        _valueFilter = ValueFilterFactory.BuildFilter(filterMode)!;
    }
    
    public bool Filter(ListBox listBox, ListBoxItem listBoxItem, object? filterValue)
    {
        if (listBox.ItemsSource != null)
        {
            if (listBoxItem.Content is IListItemData listItemData)
            {
                var valueStr = listItemData.Value?.ToString();
                return _valueFilter.Filter(valueStr, filterValue);
            }
        }
        else if (listBoxItem.Content is string header)
        {
            return _valueFilter.Filter(header, filterValue);
        }
        return false;
    }
}