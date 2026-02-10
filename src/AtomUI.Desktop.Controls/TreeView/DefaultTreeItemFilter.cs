namespace AtomUI.Desktop.Controls;

public class DefaultTreeItemFilter : ITreeItemFilter
{
    public bool Filter(TreeView treeView, TreeItem treeItem, object? filterValue)
    {
        var strFilterValue = filterValue as string;
        if (strFilterValue == null)
        {
            return true;
        }
        if (treeView.ItemsSource != null)
        {
            if (treeItem.Header is ITreeItemData treeItemData)
            {
                var headerStr = treeItemData.Header?.ToString();
                if (!string.IsNullOrWhiteSpace(headerStr) && headerStr.IndexOf(strFilterValue, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }
            }
        }
        else if (treeItem.Header is string header)
        {
            if (header.IndexOf(strFilterValue, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
        }
        return false;
    }
}