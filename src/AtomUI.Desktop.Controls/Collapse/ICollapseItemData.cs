namespace AtomUI.Desktop.Controls;

public record CollapseItemData : ICollapseItemData
{
    public bool IsSelected { get; init; } = false;
    public bool IsEnabled { get; init; } = true;
    public bool IsShowExpandIcon { get; init; } = true;
    public object? Header { get; init; }
}