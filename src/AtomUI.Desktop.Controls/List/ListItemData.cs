using Avalonia;

namespace AtomUI.Desktop.Controls;

public interface IListItemData : IListBoxItemData, IGroupHeader
{
    object? Content { get; }
}

internal interface IGroupListItemData
{
    bool IsGroupItem { get; set; }
}

public record ListItemData : ListBoxItemData, IListItemData, IGroupListItemData
{
    public object? Content { get; init; }
    public string? Group { get; init; }

    bool IGroupListItemData.IsGroupItem { get; set; } = false;
}
