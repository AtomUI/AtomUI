// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

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
    /// Arranges the content of the <see cref="T:AtomUI.Desktop.Controls.DataGridCellsPresenter" />.
    /// </summary>
    /// <returns>
    /// The actual size used by the <see cref="T:AtomUI.Desktop.Controls.DataGridCellsPresenter" />.
    /// </returns>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
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
        var    visibleColumnCount = 0;
        var    hasRightFrozen     = false;
        var columns              = OwningGrid.ColumnsInternal;
        int displayedColumnCount = columns.GetDisplayedColumnCount();
        // 需要先计算出 frozenRightEdge
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

            visibleColumnCount++;
        }
        
        var maxOffsetX = finalSize.Width - frozenRightEdge;
        
        var visibleColumnIndex = 0;
        // left and normal
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible)
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
                cell.IsShowFrozenShadow   =  (visibleColumnIndex == OwningGrid.LeftFrozenColumnCount - 1) && OwningGrid.HorizontalOffset > 0;
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
            if (!column.IsRightFrozen)
            {
                visibleColumnIndex++;
            }
        }

        if (hasRightFrozen)
        {
            visibleColumnIndex = visibleColumnCount - 1;
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
                    var horizontalScrollBarVisible = OwningGrid.HorizontalScrollBar?.IsVisible ?? false;
                    cell.IsShowFrozenShadow = (visibleColumnIndex == visibleColumnCount - OwningGrid.RightFrozenColumnCount) && 
                                              horizontalScrollBarVisible &&
                                              DataGridHelper.AreLessAt3Decimals(OwningGrid.HorizontalOffset, maxOffsetX);
                    if (cell.IsVisible)
                    {
                        cell.Arrange(new Rect(cellLeftEdge, 0, column.LayoutRoundedWidth, finalSize.Height));
                    }
                    scrollingLeftEdge                      += column.ActualWidth;
                    column.IsInitialDesiredWidthDetermined =  true;
                    visibleColumnIndex--;
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
    /// Measures the children of a <see cref="T:AtomUI.Desktop.Controls.DataGridCellsPresenter" /> to 
    /// prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)" /> pass.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
    /// </param>
    /// <returns>
    /// The size that the <see cref="T:AtomUI.Desktop.Controls.DataGridCellsPresenter" /> determines it needs during layout, based on its calculations of child object allocated sizes.
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
