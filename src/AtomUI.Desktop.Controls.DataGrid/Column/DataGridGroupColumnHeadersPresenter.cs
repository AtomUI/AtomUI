using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class DataGridGroupColumnHeadersPresenter : Panel, IChildIndexProvider
{
    private EventHandler<ChildIndexChangedEventArgs>? _childIndexChanged;
    private Control? _dragIndicator;
    private Control? _dropLocationIndicator;
    private RectangleGeometry? _dragIndicatorClipGeometry;
    private RectangleGeometry? _dropLocationIndicatorClipGeometry;

    internal DataGrid? OwningGrid { get; set; }

    event EventHandler<ChildIndexChangedEventArgs>? IChildIndexProvider.ChildIndexChanged
    {
        add => _childIndexChanged += value;
        remove => _childIndexChanged -= value;
    }

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
                    ClearColumnReorderingClip(_dragIndicator);
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

    internal double DropLocationIndicatorOffset { get; set; }

    /// <summary>
    /// The drop location indicator control.  This value is null if no column is being dragged.
    /// </summary>
    internal Control? DropLocationIndicator
    {
        get => _dropLocationIndicator;
        set
        {
            if (value != _dropLocationIndicator)
            {
                if (_dropLocationIndicator != null)
                {
                    ClearColumnReorderingClip(_dropLocationIndicator);
                    if (Children.Contains(_dropLocationIndicator))
                    {
                        Children.Remove(_dropLocationIndicator);
                    }
                }

                _dropLocationIndicator = value;
                if (_dropLocationIndicator != null)
                {
                    Children.Add(_dropLocationIndicator);
                }
            }
        }
    }

    int IChildIndexProvider.GetChildIndex(ILogical child)
    {
        return child is DataGridHeaderViewItem headerViewItem
            ? GetHeaderViewItemIndex(headerViewItem)
            : -1;
    }

    bool IChildIndexProvider.TryGetTotalCount(out int count)
    {
        count = 0;
        foreach (var child in Children)
        {
            if (child is DataGridHeaderViewItem)
            {
                count++;
            }
        }

        return true;
    }

    private int GetHeaderViewItemIndex(DataGridHeaderViewItem headerViewItem)
    {
        var index = 0;
        foreach (var child in Children)
        {
            if (child is not DataGridHeaderViewItem currentHeaderViewItem)
            {
                continue;
            }

            if (ReferenceEquals(currentHeaderViewItem, headerViewItem))
            {
                return index;
            }

            index++;
        }

        return -1;
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

    private double _columnHeight;

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

        if (OwningGrid.IsEmptyDataSource)
        {
            OwningGrid.ResolveStarColumnWidths(availableSize.Width);
        }

        _columnHeight = OwningGrid.ColumnHeaderHeight;
        bool          autoSizeHeight;
        if (double.IsNaN(_columnHeight))
        {
            // No explicit height values were set so we can autosize
            _columnHeight  = 0;
            autoSizeHeight = true;
        }
        else
        {
            autoSizeHeight = false;
        }

        double totalDisplayWidth = 0;
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

            // Measure each column header
            bool                    autoGrowWidth  = column.Width.IsAuto || column.Width.IsSizeToHeader;
            DataGridHeaderViewItem? headerViewItem = null;
            if (column is IDataGridColumnGroupItemInternal groupItem)
            {
                headerViewItem = groupItem.GroupHeaderViewItem;
            }

            Debug.Assert(headerViewItem != null);
            DataGridColumnHeader columnHeader = column.HeaderCell;
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
                double newDisplayWidth = column.Width.IsStar
                    ? Math.Min(column.ActualMaxWidth, DataGrid.MaximumStarColumnWidth)
                    : Math.Max(column.ActualMinWidth, Math.Min(column.ActualMaxWidth, column.Width.DesiredValue));
                column.SetWidthDisplayValue(newDisplayWidth);
            }

            // If we're auto-growing the column based on the header content, we want to measure it at its maximum value
            if (autoGrowWidth)
            {
                headerViewItem.Measure(new Size(column.ActualMaxWidth, double.PositiveInfinity));
                OwningGrid.AutoSizeColumn(column, headerViewItem.DesiredSize.Width);
                column.ComputeLayoutRoundedWidth(totalDisplayWidth);
            }
            else if (!OwningGrid.UsesStarSizing)
            {
                column.ComputeLayoutRoundedWidth(totalDisplayWidth);
                headerViewItem.Measure(new Size(column.LayoutRoundedWidth, double.PositiveInfinity));
            }

            // We need to track the largest height in order to auto-size
            if (autoSizeHeight)
            {
                _columnHeight = Math.Max(_columnHeight, headerViewItem.DesiredSize.Height);
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
            for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
            {
                DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
                if (!column.IsVisible)
                {
                    continue;
                }

                column.ComputeLayoutRoundedWidth(leftEdge);
                column.HeaderCell.Measure(new Size(column.LayoutRoundedWidth, double.PositiveInfinity));
                if (autoSizeHeight)
                {
                    _columnHeight = Math.Max(_columnHeight, column.HeaderCell.DesiredSize.Height);
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

        if (DropLocationIndicator != null)
        {
            DropLocationIndicator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        columns.EnsureVisibleEdgedColumnsWidth();
        
        double totalHeight = 0.0;
        // 递归求高度
        var columnGroups = OwningGrid.ColumnGroups;
        for (var i = 0; i < columnGroups.Count; i++)
        {
            IDataGridColumnGroupItem item = columnGroups[i];
            MeasureGroupRecursive(item, out var childTotalHeight);
            totalHeight =  Math.Max(totalHeight, childTotalHeight);
        }
        
        return new Size(columns.VisibleEdgedColumnsWidth, totalHeight);
    }

    private Size MeasureGroupRecursive(IDataGridColumnGroupItem item, out double totalHeight)
    {
        totalHeight = 0.0;
        var height = 0.0;
        var width = double.NaN;
        if (item is IDataGridColumnGroupItemInternal groupItem)
        {
            var hasChildren = groupItem.GroupChildren.Count > 0;
            if (hasChildren)
            {
                var childHeight = 0.0;
                var childWidth = 0.0;
                var groupChildren = groupItem.GroupChildren;
                for (var i = 0; i < groupChildren.Count; i++)
                {
                    var child = groupChildren[i];
                    var childSize = MeasureGroupRecursive(child, out var childTotalHeight);
                    childHeight =  Math.Max(childSize.Height, childHeight);
                    childWidth  += childSize.Width;
                    totalHeight = Math.Max(totalHeight, childTotalHeight);
                }
                
                width = Math.Max(double.IsNaN(width) ? 0.0 : width, childWidth);
            }
            
            if (groupItem.GroupHeaderViewItem != null)
            {
                if (hasChildren)
                {
                    groupItem.GroupHeaderViewItem.MinWidth = width;
                    groupItem.GroupHeaderViewItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    height = groupItem.GroupHeaderViewItem.DesiredSize.Height;
                    width  = groupItem.GroupHeaderViewItem.DesiredSize.Width;
                }
                else
                {
                    if (groupItem is DataGridColumn column)
                    {
                        height = _columnHeight;
                        width  = column.ActualWidth;
                    }
                }
            }
        }

        totalHeight += height;
        return new Size(width, height);
    }

    private Size ArrangeLeafItem(DataGridColumn dataGridColumn, Rect rect)
    {
        Debug.Assert(OwningGrid != null);
        DataGridHeaderViewItem? headerViewItem = null;
        if (dataGridColumn is IDataGridColumnGroupItemInternal groupItem)
        {
            headerViewItem = groupItem.GroupHeaderViewItem;
        }

        Debug.Assert(headerViewItem != null);
        DataGridColumnHeader columnHeader = dataGridColumn.HeaderCell;
        Debug.Assert(columnHeader.OwningColumn == dataGridColumn);
            
        if (dataGridColumn.IsLeftFrozen)
        {
            headerViewItem.Arrange(new Rect(_frozenLeftEdge, rect.Y, dataGridColumn.LayoutRoundedWidth, rect.Height));
            headerViewItem.ClearClipGeometry(); // The layout system could have clipped this because it's not aware of our render transform
            if (DragColumn == dataGridColumn && DragIndicator != null)
            {
                _dragIndicatorLeftEdge = _frozenLeftEdge + DragIndicatorOffset;
            }
            _frozenLeftEdge += dataGridColumn.ActualWidth;
            headerViewItem.IsFrozen             =  true;
            headerViewItem.IsShowRightFrozenShadow   =  dataGridColumn == OwningGrid.ColumnsInternal.LastVisibleLeftFrozenColumn && OwningGrid.HorizontalOffset > 0;
            SetupParentShadowInfo(dataGridColumn, headerViewItem.IsShowRightFrozenShadow, true);
        }
        else if (dataGridColumn.IsRightFrozen)
        {
            var columnFrozenRightEdge = _frozenRightEdge - RightFrozenWidth(dataGridColumn);
            headerViewItem.Arrange(new Rect(columnFrozenRightEdge,  rect.Y, dataGridColumn.LayoutRoundedWidth, rect.Height));
            headerViewItem.ClearClipGeometry(); // The layout system could have clipped this because it's not aware of our render transform
            headerViewItem.IsFrozen             =  true;
            headerViewItem.IsShowLeftFrozenShadow   =  dataGridColumn == OwningGrid.ColumnsInternal.FirstVisibleRightFrozenColumn && OwningGrid.HorizontalOffset < OwningGrid.HorizontalMaximizeOffset;
            SetupParentShadowInfo(dataGridColumn, headerViewItem.IsShowLeftFrozenShadow, false);
        }
        else
        {
            headerViewItem.Arrange(new Rect(_scrollingLeftEdge, rect.Y, dataGridColumn.LayoutRoundedWidth, rect.Height));
            EnsureColumnHeaderClip(headerViewItem, dataGridColumn.ActualWidth, rect.Height, _frozenLeftEdge, _scrollingLeftEdge);
            if (DragColumn == dataGridColumn && DragIndicator != null)
            {
                _dragIndicatorLeftEdge = _scrollingLeftEdge + DragIndicatorOffset;
            }
            headerViewItem.IsFrozen = false;
        }
        
        _scrollingLeftEdge += dataGridColumn.ActualWidth;
        return headerViewItem.Bounds.Size;
    }

    // 该右冻结列左边缘到右边界的距离：按显示顺序累加它及其后所有可见右冻结列的宽度
    private double RightFrozenWidth(DataGridColumn dataGridColumn)
    {
        var width = 0.0;
        if (OwningGrid != null)
        {
            var columns = OwningGrid.ColumnsInternal;
            for (var displayIndex = dataGridColumn.DisplayIndexWithFiller;
                 displayIndex < columns.GetDisplayedColumnCount();
                 displayIndex++)
            {
                var column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
                if (column.IsVisible && column.IsRightFrozen)
                {
                    width += column.ActualWidth;
                }
            }
        }
        return width;
    }

    private void SetupParentShadowInfo(IDataGridColumnGroupItemInternal groupItem, bool isShowShadowInfo, bool isLeftFrozen)
    {
        var current = groupItem.GroupParent;
        while (current != null)
        {
            if (current is IDataGridColumnGroupItemInternal groupItemInternal)
            {
                var headerViewItem = groupItemInternal.GroupHeaderViewItem;
                if (headerViewItem != null)
                {
                    if (isLeftFrozen)
                    {
                        headerViewItem.IsShowRightFrozenShadow = isShowShadowInfo;
                    }
                    else
                    {
                        headerViewItem.IsShowLeftFrozenShadow = isShowShadowInfo;
                    }
                }
            }
          
            current        = current.GroupParent;
        }
    }
    
    private double _leafHeight = 0.0;
    private double _dragIndicatorLeftEdge;
    private double _frozenLeftEdge;
    private double _frozenRightEdge;
    private double _scrollingLeftEdge;
    
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
            double availableCellsWidth = OwningGrid.IsEmptyDataSource ? finalSize.Width : OwningGrid.CellsWidth;
            OwningGrid.CompleteAutoSizing(availableCellsWidth);
            return base.ArrangeOverride(finalSize);
        }
        
        _leafHeight = OwningGrid.ColumnHeaderHeight;
        bool   autoSizeHeight;
        if (double.IsNaN(_leafHeight))
        {
            // No explicit height values were set so we can autosize
            _leafHeight    = 0;
            autoSizeHeight = true;
        }
        else
        {
            autoSizeHeight = false;
        }
  
        var columns              = OwningGrid.ColumnsInternal;
        int displayedColumnCount = columns.GetDisplayedColumnCount();
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible)
            {
                continue;
            }

            DataGridHeaderViewItem? headerViewItem = null;
            if (column is IDataGridColumnGroupItemInternal groupItem)
            {
                headerViewItem = groupItem.GroupHeaderViewItem;
            }

            Debug.Assert(headerViewItem != null);
            
            // We need to track the largest height in order to auto-size
            if (autoSizeHeight)
            {
                _leafHeight = Math.Max(_leafHeight, headerViewItem.DesiredSize.Height);
            }
        }

        // If we're using star sizing (and we're not waiting for an auto-column to finish growing)
        // then we will resize all the columns to fit the available space.
        if (OwningGrid.UsesStarSizing && !OwningGrid.AutoSizingColumns)
        {
            // Since we didn't know the final widths of the columns until we resized,
            // we waited until now to measure each header
            for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
            {
                DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
                if (!column.IsVisible)
                {
                    continue;
                }

                if (autoSizeHeight)
                {
                    _leafHeight = Math.Max(_leafHeight, column.HeaderCell.DesiredSize.Height);
                }
            }
        }
        
        _dragIndicatorLeftEdge = 0;
        _frozenLeftEdge        = 0;
        _frozenRightEdge       = DesiredSize.Width;
        _scrollingLeftEdge     = -OwningGrid.HorizontalOffset;
        DataGridFillerColumn? fillerColumn = columns.FillerColumn;
        Debug.Assert(fillerColumn != null);
    
        var offsetX     = 0.0d;
        var remainWidth = finalSize.Width;
        var columnGroups = OwningGrid.ColumnGroups;
        for (var i = 0; i < columnGroups.Count; i++)
        {
            IDataGridColumnGroupItem item = columnGroups[i];
            if (item is IDataGridColumnGroupItemInternal groupItem)
            {
                if (groupItem.GroupHeaderViewItem != null)
                {
                    ArrangeGroupRecursive(item, new Rect(offsetX, 0, remainWidth, finalSize.Height));
                    if (groupItem is DataGridColumn column)
                    {
                        offsetX += column.ActualWidth;
                        remainWidth -= column.ActualWidth;
                    }
                    else
                    {
                        var size = groupItem.GroupHeaderViewItem.DesiredSize;
                        offsetX += size.Width;
                        remainWidth -= size.Width;
                    }
                }
            }
        }
    
        var arrangeLeafY = finalSize.Height - _leafHeight;
        if (DragColumn != null)
        {
            if (DragIndicator != null)
            {
                EnsureColumnReorderingClip(DragIndicator, _leafHeight, _frozenLeftEdge, _dragIndicatorLeftEdge);
        
                var height = DragIndicator.Bounds.Height;
                if (height <= 0)
                {
                    height = DragIndicator.DesiredSize.Height;
                }
                
                DragIndicator.Arrange(new Rect(_dragIndicatorLeftEdge, arrangeLeafY, DragIndicator.Bounds.Width, height));
            }
        
            if (DropLocationIndicator != null)
            {
                EnsureColumnReorderingClip(DropLocationIndicator, _leafHeight, _frozenLeftEdge, DropLocationIndicatorOffset);
        
                DropLocationIndicator.Arrange(new Rect(DropLocationIndicatorOffset, arrangeLeafY,
                    DropLocationIndicator.Bounds.Width, DropLocationIndicator.Bounds.Height));
            }
        }
        
        OwningGrid.HandleFillerColumnWidthNeeded(finalSize.Width);
 
        if (fillerColumn.FillerWidth > 0)
        {
            fillerColumn.HeaderCell.IsVisible = true;
            fillerColumn.HeaderCell.Arrange(new Rect(_scrollingLeftEdge, arrangeLeafY, fillerColumn.FillerWidth, _leafHeight));
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

    private void ArrangeGroupRecursive(IDataGridColumnGroupItem item, Rect rect)
    {
        if (item is IDataGridColumnGroupItemInternal groupItem)
        {
            var selfHeight  = 0.0d;
            var hasChildren = groupItem.GroupChildren.Count > 0;
            if (groupItem.GroupHeaderViewItem != null)
            {
                // 安排自己
                if (hasChildren)
                {
                    var size = groupItem.GroupHeaderViewItem.DesiredSize;
                    selfHeight = size.Height;
                    var offsetX = rect.X;
                    if (groupItem is DataGridColumnGroupItem groupItemX)
                    {
                        if (!groupItemX.IsFrozen)
                        {
                            offsetX -= OwningGrid!.HorizontalOffset;
                        }
                    }
                    groupItem.GroupHeaderViewItem.Arrange(new Rect(offsetX, rect.Y, size.Width, hasChildren ? selfHeight : rect.Height));
                }
                else
                {
                    if (groupItem is DataGridColumn dataGridColumn)
                    {
                        var size = ArrangeLeafItem(dataGridColumn, rect);
                        selfHeight = size.Height;
                    }
                }
            }
            if (hasChildren)
            {
                var offsetX     = rect.X;
                var offsetY     = rect.Y + selfHeight;
                var remainWidth = rect.Width;
                var groupChildren = groupItem.GroupChildren;
                for (var i = 0; i < groupChildren.Count; i++)
                {
                    var child = groupChildren[i];
                    if (child is IDataGridColumnGroupItemInternal childGroupItem)
                    {
                        if (childGroupItem.GroupHeaderViewItem != null)
                        {
                            ArrangeGroupRecursive(childGroupItem, new Rect(offsetX, offsetY, remainWidth, rect.Height - selfHeight));
                            if (childGroupItem is DataGridColumn column)
                            {
                                offsetX     += column.ActualWidth;
                                remainWidth -= column.ActualWidth;
                            }
                            else
                            {
                                var size = childGroupItem.GroupHeaderViewItem.DesiredSize;
                                offsetX     += size.Width;
                                remainWidth -= size.Width;
                            }
                        }
                    }
                }
            }
        }
    }

    private static void EnsureColumnHeaderClip(DataGridHeaderViewItem headerViewItem, double width, double height,
                                               double frozenLeftEdge, double columnHeaderLeftEdge)
    {
        // Clip the cell only if it's scrolled under frozen columns.  Unfortunately, we need to clip in this case
        // because cells could be transparent
        if (frozenLeftEdge > columnHeaderLeftEdge)
        {
            double            xClip = Math.Min(width, frozenLeftEdge - columnHeaderLeftEdge);
            headerViewItem.UpdateClipGeometry(new Rect(xClip, 0, width - xClip, height));
        }
        else
        {
            headerViewItem.ClearClipGeometry();
        }
    }

    /// <summary>
    /// Clips the DragIndicator and DropLocationIndicator controls according to current ColumnHeaderPresenter constraints.
    /// </summary>
    /// <param name="control">The DragIndicator or DropLocationIndicator</param>
    /// <param name="height">The available height</param>
    /// <param name="frozenColumnsWidth">The width of the frozen column region</param>
    /// <param name="controlLeftEdge">The left edge of the control to clip</param>
    private void EnsureColumnReorderingClip(Control control, double height, double frozenColumnsWidth,
                                            double controlLeftEdge)
    {
        Debug.Assert(OwningGrid != null);
        Debug.Assert(DragColumn != null);
        double leftEdge  = 0;
        double rightEdge = OwningGrid.CellsWidth;
        double width     = control.Bounds.Width;
        if (DragColumn.IsFrozen)
        {
            // If we're dragging a frozen column, we want to clip the corresponding DragIndicator control when it goes
            // into the scrolling columns region, but not the DropLocationIndicator.
            if (control == DragIndicator)
            {
                rightEdge = Math.Min(rightEdge, frozenColumnsWidth);
            }
        }
        else if (OwningGrid.LeftFrozenColumnCount > 0)
        {
            // If we're dragging a scrolling column, we want to clip both the DragIndicator and the DropLocationIndicator
            // controls when they go into the frozen column range.
            leftEdge = frozenColumnsWidth;
        }

        Rect? clipRect = null;
        if (leftEdge > controlLeftEdge)
        {
            double xClip = Math.Min(width, leftEdge - controlLeftEdge);
            clipRect = new Rect(xClip, 0, width - xClip, height);
        }

        if (controlLeftEdge + width >= rightEdge)
        {
            var currentRect = clipRect ?? default;
            clipRect = new Rect(
                currentRect.X,
                currentRect.Y,
                Math.Max(0, rightEdge - controlLeftEdge - currentRect.X),
                height);
        }

        if (ReferenceEquals(control, DragIndicator))
        {
            UpdateColumnReorderingClip(control, ref _dragIndicatorClipGeometry, clipRect);
        }
        else
        {
            UpdateColumnReorderingClip(control, ref _dropLocationIndicatorClipGeometry, clipRect);
        }
    }

    private static void UpdateColumnReorderingClip(Control control, ref RectangleGeometry? clipGeometry, Rect? clipRect)
    {
        if (clipRect.HasValue)
        {
            if (clipGeometry is null)
            {
                clipGeometry = new RectangleGeometry
                {
                    Rect = clipRect.Value
                };
            }
            else if (!clipGeometry.Rect.Equals(clipRect.Value))
            {
                clipGeometry.Rect = clipRect.Value;
                control.InvalidateVisual();
            }

            if (!ReferenceEquals(control.Clip, clipGeometry))
            {
                control.Clip = clipGeometry;
            }
        }
        else
        {
            ClearColumnReorderingClip(control);
        }
    }

    private static void ClearColumnReorderingClip(Control control)
    {
        if (control.Clip is not null)
        {
            control.Clip = null;
        }
    }
}
