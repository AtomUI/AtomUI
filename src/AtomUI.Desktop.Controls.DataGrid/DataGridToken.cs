using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="DataGrid" />: the colors, paddings, font sizes and icon sizes of its
/// header, rows, cells, footer, sort and filter indicators, frozen column shadows and pagination.
/// </summary>
[ControlDesignToken]
public sealed partial class DataGridToken : AbstractControlDesignToken
{

    /// <summary>
    /// Background of table header
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color HeaderBg { get; set; }

    /// <summary>
    /// Color of table header text
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color HeaderColor { get; set; }

    /// <summary>
    /// Background color of table header when sorted
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color HeaderSortActiveBg { get; set; }

    /// <summary>
    /// Background color of table header when sorted and hovered
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color HeaderSortHoverBg { get; set; }

    /// <summary>
    /// Background color of table sorted column
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color BodySortBg { get; set; }

    /// <summary>
    /// Background color of table hovered row
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color RowHoverBg { get; set; }

    /// <summary>
    /// Background color of table selected row
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color RowSelectedBg { get; set; }

    /// <summary>
    /// Background color of table selected row when hovered
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color RowSelectedHoverBg { get; set; }

    /// <summary>
    /// Text color of a selected row, drawn on <see cref="RowSelectedBg" /> and <see cref="RowSelectedHoverBg" />.
    /// Defaults to the global <c>ControlItemColorActive</c> (the body text color unless a theme sets the contrast color
    /// of a solid selected background).
    /// </summary>
    public partial Color RowSelectedColor { get; set; }

    /// <summary>
    /// Background color of table expanded row
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color RowExpandedBg { get; set; }

    /// <summary>
    /// Padding of table cell (large size by default)
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Thickness CellPadding { get; set; }

    /// <summary>
    /// Padding of table cell (middle size)
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Thickness CellPaddingMD { get; set; }

    /// <summary>
    /// Padding of table cell (small size)
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Thickness CellPaddingSM { get; set; }

    /// <summary>
    /// Border color of table
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color BorderColor { get; set; }

    /// <summary>
    /// Border radius of table header
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial CornerRadius HeaderBorderRadius { get; set; }

    /// <summary>
    /// Background of footer
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color FooterBg { get; set; }

    /// <summary>
    /// Color of footer text
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color FooterColor { get; set; }

    /// <summary>
    /// Font size of table cell (large size by default)
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double CellFontSize { get; set; }

    /// <summary>
    /// Font size of table cell (middle size)
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double CellFontSizeMD { get; set; }

    /// <summary>
    /// Font size of table cell (small size)
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double CellFontSizeSM { get; set; }

    /// <summary>
    /// Split border color of table header
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color HeaderSplitColor { get; set; }

    /// <summary>
    /// Background color of fixed table header when sorted
    /// </summary>
    [ReservedToken("Reserved for a separate sticky header surface; AtomUI renders its header once in the grid viewport.")]
    public partial Color FixedHeaderSortActiveBg { get; set; }

    /// <summary>
    /// Background color of table header filter button when hovered
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color HeaderFilterHoverBg { get; set; }

    /// <summary>
    /// Background of filter dropdown menu item
    /// </summary>
    public partial Color FilterDropdownMenuBg { get; set; }

    /// <summary>
    /// Color of filter dropdown
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color FilterDropdownBg { get; set; }

    /// <summary>
    /// Background of expand button
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Color ExpandIconBg { get; set; }

    /// <summary>
    /// Width of selection column
    /// </summary>
    [ReservedToken("Reserved: selection columns auto-measure their real selector and CellPadding; the legacy default width would narrow or clip that layout.")]
    public partial double SelectionColumnWidth { get; set; }

    /// <summary>
    /// Shadow separating the left frozen columns from the horizontally scrolling columns.
    /// </summary>
    public partial BoxShadows LeftFrozenShadows { get; set; }

    /// <summary>
    /// Shadow separating the right frozen columns from the horizontally scrolling columns.
    /// </summary>
    public partial BoxShadows RightFrozenShadows { get; set; }

    /// <summary>
    /// Background of the column header being dragged to a new position.
    /// </summary>
    public partial Color ColumnReorderActiveBg { get; set; }

    /// <summary>
    /// Space around the pagination control below the grid at regular density.
    /// </summary>
    public partial Thickness PaginationMargin { get; set; }

