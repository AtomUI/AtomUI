namespace AtomUI.Desktop.Controls;

public interface ISelectOption : IListBoxItemData, IGroupHeader
{
    object? Header { get; }
    bool IsDynamicAdded { get; }
}

public record SelectOption : ListBoxItemData, ISelectOption, IGroupListItemData
{
    public object? Header { get; init; }
    
    public bool IsDynamicAdded { get; init; } = false;
    
    public string? Group { get; init; }

    bool IGroupListItemData.IsGroupItem { get; set; } = false;
}
