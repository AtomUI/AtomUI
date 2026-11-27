using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An option of the suggestion list of an AutoComplete control. It adds the <see cref="Header" /> that the
/// suggestion list shows to the data of a list item. <see cref="AutoCompleteOption" /> is an implementation
/// for item sources that need no option type of their own.
/// </summary>
public interface IAutoCompleteOption : IListItemData
{
    /// <summary>
    /// Text that the suggestion list shows for the option; the default option template binds its text block to it.
    /// </summary>
    /// <remarks>
    /// To write a chosen option into the input, or to complete the typed text from the first match, the control
    /// reads <see cref="IListItemData.Content" />, this property and the item key in that order and uses the first
    /// one that is not <see langword="null" />.
    /// </remarks>
    object? Header { get; }
}