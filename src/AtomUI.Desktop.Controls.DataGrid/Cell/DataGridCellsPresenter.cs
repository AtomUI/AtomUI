// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Panel in the template of a <see cref="DataGridRow" /> that measures and arranges the cells of the
/// row, including its filler cell, with column virtualization, clipping against frozen columns and
/// support for auto-sized columns. It implements <c>IChildIndexProvider</c> with the display index of
/// the cell's column as child index; the filler cell is not counted as a child.
/// </summary>
public sealed class DataGridCellsPresenter : Panel, IChildIndexProvider
{
    internal DataGrid? OwningGrid => OwningRow?.OwningGrid;
    
    internal DataGridRow? OwningRow { get; set; }
    internal DataGridRowsPresenter? OwningRowsPresenter { get; set; }
    
    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add => _childIndexChanged += value;
        remove => _childIndexChanged -= value;
    }
    
    private double _fillerLeftEdge;
    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;
    
    // The desired height needs to be cached due to column virtualization; otherwise, the cells
    // would grow and shrink as the DataGrid scrolls horizontally
    private double _desiredHeight;
    
    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        return child is DataGridCell cell
            ? cell.OwningColumn?.DisplayIndex ?? -1
            : throw new InvalidOperationException("Invalid cell type");
    }
    
    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = Children.Count - 1; // Adjust for filler column
        return true;
    }
    
    /// <summary>
    /// Arranges the visible cells of the row on one line: left-frozen columns against the left edge,
    /// right-frozen columns against the right edge, scrolling columns at the horizontal scroll offset
    /// and the filler cell after the last column. Scrolling cells that slide under frozen columns are
    /// clipped. Without an owning grid or rows presenter the base layout is used, and while the grid is
    /// auto-sizing its columns this method only completes that sizing and then uses the base layout.
    /// </summary>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
    /// <returns>
    /// <paramref name="finalSize" />, or the result of the base layout in the cases described above.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (OwningGrid == null || OwningRowsPresenter == null)
        {
            return base.ArrangeOverride(finalSize);
        }
        
        Debug.Assert(OwningRow != null);

        if (OwningGrid.AutoSizingColumns)
        {
            // When we initially load an auto-column, we have to wait for all the rows to be measured
            // before we know its final desired size.  We need to trigger a new round of measures now
            // that the final sizes have been calculated.
            OwningGrid.CompleteAutoSizing(OwningGrid.CellsWidth);
            return base.ArrangeOverride(finalSize);
        }

        double frozenLeftEdge  = 0;
        double frozenRightEdge = OwningRowsPresenter.DesiredSize.Width;
        double realFrozenRightEdge = OwningRowsPresenter.DesiredSize.Width;

        var    rowHeader      = OwningRow.HeaderCell;
        double rowHeaderWidth = 0.0;
        if (rowHeader != null)
        {
            rowHeaderWidth = rowHeader.DesiredSize.Width;
        }
        frozenRightEdge     -= rowHeaderWidth;
        realFrozenRightEdge -= rowHeaderWidth;
        
        double scrollingLeftEdge  = -OwningGrid.HorizontalOffset;
        var    hasRightFrozen     = false;
        var columns              = OwningGrid.ColumnsInternal;
        var lastLeftFrozenColumn  = columns.LastVisibleLeftFrozenColumn;
        var firstRightFrozenColumn = columns.FirstVisibleRightFrozenColumn;
        int displayedColumnCount = columns.GetDisplayedColumnCount();
        // Compute the right edge of the frozen area first; clipping the scrolling cells below depends on it.
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible)
            {
                continue;
            }

            if (column.IsRightFrozen)
            {
                realFrozenRightEdge -= column.ActualWidth;
                hasRightFrozen      =  true;
            }
        }
        
        
        // Left-frozen and scrolling columns only. Right-frozen columns are arranged and added to the
        // scrolling left edge exactly once, in the right-hand loop below.
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible || column.IsRightFrozen)
            {
                continue;
            }

            double       cellLeftEdge;
            DataGridCell cell = OwningRow.Cells[column.Index];
            Debug.Assert(cell.OwningColumn == column);
            if (column.IsLeftFrozen)
            {
                cellLeftEdge = frozenLeftEdge;
                // This can happen before or after clipping because frozen cells aren't clipped
                frozenLeftEdge            += column.ActualWidth;
                cell.IsFrozen             =  true;
                cell.FrozenShadowPosition =  FrozenColumnShadowPosition.Right;
                cell.IsShowFrozenShadow   =  column == lastLeftFrozenColumn && OwningGrid.IsLeftFrozenShadowVisible;
            }
 
            else
            {
                cellLeftEdge  = scrollingLeftEdge;
                cell.IsFrozen = false;
            }
            if (cell.IsVisible)
            {
                cell.Arrange(new Rect(cellLeftEdge, 0, column.LayoutRoundedWidth, finalSize.Height));
                var cellRightEdge = cell.Bounds.Right;
                EnsureCellClip(cell, column.ActualWidth, finalSize.Height, frozenLeftEdge, realFrozenRightEdge, scrollingLeftEdge, cellRightEdge);
            }
            scrollingLeftEdge                      += column.ActualWidth;
            column.IsInitialDesiredWidthDetermined =  true;
        }

        if (hasRightFrozen)
        {
            // right
            for (int displayIndex = displayedColumnCount - 1; displayIndex >= 0; displayIndex--)
            {
                DataGridColumn column       = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
                if (!column.IsVisible)
                {
                    continue;
                }

                double         cellLeftEdge = 0.0;
                DataGridCell   cell         = OwningRow.Cells[column.Index];
                Debug.Assert(cell.OwningColumn == column);
                if (column.IsRightFrozen)
                {
                    frozenRightEdge           -= column.ActualWidth;
                    // This can happen before or after clipping because frozen cells aren't clipped
                    cellLeftEdge              = frozenRightEdge;
                    cell.IsFrozen             = true;
                    cell.FrozenShadowPosition = FrozenColumnShadowPosition.Left;
                    cell.IsShowFrozenShadow = column == firstRightFrozenColumn && OwningGrid.IsRightFrozenShadowVisible;
                    if (cell.IsVisible)
                    {
                        cell.Arrange(new Rect(cellLeftEdge, 0, column.LayoutRoundedWidth, finalSize.Height));
                        // Frozen columns are not clipped; clear the clip that was set while the column was still a
                        // scrolling column.
                        cell.ClearClipGeometry();
                    }
                    // Right-frozen columns still advance the scrolling left edge: the filler cell is placed after
                    // every column; without a horizontal offset they sit at their natural position.
                    scrollingLeftEdge                      += column.ActualWidth;
                    column.IsInitialDesiredWidthDetermined =  true;
                }
            }
        }
        
        _fillerLeftEdge = scrollingLeftEdge;
        Debug.Assert(OwningGrid.ColumnsInternal.FillerColumn != null);
        OwningRow.FillerCell.Arrange(new Rect(_fillerLeftEdge, 0, OwningGrid.ColumnsInternal.FillerColumn.FillerWidth, finalSize.Height));

        return finalSize;
    }
    
    private static void EnsureCellClip(DataGridCell cell, double width, double height, double frozenLeftEdge, double frozenRightEdge, double cellLeftEdge, double cellRightEdge)
    {
        // Clip the cell only if it's scrolled under frozen columns.  Unfortunately, we need to clip in this case
        // because cells could be transparent
        if (cell.OwningColumn != null && !cell.OwningColumn.IsFrozen && frozenLeftEdge > cellLeftEdge &&
                 cellRightEdge > frozenRightEdge)
        {
            double            xClip = Math.Round(Math.Min(width, frozenLeftEdge - cellLeftEdge));
            cell.UpdateClipGeometry(new Rect(
                xClip,
                0,
                Math.Max(0, width - (frozenLeftEdge - cellLeftEdge) - (cellRightEdge - frozenRightEdge)),
                height));
        }
        else if (cell.OwningColumn != null && !cell.OwningColumn.IsFrozen && frozenLeftEdge > cellLeftEdge)
        {
            double            xClip = Math.Round(Math.Min(width, frozenLeftEdge - cellLeftEdge));
            cell.UpdateClipGeometry(new Rect(xClip, 0, Math.Max(0, width - xClip), height));
        }
        else if (cell.OwningColumn != null && !cell.OwningColumn.IsFrozen && cellRightEdge > frozenRightEdge)
        {
            double            xClip = Math.Round(Math.Min(width, cellRightEdge - frozenRightEdge));
            cell.UpdateClipGeometry(new Rect(0, 0, Math.Max(0, width - xClip), height));
        }
        else
        {
            cell.ClearClipGeometry();
        }
    }
    
    /// <summary>
    /// Called when the <c>Children</c> collection of the panel changes. Calls the base implementation,
    /// then raises <c>ChildIndexesReset</c> through <c>IChildIndexProvider.ChildIndexChanged</c> so that
    /// consumers re-query the child indexes.
    /// </summary>
    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);

        InvalidateChildIndex();
    }

    private static void EnsureCellDisplay(DataGridCell cell, bool displayColumn)
    {
        if (cell.IsCurrent)
        {
            if (displayColumn)
            {
                cell.IsVisible = true;
                cell.Clip      = null;
            }
            else
            {
                // Clip
                cell.UpdateClipGeometry(default);
            }
        }
        else
        {
            cell.IsVisible = displayColumn;
        }
    }

    internal void EnsureFillerVisibility()
    {
        Debug.Assert(OwningGrid != null);
        Debug.Assert(OwningRow != null);
        DataGridFillerColumn? fillerColumn  = OwningGrid.ColumnsInternal.FillerColumn;
        Debug.Assert(fillerColumn != null);
        bool                 newVisibility = fillerColumn.IsActive;
        if (OwningRow.FillerCell.IsVisible != newVisibility)
        {
            OwningRow.FillerCell.IsVisible = newVisibility;
            if (newVisibility)
            {
                OwningRow.FillerCell.Arrange(new Rect(_fillerLeftEdge, 0, fillerColumn.FillerWidth, Bounds.Height));
            }
        }

        // This must be done after the Filler visibility is determined.  This also must be done
        // regardless of whether or not the filler visibility actually changed values because
        // we could scroll in a cell that didn't have EnsureGridLine called yet
        DataGridColumn? lastVisibleColumn = OwningGrid.ColumnsInternal.LastVisibleColumn;
        if (lastVisibleColumn != null)
        {
            DataGridCell cell = OwningRow.Cells[lastVisibleColumn.Index];
            cell.EnsureGridLine(lastVisibleColumn);
        }
    }

    /// <summary>
    /// Measures the cells that lie in the horizontally visible range, and falls back to the base layout
    /// when the presenter has no owning grid or row. Every cell of the first row is measured so that the
    /// horizontal scrollbar is accurate, and with star sizing all cells are measured again once the
    /// final column widths are known.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this element can give to its children. Only the base layout uses it;
    /// cells are measured with the width of their column and with the row height of the grid, or with
    /// an unconstrained height when that row height is not set.
    /// </param>
    /// <returns>
    /// The width of the visible columns and the desired row height: the row height of the grid, or the
    /// height of the tallest measured cell when that row height is <see cref="double.NaN" />.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OwningGrid == null || OwningRow == null)
        {
            return base.MeasureOverride(availableSize);
        }

        bool   autoSizeHeight;
        double measureHeight;
        if (double.IsNaN(OwningGrid.RowHeight))
        {
            // No explicit height values were set so we can autosize
            autoSizeHeight = true;
            // We need to invalidate desired height in order to grow or shrink as needed
            InvalidateDesiredHeight();
            measureHeight = double.PositiveInfinity;
        }
        else
        {
            _desiredHeight  = OwningGrid.RowHeight;
            measureHeight  = _desiredHeight;
            autoSizeHeight = false;
        }

        double frozenLeftEdge    = 0;
        double totalDisplayWidth = 0;
        double scrollingLeftEdge = -OwningGrid.HorizontalOffset;
        var    columns           = OwningGrid.ColumnsInternal;
        columns.EnsureVisibleEdgedColumnsWidth();
        DataGridColumn? lastVisibleColumn = columns.LastVisibleColumn;
        int displayedColumnCount = columns.GetDisplayedColumnCount();
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible)
            {
                continue;
            }

            DataGridCell cell = OwningRow.Cells[column.Index];
            // Measure the entire first row to make the horizontal scrollbar more accurate
            bool shouldDisplayCell = ShouldDisplayCell(column, frozenLeftEdge, scrollingLeftEdge) || OwningRow.Index == 0;
            EnsureCellDisplay(cell, shouldDisplayCell);
            if (shouldDisplayCell)
            {
                DataGridLength columnWidth   = column.Width;
                bool           autoGrowWidth = columnWidth.IsSizeToCells || columnWidth.IsAuto;
                if (column != lastVisibleColumn)
                {
                    cell.EnsureGridLine(lastVisibleColumn);
                }

                // If we're not using star sizing or the current column can't be resized,
                // then just set the display width according to the column's desired width
                if (!OwningGrid.UsesStarSizing || (!column.ActualCanUserResize && !column.Width.IsStar))
                {
                    // In the edge-case where we're given infinite width and we have star columns, the 
                    // star columns grow to their predefined limit of 10,000 (or their MaxWidth)
                    double newDisplayWidth = column.Width.IsStar ?
                        Math.Min(column.ActualMaxWidth, DataGrid.MaximumStarColumnWidth) :
                        Math.Max(column.ActualMinWidth, Math.Min(column.ActualMaxWidth, column.Width.DesiredValue));
                    column.SetWidthDisplayValue(newDisplayWidth);
                }

                // If we're auto-growing the column based on the cell content, we want to measure it at its maximum value
                if (autoGrowWidth)
                {
                    cell.Measure(new Size(column.ActualMaxWidth, measureHeight));
          
                    OwningGrid.AutoSizeColumn(column, cell.DesiredSize.Width);
                    column.ComputeLayoutRoundedWidth(totalDisplayWidth);
                }
                else if (!OwningGrid.UsesStarSizing)
                {
                    column.ComputeLayoutRoundedWidth(scrollingLeftEdge);
                    cell.Measure(new Size(column.LayoutRoundedWidth, measureHeight));
                }

                // We need to track the largest height in order to auto-size
                if (autoSizeHeight)
                {
                    _desiredHeight = Math.Max(_desiredHeight, cell.DesiredSize.Height);
                }
            }

            if (column.IsFrozen)
            {
                frozenLeftEdge += column.ActualWidth;
            }
            scrollingLeftEdge += column.ActualWidth;
            totalDisplayWidth += column.ActualWidth;
        }

        // If we're using star sizing (and we're not waiting for an auto-column to finish growing)
        // then we will resize all the columns to fit the available space.
        if (OwningGrid.UsesStarSizing && !OwningGrid.AutoSizingColumns)
        {
            // Since we didn't know the final widths of the columns until we resized,
            // we waited until now to measure each cell
            double leftEdge = 0;
            if (autoSizeHeight)
            {
                _desiredHeight = 0;
            }

            for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
            {
                DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
                if (!column.IsVisible)
                {
                    continue;
                }

                DataGridCell cell = OwningRow.Cells[column.Index];
                column.ComputeLayoutRoundedWidth(leftEdge);
                cell.Measure(new Size(column.LayoutRoundedWidth, measureHeight));
                if (autoSizeHeight)
                {
                    _desiredHeight = Math.Max(_desiredHeight, cell.DesiredSize.Height);
                }
                leftEdge += column.ActualWidth;
            }
        }

        // Measure FillerCell, we're doing it unconditionally here because we don't know if we'll need the filler
        // column and we don't want to cause another Measure if we do
        OwningRow.FillerCell.Measure(new Size(double.PositiveInfinity, _desiredHeight));

        columns.EnsureVisibleEdgedColumnsWidth();
        return new Size(columns.VisibleEdgedColumnsWidth, _desiredHeight);
    }

    internal void Recycle()
    {
        // Clear out the cached desired height so it is not reused for other rows
        _desiredHeight = 0;
    }

    internal void InvalidateDesiredHeight()
    {
        _desiredHeight = 0;
    }

    internal void InvalidateChildIndex()
    {
        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
    }

    private bool ShouldDisplayCell(DataGridColumn column, double frozenLeftEdge, double scrollingLeftEdge)
    {
        if (!column.IsVisible)
        {
            return false;
        }
        Debug.Assert(OwningGrid != null);
        scrollingLeftEdge += OwningGrid.HorizontalAdjustment;
        double leftEdge  = column.IsFrozen ? frozenLeftEdge : scrollingLeftEdge;
        double rightEdge = leftEdge + column.ActualWidth;
        return 
            MathUtils.GreaterThan(rightEdge, 0) &&
            MathUtils.LessThanOrClose(leftEdge, OwningGrid.CellsWidth) &&
            MathUtils.GreaterThan(rightEdge, frozenLeftEdge); // scrolling column covered up by frozen column(s)
    }
}
