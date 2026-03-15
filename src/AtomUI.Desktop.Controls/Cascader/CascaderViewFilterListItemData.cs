using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

internal record CascaderViewFilterListItemData : ListItemData, ICascaderItemInfo
{
    public IList<ICascaderOption>? ExpandItems { get; set; }
    public string Path => Content?.ToString() ?? string.Empty;
}