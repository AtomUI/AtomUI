// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using AtomUI.Desktop.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

// 滚动：滚轮/滚动条输入、偏移提交、行列滚入视口与滚动条布局
public partial class DataGrid
{
    internal bool UpdateScroll(Vector delta)
    {
        if (IsEnabled && DisplayData.NumDisplayedScrollingElements > 0)
        {
            var handled          = false;
            var ignoreInvalidate = false;
            var scrollHeight     = 0d;

            // Vertical scroll handling
            if (IsRangePresentationActive)
            {
                if (!MathUtils.IsZero(delta.Y) &&
                    QueueRangeScrollOffset(
                        _rangeDesiredOffset - delta.Y,
                        delta.Y < 0 ? 1 : -1))
                {
                    handled = true;
                }
            }
            else if (delta.Y > 0)
            {
                scrollHeight = Math.Max(-_verticalOffset, -delta.Y);
            }
            else if (delta.Y < 0)
            {
                if (_vScrollBar != null && VerticalScrollBarVisibility == ScrollBarVisibility.Visible)
                {
                    scrollHeight = Math.Min(Math.Max(0, _vScrollBar.Maximum - _verticalOffset), -delta.Y);
                }
                else
                {
                    double maximum = EdgedRowsHeightCalculated - CellsEstimatedHeight;
                    scrollHeight = Math.Min(Math.Max(0, maximum - _verticalOffset), -delta.Y);
                }
            }

            if (scrollHeight != 0)
            {
                DisplayData.PendingVerticalScrollHeight = scrollHeight;
                handled                                 = true;
            }

            // Horizontal scroll handling
            if (delta.X != 0)
            {
                var horizontalOffset = HorizontalOffset - delta.X;
                var widthNotVisible  = Math.Max(0, ColumnsInternal.VisibleEdgedColumnsWidth - CellsWidth);

                if (horizontalOffset < 0)
                {
                    horizontalOffset = 0;
                }

                if (horizontalOffset > widthNotVisible)
                {
                    horizontalOffset = widthNotVisible;
                }

                if (UpdateHorizontalOffset(horizontalOffset))
                {
                    // We don't need to invalidate once again after UpdateHorizontalOffset.
                    ignoreInvalidate = true;
                    handled          = true;
                }
            }

            if (handled)
            {
                if (!ignoreInvalidate)
                {
                    InvalidateRowsMeasure(invalidateIndividualElements: false);
                }

                return true;
            }
        }

        return false;
    }

    internal void ProcessHorizontalScroll(ScrollEventType scrollEventType)
    {
        if (_horizontalScrollChangesIgnored > 0)
        {
            return;
        }

        // If the user scrolls with the buttons, we need to update the new value of the scroll bar since we delay
        // this calculation.  If they scroll in another other way, the scroll bar's correct value has already been set
        double scrollBarValueDifference = 0;
        if (scrollEventType == ScrollEventType.SmallIncrement)
        {
            scrollBarValueDifference = GetHorizontalSmallScrollIncrease();
        }
        else if (scrollEventType == ScrollEventType.SmallDecrement)
        {
            scrollBarValueDifference = -GetHorizontalSmallScrollDecrease();
        }

        _horizontalScrollChangesIgnored++;
        try
        {
            Debug.Assert(_hScrollBar != null);
            if (scrollBarValueDifference != 0)
            {
                Debug.Assert(_horizontalOffset + scrollBarValueDifference >= 0);
                _hScrollBar.Value = _horizontalOffset + scrollBarValueDifference;
            }

            UpdateHorizontalOffset(_hScrollBar.Value);
        }
        finally
        {
            _horizontalScrollChangesIgnored--;
        }
    }

