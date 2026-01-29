namespace AtomUI.Desktop.Controls;

public class DefaultListBoxItemFilter : IListBoxItemFilter
{
    public bool Filter(ListBox listBox, ListBoxItem listBoxItem, object? filterValue)
    {
        var strFilterValue = filterValue as string;
        if (strFilterValue == null)
        {
            return true;
        }
        if (listBox.ItemsSource != null)
        {
            if (listBoxItem.Content is IListItemData listItemData)
            {
                var contentStr = listItemData.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(contentStr) && contentStr.IndexOf(strFilterValue, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }
            }
        }
        else if (listBoxItem.Content is string header)
        {
            if (header.IndexOf(strFilterValue, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
        }
        return false;
    }
}