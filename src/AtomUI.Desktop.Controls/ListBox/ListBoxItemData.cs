namespace AtomUI.Desktop.Controls;

public interface IListBoxItemData
{
    bool IsEnabled { get; }
    bool IsSelected { get; }
    object? Value { get; }
}

public record ListBoxItemData : IListBoxItemData
{
    public bool IsEnabled { get; init; } = true;
    public bool IsSelected { get; init; }
    public object? Value { get; init; }
}