    //internal void ProcessVerticalScroll(double oldValue, double newValue)
    internal void ProcessVerticalScroll(ScrollEventType scrollEventType)
    {
        if (_verticalScrollChangesIgnored > 0)
        {
            return;
        }

        Debug.Assert(_vScrollBar != null);
        Debug.Assert(MathUtils.LessThanOrClose(_vScrollBar.Value, _vScrollBar.Maximum));

        if (IsRangePresentationActive)
        {
            double targetOffset;
            int direction;
            if (scrollEventType == ScrollEventType.SmallIncrement)
            {
                targetOffset = _rangeDesiredOffset + Math.Max(
                    GetVerticalSmallScrollIncrease(),
                    GetRangeDefaultHeight());
                direction = 1;
            }
            else if (scrollEventType == ScrollEventType.SmallDecrement)
            {
                targetOffset = _rangeDesiredOffset - Math.Max(
                    NegVerticalOffset,
                    GetRangeDefaultHeight());
                direction = -1;
            }
            else
            {
                targetOffset = _vScrollBar.Value;
                direction = targetOffset.CompareTo(_rangeDesiredOffset);
            }
            QueueRangeScrollOffset(targetOffset, Math.Sign(direction));
            return;
        }

        _verticalScrollChangesIgnored++;
        try
        {
            Debug.Assert(_vScrollBar != null);
            if (scrollEventType == ScrollEventType.SmallIncrement)
            {
                DisplayData.PendingVerticalScrollHeight = GetVerticalSmallScrollIncrease();
                double newVerticalOffset = _verticalOffset + DisplayData.PendingVerticalScrollHeight;
                if (newVerticalOffset > _vScrollBar.Maximum)
                {
                    DisplayData.PendingVerticalScrollHeight -= newVerticalOffset - _vScrollBar.Maximum;
                }
            }
            else if (scrollEventType == ScrollEventType.SmallDecrement)
            {
                if (MathUtils.GreaterThan(NegVerticalOffset, 0))
                {
                    DisplayData.PendingVerticalScrollHeight -= NegVerticalOffset;
                }
                else
                {
                    int previousScrollingSlot = GetPreviousVisibleSlot(DisplayData.FirstScrollingSlot);
                    if (previousScrollingSlot >= 0)
                    {
                        ScrollSlotIntoView(previousScrollingSlot, scrolledHorizontally: false);
                    }

                    return;
                }
            }
            else if (scrollEventType == ScrollEventType.ThumbTrack)
            {
                double displayVerticalOffset = GetDisplayedRowsVerticalOffsetEstimate();
                _verticalOffset = displayVerticalOffset;
                DisplayData.PendingVerticalScrollHeight = _vScrollBar.Value - displayVerticalOffset;
            }
            else
            {
                DisplayData.PendingVerticalScrollHeight = _vScrollBar.Value - _verticalOffset;
            }

            if (!MathUtils.IsZero(DisplayData.PendingVerticalScrollHeight))
            {
                // Invalidate so the scroll happens on idle
                InvalidateRowsMeasure(invalidateIndividualElements: false);
            }
        }
        finally
        {
            _verticalScrollChangesIgnored--;
        }
    }

