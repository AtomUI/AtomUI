using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// Names the language resources of the data grid. Each member is the id of one unit in the data grid's translation
/// files, and the themes read its localized text through the <c>DataGridLangResource</c> markup extension.
/// </summary>
[LanguageCatalog]
public enum DataGridLangResourceKind
{
    /// <summary>
    /// Text of the entry in a column's filter list that selects every item. It is the theme's default for the
    /// <c>SelectedAllText</c> property of the filter indicator.
    /// </summary>
    SelectAllFilterItems,
    /// <summary>
    /// Tooltip of a column header whose <c>SortTooltipType</c> is <c>Ascending</c>.
    /// </summary>
    AscendTooltip,
    /// <summary>
    /// Tooltip of a column header whose <c>SortTooltipType</c> is <c>Descending</c>.
    /// </summary>
    DescendTooltip,
    /// <summary>
    /// Tooltip of a column header whose <c>SortTooltipType</c> is <c>Cancel</c>.
    /// </summary>
    CancelTooltip,
    /// <summary>
    /// Title of the confirmation popup of the delete action in a row's operation buttons. The action is shown while
    /// the row is not being edited.
    /// </summary>
    DeleteConfirmText,
    /// <summary>
    /// Title of the confirmation popup of the cancel action in a row's operation buttons. The action is shown while
    /// the row is being edited.
    /// </summary>
    CancelConfirmText,
    /// <summary>
    /// Text shown with the loading indicator while an operation runs. It is the theme's default for the grid's
    /// <c>OperatingMsg</c> property.
    /// </summary>
    Operating
}
