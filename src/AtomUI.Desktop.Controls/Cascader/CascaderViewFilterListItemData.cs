namespace AtomUI.Desktop.Controls;

internal record CascaderViewFilterListItemData : ListBoxItemData, ICascaderItemInfo
{
    public IList<ICascaderViewOption>? ExpandItems { get; set; }
    public string Path => Value?.ToString() ?? string.Empty;
}