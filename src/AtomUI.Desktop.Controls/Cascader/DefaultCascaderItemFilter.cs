namespace AtomUI.Desktop.Controls;

public class DefaultCascaderItemFilter : ICascaderItemFilter
{
    public bool Filter(CascaderView cascaderView, ICascaderItemInfo cascaderItemInfo, object? filterValue)
    {
        var strFilterValue = filterValue as string;
        if (strFilterValue == null)
        {
            return true;
        }

        var path = cascaderItemInfo.Path;
        if (!string.IsNullOrWhiteSpace(path) && path.IndexOf(strFilterValue, StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        return false;
    }
}