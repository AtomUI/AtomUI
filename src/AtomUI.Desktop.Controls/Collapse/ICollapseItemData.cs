namespace AtomUI.Desktop.Controls;

/// <summary>
/// A ready-made <see cref="ICollapseItemData" /> for the items of a <see cref="Collapse" />, describing one
/// panel by its header, its selected state and the visibility of its expand icon.
/// </summary>
public record CollapseItemData : ICollapseItemData
{
    /// <inheritdoc />
    public bool IsSelected { get; init; } = false;
    /// <inheritdoc />
    public bool IsEnabled { get; init; } = true;
    /// <inheritdoc />
    public bool IsShowExpandIcon { get; init; } = true;
    /// <inheritdoc />
    public object? Header { get; init; }
}