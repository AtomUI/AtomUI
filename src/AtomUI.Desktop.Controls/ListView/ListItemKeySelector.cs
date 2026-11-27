using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Returns the key that identifies an item of a list, or <see langword="null" /> when the item has no key.
/// Used by <see cref="ListView.ItemKeySelector" />.
/// </summary>
/// <param name="item">
/// The item to identify.
/// </param>
/// <returns>
/// The key of <paramref name="item" />, or <see langword="null" /> when the item has no key.
/// </returns>
public delegate EntityKey? ListItemKeySelector(object? item);
