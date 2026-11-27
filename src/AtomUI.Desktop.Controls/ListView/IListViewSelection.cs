namespace AtomUI.Desktop.Controls;

/// <summary>
/// Exposes and changes the selection of a <see cref="ListView" /> through source indexes.
/// </summary>
/// <remarks>
/// <para>
/// A source index is the position of an item in the source collection of the list, not its position among the
/// displayed items.
/// </para>
/// <para>
/// Disabled items are never selected. The owning list is informed of each change after the change has been
/// applied.
/// </para>
/// </remarks>
public interface IListViewSelection
{
    /// <summary>
    /// Source index of the anchor item, the fixed end that a range selection extends from, or -1 when there is
    /// none.
    /// </summary>
    int AnchorIndex { get; }
    /// <summary>
    /// Source index of the active item, the item handled last by <see cref="Select" /> or
    /// <see cref="SelectRange" />, or -1 when there is none.
    /// </summary>
    int SelectedIndex { get; }
    /// <summary>
    /// Source indexes of all selected items, in ascending order.
    /// </summary>
    IReadOnlyList<int> SelectedIndexes { get; }
    /// <summary>
    /// The active item, the item at <see cref="SelectedIndex" />, or <see langword="null" /> when there is none.
    /// </summary>
    object? SelectedItem { get; }
    /// <summary>
    /// The selected items, in the order of <see cref="SelectedIndexes" />.
    /// </summary>
    IReadOnlyList<object?> SelectedItems { get; }

    /// <summary>
    /// Returns whether the item at the given source index is selected.
    /// </summary>
    /// <param name="sourceIndex">
    /// The source index of the item.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item is selected, <see langword="false" /> when it is not selected or when
    /// the index is invalid.
    /// </returns>
    bool IsSelected(int sourceIndex);
    /// <summary>
    /// Selects the item at the given source index and makes it the anchor and the active item. With both
    /// <c>Toggle</c> and <c>Multiple</c> set, the item is toggled instead, so a selected item is deselected.
    /// </summary>
    /// <param name="sourceIndex">
    /// The source index of the item.
    /// </param>
    /// <remarks>
    /// <para>
    /// In a selection mode without <c>Multiple</c>, the other selected items are deselected.
    /// </para>
    /// <para>
    /// The call does nothing when the index is invalid or the item is disabled.
    /// </para>
    /// </remarks>
    void Select(int sourceIndex);
    /// <summary>
    /// Removes the item at the given source index from the selection.
    /// </summary>
    /// <param name="sourceIndex">
    /// The source index of the item.
    /// </param>
    /// <remarks>
    /// An item that is the anchor or the active item loses that role. In the <c>AlwaysSelected</c> selection
    /// mode a fallback item is selected when the selection becomes empty. The call does nothing when the index
    /// is invalid.
    /// </remarks>
    void Deselect(int sourceIndex);
    /// <summary>
    /// Replaces the selection with the enabled items that lie, in display order, between the anchor and the item
    /// at <paramref name="endSourceIndex" />. The item at <paramref name="startSourceIndex" /> becomes the anchor
    /// and the item at <paramref name="endSourceIndex" /> becomes the active item.
    /// </summary>
    /// <param name="startSourceIndex">
    /// Source index of the item that becomes the anchor.
    /// </param>
    /// <param name="endSourceIndex">
    /// Source index of the item that ends the range and becomes the active item.
    /// </param>
    /// <remarks>
    /// <para>
    /// Both ends are included. When there is no anchor, only the item at <paramref name="endSourceIndex" /> is
    /// selected.
    /// </para>
    /// <para>
    /// The call does nothing when either index is invalid.
    /// </para>
    /// </remarks>
    void SelectRange(int startSourceIndex, int endSourceIndex);
    /// <summary>
    /// Removes all items from the selection and clears the anchor and the active item.
    /// </summary>
    /// <remarks>
    /// In the <c>AlwaysSelected</c> selection mode a fallback item is selected instead.
    /// </remarks>
    void Clear();
    /// <summary>
    /// Adds every enabled item to the selection.
    /// </summary>
    /// <remarks>
    /// The anchor and the active item are kept. When there are none, the first enabled item takes both roles.
    /// </remarks>
    void SelectAll();
}