    /// <summary>
    /// Space around the pagination control below a small grid.
    /// </summary>
    public partial Thickness PaginationMarginSM { get; set; }

    /// <summary>
    /// Outer spacing of the pagination panel in the regular grid layout.
    /// </summary>
    public partial Thickness PaginationPanelMargin { get; set; }

    /// <summary>
    /// Outer spacing of the pagination panel in the small grid layout.
    /// </summary>
    public partial Thickness PaginationPanelMarginSM { get; set; }

    #region Detail and header glyphs

    /// <summary>
    /// Reserved margin for an expander placed within the row indentation.
    /// </summary>
    [ReservedToken("Reserved for the Ant Design inline row-indent expander layout; AtomUI uses a dedicated detail column.")]
    public partial Thickness ExpandIconMargin { get; set; }
    /// <summary>
    /// Reserved half-size of the interior of an inline row expander.
    /// </summary>
    [ReservedToken("Reserved for the Ant Design CSS expander positioning model; AtomUI centers fixed plus/minus glyphs in a Canvas.")]
    public partial double ExpandIconHalfInner { get; set; }
    /// <summary>
    /// Reserved unscaled size of the inline row expander.
    /// </summary>
    [ReservedToken("Reserved for the Ant Design unscaled CSS expander box; AtomUI uses SizeMS for its final box size.")]
    public partial double ExpandIconSize { get; set; }
    /// <summary>
    /// Reserved scale factor of the inline row expander.
    /// </summary>
    [ReservedToken("Reserved for the Ant Design CSS expander transform; AtomUI sizes its centered expander directly.")]
    public partial double ExpandIconScale { get; set; }
    /// <summary>
    /// Width and height of each ascending or descending sort glyph in a column header.
    /// </summary>
    public partial double SortIconSize { get; set; }
    /// <summary>
    /// Size of the drop indicator shown when rows are reordered.
    /// </summary>
    public partial double RowReorderIndicatorSize { get; set; }
    /// <summary>
    /// Resting foreground of the sort and filter glyphs in a column header.
    /// </summary>
    public partial Color HeaderIconColor { get; set; }
    /// <summary>
    /// Pointer-over foreground of the sort and filter glyphs. Pair with <see cref="HeaderIconColor" />.
    /// </summary>
    public partial Color HeaderIconHoverColor { get; set; }
    /// <summary>
    /// Space around the stack of sort arrows in a column header.
    /// </summary>
    public partial Thickness SortIndicatorLayoutMargin { get; set; }
    /// <summary>
    /// Padding that centers the filter glyph in the header’s filter button.
    /// </summary>
    public partial Thickness FilterIndicatorPadding { get; set; }

    #endregion

    #region Effective table theme
    /// <summary>
    /// Effective font size for headers and cells at the default grid size.
    /// </summary>
    public partial double TableFontSize { get; set; }
    /// <summary>
    /// Fill of the regular table body and unselected rows.
    /// </summary>
    public partial Color TableBg { get; set; }
    /// <summary>
    /// Corner radius of the outer grid frame.
    /// </summary>
    public partial CornerRadius TableRadius { get; set; }
    /// <summary>
    /// Effective cell padding at the default grid size.
    /// </summary>
    public partial Thickness TablePadding { get; set; }
    /// <summary>
    /// Effective cell padding at the middle grid size.
    /// </summary>
    public partial Thickness TablePaddingMiddle { get; set; }
    /// <summary>
    /// Effective cell padding at the small grid size.
    /// </summary>
    public partial Thickness TablePaddingSmall { get; set; }
    /// <summary>
    /// Stroke of the table frame and its row and cell separators.
    /// </summary>
    public partial Color TableBorderColor { get; set; }
    /// <summary>
    /// Foreground of the header text drawn over <see cref="TableHeaderBg" />.
    /// </summary>
    public partial Color TableHeaderTextColor { get; set; }
    /// <summary>
    /// Fill of column and row headers. Pair with <see cref="TableHeaderTextColor" />.
    /// </summary>
    public partial Color TableHeaderBg { get; set; }
    /// <summary>
    /// Foreground of the footer text drawn over <see cref="TableFooterBg" />.
    /// </summary>
    public partial Color TableFooterTextColor { get; set; }
    /// <summary>
    /// Fill of the footer. Pair with <see cref="TableFooterTextColor" />.
    /// </summary>
    public partial Color TableFooterBg { get; set; }
    /// <summary>
    /// Stroke that separates adjacent column header cells.
    /// </summary>
    public partial Color TableHeaderCellSplitColor { get; set; }
    /// <summary>
    /// Fill of the column header for an active sort.
    /// </summary>
    public partial Color TableHeaderSortBg { get; set; }
    /// <summary>
    /// Fill of the sorted header while the pointer is over it.
    /// </summary>
    public partial Color TableHeaderSortHoverBg { get; set; }
    /// <summary>
    /// Cell fill used in the sorted column of the table body.
    /// </summary>
    public partial Color TableBodySortBg { get; set; }
    /// <summary>
    /// Reserved sorted-column header fill for a separately rendered sticky header.
    /// </summary>
    [ReservedToken("Reserved alias for the separate sticky header surface that AtomUI does not create.")]
    public partial Color TableFixedHeaderSortActiveBg { get; set; }
    /// <summary>
    /// Background of a header filter button while its popup is open.
    /// </summary>
    public partial Color TableHeaderFilterActiveBg { get; set; }
    /// <summary>
    /// Fill of the filter popup surface behind its options and actions.
    /// </summary>
    public partial Color TableFilterDropdownBg { get; set; }
    /// <summary>
    /// Gap between the action buttons at the bottom of the filter popup.
    /// </summary>
    public partial double TableFilterButtonSpacing { get; set; }
    /// <summary>
    /// Space around the footer action group in the filter popup.
    /// </summary>
    public partial Thickness TableFilterButtonContainerMargin { get; set; }
    /// <summary>
    /// Space around the separator above the filter popup actions.
    /// </summary>
    public partial Thickness TableFilterButtonLayoutSeparatorMargin { get; set; }

