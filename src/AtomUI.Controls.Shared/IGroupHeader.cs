namespace AtomUI.Controls;

/// <summary>
/// A data item that names the group it belongs to. A <c>ListView</c> that groups its items and has no
/// <c>GroupPropertySelector</c> uses <see cref="Group" /> as the group key.
/// </summary>
public interface IGroupHeader
{
    /// <summary>
    /// The name of the group the item is listed under, or <see langword="null" /> when the item has no group.
    /// </summary>
    string? Group { get; }
}