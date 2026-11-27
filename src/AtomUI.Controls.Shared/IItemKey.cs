namespace AtomUI.Controls;

/// <summary>
/// A data item that carries the key that identifies it among the other items, for example in a collection of
/// selected keys or in a tree path.
/// </summary>
public interface IItemKey
{
    /// <summary>
    /// The key of the item, or <see langword="null" /> when the item has no key. In a <c>TreeView</c> and a
    /// <c>NavMenu</c> the key text is the segment that stands for the item in a path.
    /// </summary>
    EntityKey? ItemKey { get; }
}