    internal bool ScrollSlotIntoView(int columnIndex, int slot, bool forCurrentCellChange, bool forceHorizontalScroll)
    {
        Debug.Assert(columnIndex >= 0 && columnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(DisplayData.FirstDisplayedScrollingCol >= -1 &&
                     DisplayData.FirstDisplayedScrollingCol < ColumnsItemsInternal.Count);
        Debug.Assert(DisplayData.LastTotallyDisplayedScrollingCol >= -1 &&
                     DisplayData.LastTotallyDisplayedScrollingCol < ColumnsItemsInternal.Count);
        Debug.Assert(!IsSlotOutOfBounds(slot));
        Debug.Assert(DisplayData.FirstScrollingSlot >= -1 && DisplayData.FirstScrollingSlot < SlotCount);
        Debug.Assert(ColumnsItemsInternal[columnIndex].IsVisible);

        if (CurrentColumnIndex >= 0 &&
            (CurrentColumnIndex != columnIndex || CurrentSlot != slot))
        {
            if (!CommitEditForOperation(columnIndex, slot, forCurrentCellChange) ||
                IsInnerCellOutOfBounds(columnIndex, slot))
            {
                return false;
            }
        }

        double oldHorizontalOffset = HorizontalOffset;

        //scroll horizontally unless we're on a RowGroupHeader and we're not forcing horizontal scrolling
        if ((forceHorizontalScroll || (slot != -1))
            && !ScrollColumnIntoView(columnIndex))
        {
            return false;
        }

        //scroll vertically
        if (!ScrollSlotIntoView(slot, scrolledHorizontally: !MathUtils.AreClose(oldHorizontalOffset, HorizontalOffset)))
        {
            return false;
        }

        return true;
    }

    internal bool UpdateHorizontalOffset(double newValue)
    {
        if (!MathUtils.AreClose(HorizontalOffset, newValue))
        {
            HorizontalOffset = newValue;
            InvalidateColumnHeadersMeasure();
            InvalidateRowsMeasure(true);
            return true;
        }

        return false;
    }

    internal void UpdateVerticalScrollBar()
    {
        if (_vScrollBar != null && _vScrollBar.IsVisible)
        {
            double cellsHeight               = CellsEstimatedHeight;
            double edgedRowsHeightCalculated = EdgedRowsHeightCalculated;
            UpdateVerticalScrollBar(
                needVertScrollbar: edgedRowsHeightCalculated > cellsHeight,
                forceVertScrollbar: VerticalScrollBarVisibility == ScrollBarVisibility.Visible,
                totalVisibleHeight: edgedRowsHeightCalculated,
                cellsHeight: cellsHeight);
        }
    }

    private void ComputeScrollBarsLayout()
    {
        bool isHorizontalScrollBarOverCells = IsHorizontalScrollBarOverCells;
        bool isVerticalScrollBarOverCells   = IsVerticalScrollBarOverCells;

        double cellsWidth  = CellsWidth;
        double cellsHeight = CellsEstimatedHeight;

        bool   allowHorizScrollbar  = false;
        bool   forceHorizScrollbar  = false;
        double horizScrollBarHeight = 0;
        if (_hScrollBar != null)
        {
            forceHorizScrollbar = HorizontalScrollBarVisibility == ScrollBarVisibility.Visible;
            allowHorizScrollbar = forceHorizScrollbar || (ColumnsInternal.VisibleColumnCount > 0 &&
                                                          HorizontalScrollBarVisibility !=
                                                          ScrollBarVisibility.Disabled &&
                                                          HorizontalScrollBarVisibility != ScrollBarVisibility.Hidden);
            // Compensate if the horizontal scrollbar is already taking up space
            if (!forceHorizScrollbar && _hScrollBar.IsVisible)
            {
                if (!isHorizontalScrollBarOverCells)
                {
                    cellsHeight += _hScrollBar.Bounds.Height;
                }
            }

            if (!isHorizontalScrollBarOverCells)
            {
                horizScrollBarHeight = _hScrollBar.Bounds.Height + _hScrollBar.Margin.Top + _hScrollBar.Margin.Bottom;
            }
        }

        bool   allowVertScrollbar = false;
        bool   forceVertScrollbar = false;
        double vertScrollBarWidth = 0;
        if (_vScrollBar != null)
        {
            forceVertScrollbar = VerticalScrollBarVisibility == ScrollBarVisibility.Visible;
            allowVertScrollbar = forceVertScrollbar || (ColumnsItemsInternal.Count > 0 &&
                                                        VerticalScrollBarVisibility != ScrollBarVisibility.Disabled &&
                                                        VerticalScrollBarVisibility != ScrollBarVisibility.Hidden);
            // Compensate if the vertical scrollbar is already taking up space
            if (!forceVertScrollbar && _vScrollBar.IsVisible)
            {
                if (!isVerticalScrollBarOverCells)
                {
                    cellsWidth += _vScrollBar.Bounds.Width;
                }
            }

            if (!isVerticalScrollBarOverCells)
            {
                vertScrollBarWidth = _vScrollBar.Bounds.Width + _vScrollBar.Margin.Left + _vScrollBar.Margin.Right;
            }
        }

        // Now cellsWidth is the width potentially available for displaying data cells.
        // Now cellsHeight is the height potentially available for displaying data cells.

        bool needHorizScrollbar = false;
        bool needVertScrollbar  = false;

        double totalVisibleWidth       = ColumnsInternal.VisibleEdgedColumnsWidth;
        double totalVisibleFrozenWidth = ColumnsInternal.GetVisibleFrozenEdgedColumnsWidth();

        UpdateDisplayedRows(DisplayData.FirstScrollingSlot, CellsEstimatedHeight);
        double totalVisibleHeight = EdgedRowsHeightCalculated;

        if (!forceHorizScrollbar && !forceVertScrollbar)
        {
            bool needHorizScrollbarWithoutVertScrollbar = false;

            if (allowHorizScrollbar &&
                MathUtils.GreaterThan(totalVisibleWidth, cellsWidth) &&
                MathUtils.LessThan(totalVisibleFrozenWidth, cellsWidth) &&
                MathUtils.LessThanOrClose(horizScrollBarHeight, cellsHeight))
            {
                double oldDataHeight = cellsHeight;
                cellsHeight -= horizScrollBarHeight;
                Debug.Assert(cellsHeight >= 0);
                needHorizScrollbarWithoutVertScrollbar = needHorizScrollbar = true;

                if (vertScrollBarWidth > 0 &&
                    allowVertScrollbar &&
                    (MathUtils.LessThanOrClose(totalVisibleWidth - cellsWidth, vertScrollBarWidth) ||
                     MathUtils.LessThanOrClose(cellsWidth - totalVisibleFrozenWidth, vertScrollBarWidth)))
                {
                    // Would we still need a horizontal scrollbar without the vertical one?
                    UpdateDisplayedRows(DisplayData.FirstScrollingSlot, cellsHeight);
                    if (IsRangePresentationActive
                            ? MathUtils.GreaterThan(totalVisibleHeight, cellsHeight)
                            : DisplayData.NumTotallyDisplayedScrollingElements != VisibleSlotCount)
                    {
                        needHorizScrollbar =
                            MathUtils.LessThan(totalVisibleFrozenWidth, cellsWidth - vertScrollBarWidth);
                    }
                }

                if (!needHorizScrollbar)
                {
                    // Restore old data height because turns out a horizontal scroll bar wouldn't make sense
                    cellsHeight = oldDataHeight;
                }
            }

            // Store the current FirstScrollingSlot because removing the horizontal scrollbar could scroll
            // the DataGrid up; however, if we realize later that we need to keep the horizontal scrollbar
            // then we should use the first slot stored here which is not scrolled.
            int firstScrollingSlot = DisplayData.FirstScrollingSlot;

            UpdateDisplayedRows(firstScrollingSlot, cellsHeight);
            if (allowVertScrollbar &&
                MathUtils.GreaterThan(cellsHeight, 0) &&
                MathUtils.LessThanOrClose(vertScrollBarWidth, cellsWidth) &&
                (IsRangePresentationActive
                    ? MathUtils.GreaterThan(totalVisibleHeight, cellsHeight)
                    : DisplayData.NumTotallyDisplayedScrollingElements != VisibleSlotCount))
            {
                cellsWidth -= vertScrollBarWidth;
                Debug.Assert(cellsWidth >= 0);
                needVertScrollbar = true;
            }

            DisplayData.FirstDisplayedScrollingCol = ComputeFirstVisibleScrollingColumn();

            // we compute the number of visible columns only after we set up the vertical scroll bar.
            ComputeDisplayedColumns();

            if ((vertScrollBarWidth > 0 || horizScrollBarHeight > 0) &&
                allowHorizScrollbar &&
                needVertScrollbar && !needHorizScrollbar &&
                MathUtils.GreaterThan(totalVisibleWidth, cellsWidth) &&
                MathUtils.LessThan(totalVisibleFrozenWidth, cellsWidth) &&
                MathUtils.LessThanOrClose(horizScrollBarHeight, cellsHeight))
            {
                cellsWidth  += vertScrollBarWidth;
                cellsHeight -= horizScrollBarHeight;
                Debug.Assert(cellsHeight >= 0);
                needVertScrollbar = false;

                UpdateDisplayedRows(firstScrollingSlot, cellsHeight);
                if (cellsHeight > 0 &&
                    vertScrollBarWidth <= cellsWidth &&
                    (IsRangePresentationActive
                        ? MathUtils.GreaterThan(totalVisibleHeight, cellsHeight)
                        : DisplayData.NumTotallyDisplayedScrollingElements != VisibleSlotCount))
                {
                    cellsWidth -= vertScrollBarWidth;
                    Debug.Assert(cellsWidth >= 0);
                    needVertScrollbar = true;
                }

                if (needVertScrollbar)
                {
                    needHorizScrollbar = true;
                }
                else
                {
                    needHorizScrollbar = needHorizScrollbarWithoutVertScrollbar;
                }
            }
        }
        else if (forceHorizScrollbar && !forceVertScrollbar)
        {
            if (allowVertScrollbar)
            {
                if (cellsHeight > 0 &&
                    MathUtils.LessThanOrClose(vertScrollBarWidth, cellsWidth) &&
                    (IsRangePresentationActive
                        ? MathUtils.GreaterThan(totalVisibleHeight, cellsHeight)
                        : DisplayData.NumTotallyDisplayedScrollingElements != VisibleSlotCount))
                {
                    cellsWidth -= vertScrollBarWidth;
                    Debug.Assert(cellsWidth >= 0);
                    needVertScrollbar = true;
                }

                DisplayData.FirstDisplayedScrollingCol = ComputeFirstVisibleScrollingColumn();
                ComputeDisplayedColumns();
            }

            needHorizScrollbar = totalVisibleWidth > cellsWidth && totalVisibleFrozenWidth < cellsWidth;
        }
        else if (!forceHorizScrollbar && forceVertScrollbar)
        {
            if (allowHorizScrollbar)
            {
                if (cellsWidth > 0 &&
                    MathUtils.LessThanOrClose(horizScrollBarHeight, cellsHeight) &&
                    MathUtils.GreaterThan(totalVisibleWidth, cellsWidth) &&
                    MathUtils.LessThan(totalVisibleFrozenWidth, cellsWidth))
                {
                    cellsHeight -= horizScrollBarHeight;
                    Debug.Assert(cellsHeight >= 0);
                    needHorizScrollbar = true;
                    UpdateDisplayedRows(DisplayData.FirstScrollingSlot, cellsHeight);
                }

                DisplayData.FirstDisplayedScrollingCol = ComputeFirstVisibleScrollingColumn();
                ComputeDisplayedColumns();
            }

            needVertScrollbar = IsRangePresentationActive
                ? MathUtils.GreaterThan(totalVisibleHeight, cellsHeight)
                : DisplayData.NumTotallyDisplayedScrollingElements != VisibleSlotCount;
        }
        else
        {
            Debug.Assert(forceHorizScrollbar && forceVertScrollbar);
            Debug.Assert(allowHorizScrollbar && allowVertScrollbar);
            DisplayData.FirstDisplayedScrollingCol = ComputeFirstVisibleScrollingColumn();
            ComputeDisplayedColumns();
            needVertScrollbar = IsRangePresentationActive
                ? MathUtils.GreaterThan(totalVisibleHeight, cellsHeight)
                : DisplayData.NumTotallyDisplayedScrollingElements != VisibleSlotCount;
            needHorizScrollbar = totalVisibleWidth > cellsWidth && totalVisibleFrozenWidth < cellsWidth;
        }

        UpdateHorizontalScrollBar(needHorizScrollbar, forceHorizScrollbar, totalVisibleWidth, totalVisibleFrozenWidth,
            cellsWidth);
        UpdateVerticalScrollBar(needVertScrollbar, forceVertScrollbar, totalVisibleHeight, cellsHeight);
        ClampRangeOffsetToMaximum();

        if (_topRightCornerHeader != null)
        {
            // Show the TopRightHeaderCell based on vertical ScrollBar visibility
            if (IsColumnHeadersVisible &&
                _vScrollBar != null && _vScrollBar.IsVisible)
            {
                _topRightCornerHeader.IsVisible = true;
            }
            else
            {
                _topRightCornerHeader.IsVisible = false;
            }
        }

        if (_bottomRightCorner != null)
        {
            // Show the BottomRightCorner when both scrollbars are visible.
            _bottomRightCorner.IsVisible =
                _hScrollBar != null && _hScrollBar.IsVisible &&
                _vScrollBar != null && _vScrollBar.IsVisible;
        }

        DisplayData.FullyRecycleElements();
    }

    // Calculates the amount to scroll for the ScrollLeft button
    // This is a method rather than a property to emphasize a calculation
    private double GetHorizontalSmallScrollDecrease()
    {
        // If the first column is covered up, scroll to the start of it when the user clicks the left button
        if (_negHorizontalOffset > 0)
        {
            return _negHorizontalOffset;
        }

        // The entire first column is displayed, show the entire previous column when the user clicks
        // the left button
        DataGridColumn? previousColumn = ColumnsInternal.GetPreviousVisibleScrollingColumn(
            ColumnsItemsInternal[DisplayData.FirstDisplayedScrollingCol]);
        if (previousColumn != null)
        {
            return GetEdgedColumnWidth(previousColumn);
        }

        // There's no previous column so don't move
        return 0;
    }

    // Calculates the amount to scroll for the ScrollRight button
    // This is a method rather than a property to emphasize a calculation
    private double GetHorizontalSmallScrollIncrease()
    {
        if (DisplayData.FirstDisplayedScrollingCol >= 0)
        {
            return GetEdgedColumnWidth(ColumnsItemsInternal[DisplayData.FirstDisplayedScrollingCol]) -
                   _negHorizontalOffset;
        }

        return 0;
    }

    // Calculates the amount the ScrollDown button should scroll
    // This is a method rather than a property to emphasize that calculations are taking place
    private double GetVerticalSmallScrollIncrease()
    {
        if (DisplayData.FirstScrollingSlot >= 0)
        {
            return GetExactSlotElementHeight(DisplayData.FirstScrollingSlot) - NegVerticalOffset;
        }

        return 0;
    }

    private void HandleHorizontalScrollBarScroll(object? sender, ScrollEventArgs e)
    {
        ProcessHorizontalScroll(e.ScrollEventType);
        HorizontalScroll?.Invoke(sender, e);
    }

    private void SetVerticalOffset(double newVerticalOffset)
    {
        _verticalOffset = newVerticalOffset;
        if (_vScrollBar != null && !MathUtils.AreClose(newVerticalOffset, _vScrollBar.Value))
        {
            _vScrollBar.Value = _verticalOffset;
        }
    }

    private void UpdateHorizontalScrollBar(bool needHorizScrollbar,
                                           bool forceHorizScrollbar,
                                           double totalVisibleWidth,
                                           double totalVisibleFrozenWidth,
                                           double cellsWidth)
    {
        if (_hScrollBar != null)
        {
            var wasVisible = _hScrollBar.IsVisible;
            if (needHorizScrollbar || forceHorizScrollbar)
            {
                //          viewportSize
                //        v---v
                //|<|_____|###|>|
                //  ^     ^
                //  min   max

                // we want to make the relative size of the thumb reflect the relative size of the viewing area
                // viewportSize / (max + viewportSize) = cellsWidth / max
                // -> viewportSize = max * cellsWidth / (max - cellsWidth)

                // always zero
                _hScrollBar.Minimum = 0;
                if (needHorizScrollbar)
                {
                    // maximum travel distance -- not the total width
                    _hScrollBar.Maximum = totalVisibleWidth - cellsWidth;
                    Debug.Assert(totalVisibleFrozenWidth >= 0);

                    Debug.Assert(_hScrollBar.Maximum >= 0);

                    // width of the scrollable viewing area
                    double viewPortSize = Math.Max(0, cellsWidth - totalVisibleFrozenWidth);
                    _hScrollBar.ViewportSize = viewPortSize;
                    _hScrollBar.LargeChange  = viewPortSize;
                    // The ScrollBar should be in sync with HorizontalOffset at this point.  There's a resize case
                    // where the ScrollBar will coerce an old value here, but we don't want that
                    if (!MathUtils.AreClose(_hScrollBar.Value, _horizontalOffset))
                    {
                        _hScrollBar.Value = _horizontalOffset;
                    }

                    _hScrollBar.IsEnabled = true;
                }
                else
                {
                    _hScrollBar.Maximum      = 0;
                    _hScrollBar.ViewportSize = 0;
                    _hScrollBar.IsEnabled    = false;
                }

                if (!_hScrollBar.IsVisible)
                {
                    _hScrollBar.IsVisible = true;
                    if (_hScrollBar.DesiredSize.Height == 0)
                    {
                        // We need to know the height for the rest of layout to work correctly so measure it now
                        _hScrollBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }
                }
            }
            else
            {
                _hScrollBar.Maximum = 0;
                if (_hScrollBar.IsVisible)
                {
                    _hScrollBar.IsVisible       = false;
                }
            }

            if (wasVisible != _hScrollBar.IsVisible)
            {
                RefreshDisplayedRowsGridLines();
            }
        }
    }

    private void UpdateVerticalScrollBar(bool needVertScrollbar, bool forceVertScrollbar, double totalVisibleHeight,
                                         double cellsHeight)
    {
        if (_vScrollBar != null)
        {
            if (needVertScrollbar || forceVertScrollbar)
            {
                //          viewportSize
                //        v---v
                //|<|_____|###|>|
                //  ^     ^
                //  min   max

                // we want to make the relative size of the thumb reflect the relative size of the viewing area
                // viewportSize / (max + viewportSize) = cellsWidth / max
                // -> viewportSize = max * cellsHeight / (totalVisibleHeight - cellsHeight)
                // ->              = max * cellsHeight / (totalVisibleHeight - cellsHeight)
                // ->              = max * cellsHeight / max
                // ->              = cellsHeight

                // always zero
                _vScrollBar.Minimum = 0;
                if (needVertScrollbar && !double.IsInfinity(cellsHeight))
                {
                    // maximum travel distance -- not the total height
                    _vScrollBar.Maximum = Math.Max(0, totalVisibleHeight - cellsHeight);

                    // total height of the display area
                    _vScrollBar.ViewportSize = cellsHeight;
                    _vScrollBar.IsEnabled    = true;
                    // 与横向一致：上界较低时写入的偏移会被 RangeBase 夹住，上界回升后需按偏移重新同步 Value
                    if (!MathUtils.AreClose(_vScrollBar.Value, _verticalOffset))
                    {
                        _vScrollBar.Value = _verticalOffset;
                    }
                }
                else
                {
                    _vScrollBar.Maximum      = 0;
                    _vScrollBar.ViewportSize = 0;
                    _vScrollBar.IsEnabled    = false;
                }

                if (!_vScrollBar.IsVisible)
                {
                    _vScrollBar.IsVisible = true;
                    if (_vScrollBar.DesiredSize.Width == 0)
                    {
                        // We need to know the width for the rest of layout to work correctly so measure it now
                        _vScrollBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }
                }
            }
            else
            {
                _vScrollBar.Maximum = 0;
                if (_vScrollBar.IsVisible)
                {
                    _vScrollBar.IsVisible       = false;
                }
            }
        }
    }

    private void HandleVerticalScrollBarScroll(object? sender, ScrollEventArgs e)
    {
        ProcessVerticalScroll(e.ScrollEventType);
        VerticalScroll?.Invoke(sender, e);
    }
}
