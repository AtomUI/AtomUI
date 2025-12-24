// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public sealed class DataGridColumnHeadersPresenter : Panel, IChildIndexProvider
{
    private Control? _dragIndicator;
    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;

    /// <summary>
    /// Tracks which column is currently being dragged.
    /// </summary>
    internal DataGridColumn? DragColumn { get; set; }

    /// <summary>
    /// The current drag indicator control.  This value is null if no column is being dragged.
    /// </summary>
    internal Control? DragIndicator
    {
        get => _dragIndicator;
        set
        {
            if (value != _dragIndicator)
            {
                if (_dragIndicator != null)
                {
                    if (Children.Contains(_dragIndicator))
                    {
                        Children.Remove(_dragIndicator);
                    }
                }
                _dragIndicator = value;
                if (_dragIndicator != null)
                {
                    Children.Add(_dragIndicator);
                }
            }
        }
    }

    /// <summary>
    /// The distance, in pixels, that the DragIndicator should be positioned away from the corresponding DragColumn.
    /// </summary>
    internal Double DragIndicatorOffset { get; set; }

    internal DataGrid? OwningGrid { get; set; }

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add => _childIndexChanged += value;
        remove => _childIndexChanged -= value;
    }

    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        return child is DataGridColumnHeader header
            ? header.OwningColumn?.DisplayIndex ?? -1
            : throw new InvalidOperationException("Invalid cell type");
    }

    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = Children.Count - 1; // Adjust for filler column
        return true;
    }

    // protected override AutomationPeer OnCreateAutomationPeer()
    // {
    //     return new DataGridColumnHeadersPresenterAutomationPeer(this);
    // }

    /// <summary>
    /// Arranges the content of the <see cref="T:AtomUI.Desktop.Controls.DataGridColumnHeadersPresenter" />.
    /// </summary>
    /// <returns>
    /// The actual size used by the <see cref="T:AtomUI.Desktop.Controls.DataGridColumnHeadersPresenter" />.
    /// </returns>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (OwningGrid == null)
        {
            return base.ArrangeOverride(finalSize);
        }

        if (OwningGrid.AutoSizingColumns)
        {
            // When we initially load an auto-column, we have to wait for all the rows to be measured
            // before we know its final desired size.  We need to trigger a new round of measures now
            // that the final sizes have been calculated.
            OwningGrid.AutoSizingColumns = false;
            return base.ArrangeOverride(finalSize);
        }

        double dragIndicatorLeftEdge = 0;
        double frozenLeftEdge        = 0;
        double frozenRightEdge       = DesiredSize.Width;
        double realFrozenRightEdge   = DesiredSize.Width;
        
        double scrollingLeftEdge  = -OwningGrid.HorizontalOffset;
        var    visibleColumnIndex = 0;
        var    visibleColumnCount = OwningGrid.ColumnsInternal.GetDisplayedColumnCount();
        var    hasRightFrozen     = false;
        
        // 需要先计算出 frozenRightEdge
        foreach (DataGridColumn column in OwningGrid.ColumnsInternal.GetVisibleColumns())
        {
            if (column.IsRightFrozen)
            {
                realFrozenRightEdge -= column.ActualWidth;
                hasRightFrozen      =  true;
            }
        }
        var visibleColumns     = OwningGrid.ColumnsInternal.GetVisibleColumns().ToList();
        // left and normal
         foreach (DataGridColumn dataGridColumn in visibleColumns)
        {
            DataGridColumnHeader columnHeader = dataGridColumn.HeaderCell;
            Debug.Assert(columnHeader.OwningColumn == dataGridColumn);
            if (dataGridColumn.IsLeftFrozen)
            {
                columnHeader.Arrange(new Rect(frozenLeftEdge, 0, dataGridColumn.LayoutRoundedWidth, finalSize.Height));
                columnHeader.Clip = null; // The layout system could have clipped this because it's not aware of our render transform
                if (DragColumn == dataGridColumn && DragIndicator != null)
                {
                    dragIndicatorLeftEdge = frozenLeftEdge + DragIndicatorOffset;
                }
                frozenLeftEdge += dataGridColumn.ActualWidth;
                columnHeader.IsFrozen             =  true;
                columnHeader.FrozenShadowPosition =  FrozenColumnShadowPosition.Right;
                columnHeader.IsShowFrozenShadow   =  (visibleColumnIndex == OwningGrid.LeftFrozenColumnCount - 1) && OwningGrid.HorizontalOffset > 0;
            }
            else
            {
                columnHeader.Arrange(new Rect(scrollingLeftEdge, 0, dataGridColumn.LayoutRoundedWidth, finalSize.Height));
                var columnHeaderRightEdge = columnHeader.Bounds.Right;
                EnsureColumnHeaderClip(columnHeader, dataGridColumn.ActualWidth, finalSize.Height, frozenLeftEdge, realFrozenRightEdge, scrollingLeftEdge, columnHeaderRightEdge);
                if (DragColumn == dataGridColumn && DragIndicator != null)
                {
                    dragIndicatorLeftEdge = scrollingLeftEdge + DragIndicatorOffset;
                }
                columnHeader.IsFrozen = false;
            }
            scrollingLeftEdge += dataGridColumn.ActualWidth;
            if (!dataGridColumn.IsRightFrozen)
            {
                visibleColumnIndex++;
            }
        }


        if (hasRightFrozen)
        {
            visibleColumnIndex = visibleColumns.Count - 1;
            // right
            for (var i = visibleColumns.Count - 1; i >= 0; i--)
            {
                DataGridColumn       dataGridColumn = visibleColumns[i];
                DataGridColumnHeader columnHeader   = dataGridColumn.HeaderCell;
                Debug.Assert(columnHeader.OwningColumn == dataGridColumn);
                if (dataGridColumn.IsRightFrozen)
                {
                    frozenRightEdge -= dataGridColumn.ActualWidth;
                    columnHeader.Arrange(new Rect(frozenRightEdge, 0, dataGridColumn.LayoutRoundedWidth, finalSize.Height));
                    columnHeader.Clip = null; // The layout system could have clipped this because it's not aware of our render transform
                    columnHeader.IsFrozen             =  true;
                    columnHeader.FrozenShadowPosition =  FrozenColumnShadowPosition.Left;
                    var horizontalScrollBarVisible = OwningGrid.HorizontalScrollBar?.IsVisible ?? false;
                    columnHeader.IsShowFrozenShadow   =  (visibleColumnIndex == visibleColumnCount - OwningGrid.RightFrozenColumnCount) && 
                                                         horizontalScrollBarVisible &&
                                                         OwningGrid.HorizontalOffset < OwningGrid.HorizontalMaximizeOffset;
                    visibleColumnIndex--;
                }
            }
        }
        
        if (DragColumn != null)
        {
            if (DragIndicator != null)
            {
                DragIndicator.IsVisible = true;
                var dragIndicatorRightEdge = DragIndicator.Bounds.Right;
                // EnsureColumnReorderingClip(DragIndicator, finalSize.Height, frozenLeftEdge, dragIndicatorLeftEdge);
                EnsureColumnReorderingClip(DragIndicator, finalSize.Height, frozenLeftEdge, realFrozenRightEdge, dragIndicatorLeftEdge, dragIndicatorRightEdge);

                var height = DragIndicator.Bounds.Height;
                if (height <= 0)
                {
                    height = DragIndicator.DesiredSize.Height;
                }
                
                DragIndicator.Arrange(new Rect(dragIndicatorLeftEdge, 0, DragIndicator.Bounds.Width, height));
            }
        }

        // Arrange filler
        OwningGrid.HandleFillerColumnWidthNeeded(finalSize.Width);
        DataGridFillerColumn? fillerColumn = OwningGrid.ColumnsInternal.FillerColumn;
        Debug.Assert(fillerColumn != null);
        if (fillerColumn.FillerWidth > 0)
        {
            fillerColumn.HeaderCell.IsVisible = true;
            fillerColumn.HeaderCell.Arrange(new Rect(scrollingLeftEdge, 0, fillerColumn.FillerWidth, finalSize.Height));
        }
        else
        {
            fillerColumn.HeaderCell.IsVisible = false;
        }

        // This needs to be updated after the filler column is configured
        DataGridColumn? lastVisibleColumn = OwningGrid.ColumnsInternal.LastVisibleColumn;
        if (lastVisibleColumn != null)
        {
            lastVisibleColumn.HeaderCell.UpdateSeparatorVisibility(lastVisibleColumn);
        }
        return finalSize;
    }

    private static void EnsureColumnHeaderClip(DataGridColumnHeader columnHeader, double width, double height,
                                               double frozenLeftEdge,
                                               double frozenRightEdge,
                                               double columnHeaderLeftEdge,
                                               double columnHeaderRightEdge)
    {
        // Clip the cell only if it's scrolled under frozen columns.  Unfortunately, we need to clip in this case
        // because cells could be transparent
        if (columnHeader.OwningColumn != null && 
            !columnHeader.OwningColumn.IsFrozen && 
            frozenLeftEdge > columnHeaderLeftEdge &&
            columnHeaderRightEdge > frozenRightEdge)
        {
            RectangleGeometry rg    = new RectangleGeometry();
            double            xClip = Math.Round(Math.Min(width, frozenLeftEdge - columnHeaderLeftEdge));
            rg.Rect   = new Rect(xClip, 0, Math.Max(0, width - (frozenLeftEdge - columnHeaderLeftEdge) - (columnHeaderRightEdge - frozenRightEdge)), height);
            columnHeader.Clip = rg;
        }
        else if (columnHeader.OwningColumn != null && !columnHeader.OwningColumn.IsFrozen && 
                 frozenLeftEdge > columnHeaderLeftEdge)
        {
            RectangleGeometry rg    = new RectangleGeometry();
            double            xClip = Math.Round(Math.Min(width, frozenLeftEdge - columnHeaderLeftEdge));
            rg.Rect           = new Rect(xClip, 0, Math.Max(0, width - xClip), height);
            columnHeader.Clip = rg;
        }
        else if (columnHeader.OwningColumn != null && !columnHeader.OwningColumn.IsFrozen && 
                 columnHeaderRightEdge > frozenRightEdge)
        {
            RectangleGeometry rg    = new RectangleGeometry();
            double            xClip = Math.Round(Math.Min(width, columnHeaderRightEdge - frozenRightEdge));
            rg.Rect           = new Rect(0, 0, Math.Max(0, width - xClip), height);
            columnHeader.Clip = rg;
        }
        else
        {
            columnHeader.Clip = null;
        }
        
    }
    
    private void EnsureColumnReorderingClip(Control control, double height, 
                                            double frozenLeftEdge,
                                            double frozenRightEdge,
                                            double columnHeaderLeftEdge,
                                            double columnHeaderRightEdge)
    {
        Debug.Assert(OwningGrid != null);
        Debug.Assert(DragColumn != null);
        double width     = control.Bounds.Width;
        
        RectangleGeometry? rg = null;
        if (frozenLeftEdge > columnHeaderLeftEdge)
        {
            rg = new RectangleGeometry();
            double xClip = Math.Min(width, frozenLeftEdge - columnHeaderLeftEdge);
            rg.Rect = new Rect(xClip, 0, width - xClip, height);
        }
        if (columnHeaderRightEdge >= frozenRightEdge)
        {
            if (rg == null)
            {
                rg = new RectangleGeometry();
            }
            rg.Rect = new Rect(rg.Rect.X, rg.Rect.Y, Math.Max(0, frozenRightEdge - columnHeaderLeftEdge - rg.Rect.X), height);
        }
        control.Clip = rg;
    }

    /// <summary>
    /// Clips the DragIndicator and DropLocationIndicator controls according to current ColumnHeaderPresenter constraints.
    /// </summary>
    /// <param name="control">The DragIndicator or DropLocationIndicator</param>
    /// <param name="height">The available height</param>
    /// <param name="frozenColumnsWidth">The width of the frozen column region</param>
    /// <param name="controlLeftEdge">The left edge of the control to clip</param>
    // private void EnsureColumnReorderingClip(Control control, double height, double frozenColumnsWidth, double controlLeftEdge)
    // {
    //     Debug.Assert(OwningGrid != null);
    //     Debug.Assert(DragColumn != null);
    //     double leftEdge  = 0;
    //     double rightEdge = OwningGrid.CellsWidth;
    //     double width     = control.Bounds.Width;
    //     if (DragColumn.IsFrozen)
    //     {
    //         // If we're dragging a frozen column, we want to clip the corresponding DragIndicator control when it goes
    //         // into the scrolling columns region, but not the DropLocationIndicator.
    //         if (control == DragIndicator)
    //         {
    //             rightEdge = Math.Min(rightEdge, frozenColumnsWidth);
    //         }
    //     }
    //     else if (OwningGrid.LeftFrozenColumnCount > 0)
    //     {
    //         // If we're dragging a scrolling column, we want to clip both the DragIndicator and the DropLocationIndicator
    //         // controls when they go into the frozen column range.
    //         leftEdge = frozenColumnsWidth;
    //     }
    //     RectangleGeometry? rg = null;
    //     if (leftEdge > controlLeftEdge)
    //     {
    //         rg = new RectangleGeometry();
    //         double xClip = Math.Min(width, leftEdge - controlLeftEdge);
    //         rg.Rect = new Rect(xClip, 0, width - xClip, height);
    //     }
    //     if (controlLeftEdge + width >= rightEdge)
    //     {
    //         if (rg == null)
    //         {
    //             rg = new RectangleGeometry();
    //         }
    //         rg.Rect = new Rect(rg.Rect.X, rg.Rect.Y, Math.Max(0, rightEdge - controlLeftEdge - rg.Rect.X), height);
    //     }
    //     control.Clip = rg;
    // }

    /// <summary>
    /// Measures the children of a <see cref="T:AtomUI.Desktop.Controls.DataGridColumnHeadersPresenter" /> to 
    /// prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)" /> pass.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
    /// </param>
    /// <returns>
    /// The size that the <see cref="T:AtomUI.Desktop.Controls.DataGridColumnHeadersPresenter" /> determines it needs during layout, based on its calculations of child object allocated sizes.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OwningGrid == null)
        {
            return base.MeasureOverride(availableSize);
        }
        if (!OwningGrid.IsColumnHeadersVisible)
        {
            return default;
        }
        double height = OwningGrid.ColumnHeaderHeight;
        bool   autoSizeHeight;
        if (double.IsNaN(height))
        {
            // No explicit height values were set so we can autosize
            height         = 0;
            autoSizeHeight = true;
        }
        else
        {
            autoSizeHeight = false;
        }

        double totalDisplayWidth = 0;
        OwningGrid.ColumnsInternal.EnsureVisibleEdgedColumnsWidth();
        DataGridColumn? lastVisibleColumn = OwningGrid.ColumnsInternal.LastVisibleColumn;
        foreach (DataGridColumn column in OwningGrid.ColumnsInternal.GetVisibleColumns())
        {
            // Measure each column header
            bool                 autoGrowWidth = column.Width.IsAuto || column.Width.IsSizeToHeader;
            DataGridColumnHeader columnHeader  = column.HeaderCell;
            columnHeader.IsSeparatorFullHeight = OwningGrid.AreVerticalGridLinesVisible;
            if (column != lastVisibleColumn)
            {
                columnHeader.UpdateSeparatorVisibility(lastVisibleColumn);
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

            // If we're auto-growing the column based on the header content, we want to measure it at its maximum value
            if (autoGrowWidth)
            {
                columnHeader.Measure(new Size(column.ActualMaxWidth, double.PositiveInfinity));
                OwningGrid.AutoSizeColumn(column, columnHeader.DesiredSize.Width);
                column.ComputeLayoutRoundedWidth(totalDisplayWidth);
            }
            else if (!OwningGrid.UsesStarSizing)
            {
                column.ComputeLayoutRoundedWidth(totalDisplayWidth);
                columnHeader.Measure(new Size(column.LayoutRoundedWidth, double.PositiveInfinity));
            }

            // We need to track the largest height in order to auto-size
            if (autoSizeHeight)
            {
                height = Math.Max(height, columnHeader.DesiredSize.Height);
            }
            totalDisplayWidth += column.ActualWidth;
        }

        // If we're using star sizing (and we're not waiting for an auto-column to finish growing)
        // then we will resize all the columns to fit the available space.
        if (OwningGrid.UsesStarSizing && !OwningGrid.AutoSizingColumns)
        {
            // Since we didn't know the final widths of the columns until we resized,
            // we waited until now to measure each header
            double leftEdge = 0;
            foreach (DataGridColumn column in OwningGrid.ColumnsInternal.GetVisibleColumns())
            {
                column.ComputeLayoutRoundedWidth(leftEdge);
                column.HeaderCell.Measure(new Size(column.LayoutRoundedWidth, double.PositiveInfinity));
                if (autoSizeHeight)
                {
                    height = Math.Max(height, column.HeaderCell.DesiredSize.Height);
                }
                leftEdge += column.ActualWidth;
            }
        }

        // Add the filler column if it's not represented.  We won't know whether we need it or not until Arrange
        DataGridFillerColumn? fillerColumn = OwningGrid.ColumnsInternal.FillerColumn;
        Debug.Assert(fillerColumn != null);
        if (!fillerColumn.IsRepresented)
        {
            Debug.Assert(!Children.Contains(fillerColumn.HeaderCell));
            fillerColumn.HeaderCell.IsSeparatorsVisible = false;
            Children.Insert(OwningGrid.ColumnsInternal.Count, fillerColumn.HeaderCell);
            fillerColumn.IsRepresented = true;
            // Optimize for the case where we don't need the filler cell 
            fillerColumn.HeaderCell.IsVisible = false;
        }
        fillerColumn.HeaderCell.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        if (DragIndicator != null)
        {
            DragIndicator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        OwningGrid.ColumnsInternal.EnsureVisibleEdgedColumnsWidth();
        return new Size(OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth, height);
    }

    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);

        InvalidateChildIndex();
    }

    internal void InvalidateChildIndex()
    {
        _childIndexChanged?.Invoke(this, ChildIndexChangedEventArgs.ChildIndexesReset);
    }
}
