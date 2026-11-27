namespace AtomUI.Controls.Data;

/// <summary>
/// The data of one item in an item list control. Controls that build their items from it, such as <c>ListBox</c>,
/// <c>ListView</c>, <c>Select</c> and <c>Transfer</c>, read the value the item displays, whether the user may
/// interact with it, the key that identifies it and the group it belongs to.
/// </summary>
[GenerateDataMemberAccessors]
public interface IListItemData : IItemKey, IGroupHeader
{
    /// <summary>
    /// Whether the user can interact with the item. A disabled item is still displayed, but it cannot be selected or
    /// moved between the lists of a <c>Transfer</c>.
    /// </summary>
    bool IsEnabled { get; set; }
    /// <summary>
    /// The value the item displays: the default item templates of the list controls render it as text, and
    /// <c>ListView</c> matches its filter text against it when no filter value selector is set.
    /// </summary>
    object? Content { get; set; }
}

/// <summary>
/// Marks a list item data object as the heading of a group instead of a selectable item, for ListView and Select
/// in AtomUI.Desktop.Controls and for <c>GroupListItemData</c> in this assembly.
/// </summary>
/// <remarks>
/// <see cref="IsGroupItem" /> is <see langword="true" /> for a heading. <c>GroupListItemData</c> implements it
/// publicly; <c>SelectOption</c> in AtomUI.Desktop.Controls implements it explicitly, with <see langword="false" />
/// as the default. <c>ListView</c> copies the flag onto its item container, and <c>SelectCandidateList</c> uses it
/// to tell headings apart from options.
/// </remarks>
internal interface IGroupListItemData
{
    bool IsGroupItem { get; set; }
}
