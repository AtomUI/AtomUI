using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

public interface ISelectOption : IListItemData
{
    object? Header { get; }
    bool IsDynamicAdded { get; }
}

public record SelectOption : ListItemData, ISelectOption, IGroupListItemData
{
    public object? Header { get; init; }
    
    public bool IsDynamicAdded { get; init; } = false;

    bool IGroupListItemData.IsGroupItem { get; set; } = false;
}
