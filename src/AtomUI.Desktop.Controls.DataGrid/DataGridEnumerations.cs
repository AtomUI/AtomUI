// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Reports the progress of the latest data load of a <see cref="DataGrid" /> and whether it succeeded.
/// </summary>
public enum DataGridLoadState
{
    /// <summary>
    /// No load is in progress. This is the state before the first load.
    /// </summary>
    Idle,
    /// <summary>
    /// A load is in progress and no data of an earlier load is shown; the loading indicator is shown instead.
    /// </summary>
    Loading,
    /// <summary>
    /// A load is in progress while the data of an earlier load is still shown.
    /// </summary>
    Refreshing,
    /// <summary>
    /// The latest load succeeded.
    /// </summary>
    Ready,
    /// <summary>
    /// The latest load failed.
    /// </summary>
    Error
}

/// <summary>
/// Used to specify action to take out of edit mode.
/// </summary>
public enum DataGridEditAction
{
    /// <summary>
    /// Cancel the changes.
    /// </summary>
    Cancel,

    /// <summary>
    /// Commit edited value.
    /// </summary>
    Commit
}

// Determines the location and visibility of the editing row.
internal enum DataGridEditingRowLocation
{
    Bottom = 0, // The editing row is collapsed below the displayed rows
    Inline = 1, // The editing row is visible and displayed
    Top = 2 // The editing row is collapsed above the displayed rows
}

/// <summary>
/// Determines whether the inner cells' vertical/horizontal gridlines are shown or not.
/// </summary>
[Flags]
public enum DataGridGridLinesVisibility
{
    /// <summary>
    /// Shows no grid lines between cells.
    /// </summary>
    None = 0,
    /// <summary>
    /// Shows the horizontal grid lines between the rows.
    /// </summary>
    Horizontal = 1,
    /// <summary>
    /// Shows the vertical grid lines between the columns.
    /// </summary>
    Vertical = 2,
    /// <summary>
    /// Shows the horizontal and the vertical grid lines.
    /// </summary>
    All = 3,
}

/// <summary>
/// Determines at which edges of the grid a pager is shown.
/// </summary>
[Flags]
public enum DataGridPaginationVisibility
{
    /// <summary>
    /// Shows no pager.
    /// </summary>
    None = 0x00,
    /// <summary>
    /// Shows the pager at the top edge of the grid.
    /// </summary>
    Top = 0x01,
    /// <summary>
    /// Shows the pager at the bottom edge of the grid; this is the default.
    /// </summary>
    Bottom = 0x02,
    /// <summary>
    /// Shows a pager at the top and at the bottom edge of the grid.
    /// </summary>
    All = Top | Bottom,
}

/// <summary>
/// Names the scope of an edit that is committed or canceled, for example by
/// <see cref="DataGrid.CancelEdit(DataGridEditingUnit)" />.
/// </summary>
public enum DataGridEditingUnit
{
    /// <summary>
    /// The edit of the current cell.
    /// </summary>
    Cell = 0,
    /// <summary>
    /// The edits of the whole row that is being edited.
    /// </summary>
    Row = 1,
}

/// <summary>
/// Determines whether the row/column headers are shown or not.
/// </summary>
[Flags]
public enum DataGridHeadersVisibility
{
    /// <summary>
    /// Show Row, Column, and Corner Headers
    /// </summary>
    All = Row | Column,

    /// <summary>
    /// Show only Column Headers with top-right corner Header
    /// </summary>
    Column = 0x01,

    /// <summary>
    /// Show only Row Headers with bottom-left corner
    /// </summary>
    Row = 0x02,

    /// <summary>
    /// Don’t show any Headers
    /// </summary>
    None = 0x00
}

/// <summary>
/// The sort directions that a column or a data field supports, as a set of flags.
/// </summary>
[Flags]
public enum DataGridSortDirections
{
    /// <summary>
    /// No sort direction is supported.
    /// </summary>
    None = 0x00,
    /// <summary>
    /// Ascending order is supported.
    /// </summary>
    Ascending = 0x01,
    /// <summary>
    /// Descending order is supported.
    /// </summary>
    Descending = 0x02,
    /// <summary>
    /// Both sort directions are supported; this is the default for a column.
    /// </summary>
    All = Ascending | Descending
}

/// <summary>
/// Determines which rows show their details section.
/// </summary>
public enum DataGridRowDetailsVisibilityMode
{
    /// <summary>
    /// The grid does not decide whether the row details are visible; the application does, so they stay hidden
    /// until it shows them. <c>DataGridDetailExpanderColumn</c> can be used only in this mode.
    /// </summary>
    Collapsed = 2,
    /// <summary>
    /// The details of every row are shown.
    /// </summary>
    Visible = 1,
    /// <summary>
    /// The details of a row are shown only while the row is selected.
    /// </summary>
    VisibleWhenSelected = 0
}

/// <summary>
/// Determines the type of action to take when selecting items
/// </summary>
internal enum DataGridSelectionAction
{
    AddCurrentToSelection,
    None,
    RemoveCurrentFromSelection,
    SelectCurrent,
    SelectFromAnchorToCurrent
}

/// <summary>
/// Determines the selection model
/// </summary>
public enum DataGridSelectionMode
{
    /// <summary>
    /// No row can be selected.
    /// </summary>
    None = 0,
    /// <summary>
    /// Several rows can be selected at the same time, for example a range with Shift or single rows with Ctrl.
    /// </summary>
    Extended = 1,
    /// <summary>
    /// Only one row can be selected at a time.
    /// </summary>
    Single = 2
}

/// <summary>
/// Determines which click inside a row changes the row selection.
/// </summary>
public enum DataGridSelectTriggerType
{
    /// <summary>
    /// A click on a cell of the row changes the selection.
    /// </summary>
    Cell,
    /// <summary>
    /// Only a click on the select indicator changes the selection; this is the default.
    /// </summary>
    SelectIndicator,
    /// <summary>
    /// A click anywhere on the row changes the selection.
    /// </summary>
    Row
}

/// <summary>
/// Determines which flyout presents the filter of a column.
/// </summary>
public enum DataGridFilterPresenterMode
{
    /// <summary>
    /// The filter values are presented in a menu flyout (<c>DataGridMenuFilterFlyout</c>); this is the default.
    /// </summary>
    Menu,
    /// <summary>
    /// The filter values are presented in a tree (<c>DataGridTreeFilterFlyout</c>).
    /// </summary>
    Tree
}

/// <summary>
/// Determines how many values the user can select in the filter of a column.
/// </summary>
public enum DataGridFilterSelectionMode
{
    /// <summary>
    /// The user can select one filter value at a time.
    /// </summary>
    Single,
    /// <summary>
    /// The user can select several filter values at the same time; this is the default.
    /// </summary>
    Multiple
}

/// <summary>
/// Determines which user action applies the filter of a column.
/// </summary>
public enum DataGridFilterApplyMode
{
    /// <summary>
    /// The filter is applied only when the user confirms it; this is the default.
    /// </summary>
    Confirm,
    /// <summary>
    /// The filter is applied when the filter flyout is dismissed without the user confirming it.
    /// </summary>
    Close,
    /// <summary>
    /// The filter is applied every time the user changes the filter selection.
    /// </summary>
    SelectionChanged
}
