namespace AtomUI.Controls.Data;

/// <summary>
/// Item data of one record of an item list control, for items that are created in code: it holds the value the
/// item displays, whether the user can interact with it, the key that identifies it and the group it belongs to.
/// </summary>
public class ListItemData : IListItemData
{
    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;
    /// <inheritdoc />
    public object? Content { get; set; }
    /// <inheritdoc />
    public EntityKey? ItemKey { get; init; }
    /// <inheritdoc />
    public string? Group { get; init; }
}

/// <summary>
/// Item data of a group header in a grouped list. The collection view creates one for each group, with the group
/// name as <see cref="ListItemData.Content" /> and <see cref="IsGroupItem" /> set to <see langword="true" />.
/// </summary>
public class GroupListItemData : ListItemData, IGroupListItemData
{
    /// <summary>
    /// Whether the item is a group header rather than a record. <see langword="true" /> makes the list controls render
    /// it as a group header, which the user cannot select.
    /// </summary>
    public bool IsGroupItem { get; set; }
}
