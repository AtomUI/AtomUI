namespace AtomUI.Controls;

/// <summary>
/// An object that supplies a header for the container that shows it.
/// </summary>
public interface IReadOnlyHeadered
{
    /// <summary>
    /// The header of the object. When the object is the <c>DataContext</c> of a <c>CollapseItem</c> whose own
    /// <c>Header</c> is <see langword="null" />, the item shows this value as its header.
    /// </summary>
    object? Header { get; }
}