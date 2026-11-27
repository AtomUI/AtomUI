using AtomUI.Localization;

namespace AtomUI.Controls.Localization;

/// <summary>
/// Identifies the text resources that several controls share, such as the labels of common buttons and the text
/// of an empty state. Each member is the id of a unit in the XLF files next to this enum. A theme reads the text
/// of the current language through the <c>atom:CommonLangResource</c> markup extension.
/// </summary>
[LanguageCatalog]
public enum CommonLangResourceKind
{
    /// <summary>
    /// The label of the button that confirms a choice made in a popup, such as a picker or a filter flyout.
    /// </summary>
    Ok,
    /// <summary>
    /// The label of the button that submits a form.
    /// </summary>
    Submit,
    /// <summary>
    /// The label of the button that abandons the pending operation and discards its changes.
    /// </summary>
    Cancel,
    /// <summary>
    /// The label of the button that returns a form or a filter to its initial values.
    /// </summary>
    Reset,
    /// <summary>
    /// The label of the action that opens an item for editing.
    /// </summary>
    Edit,
    /// <summary>
    /// The label of the action that removes an item.
    /// </summary>
    Delete,
    /// <summary>
    /// The label of the button that stores pending changes.
    /// </summary>
    Save,
    /// <summary>
    /// The description of an empty state, shown where a data area has nothing to display.
    /// </summary>
    NoData,
    /// <summary>
    /// The text shown while content is still being loaded.
    /// </summary>
    Loading,
    /// <summary>
    /// The label that marks a form field whose value may be left empty.
    /// </summary>
    Optional,
    // Appended to keep the values of the existing members stable for compiled callers.
    /// <summary>
    /// The label of the button that clears the content of an input.
    /// </summary>
    Clear,
    /// <summary>
    /// The label of the button that dismisses a dialog or a popup.
    /// </summary>
    Close,
    /// <summary>
    /// The label that marks a form field whose value must be entered.
    /// </summary>
    Required
}