    /// <summary>
    /// Reserved viewport height for a constrained filter popup.
    /// </summary>
    [ReservedToken("Reserved for a constrained filter dropdown viewport; the current flyout measures its actual items.")]
    public partial double TableFilterDropdownHeight { get; set; }
    /// <summary>
    /// Width of the menu or tree filter popup.
    /// </summary>
    public partial double TableFilterDropdownWidth { get; set; }
    /// <summary>
    /// Padding around the option list and action area of the filter popup.
    /// </summary>
    public partial Thickness TableFilterDropdownPadding { get; set; }
    /// <summary>
    /// Reserved width of a search editor in the filter popup.
    /// </summary>
    [ReservedToken("Reserved for a filter search editor; the current filter flyout has no search editor.")]
    public partial double TableFilterDropdownSearchWidth { get; set; }

    /// <summary>
    /// Fill of an unselected body row under the pointer.
    /// </summary>
    public partial Color TableRowHoverBg { get; set; }
    /// <summary>
    /// Fill of the selected body row at rest, under <see cref="RowSelectedColor" />.
    /// </summary>
    public partial Color TableSelectedRowBg { get; set; }
    /// <summary>
    /// Fill of the selected body row under the pointer, under <see cref="RowSelectedColor" />.
    /// </summary>
    public partial Color TableSelectedRowHoverBg { get; set; }
    /// <summary>
    /// Effective font size for headers and cells at the middle grid size.
    /// </summary>
    public partial double TableFontSizeMiddle { get; set; }
    /// <summary>
    /// Effective font size for headers and cells at the small grid size.
    /// </summary>
    public partial double TableFontSizeSmall { get; set; }
    /// <summary>
    /// Reserved fixed width for the row selection column, which usually measures its content.
    /// </summary>
    [ReservedToken("Reserved alias: selection column width follows automatic selector and CellPadding measurement, or the explicit column Width.")]
    public partial double TableSelectionColumnWidth { get; set; }
    /// <summary>
    /// Fill of the expand or collapse button in the row detail column.
    /// </summary>
    public partial Color TableExpandIconBg { get; set; }
    /// <summary>
    /// Reserved fixed width of the row detail toggler column.
    /// </summary>
    [ReservedToken("Reserved: detail columns auto-measure the expander and CellPadding; applying the legacy fixed width changes the existing layout.")]
    public partial double TableExpandColumnWidth { get; set; }
    /// <summary>
    /// Fill of the body row while its details are expanded.
    /// </summary>
    public partial Color TableExpandedRowBg { get; set; }

    /// <summary>
    /// Corner radius of the header at the intersection of the row and column header bands.
    /// </summary>
    public partial CornerRadius TableTopLeftColumnCornerRadius { get; set; }

    #endregion
}
