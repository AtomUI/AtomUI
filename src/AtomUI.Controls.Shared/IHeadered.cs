namespace AtomUI.Controls;

/// <summary>
/// Item data that supplies the header for the container that is created for it.
/// </summary>
public interface IHeadered
{
    /// <summary>
    /// The header of the item. When the item is the <c>DataContext</c> of a <c>TabItem</c> or a <c>CollapseItem</c>
    /// whose own <c>Header</c> is <see langword="null" />, the container shows this value as its header.
    /// </summary>
    object? Header { get; }
}