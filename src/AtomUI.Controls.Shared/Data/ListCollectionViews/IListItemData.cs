namespace AtomUI.Controls.Data;

public interface IListItemData : IItemKey, IGroupHeader
{
    bool IsEnabled { get; set; }
    bool IsSelected { get; set; }
    object? Content { get; set; }
}

internal interface IGroupListItemData
{
    bool IsGroupItem { get; set; }
}
