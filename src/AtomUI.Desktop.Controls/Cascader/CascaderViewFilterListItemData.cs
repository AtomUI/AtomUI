using System.Collections;

namespace AtomUI.Desktop.Controls;

internal class CascaderViewFilterListItemData : ListBoxItemData, ICascaderItemInfo
{
    public IList? ExpandItems { get; set; }
    public string Path => Value?.ToString() ?? string.Empty;
}