namespace AtomUI.Controls.Data;

public record ListItemData : IListItemData
{
    public bool IsEnabled { get; set; } = true;
    public bool IsSelected { get; set; }
    public object? Content { get; set; }
    public EntityKey? ItemKey { get; init; }
    public string? Group { get; init; }
}

public record GroupListItemData : ListItemData, IGroupListItemData
{
    public bool IsGroupItem { get; set; }
}