namespace AtomUI.Desktop.Controls;

/// <summary>
/// Chooses between a calendar grid of fixed-size cells and cells that leave room for the cell content.
/// The viewer passes the value down to its panels and cells.
/// </summary>
public enum DateViewerPresentation
{
    /// <summary>
    /// Cells of a fixed size in a panel of fixed width, as in a calendar. This is the default.
    /// </summary>
    Compact,
    /// <summary>
    /// Cells that leave room for the cell content: the date value moves to the top right corner, the cell is at least
    /// <c>ContentCellMinHeight</c> high and draws a line in the split color along its top edge. The panel has no
    /// fixed width, so the viewer stretches to the width it is given.
    /// </summary>
    Content
}
