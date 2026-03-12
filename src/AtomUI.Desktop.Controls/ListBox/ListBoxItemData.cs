using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface IListBoxItemData : IItemKey
{
    bool IsEnabled { get; set; }
    bool IsSelected { get; set; }
    object? Value { get; set; }
}

public record ListBoxItemData : IListBoxItemData
{
    public bool IsEnabled { get; set; } = true;
    public bool IsSelected { get; set; }
    public object? Value { get; set; }
    public EntityKey? ItemKey { get; init; }
}
