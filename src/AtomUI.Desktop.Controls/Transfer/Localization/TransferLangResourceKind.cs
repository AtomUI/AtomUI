using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// The localized texts of a transfer panel: the unit word of the item count in its title bar and the
/// captions of the entries of its selection menu. The transfer themes resolve the members through the
/// <c>TransferLangResource</c> markup.
/// </summary>
[LanguageCatalog]
public enum TransferLangResourceKind
{
    /// <summary>
    /// The singular unit word after the item count in the title bar, as in <c>1 item</c>.
    /// </summary>
    Item,
    /// <summary>
    /// The plural unit word after the item count in the title bar, as in <c>3 items</c>.
    /// </summary>
    Items,
    /// <summary>
    /// The caption of the selection menu entry that issues
    /// <see cref="TransferSelectAction.SelectAll" />.
    /// </summary>
    SelectAll,
    /// <summary>
    /// The caption of the selection menu entry that issues
    /// <see cref="TransferSelectAction.DeselectAll" />.
    /// </summary>
    DeSelectAll,
    /// <summary>
    /// The caption of the selection menu entry that issues
    /// <see cref="TransferSelectAction.RemoveCurrentPage" />.
    /// </summary>
    RemoveCurrentPage,
    /// <summary>
    /// The caption of the selection menu entry that issues
    /// <see cref="TransferSelectAction.RemoveAll" />.
    /// </summary>
    RemoveAll,
    /// <summary>
    /// The caption of the selection menu entry that issues
    /// <see cref="TransferSelectAction.InvertSelectCurrentPage" />.
    /// </summary>
    InvertSelectCurrentPage,
    /// <summary>
    /// The caption of the selection menu entry that issues
    /// <see cref="TransferSelectAction.SelectCurrentPage" />.
    /// </summary>
    SelectCurrentPage
}
