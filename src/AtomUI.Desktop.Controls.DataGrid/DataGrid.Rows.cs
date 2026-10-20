// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    #region 内部属性定义

    // When the RowsPresenter's width increases, the HorizontalOffset will be incorrect until
    // the scrollbar's layout is recalculated, which doesn't occur until after the cells are measured.
    // This property exists to account for this scenario, and avoid collapsing the incorrect cells.
    internal double HorizontalAdjustment { get; private set; }

    private Size? _rowsPresenterAvailableSize;

    internal Size? RowsPresenterAvailableSize
    {
        get => _rowsPresenterAvailableSize;
        set
        {
            if (_rowsPresenterAvailableSize.HasValue && value.HasValue &&
                value.Value.Width > _rowsPresenterAvailableSize.Value.Width)
            {
                // When the available cells width increases, the horizontal offset can be incorrect.
                // Store away an adjustment to use during the CellsPresenter's measure, so that the
                // ShouldDisplayCell method correctly determines if a cell will be in view.
                //
                //     |   h. offset   |       new available cells width          |
                //     |-------------->|----------------------------------------->|
                //      __________________________________________________        |
                //     |           |           |             |            |       |
                //     |  column0  |  column1  |   column2   |  column3   |<----->|
                //     |           |           |             |            |  adj. |
                //
                double adjustment = (_horizontalOffset + value.Value.Width) - ColumnsInternal.VisibleEdgedColumnsWidth;
                HorizontalAdjustment = Math.Min(HorizontalOffset, Math.Max(0, adjustment));
            }
            else
            {
                HorizontalAdjustment = 0;
            }

            _rowsPresenterAvailableSize = value;
            if (value.HasValue)
            {
                double availableCellsWidth = double.IsPositiveInfinity(value.Value.Width)
                    ? double.PositiveInfinity
                    : Math.Max(0, value.Value.Width - ActualRowHeaderWidth);
                ResolveStarColumnWidths(availableCellsWidth);
            }
        }
    }

    internal double ActualRowHeaderWidth
    {
        get
        {
            if (!IsRowHeadersVisible)
            {
                return 0;
            }

            return !double.IsNaN(RowHeaderWidth) ? RowHeaderWidth : RowHeadersDesiredWidth;
        }
    }

    internal bool IsRowHeadersVisible =>
        (HeadersVisibility & DataGridHeadersVisibility.Row) == DataGridHeadersVisibility.Row;

    internal bool AreHorizontalGridLinesVisible =>
        GridLinesVisibility == DataGridGridLinesVisibility.Horizontal ||
        GridLinesVisibility == DataGridGridLinesVisibility.All;

    internal bool AreVerticalGridLinesVisible =>
        GridLinesVisibility == DataGridGridLinesVisibility.Vertical ||
        GridLinesVisibility == DataGridGridLinesVisibility.All;

    internal double RowHeadersDesiredWidth
    {
        get => _rowHeaderDesiredWidth;
        set
        {
            // We only auto grow
            if (_rowHeaderDesiredWidth < value)
            {
                double oldActualRowHeaderWidth = ActualRowHeaderWidth;
                _rowHeaderDesiredWidth = value;
                if (!MathUtils.AreClose(oldActualRowHeaderWidth, ActualRowHeaderWidth))
                {
                    EnsureRowHeaderWidth();
                }
            }
        }
    }

    internal bool AreRowBottomGridLinesRequired =>
        AreHorizontalGridLinesVisible;

    private bool IsBottomPaginationVisible =>
        (EffectivePaginationVisibility & DataGridPaginationVisibility.Bottom) ==
        DataGridPaginationVisibility.Bottom;

    internal bool ShouldDisplayRowBottomGridLine(int slot)
    {
        if (!AreRowBottomGridLinesRequired)
        {
            return false;
        }

        if (!IsFrameBorderVisible ||
            Footer is not null ||
            (_hScrollBar?.IsVisible ?? false) ||
            IsBottomPaginationVisible)
        {
            return true;
        }

        return slot != DisplayData.LastScrollingSlot;
    }

    internal int FirstVisibleSlot => (SlotCount > 0) ? GetNextVisibleSlot(-1) : -1;

    internal int LeftFrozenColumnCountWithFiller
    {
        get
        {
            int count = LeftFrozenColumnCount;
            if (ColumnsInternal.RowGroupSpacerColumn != null && 
                ColumnsInternal.RowGroupSpacerColumn.IsRepresented && 
                (IsRowGroupHeadersFrozen || count > 0))
            {
                // Either the RowGroupHeaders are frozen by default or the user set a frozen column count.  In both cases, we need to freeze
                // one more column than the what the public value says
                count++;
            }

            return count;
        }
    }

    internal int FrozenColumnCountWithFiller => LeftFrozenColumnCountWithFiller + RightFrozenColumnCount;

    internal int LastVisibleSlot => SlotCount > 0 ? GetPreviousVisibleSlot(SlotCount) : -1;

    internal DataGridRow? EditingRow { get; private set; }
    
    internal bool LoadingOrUnloadingRow { get; private set; }
    
    internal double[] RowGroupSublevelIndents { get; private set; }
    
    internal DataGridRowsPresenter? RowsPresenter => _rowsPresenter;
    
    #endregion
    
    // Cumulated height of all known rows, including the gridlines and details section.
    // This property returns an approximation of the actual total row heights and also
    // updates the RowHeightEstimate
    private double EdgedRowsHeightCalculated
    {
        get
        {
            if (IsRangePresentationActive)
            {
                return GetRangeExtent();
            }
            // If we're not displaying any rows or if we have infinite space the, relative height of our rows is 0
            if (DisplayData.LastScrollingSlot == -1 || double.IsPositiveInfinity(AvailableSlotElementRoom))
            {
                return 0;
            }

            UpdateRowHeightEstimateFromDisplayedRows();
            return GetHeightEstimate(0, SlotCount - 1);
        }
    }

    /// <summary>
    /// Clears the entire selection. Displayed rows are deselected explicitly to visualize
    /// potential transition effects
    /// </summary>
    internal void ClearRowSelection(bool resetAnchorSlot)
    {
        if (resetAnchorSlot)
        {
            AnchorSlot = -1;
            _rangeSelectionAnchorDataIndex = -1;
        }
        Selection = DataGridSelectionState.Empty;
    }

    /// <summary>
    /// Clears the entire selection except the indicated row. Displayed rows are deselected explicitly to
    /// visualize potential transition effects. The row indicated is selected if it is not already.
    /// </summary>
    internal void ClearRowSelection(int slotException, bool setAnchorSlot)
    {
        if (!TryGetRangeSelectionOperand(slotException, out var entry, out var scope))
        {
            return;
        }
        Selection = DataGridSelectionState.Empty.WithKey(
            entry.RowKey,
            entry.DataIndex,
            scope,
            isSelected: true,
            single: true);
        if (setAnchorSlot)
        {
            AnchorSlot = slotException;
            _rangeSelectionAnchorDataIndex = entry.DataIndex;
        }
    }

    internal int GetCollapsedSlotCount(int startSlot, int endSlot)
    {
        return 0;
    }

    internal int GetNextVisibleSlot(int slot)
    {
        return checked(slot + 1);
    }

    internal int GetPreviousVisibleSlot(int slot)
    {
        return slot - 1;
    }

    /// <summary>
    /// Returns the row associated to the provided backend data item.
    /// </summary>
    /// <param name="dataItem">backend data item</param>
    /// <returns>null if the DataSource is null, the provided item in not in the source, or the item is not displayed; otherwise, the associated Row</returns>
    internal DataGridRow? GetRowFromItem(object dataItem)
    {
        foreach (var row in DisplayData.GetScrollingRows().Cast<DataGridRow>())
        {
            if (ReferenceEquals(row.DataContext, dataItem) || Equals(row.DataContext, dataItem))
            {
                return row;
            }
        }
        return null;
    }

    internal bool GetRowSelection(int slot)
    {
        Debug.Assert(slot != -1);
        return TryGetRangeSelectionOperand(slot, out var entry, out var scope) &&
               Selection.Contains(entry.RowKey, entry.DataIndex, scope);
    }

    internal bool IsColumnDisplayed(int columnIndex)
    {
        return columnIndex >= FirstDisplayedNonFillerColumnIndex &&
               columnIndex <= DisplayData.LastTotallyDisplayedScrollingCol;
    }

    internal bool IsRowRecyclable(DataGridRow row)
    {
        return (row != EditingRow && row != _focusedRow);
    }

    internal bool IsSlotVisible(int slot)
    {
        return DisplayData.NumDisplayedScrollingElements > 0
               && slot >= DisplayData.FirstScrollingSlot
               && slot <= DisplayData.LastScrollingSlot
               && slot != -1
               && !_collapsedSlotsTable.Contains(slot);
    }

    internal void OnRowsMeasure()
    {
        if (IsRangePresentationActive)
        {
            DisplayData.PendingVerticalScrollHeight = 0;
        }
        else if (!MathUtils.IsZero(DisplayData.PendingVerticalScrollHeight))
        {
            ScrollSlotsByHeight(DisplayData.PendingVerticalScrollHeight);
            DisplayData.PendingVerticalScrollHeight = 0;
        }
        NormalizeDisplayedRowsOffset();
    }

    internal void RefreshRows(bool recycleRows, bool clearRows)
    {
        if (_rangePresentationIndex is { } presentation &&
            _rowsPresenter is not null &&
            ColumnsItemsInternal.Count > 0)
        {
            CommitRangePresentation(
                presentation.Snapshot,
                CaptureFirstCompleteRangeAnchor());
            return;
        }

        if (clearRows)
        {
            ClearRows(recycleRows);
        }
        ClearRowGroupHeadersTable();
        PopulateRowGroupHeadersTable();
        RefreshRowGroupHeaders();
        EnsureRowGroupSpacerColumn();
        InvalidateMeasure();
    }

    internal int RowIndexFromSlot(int slot)
    {
        return GetCommittedRangeRowIndex(slot);
    }

    internal bool ScrollSlotIntoView(int slot, bool scrolledHorizontally)
    {
        Debug.Assert(_collapsedSlotsTable.Contains(slot) || !IsSlotOutOfBounds(slot));

        if (scrolledHorizontally && DisplayData.FirstScrollingSlot <= slot && DisplayData.LastScrollingSlot >= slot)
        {
            // If the slot is displayed and we scrolled horizontally, column virtualization could cause the rows to grow.
            // As a result we need to force measure on the rows we're displaying and recalculate our First and Last slots
            // so they're accurate
            int displayedElementCount = DisplayData.NumDisplayedScrollingElements;
            for (int displayIndex = 0; displayIndex < displayedElementCount; displayIndex++)
            {
                if (DisplayData.GetScrollingElementAtDisplayIndex(displayIndex) is DataGridRow row)
                {
                    row.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }
            }

            UpdateDisplayedRows(DisplayData.FirstScrollingSlot, CellsEstimatedHeight);
        }

        if (DisplayData.FirstScrollingSlot < slot &&
            (DisplayData.LastScrollingSlot > slot || DisplayData.LastScrollingSlot == -1))
        {
            // The row is already displayed in its entirety
            return true;
        }
        if (DisplayData.FirstScrollingSlot == slot && slot != -1)
        {
            if (!MathUtils.IsZero(NegVerticalOffset))
            {
                // First displayed row is partially scrolled of. Let's scroll it so that NegVerticalOffset becomes 0.
                DisplayData.PendingVerticalScrollHeight = -NegVerticalOffset;
                InvalidateRowsMeasure(false /*invalidateIndividualRows*/);
            }

            return true;
        }

        double deltaY = 0;
        int    firstFullSlot;
        if (DisplayData.FirstScrollingSlot > slot)
        {
            // Scroll up to the new row so it becomes the first displayed row
            firstFullSlot = DisplayData.FirstScrollingSlot - 1;
            if (MathUtils.GreaterThan(NegVerticalOffset, 0))
            {
                deltaY = -NegVerticalOffset;
            }

            deltaY -= GetSlotElementsHeight(slot, firstFullSlot);
            if (DisplayData.FirstScrollingSlot - slot > 1)
            {
                ResetDisplayedRows();
            }

            NegVerticalOffset = 0;
            UpdateDisplayedRows(slot, CellsEstimatedHeight);
        }
        else if (DisplayData.LastScrollingSlot <= slot)
        {
            // Scroll down to the new row so it's entirely displayed.  If the height of the row
            // is greater than the height of the DataGrid, then show the top of the row at the top
            // of the grid
            firstFullSlot = DisplayData.LastScrollingSlot;
            // Figure out how much of the last row is cut off
            double rowHeight       = GetExactSlotElementHeight(DisplayData.LastScrollingSlot);
            double availableHeight = AvailableSlotElementRoom + rowHeight;
            if (MathUtils.AreClose(rowHeight, availableHeight))
            {
                if (DisplayData.LastScrollingSlot == slot)
                {
                    // We're already at the very bottom so we don't need to scroll down further
                    return true;
                }
                // We're already showing the entire last row so don't count it as part of the delta
                firstFullSlot++;
            }
            else if (rowHeight > availableHeight)
            {
                firstFullSlot++;
                deltaY += rowHeight - availableHeight;
            }

            // sum up the height of the rest of the full rows
            if (slot >= firstFullSlot)
            {
                deltaY += GetSlotElementsHeight(firstFullSlot, slot);
            }

            // If the first row we're displaying is no longer adjacent to the rows we have
            // simply discard the ones we have
            if (slot - DisplayData.LastScrollingSlot > 1)
            {
                ResetDisplayedRows();
            }

            if (MathUtils.GreaterThanOrClose(GetExactSlotElementHeight(slot), CellsEstimatedHeight))
            {
                // The entire row won't fit in the DataGrid so we start showing it from the top
                NegVerticalOffset = 0;
                UpdateDisplayedRows(slot, CellsEstimatedHeight);
            }
            else
            {
                UpdateDisplayedRowsFromBottom(slot);
            }
        }

        _verticalOffset += deltaY;
        if (_verticalOffset < 0 || DisplayData.FirstScrollingSlot == 0)
        {
            // We scrolled too far because a row's height was larger than its approximation
            _verticalOffset = NegVerticalOffset;
        }
        
        Debug.Assert(MathUtils.LessThanOrClose(NegVerticalOffset, _verticalOffset));

        SetVerticalOffset(_verticalOffset);

        InvalidateMeasure();
        InvalidateRowsMeasure(false /*invalidateIndividualRows*/);

        return true;
    }

    internal void SetRowSelection(int slot, bool isSelected, bool setAnchorSlot)
    {
        Debug.Assert(!(!isSelected && setAnchorSlot));
        if (SelectionMode == DataGridSelectionMode.None ||
            !TryGetRangeSelectionOperand(slot, out var entry, out var scope))
        {
            return;
        }
        Selection = Selection.WithKey(
            entry.RowKey,
            entry.DataIndex,
            scope,
            isSelected,
            single: SelectionMode == DataGridSelectionMode.Single);
        if (setAnchorSlot)
        {
            AnchorSlot = slot;
            _rangeSelectionAnchorDataIndex = entry.DataIndex;
        }
    }

    // For now, all scenarios are for isSelected == true.
    internal void SetRowsSelection(int startSlot, int endSlot /*, bool isSelected*/)
    {
        Debug.Assert(startSlot >= 0 && startSlot < SlotCount);
        Debug.Assert(endSlot >= 0 && endSlot < SlotCount);
        Debug.Assert(startSlot <= endSlot);

        if (SelectionMode == DataGridSelectionMode.None ||
            !TryGetRangeSelectionOperand(startSlot, out var start, out var scope) ||
            !TryGetRangeSelectionOperand(endSlot, out var end, out _))
        {
            return;
        }
        if (SelectionMode == DataGridSelectionMode.Single)
        {
            Selection = DataGridSelectionState.Empty.WithKey(
                end.RowKey,
                end.DataIndex,
                scope,
                isSelected: true,
                single: true);
            return;
        }
        Selection = Selection.WithInterval(
            Math.Min(start.DataIndex, end.DataIndex),
            checked(Math.Max(start.DataIndex, end.DataIndex) + 1),
            scope);
    }

    internal int SlotFromRowIndex(int rowIndex)
    {
        return GetCommittedRangeSlot(rowIndex);
    }

    private void ApplyDisplayedRowsState(int startSlot, int endSlot)
    {
        if (DisplayData.NumDisplayedScrollingElements == 0)
        {
            return;
        }

        int firstSlot = Math.Max(DisplayData.FirstScrollingSlot, startSlot);
        int lastSlot  = Math.Min(DisplayData.LastScrollingSlot, endSlot);

        if (firstSlot >= 0 && lastSlot >= firstSlot)
        {
            Debug.Assert(lastSlot >= firstSlot);
            int slot = GetNextVisibleSlot(firstSlot - 1);
            while (slot <= lastSlot)
            {
                if (DisplayData.GetDisplayedElement(slot) is DataGridRow row)
                {
                    row.ApplyState();
                }

                slot = GetNextVisibleSlot(slot);
            }
        }
    }

    private void RefreshDisplayedRowsGridLines()
    {
        foreach (var element in DisplayData.GetScrollingElements())
        {
            if (element is DataGridRow row)
            {
                row.EnsureGridLines();
            }
            else if (element is DataGridRowGroupHeader groupHeader)
            {
                groupHeader.EnsureGridLines();
            }
        }
    }

    private void ClearRows(bool recycle)
    {
        // Need to clean up recycled rows even if the RowCount is 0
        SetCurrentCellCore(-1, -1, commitEdit: false, endRowEdit: false);
        ClearRowSelection(resetAnchorSlot: true);
        UnloadElements(recycle);

        _showDetailsTable.Clear();
        _rowDetailsHeightEstimateTable.Clear();
        SlotCount         = 0;
        NegVerticalOffset = 0;
        SetVerticalOffset(0);
        ComputeScrollBarsLayout();
    }

    // Updates _collapsedSlotsTable and returns the number of pixels that were collapsed
    internal DataGridRowsEnumerable GetAllRows()
    {
        return new DataGridRowsEnumerable(_rowsPresenter);
    }

    internal readonly struct DataGridRowsEnumerable
    {
        private readonly DataGridRowsPresenter? _rowsPresenter;

        public DataGridRowsEnumerable(DataGridRowsPresenter? rowsPresenter)
        {
            _rowsPresenter = rowsPresenter;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_rowsPresenter?.Children);
        }

        internal struct Enumerator
        {
            private readonly IList<Control>? _children;
            private int _index;
            private DataGridRow? _current;

            public Enumerator(IList<Control>? children)
            {
                _children = children;
                _index    = -1;
                _current  = null;
            }

            public DataGridRow Current => _current!;

            public bool MoveNext()
            {
                if (_children == null)
                {
                    return false;
                }

                while (++_index < _children.Count)
                {
                    if (_children[_index] is DataGridRow row)
                    {
                        _current = row;
                        return true;
                    }
                }

                _current = null;
                return false;
            }
        }
    }

    // Expands slots from startSlot to endSlot inclusive and adds the amount expanded in this suboperation to
    // the given totalHeightChanged of the entire operation
    private void GenerateEditingElements()
    {
        if (EditingRow != null)
        {
            Debug.Assert(EditingRow.Cells.Count == ColumnsItemsInternal.Count);
            Debug.Assert(EditingRow.DataContext != null);
            int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
            for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
            {
                DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
                if (!column.IsVisible || column.IsReadOnly)
                {
                    continue;
                }

                column.GenerateEditingElementInternal(EditingRow.Cells[column.Index], EditingRow.DataContext);
            }
        }
    }

    /// <summary>
    /// Returns a row for the provided index. The row gets first loaded through the LoadingRow event.
    /// </summary>
    private DataGridRow GenerateRow(int rowIndex, int slot)
    {
        if (IsRangePresentationActive)
        {
            var entry = GetCommittedRangeEntry(slot);
            if (entry.Kind != DataGridSourceEntryKind.Data || entry.WindowDataIndex != rowIndex)
            {
                throw new DataGridPresentationInvariantException(
                    $"Committed slot {slot} is not data row {rowIndex}.");
            }
            return (DataGridRow)CreateRangeElement(entry, slot);
        }
        return GenerateRow(rowIndex, slot, RangeDataAccess.GetDataItem(rowIndex));
    }

    /// <summary>
    /// Returns a row for the provided index. The row gets first loaded through the LoadingRow event.
    /// </summary>
    private DataGridRow GenerateRow(int rowIndex, int slot, object? dataContext)
    {
        Debug.Assert(rowIndex > -1);
        DataGridRow? dataGridRow = GetGeneratedRow(dataContext);
        if (dataGridRow == null)
        {
            dataGridRow                                       = DisplayData.GetUsedRow() ?? new DataGridRow();
            dataGridRow.RowKey                                = default;
            dataGridRow.DataIndex                              = -1;
            dataGridRow.IsRangeBacked                          = false;
            dataGridRow.Index                                 = rowIndex;
            dataGridRow.Slot                                  = slot;
            dataGridRow.OwningGrid                            = this;
            dataGridRow.DataContext                           = dataContext;
            dataGridRow[!DataGridRow.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            dataGridRow[!DataGridRow.SizeTypeProperty]        = this[!SizeTypeProperty];

            CompleteCellsCollection(dataGridRow);
            NotifyLoadingRow(new DataGridRowEventArgs(dataGridRow));
        }

        return dataGridRow;
    }

    internal DataGridRow GetGeneratedGhostRow(object? dataContext)
    {
        var dataGridRow = new DataGridRow();
        dataGridRow.OwningGrid                            = this;
        dataGridRow.DataContext                           = dataContext;
        dataGridRow[!DataGridRow.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        dataGridRow[!DataGridRow.SizeTypeProperty]        = this[!SizeTypeProperty];
        CompleteCellsCollection(dataGridRow);
        return dataGridRow;
    }

    /// <summary>
    /// Returns the exact row height, whether it is currently displayed or not.
    /// The row is generated and added to the displayed rows in case it is not already displayed.
    /// The horizontal gridlines thickness are added.
    /// </summary>
    private double GetExactSlotElementHeight(int slot)
    {
        Debug.Assert((slot >= 0) && slot < SlotCount);

        if (IsSlotVisible(slot))
        {
            Debug.Assert(DisplayData.GetDisplayedElement(slot) != null);
            return GetMeasuredSlotElementHeight(slot, DisplayData.GetDisplayedElement(slot));
        }

        Control slotElement = InsertDisplayedElement(slot, true /*updateSlotInformation*/);
        Debug.Assert(slotElement != null);
        return GetMeasuredSlotElementHeight(slot, slotElement);
    }

    // Returns an estimate for the height of the slots between fromSlot and toSlot
    private double GetHeightEstimate(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || toSlot < fromSlot || toSlot >= SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(fromSlot));
        }
        return SparseHeightDeltaIndex.SafeAdd(
            GetRangeOffset(checked(toSlot + 1)),
            -GetRangeOffset(fromSlot));
    }

    /// <summary>
    /// If the provided slot is displayed, returns the exact height.
    /// If the slot is not displayed, returns a default height.
    /// </summary>
    private double GetSlotElementHeight(int slot)
    {
        Debug.Assert(slot >= 0 && slot < SlotCount);
        if (IsSlotVisible(slot))
        {
            Debug.Assert(DisplayData.GetDisplayedElement(slot) != null);
            return GetMeasuredSlotElementHeight(slot, DisplayData.GetDisplayedElement(slot));
        }
        
        return GetEstimatedSlotElementHeight(slot);
    }

    private static bool IsInvalidSlotElementHeight(double height)
    {
        return double.IsNaN(height) || double.IsInfinity(height) || height < 0;
    }

    private double GetEstimatedSlotElementHeight(int slot)
    {
        return GetRangeHeight(slot);
    }

    private double GetMeasuredSlotElementHeight(int slot, Control slotElement)
    {
        double estimatedHeight = GetEstimatedSlotElementHeight(slot);
        double desiredHeight = slotElement is DataGridRow row
            ? row.GetVirtualizedDisplayHeight(estimatedHeight, RowDetailsHeightEstimate)
            : slotElement.DesiredSize.Height;
        if (IsInvalidSlotElementHeight(desiredHeight))
        {
            return estimatedHeight;
        }

        var measuredHeight = desiredHeight;
        if (slotElement is DataGridRow dataGridRow && GetRowDetailsVisibility(dataGridRow.Index))
        {
            double targetHeight = dataGridRow.TargetHeight;
            if (!IsInvalidSlotElementHeight(targetHeight))
            {
                measuredHeight = MathUtils.GreaterThan(targetHeight, desiredHeight)
                    ? targetHeight
                    : desiredHeight;
            }
            if (!IsInvalidSlotElementHeight(estimatedHeight) &&
                MathUtils.GreaterThan(estimatedHeight, measuredHeight))
            {
                measuredHeight = estimatedHeight;
            }
        }

        if (IsRangePresentationActive)
        {
            var heightClass = slotElement is DataGridRowGroupHeader groupHeader
                ? DataGridHeightClass.GroupHeader(groupHeader.Level)
                : DataGridHeightClass.Data;
            RecordRangeMeasuredHeight(slot, measuredHeight, heightClass);
        }
        return measuredHeight;
    }

    internal double GetDisplayedElementHeight(Control element)
    {
        return element switch
        {
            DataGridRow row => GetMeasuredSlotElementHeight(row.Slot, row),
            DataGridRowGroupHeader groupHeader when groupHeader.DisplaySlot >= 0 =>
                GetMeasuredSlotElementHeight(groupHeader.DisplaySlot, groupHeader),
            _ => IsInvalidSlotElementHeight(element.DesiredSize.Height) ? 0 : element.DesiredSize.Height
        };
    }

    internal double GetAutoSizeDesiredHeight()
    {
        double desiredHeight = DesiredSize.Height;
        if (_rowsPresenter == null || DisplayData.LastScrollingSlot == -1)
        {
            return desiredHeight;
        }

        double rowsHeight = EdgedRowsHeightCalculated;
        if (IsInvalidSlotElementHeight(rowsHeight) || MathUtils.LessThanOrClose(rowsHeight, 0))
        {
            return desiredHeight;
        }

        double rowsPresenterHeight = _rowsPresenter.DesiredSize.Height;
        double chromeHeight = IsInvalidSlotElementHeight(desiredHeight) ||
                              IsInvalidSlotElementHeight(rowsPresenterHeight)
            ? 0
            : Math.Max(0, desiredHeight - rowsPresenterHeight);

        return chromeHeight + rowsHeight;
    }

    /// <summary>
    /// Cumulates the approximate height of the rows from fromRowIndex to toRowIndex included.
    /// Including the potential gridline thickness.
    /// </summary>
    private double GetSlotElementsHeight(int fromSlot, int toSlot)
    {
        Debug.Assert(toSlot >= fromSlot);

        if (IsRangePresentationActive)
        {
            return SparseHeightDeltaIndex.SafeAdd(
                GetRangeOffset(checked(toSlot + 1)),
                -GetRangeOffset(fromSlot));
        }

        double height = 0;
        for (int slot = fromSlot; slot <= toSlot; slot++)
        {
            height += GetSlotElementHeight(slot);
        }

        return height;
    }

    /// <summary>
    /// Checks if the row for the provided dataContext has been generated and is present
    /// in either the loaded rows, pre-fetched rows, or editing row.
    /// The displayed rows are *not* searched. Returns null if the row does not belong to those 3 categories.
    /// </summary>
    private DataGridRow? GetGeneratedRow(object? dataContext)
    {
        // Check the list of rows being loaded via the LoadingRow event.
        DataGridRow? dataGridRow = GetLoadedRow(dataContext);
        if (dataGridRow != null)
        {
            return dataGridRow;
        }

        // Check the potential editing row.
        if (EditingRow != null && dataContext == EditingRow.DataContext)
        {
            return EditingRow;
        }

        // Check the potential focused row.
        if (_focusedRow != null && dataContext == _focusedRow.DataContext)
        {
            return _focusedRow;
        }

        return null;
    }

    private DataGridRow? GetLoadedRow(object? dataContext)
    {
        foreach (DataGridRow dataGridRow in _loadedRows)
        {
            if (dataGridRow.DataContext == dataContext)
            {
                return dataGridRow;
            }
        }

        return null;
    }

    private Control InsertDisplayedElement(int slot, bool updateSlotInformation)
    {
        var slotElement = CreateRangeElement(GetCommittedRangeEntry(slot), slot);

        InsertDisplayedElement(slot, slotElement, wasNewlyAdded: false, updateSlotInformation: updateSlotInformation);
        return slotElement;
    }

    private void InsertDisplayedElement(int slot, Control element, bool wasNewlyAdded, bool updateSlotInformation)
    {
        // We can only support creating new rows that are adjacent to the currently visible rows
        // since they need to be added to the visual tree for us to Measure them.
        Debug.Assert(DisplayData.FirstScrollingSlot == -1 ||
                     slot >= GetPreviousVisibleSlot(DisplayData.FirstScrollingSlot) &&
                     slot <= GetNextVisibleSlot(DisplayData.LastScrollingSlot));
        Debug.Assert(element != null);

        if (_rowsPresenter != null)
        {
            DataGridRowGroupHeader? groupHeader = null;
            DataGridRow?            row         = element as DataGridRow;
            if (row != null)
            {
                LoadRowVisualsForDisplay(row);
                row.InvalidateMeasure();
                row.InvalidateArrange();

                if (IsRowRecyclable(row))
                {
                    if (!_rowsPresenter.Children.Contains(element))
                    {
                        _rowsPresenter.Children.Add(row);
                    }
                }
                else
                {
                    row.ClearHiddenClipGeometry();
                    Debug.Assert(row.Index == RowIndexFromSlot(slot));
                }
            }
            else
            {
                groupHeader = element as DataGridRowGroupHeader;
                Debug.Assert(groupHeader != null); // Nothing other and Rows and RowGroups now
                groupHeader.TotalIndent =
                    (groupHeader.Level == 0) ? 0 : RowGroupSublevelIndents[groupHeader.Level - 1];
                if (!_rowsPresenter.Children.Contains(element))
                {
                    _rowsPresenter.Children.Add(element);
                }

                groupHeader.LoadVisualsForDisplay();
            }

            // Measure the element and update AvailableRowRoom
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double measuredHeight = GetMeasuredSlotElementHeight(slot, element);
            AvailableSlotElementRoom -= measuredHeight;

            if (groupHeader != null)
            {
                EnsureRangeGroupMetrics(groupHeader.Level + 1);
                _rowGroupHeightsByLevel[groupHeader.Level] = measuredHeight;
            }

            if (row != null &&
                MathUtils.AreClose(RowHeightEstimate, DefaultRowHeight) &&
                double.IsNaN(row.Height) &&
                !GetRowDetailsVisibility(row.Index))
            {
                RowHeightEstimate = measuredHeight;
                RebaseRangeHeightIndex(discardMeasuredDataHeights: false);
            }
        }

        if (wasNewlyAdded)
        {
            DisplayData.CorrectSlotsAfterInsertion(slot, element, isCollapsed: false);
        }
        else
        {
            DisplayData.LoadScrollingSlot(slot, element, updateSlotInformation);
        }
    }

    private void InvalidateRowHeightEstimate()
    {
        // Start from scratch and assume that we haven't estimated any rows
        _lastEstimatedRow = -1;
    }

    private void ResetRowHeightEstimateForDataGeneration()
    {
        RowHeightEstimate = DefaultRowHeight;
        InvalidateRowHeightEstimate();
    }

    private void NotifyElementsChanged(bool grew)
    {
        if (grew &&
            ColumnsItemsInternal.Count > 0 &&
            CurrentColumnIndex == -1)
        {
            MakeFirstDisplayedCellCurrentCell();
        }
    }

    private void LoadRowVisualsForDisplay(DataGridRow row)
    {
        // If the row has been recycled, reapply the BackgroundBrush
        if (row.IsRecycled)
        {
            row.ApplyCellsState();
            _rowsPresenter?.InvalidateChildIndex(row);
        }
        else if (row == EditingRow)
        {
            row.ApplyCellsState();
        }

        // Set the Row's Style if we one's defined at the DataGrid level and the user didn't
        // set one at the row level
        //EnsureElementStyle(row, null, RowStyle);
        row.EnsureHeaderStyleAndVisibility(null);

        // Check to see if the row contains the CurrentCell, apply its state.
        if (CurrentColumnIndex != -1 &&
            CurrentSlot != -1 &&
            row.Index == CurrentSlot)
        {
            row.Cells[CurrentColumnIndex].UpdatePseudoClasses();
        }

        if (row.IsSelected || row.IsRecycled)
        {
            row.ApplyState();
        }

        // Show or hide RowDetails based on DataGrid settings
        EnsureRowDetailsVisibility(row, raiseNotification: false, animate: false);
    }

    private void RemoveDisplayedElement(int slot, bool wasDeleted, bool updateSlotInformation)
    {
        Debug.Assert(slot >= DisplayData.FirstScrollingSlot &&
                     slot <= DisplayData.LastScrollingSlot);

        RemoveDisplayedElement(DisplayData.GetDisplayedElement(slot), slot, wasDeleted, updateSlotInformation);
    }

    // Removes an element from display either because it was deleted or it was scrolled out of view.
    // If the element was provided, it will be the element removed; otherwise, the element will be
    // retrieved from the slot information
    private void RemoveDisplayedElement(Control element, int slot, bool wasDeleted, bool updateSlotInformation)
    {
        if (element is DataGridRow dataGridRow)
        {
            if (IsRowRecyclable(dataGridRow))
            {
                UnloadRow(dataGridRow);
            }
            else
            {
                dataGridRow.ApplyHiddenClipGeometry();
            }
        }
        else if (element is DataGridRowGroupHeader groupHeader)
        {
            OnUnloadingRowGroup(new DataGridRowGroupHeaderEventArgs(groupHeader));
            DisplayData.AddRecyclableRowGroupHeader(groupHeader);
        }
        else if (_rowsPresenter != null)
        {
            _rowsPresenter.Children.Remove(element);
        }

        // Update DisplayData
        if (wasDeleted)
        {
            DisplayData.CorrectSlotsAfterDeletion(slot, wasCollapsed: false);
        }
        else
        {
            DisplayData.UnloadScrollingElement(slot, updateSlotInformation, wasDeleted: false);
        }
    }

    /// <summary>
    /// Removes all of the editing elements for the row that is just leaving editing mode.
    /// </summary>
    private void RemoveEditingElements()
    {
        if (EditingRow != null)
        {
            Debug.Assert(EditingRow.Cells.Count == ColumnsItemsInternal.Count);
            foreach (DataGridColumn column in Columns)
            {
                column.RemoveEditingElement();
            }
        }
    }

    private void RemoveNonDisplayedRows(int newFirstDisplayedSlot, int newLastDisplayedSlot)
    {
        while (DisplayData.FirstScrollingSlot < newFirstDisplayedSlot)
        {
            // Need to add rows above the lastDisplayedScrollingRow
            RemoveDisplayedElement(DisplayData.FirstScrollingSlot, false /*wasDeleted*/,
                true /*updateSlotInformation*/);
        }

        while (DisplayData.LastScrollingSlot > newLastDisplayedSlot)
        {
            // Need to remove rows below the lastDisplayedScrollingRow
            RemoveDisplayedElement(DisplayData.LastScrollingSlot, false /*wasDeleted*/, true /*updateSlotInformation*/);
        }
    }

    private void ResetDisplayedRows()
    {
        if (UnloadingRow != null || UnloadingRowGroup != null)
        {
            int displayedElementCount = DisplayData.NumDisplayedScrollingElements;
            for (int displayIndex = 0; displayIndex < displayedElementCount; displayIndex++)
            {
                Control element = DisplayData.GetScrollingElementAtDisplayIndex(displayIndex);
                // Raise Unloading Row for all the rows we're displaying
                if (element is DataGridRow row)
                {
                    if (IsRowRecyclable(row))
                    {
                        NotifyUnloadingRow(new DataGridRowEventArgs(row));
                    }
                }
                // Raise Unloading Row for all the RowGroupHeaders we're displaying
                else if (element is DataGridRowGroupHeader groupHeader)
                {
                    OnUnloadingRowGroup(new DataGridRowGroupHeaderEventArgs(groupHeader));
                }
            }
        }

        DisplayData.ClearElements(recycle: true);
        AvailableSlotElementRoom = CellsEstimatedHeight;
    }

    /// <summary>
    /// Determines whether the row at the provided index must be displayed or not.
    /// </summary>
    private bool SlotIsDisplayed(int slot)
    {
        Debug.Assert(slot >= 0);

        if (slot >= DisplayData.FirstScrollingSlot &&
            slot <= DisplayData.LastScrollingSlot)
        {
            // Additional row takes the spot of a displayed row - it is necessarily displayed
            return true;
        }
        if (DisplayData.FirstScrollingSlot == -1 &&
                 CellsEstimatedHeight > 0 &&
                 CellsWidth > 0)
        {
            return true;
        }
        if (slot == GetNextVisibleSlot(DisplayData.LastScrollingSlot))
        {
            if (AvailableSlotElementRoom > 0)
            {
                // There is room for this additional row
                return true;
            }
        }

        return false;
    }

    // Updates display information and displayed rows after scrolling the given number of pixels
    internal void ScrollSlotsByHeight(double height)
    {
        Debug.Assert(DisplayData.FirstScrollingSlot >= 0);
        Debug.Assert(!MathUtils.IsZero(height));

        _scrollingByHeight = true;
        try
        {
            double deltaY                = 0;
            int    newFirstScrollingSlot = DisplayData.FirstScrollingSlot;
            double newVerticalOffset     = _verticalOffset + height;
            if (height > 0)
            {
                // Scrolling Down
                int lastVisibleSlot = GetPreviousVisibleSlot(SlotCount);
                if (_vScrollBar != null && MathUtils.AreClose(_vScrollBar.Maximum, newVerticalOffset))
                {
                    // We've scrolled to the bottom of the ScrollBar, automatically place the user at the very bottom
                    // of the DataGrid.  If this produces very odd behavior, evaluate the coping strategy used by
                    // OnRowMeasure(Size).  For most data, this should be unnoticeable.
                    ResetDisplayedRows();
                    UpdateDisplayedRowsFromBottom(lastVisibleSlot);
                    newFirstScrollingSlot = DisplayData.FirstScrollingSlot;
                }
                else
                {
                    deltaY = GetSlotElementHeight(newFirstScrollingSlot) - NegVerticalOffset;
                    if (MathUtils.LessThan(height, deltaY))
                    {
                        // We've merely covered up more of the same row we're on
                        NegVerticalOffset += height;
                    }
                    else
                    {
                        // Figure out what row we've scrolled down to and update the value for NegVerticalOffset
                        NegVerticalOffset = 0;
                        //
                        if (height > 2 * CellsEstimatedHeight &&
                            (RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.VisibleWhenSelected ||
                             RowDetailsTemplate == null))
                        {
                            // Very large scroll occurred. Instead of determining the exact number of scrolled off rows,
                            // let's estimate the number based on RowHeight.
                            ResetDisplayedRows();
                            double singleRowHeightEstimate = RowHeightEstimate +
                                                             (RowDetailsVisibilityMode ==
                                                              DataGridRowDetailsVisibilityMode.Visible
                                                                 ? RowDetailsHeightEstimate
                                                                 : 0);
                            int scrolledToSlot = newFirstScrollingSlot + (int)(height / singleRowHeightEstimate);
                            scrolledToSlot += _collapsedSlotsTable.GetIndexCount(newFirstScrollingSlot,
                                newFirstScrollingSlot + scrolledToSlot);
                            newFirstScrollingSlot = Math.Min(GetNextVisibleSlot(scrolledToSlot), lastVisibleSlot);
                        }
                        else
                        {
                            while (MathUtils.LessThanOrClose(deltaY, height))
                            {
                                if (newFirstScrollingSlot < lastVisibleSlot)
                                {
                                    if (IsSlotVisible(newFirstScrollingSlot))
                                    {
                                        // Make the top row available for reuse
                                        RemoveDisplayedElement(newFirstScrollingSlot, false /*wasDeleted*/,
                                            true /*updateSlotInformation*/);
                                    }

                                    newFirstScrollingSlot = GetNextVisibleSlot(newFirstScrollingSlot);
                                }
                                else
                                {
                                    // We're being told to scroll beyond the last row, ignore the extra
                                    NegVerticalOffset = 0;
                                    break;
                                }

                                double rowHeight       = GetExactSlotElementHeight(newFirstScrollingSlot);
                                double remainingHeight = height - deltaY;
                                if (MathUtils.LessThanOrClose(rowHeight, remainingHeight))
                                {
                                    deltaY += rowHeight;
                                }
                                else
                                {
                                    NegVerticalOffset = remainingHeight;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Scrolling Up
                if (MathUtils.GreaterThanOrClose(height + NegVerticalOffset, 0))
                {
                    // We've merely exposing more of the row we're on
                    NegVerticalOffset += height;
                }
                else
                {
                    // Figure out what row we've scrolled up to and update the value for NegVerticalOffset
                    deltaY            = -NegVerticalOffset;
                    NegVerticalOffset = 0;
                    //

                    if (height < -2 * CellsEstimatedHeight &&
                        (RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.VisibleWhenSelected ||
                         RowDetailsTemplate == null))
                    {
                        // Very large scroll occurred. Instead of determining the exact number of scrolled off rows,
                        // let's estimate the number based on RowHeight.
                        if (newVerticalOffset == 0)
                        {
                            newFirstScrollingSlot = 0;
                        }
                        else
                        {
                            double singleRowHeightEstimate = RowHeightEstimate +
                                                             (RowDetailsVisibilityMode ==
                                                              DataGridRowDetailsVisibilityMode.Visible
                                                                 ? RowDetailsHeightEstimate
                                                                 : 0);
                            int scrolledToSlot = newFirstScrollingSlot + (int)(height / singleRowHeightEstimate);
                            scrolledToSlot -= _collapsedSlotsTable.GetIndexCount(scrolledToSlot, newFirstScrollingSlot);

                            newFirstScrollingSlot = Math.Max(0, GetPreviousVisibleSlot(scrolledToSlot + 1));
                        }

                        ResetDisplayedRows();
                    }
                    else
                    {
                        int lastScrollingSlot = DisplayData.LastScrollingSlot;
                        while (MathUtils.GreaterThan(deltaY, height))
                        {
                            if (newFirstScrollingSlot > 0)
                            {
                                if (IsSlotVisible(lastScrollingSlot))
                                {
                                    // Make the bottom row available for reuse
                                    RemoveDisplayedElement(lastScrollingSlot, wasDeleted: false,
                                        updateSlotInformation: true);
                                    lastScrollingSlot = GetPreviousVisibleSlot(lastScrollingSlot);
                                }

                                newFirstScrollingSlot = GetPreviousVisibleSlot(newFirstScrollingSlot);
                            }
                            else
                            {
                                NegVerticalOffset = 0;
                                break;
                            }

                            double rowHeight       = GetExactSlotElementHeight(newFirstScrollingSlot);
                            double remainingHeight = height - deltaY;
                            if (MathUtils.LessThanOrClose(rowHeight + remainingHeight, 0))
                            {
                                deltaY -= rowHeight;
                            }
                            else
                            {
                                NegVerticalOffset = rowHeight + remainingHeight;
                                break;
                            }
                        }
                    }
                }

                if (MathUtils.GreaterThanOrClose(0, newVerticalOffset) && newFirstScrollingSlot != 0)
                {
                    // We've scrolled to the top of the ScrollBar, automatically place the user at the very top
                    // of the DataGrid.  If this produces very odd behavior, evaluate the RowHeight estimate.
                    // strategy. For most data, this should be unnoticeable.
                    ResetDisplayedRows();
                    NegVerticalOffset = 0;
                    UpdateDisplayedRows(0, CellsEstimatedHeight);
                    newFirstScrollingSlot = 0;
                }
            }

            newFirstScrollingSlot = NormalizeFirstScrollingSlotOffset(newFirstScrollingSlot);

            UpdateDisplayedRows(newFirstScrollingSlot, CellsEstimatedHeight);

            NormalizeDisplayedRowsOffset(newVerticalOffset);

            Debug.Assert(DisplayData.FirstScrollingSlot >= 0);
            Debug.Assert(GetExactSlotElementHeight(DisplayData.FirstScrollingSlot) > NegVerticalOffset);

            if (DisplayData.FirstScrollingSlot == 0)
            {
                _verticalOffset = NegVerticalOffset;
            }
            else if (MathUtils.GreaterThan(NegVerticalOffset, newVerticalOffset))
            {
                // The scrolled-in row was larger than anticipated. Adjust the DataGrid so the ScrollBar thumb
                // can stay in the same place
                NegVerticalOffset = newVerticalOffset;
                _verticalOffset   = newVerticalOffset;
            }
            else
            {
                _verticalOffset = newVerticalOffset;
            }

            Debug.Assert(!(_verticalOffset == 0 && NegVerticalOffset == 0 && DisplayData.FirstScrollingSlot > 0));

            SetVerticalOffset(_verticalOffset);

            DisplayData.FullyRecycleElements();

            Debug.Assert(MathUtils.GreaterThanOrClose(NegVerticalOffset, 0));
            Debug.Assert(MathUtils.GreaterThanOrClose(_verticalOffset, NegVerticalOffset));
        }
        finally
        {
            _scrollingByHeight = false;
        }
    }

    private int NormalizeFirstScrollingSlotOffset(int firstScrollingSlot)
    {
        var committedEndExclusive = IsRangePresentationActive
            ? checked(
                _rangePresentationIndex!.Snapshot.CommittedViewport.FirstVisibleIndex +
                _rangePresentationIndex.Snapshot.CommittedViewport.VisibleCount)
            : SlotCount;
        while (firstScrollingSlot >= 0 &&
               firstScrollingSlot < SlotCount &&
               firstScrollingSlot < committedEndExclusive)
        {
            double firstElementHeight = GetExactSlotElementHeight(firstScrollingSlot);
            if (MathUtils.LessThan(NegVerticalOffset, firstElementHeight))
            {
                break;
            }

            int nextVisibleSlot = GetNextVisibleSlot(firstScrollingSlot);
            if (nextVisibleSlot < 0 || nextVisibleSlot >= SlotCount)
            {
                NegVerticalOffset = 0;
                break;
            }
            if (nextVisibleSlot >= committedEndExclusive)
            {
                QueueRangeViewportBeyondCommittedRange(firstScrollingSlot);
                NegVerticalOffset = Math.Min(
                    NegVerticalOffset,
                    Math.Max(0, Math.BitDecrement(firstElementHeight)));
                break;
            }

            NegVerticalOffset = Math.Max(0, NegVerticalOffset - firstElementHeight);
            firstScrollingSlot = nextVisibleSlot;
        }

        return firstScrollingSlot;
    }

    private void QueueRangeViewportBeyondCommittedRange(int firstVisibleSlot)
    {
        if (!IsRangePresentationActive || firstVisibleSlot < 0 || firstVisibleSlot >= SlotCount)
        {
            return;
        }
        var committed = _rangePresentationIndex!.Snapshot.CommittedViewport;
        var requestedCount = (int)Math.Min(
            SlotCount - (long)firstVisibleSlot,
            (long)committed.VisibleCount + GetInitialRangeVisibleCount());
        if (requestedCount > committed.VisibleCount ||
            firstVisibleSlot != committed.FirstVisibleIndex)
        {
            QueueRangeViewport(new DataGridDesiredViewport(
                firstVisibleSlot,
                Math.Max(1, requestedCount),
                1));
        }
    }

    private double GetDisplayedRowsVerticalOffsetEstimate()
    {
        if (DisplayData.FirstScrollingSlot < 0)
        {
            return 0;
        }

        double offset = 0;
        int slot = FirstVisibleSlot;
        while (slot >= 0 && slot < DisplayData.FirstScrollingSlot)
        {
            offset += GetSlotElementHeight(slot);
            slot = GetNextVisibleSlot(slot);
        }

        return offset + NegVerticalOffset;
    }

    private void NormalizeDisplayedRowsOffset(double? verticalOffset = null)
    {
        if (DisplayData.FirstScrollingSlot < 0)
        {
            return;
        }

        int normalizedFirstScrollingSlot = NormalizeFirstScrollingSlotOffset(DisplayData.FirstScrollingSlot);
        if (normalizedFirstScrollingSlot != DisplayData.FirstScrollingSlot)
        {
            UpdateDisplayedRows(normalizedFirstScrollingSlot, CellsEstimatedHeight);
        }

        NormalizeBottomDisplayedRowsOffset(verticalOffset ?? _verticalOffset);
    }

    private void NormalizeBottomDisplayedRowsOffset(double verticalOffset)
    {
        if (DisplayData.LastScrollingSlot != LastVisibleSlot ||
            MathUtils.LessThanOrClose(CellsEstimatedHeight, 0) ||
            !IsVerticalOffsetAtBottom(verticalOffset))
        {
            return;
        }

        double displayedHeight = 0;
        int displayedElementCount = DisplayData.NumDisplayedScrollingElements;
        for (int displayIndex = 0; displayIndex < displayedElementCount; displayIndex++)
        {
            displayedHeight += GetDisplayedElementHeight(DisplayData.GetScrollingElementAtDisplayIndex(displayIndex));
        }

        double overflow = displayedHeight - NegVerticalOffset - CellsEstimatedHeight;
        if (!MathUtils.GreaterThan(overflow, 0))
        {
            return;
        }

        NegVerticalOffset += overflow;
        int normalizedFirstScrollingSlot = NormalizeFirstScrollingSlotOffset(DisplayData.FirstScrollingSlot);
        UpdateDisplayedRows(normalizedFirstScrollingSlot, CellsEstimatedHeight);
    }

    private bool IsVerticalOffsetAtBottom(double verticalOffset)
    {
        if (_vScrollBar != null && _vScrollBar.IsVisible)
        {
            return MathUtils.GreaterThanOrClose(verticalOffset, _vScrollBar.Maximum);
        }

        double maximum = EdgedRowsHeightCalculated - CellsEstimatedHeight;
        return MathUtils.GreaterThanOrClose(verticalOffset, maximum);
    }

    private void SelectDisplayedElement(int slot)
    {
        Debug.Assert(IsSlotVisible(slot));
        Control element = DisplayData.GetDisplayedElement(slot);
        if (element is DataGridRow row)
        {
            row.ApplyState();
            EnsureRowDetailsVisibility(row, raiseNotification: true, animate: true);
        }
        else
        {
            // Assume it's a RowGroupHeader
            DataGridRowGroupHeader? groupHeader = element as DataGridRowGroupHeader;
            groupHeader?.UpdatePseudoClasses();
        }
    }

    private void UnloadElements(bool recycle)
    {
        // Since we're unloading all the elements, we can't be in editing mode anymore,
        // so commit if we can, otherwise force cancel.
        if (!CommitEdit())
        {
            CancelEdit(DataGridEditingUnit.Row, false);
        }

        ResetEditingRow();

        // Make sure to clear the focused row (because it's no longer relevant).
        if (_focusedRow != null)
        {
            ResetFocusedRow();
            Focus();
        }

        if (_rowsPresenter != null)
        {
            foreach (Control element in _rowsPresenter.Children)
            {
                if (element is DataGridRow row)
                {
                    // Raise UnloadingRow for any row that was visible
                    if (IsSlotVisible(row.Slot))
                    {
                        NotifyUnloadingRow(new DataGridRowEventArgs(row));
                    }

                    row.DetachFromDataGrid(recycle && row.IsRecyclable /*recycle*/);
                }
                else if (element is DataGridRowGroupHeader groupHeader)
                {
                    if (groupHeader.DisplaySlot >= 0 && IsSlotVisible(groupHeader.DisplaySlot))
                    {
                        OnUnloadingRowGroup(new DataGridRowGroupHeaderEventArgs(groupHeader));
                    }
                    if (!recycle)
                    {
                        groupHeader.DetachFromDataGrid();
                    }
                }
            }

            if (!recycle)
            {
                _rowsPresenter.Children.Clear();
            }
        }

        DisplayData.ClearElements(recycle);

        // Update the AvailableRowRoom since we're displaying 0 rows now
        AvailableSlotElementRoom = CellsEstimatedHeight;
        VisibleSlotCount         = 0;
    }

    private void UnloadRow(DataGridRow dataGridRow)
    {
        Debug.Assert(dataGridRow != null);
        Debug.Assert(_rowsPresenter != null);
        Debug.Assert(_rowsPresenter.Children.Contains(dataGridRow));

        if (_loadedRows.Contains(dataGridRow))
        {
            return; // The row is still referenced, we can't release it.
        }

        // Raise UnloadingRow regardless of whether the row will be recycled
        NotifyUnloadingRow(new DataGridRowEventArgs(dataGridRow));
        bool recycleRow = CurrentSlot != dataGridRow.Index;

        if (recycleRow)
        {
            DisplayData.AddRecyclableRow(dataGridRow);
        }
        else
        {
            _rowsPresenter.Children.Remove(dataGridRow);
            dataGridRow.DetachFromDataGrid(false);
        }
    }

    private void UpdateDisplayedRows(int newFirstDisplayedSlot, double displayHeight)
    {
        Debug.Assert(!_collapsedSlotsTable.Contains(newFirstDisplayedSlot));
        var committedStart = IsRangePresentationActive
            ? _rangePresentationIndex!.Snapshot.CommittedViewport.FirstVisibleIndex
            : 0;
        var committedEndExclusive = IsRangePresentationActive
            ? checked(committedStart + _rangePresentationIndex!.Snapshot.CommittedViewport.VisibleCount)
            : SlotCount;
        int    firstDisplayedScrollingSlot = newFirstDisplayedSlot;
        int    lastDisplayedScrollingSlot  = -1;
        double deltaY                      = -NegVerticalOffset;
        int    visibleScrollingRows        = 0;

        if (MathUtils.LessThanOrClose(displayHeight, 0) || SlotCount == 0 || ColumnsItemsInternal.Count == 0)
        {
            return;
        }

        if (firstDisplayedScrollingSlot == -1)
        {
            // 0 is fine because the element in the first slot cannot be collapsed
            firstDisplayedScrollingSlot = committedStart;
        }
        firstDisplayedScrollingSlot = Math.Clamp(
            firstDisplayedScrollingSlot,
            committedStart,
            Math.Max(committedStart, committedEndExclusive - 1));

        int slot = firstDisplayedScrollingSlot;
        while (slot < committedEndExclusive && !MathUtils.GreaterThanOrClose(deltaY, displayHeight))
        {
            deltaY += GetExactSlotElementHeight(slot);
            visibleScrollingRows++;
            lastDisplayedScrollingSlot = slot;
            slot                       = GetNextVisibleSlot(slot);
        }

        if (IsRangePresentationActive &&
            slot >= committedEndExclusive &&
            slot < SlotCount &&
            MathUtils.LessThan(deltaY, displayHeight))
        {
            QueueRangeViewportBeyondCommittedRange(firstDisplayedScrollingSlot);
        }

        while (MathUtils.LessThan(deltaY, displayHeight) &&
               firstDisplayedScrollingSlot > committedStart)
        {
            slot = GetPreviousVisibleSlot(firstDisplayedScrollingSlot);
            if (slot >= committedStart)
            {
                deltaY                      += GetExactSlotElementHeight(slot);
                firstDisplayedScrollingSlot =  slot;
                visibleScrollingRows++;
            }
        }

        // If we're up to the first row, and we still have room left, uncover as much of the first row as we can
        if (firstDisplayedScrollingSlot == 0 && MathUtils.LessThan(deltaY, displayHeight))
        {
            double newNegVerticalOffset = Math.Max(0, NegVerticalOffset - displayHeight + deltaY);
            deltaY            += NegVerticalOffset - newNegVerticalOffset;
            NegVerticalOffset =  newNegVerticalOffset;
        }

        if (MathUtils.GreaterThan(deltaY, displayHeight) || (MathUtils.AreClose(deltaY, displayHeight) &&
                                                                 MathUtils.GreaterThan(NegVerticalOffset, 0)))
        {
            DisplayData.NumTotallyDisplayedScrollingElements = visibleScrollingRows - 1;
        }
        else
        {
            DisplayData.NumTotallyDisplayedScrollingElements = visibleScrollingRows;
        }

        if (visibleScrollingRows == 0)
        {
            firstDisplayedScrollingSlot = -1;
            Debug.Assert(lastDisplayedScrollingSlot == -1);
        }

        Debug.Assert(lastDisplayedScrollingSlot < SlotCount, "lastDisplayedScrollingRow larger than number of rows");

        RemoveNonDisplayedRows(firstDisplayedScrollingSlot, lastDisplayedScrollingSlot);

        Debug.Assert(DisplayData.NumDisplayedScrollingElements >= 0,
            "the number of visible scrolling rows can't be negative");
        Debug.Assert(DisplayData.NumTotallyDisplayedScrollingElements >= 0,
            "the number of totally visible scrolling rows can't be negative");
        Debug.Assert(DisplayData.FirstScrollingSlot < SlotCount,
            "firstDisplayedScrollingRow larger than number of rows");
        Debug.Assert(DisplayData.FirstScrollingSlot == firstDisplayedScrollingSlot);
        Debug.Assert(DisplayData.LastScrollingSlot == lastDisplayedScrollingSlot);
        RefreshDisplayedRowsGridLines();
    }

    // Similar to UpdateDisplayedRows except that it starts with the LastDisplayedScrollingRow
    // and computes the FirstDisplayScrollingRow instead of doing it the other way around.  We use this
    // when scrolling down to a full row
    private void UpdateDisplayedRowsFromBottom(int newLastDisplayedScrollingRow)
    {
        //Debug.Assert(!_collapsedSlotsTable.Contains(newLastDisplayedScrollingRow));

        var committedStart = IsRangePresentationActive
            ? _rangePresentationIndex!.Snapshot.CommittedViewport.FirstVisibleIndex
            : 0;
        var committedEndExclusive = IsRangePresentationActive
            ? checked(committedStart + _rangePresentationIndex!.Snapshot.CommittedViewport.VisibleCount)
            : SlotCount;

        int    lastDisplayedScrollingRow  = newLastDisplayedScrollingRow;
        int    firstDisplayedScrollingRow = -1;
        double displayHeight              = CellsEstimatedHeight;
        double deltaY                     = 0;
        int    visibleScrollingRows       = 0;

        if (MathUtils.LessThanOrClose(displayHeight, 0) || SlotCount == 0 || ColumnsItemsInternal.Count == 0)
        {
            ResetDisplayedRows();
            return;
        }

        if (lastDisplayedScrollingRow == -1)
        {
            lastDisplayedScrollingRow = committedStart;
        }
        lastDisplayedScrollingRow = Math.Clamp(
            lastDisplayedScrollingRow,
            committedStart,
            Math.Max(committedStart, committedEndExclusive - 1));

        int slot = lastDisplayedScrollingRow;
        while (MathUtils.LessThan(deltaY, displayHeight) && slot >= committedStart)
        {
            deltaY += GetExactSlotElementHeight(slot);
            visibleScrollingRows++;
            firstDisplayedScrollingRow = slot;
            slot                       = GetPreviousVisibleSlot(slot);
        }

        DisplayData.NumTotallyDisplayedScrollingElements =
            deltaY > displayHeight ? visibleScrollingRows - 1 : visibleScrollingRows;

        Debug.Assert(DisplayData.NumTotallyDisplayedScrollingElements >= 0);
        Debug.Assert(lastDisplayedScrollingRow < SlotCount, "lastDisplayedScrollingRow larger than number of rows");

        NegVerticalOffset = Math.Max(0, deltaY - displayHeight);
        firstDisplayedScrollingRow = NormalizeFirstScrollingSlotOffset(firstDisplayedScrollingRow);

        RemoveNonDisplayedRows(firstDisplayedScrollingRow, lastDisplayedScrollingRow);

        Debug.Assert(DisplayData.NumDisplayedScrollingElements >= 0,
            "the number of visible scrolling rows can't be negative");
        Debug.Assert(DisplayData.NumTotallyDisplayedScrollingElements >= 0,
            "the number of totally visible scrolling rows can't be negative");
        Debug.Assert(DisplayData.FirstScrollingSlot < SlotCount,
            "firstDisplayedScrollingRow larger than number of rows");
        RefreshDisplayedRowsGridLines();
    }

    private void ClearRowGroupHeadersTable()
    {
        _collapsedSlotsTable.Clear();
        _rowDetailsHeightEstimateTable.Clear();
        _rowGroupHeightsByLevel = [];
        RowGroupSublevelIndents = [];
    }

    private void PopulateRowGroupHeadersTable()
    {
        SlotCount = RangeWindowDataCount;
        VisibleSlotCount = SlotCount;
    }

    private void RefreshRowGroupHeaders()
    {
        var levelCount = Query.Groups.Length;
        if (levelCount > 0)
        {
            EnsureRangeGroupMetrics(levelCount);
            EnsureRowGroupSpacerColumnWidth(levelCount);
        }
    }

    private void EnsureRowGroupSpacerColumn()
    {
        bool spacerColumnChanged = ColumnsInternal.EnsureRowGrouping(Query.Groups.Length > 0);
        if (spacerColumnChanged)
        {
            Debug.Assert(ColumnsInternal.RowGroupSpacerColumn != null);
            if (ColumnsInternal.RowGroupSpacerColumn.IsRepresented && CurrentColumnIndex == 0)
            {
                CurrentColumn = ColumnsInternal.FirstVisibleNonFillerColumn;
            }

            ProcessFrozenColumnCount();
        }
    }

    private void EnsureRowGroupSpacerColumnWidth(int groupLevelCount)
    {
        Debug.Assert(ColumnsInternal.RowGroupSpacerColumn != null);
        if (groupLevelCount == 0)
        {
            ColumnsInternal.RowGroupSpacerColumn.Width = new DataGridLength(0);
        }
        else
        {
            ColumnsInternal.RowGroupSpacerColumn.Width =
                new DataGridLength(RowGroupSublevelIndents[groupLevelCount - 1]);
        }
    }

    internal void OnSublevelIndentUpdated(DataGridRowGroupHeader groupHeader, double newValue)
    {
        var groupLevelCount = RowGroupSublevelIndents.Length;
        if (groupHeader.Level < 0 || groupHeader.Level >= groupLevelCount)
        {
            return;
        }

        var oldValue = RowGroupSublevelIndents[groupHeader.Level];
        if (groupHeader.Level > 0)
        {
            oldValue -= RowGroupSublevelIndents[groupHeader.Level - 1];
        }

        var change = newValue - oldValue;
        for (var level = groupHeader.Level; level < groupLevelCount; level++)
        {
            RowGroupSublevelIndents[level] += change;
        }
        EnsureRowGroupSpacerColumnWidth(groupLevelCount);
    }

    private int GetDetailsCountInclusive(int lowerBound, int upperBound)
    {
        if (upperBound < lowerBound)
        {
            return 0;
        }
        var count = 0;
        for (var slot = lowerBound; slot <= upperBound; slot++)
        {
            if (TryGetCommittedRangeEntry(slot, out var entry) &&
                entry.Kind == DataGridSourceEntryKind.Data &&
                GetRowDetailsVisibility(entry.WindowDataIndex))
            {
                count++;
            }
        }
        return count;
    }

    private double GetDisplayedRowDetailsHeight(DataGridRow row, double rowHeight)
    {
        if (row.Slot < 0 || !GetRowDetailsVisibility(row.Index))
        {
            return 0;
        }

        double detailsHeight = rowHeight - RowHeightEstimate;
        if (IsInvalidSlotElementHeight(detailsHeight) || MathUtils.LessThanOrClose(detailsHeight, 0))
        {
            return GetRowDetailsHeightEstimate(row.Slot);
        }

        return detailsHeight;
    }

    private void UpdateRowHeightEstimateFromDisplayedRows()
    {
        if (DisplayData.LastScrollingSlot < _lastEstimatedRow)
        {
            return;
        }

        double totalRowHeight = 0;
        int rowCount = 0;
        int displayedElementCount = DisplayData.NumDisplayedScrollingElements;
        for (int displayIndex = 0; displayIndex < displayedElementCount; displayIndex++)
        {
            if (DisplayData.GetScrollingElementAtDisplayIndex(displayIndex) is not DataGridRow row)
            {
                continue;
            }

            double rowHeight = IsInvalidSlotElementHeight(row.TargetHeight)
                ? GetDisplayedElementHeight(row)
                : row.TargetHeight;
            if (IsInvalidSlotElementHeight(rowHeight))
            {
                continue;
            }

            double detailsHeight = GetDisplayedRowDetailsHeight(row, rowHeight);
            double baseRowHeight = Math.Max(0, rowHeight - detailsHeight);
            if (MathUtils.GreaterThan(baseRowHeight, 0))
            {
                totalRowHeight += baseRowHeight;
                rowCount++;
            }
        }

        if (rowCount > 0)
        {
            _lastEstimatedRow = DisplayData.LastScrollingSlot;
            RowHeightEstimate = totalRowHeight / rowCount;
        }
    }

    private double GetRowDetailsHeightEstimate(int slot)
    {
        if (slot < 0 || !GetRowDetailsVisibilityFromSlot(slot))
        {
            return 0;
        }

        double? measuredDetailsHeight = _rowDetailsHeightEstimateTable.GetValueAt(slot, out bool found);
        if (found &&
            measuredDetailsHeight.HasValue &&
            !IsInvalidSlotElementHeight(measuredDetailsHeight.Value) &&
            MathUtils.GreaterThanOrClose(measuredDetailsHeight.Value, RowDetailsHeightEstimate))
        {
            return measuredDetailsHeight.Value;
        }

        return RowDetailsHeightEstimate;
    }

    private double GetRowDetailsHeightEstimateInclusive(int lowerBound, int upperBound)
    {
        if (upperBound < lowerBound)
        {
            return 0;
        }

        int detailsCount = GetDetailsCountInclusive(lowerBound, upperBound);
        if (detailsCount == 0)
        {
            return 0;
        }

        double totalHeight = detailsCount * RowDetailsHeightEstimate;
        foreach (int slot in _rowDetailsHeightEstimateTable.EnumerateIndexes(lowerBound))
        {
            if (slot > upperBound)
            {
                break;
            }

            if (!GetRowDetailsVisibilityFromSlot(slot))
            {
                continue;
            }

            double? measuredDetailsHeight = _rowDetailsHeightEstimateTable.GetValueAt(slot, out bool found);
            if (found &&
                measuredDetailsHeight.HasValue &&
                !IsInvalidSlotElementHeight(measuredDetailsHeight.Value) &&
                MathUtils.GreaterThanOrClose(measuredDetailsHeight.Value, RowDetailsHeightEstimate))
            {
                totalHeight += measuredDetailsHeight.Value - RowDetailsHeightEstimate;
            }
        }

        return Math.Max(0, totalHeight);
    }

    private bool GetRowDetailsVisibilityFromSlot(int slot)
    {
        return TryGetCommittedRangeEntry(slot, out var entry) &&
               entry.Kind == DataGridSourceEntryKind.Data &&
               GetRowDetailsVisibility(entry.WindowDataIndex);
    }

    private void EnsureRowDetailsVisibility(DataGridRow row, bool raiseNotification, bool animate)
    {
        // Show or hide RowDetails based on DataGrid settings
        row.SetDetailsVisibilityInternal(GetRowDetailsVisibility(row.Index), raiseNotification, animate);
    }

    private void UpdateRowDetailsHeightEstimate()
    {
        if (_rowsPresenter != null && _measured && RowDetailsTemplate != null)
        {
            object? dataItem = null;
            if (VisibleSlotCount > 0)
            {
                dataItem = RangeDataAccess.GetDataItem(0);
            }

            var detailsContent = RowDetailsTemplate.Build(dataItem);
            if (detailsContent != null)
            {
                detailsContent.DataContext = dataItem;
                _rowsPresenter.Children.Add(detailsContent);
                detailsContent.ApplyTemplate();
                detailsContent.Measure(new Size(GetRowDetailsMeasureWidth(), double.PositiveInfinity));
                UpdateRowDetailsHeightEstimateFromMeasuredDetails(detailsContent.DesiredSize.Height);
                _rowsPresenter.Children.Remove(detailsContent);
            }
        }
    }

    internal void UpdateRowDetailsHeightEstimateFromMeasuredDetails(double measuredDetailsHeight)
    {
        UpdateRowDetailsHeightEstimateFromMeasuredDetails(slot: null, measuredDetailsHeight, invalidateMeasure: true);
    }

    internal void UpdateRowDetailsHeightEstimateFromMeasuredDetails(int slot, double measuredDetailsHeight)
    {
        UpdateRowDetailsHeightEstimateFromMeasuredDetails(slot, measuredDetailsHeight, invalidateMeasure: true);
    }

    private void UpdateRowDetailsHeightEstimateFromMeasuredDetails(int? slot, double measuredDetailsHeight, bool invalidateMeasure)
    {
        if (IsInvalidSlotElementHeight(measuredDetailsHeight))
        {
            return;
        }

        if (IsRangePresentationActive)
        {
            var storedRangeMeasurement = false;
            if (slot.HasValue &&
                TryGetCommittedRangeEntry(slot.Value, out var entry) &&
                entry.Kind == DataGridSourceEntryKind.Data &&
                GetRowDetailsVisibility(entry.RowKey))
            {
                if (!_rangeMeasuredDetailsHeights.TryGetValue(entry.RowKey, out var previousHeight) ||
                    MathUtils.GreaterThan(measuredDetailsHeight, previousHeight))
                {
                    _rangeMeasuredDetailsHeights[entry.RowKey] = measuredDetailsHeight;
                    storedRangeMeasurement = true;
                }
            }

            var estimateChanged = false;
            if (IsInvalidSlotElementHeight(RowDetailsHeightEstimate) ||
                MathUtils.GreaterThan(measuredDetailsHeight, RowDetailsHeightEstimate))
            {
                RowDetailsHeightEstimate = measuredDetailsHeight;
                estimateChanged = true;
                // The range default includes details only when every row shows them.
                // In Collapsed/VisibleWhenSelected modes expanded rows are represented
                // by their own sparse measured height, so rebasing here would move the
                // viewport even though its default height did not change.
                if (RowDetailsVisibilityMode == DataGridRowDetailsVisibilityMode.Visible)
                {
                    RebaseRangeHeightIndex(discardMeasuredDataHeights: false);
                }
            }
            if ((storedRangeMeasurement || estimateChanged) &&
                RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.Visible &&
                _rangePresentationIndex is { } presentation &&
                _rangeHeightIndex is { } heightIndex)
            {
                SeedRangeDetailsHeights(
                    presentation.Snapshot,
                    heightIndex,
                    preserveViewportAnchor: true);
            }
            if (invalidateMeasure && _measured && !_scrollingByHeight)
            {
                InvalidateMeasure();
            }
            return;
        }

        if (slot.HasValue)
        {
            if (IsInvalidSlotElementHeight(RowDetailsHeightEstimate) ||
                MathUtils.GreaterThanOrClose(measuredDetailsHeight, RowDetailsHeightEstimate))
            {
                _rowDetailsHeightEstimateTable.AddValue(slot.Value, measuredDetailsHeight);
            }
            else if (_rowDetailsHeightEstimateTable.Contains(slot.Value))
            {
                _rowDetailsHeightEstimateTable.RemoveIndexAndValue(slot.Value);
            }
        }

        if (IsInvalidSlotElementHeight(RowDetailsHeightEstimate) ||
            MathUtils.GreaterThan(measuredDetailsHeight, RowDetailsHeightEstimate))
        {
            RowDetailsHeightEstimate = measuredDetailsHeight;
        }
        if (invalidateMeasure && _measured && !_scrollingByHeight)
        {
            InvalidateMeasure();
        }
    }

    internal double GetRowDetailsMeasureWidth()
    {
        double width = CellsWidth;
        if (ColumnsInternal.RowGroupSpacerColumn != null)
        {
            width -= ColumnsInternal.RowGroupSpacerColumn.Width.Value;
        }

        return MathUtils.GreaterThan(width, 0) && !double.IsPositiveInfinity(width)
            ? width
            : double.PositiveInfinity;
    }

    // detailsElement is the FrameworkElement created by the DetailsTemplate
    internal void NotifyUnloadingRowDetails(DataGridRow row, Control detailsElement)
    {
        NotifyUnloadingRowDetails(new DataGridRowDetailsEventArgs(row, detailsElement));
    }

    // detailsElement is the FrameworkElement created by the DetailsTemplate
    internal void NotifyLoadingRowDetails(DataGridRow row, Control detailsElement)
    {
        NotifyLoadingRowDetails(new DataGridRowDetailsEventArgs(row, detailsElement));
    }

    internal void NotifyRowDetailsVisibilityPropertyChanged(int rowIndex, bool isVisible)
    {
        var slot = GetCommittedRangeSlot(rowIndex);
        if (slot >= 0 && TryGetCommittedRangeEntry(slot, out var entry))
        {
            NotifyRowDetailsVisibilityPropertyChanged(entry.RowKey, isVisible);
            if (isVisible && _rangePresentationIndex is { } presentation)
            {
                _rangeDetailsSlotHints[entry.RowKey] =
                    (presentation.Snapshot.DataGeneration, slot);
            }
        }
    }

    internal void NotifyRowDetailsVisibilityPropertyChanged(
        DataGridRowKey rowKey,
        int ungroupedWindowSlotHint,
        bool isVisible)
    {
        if (ungroupedWindowSlotHint < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ungroupedWindowSlotHint));
        }
        NotifyRowDetailsVisibilityPropertyChanged(rowKey, isVisible);
        if (isVisible)
        {
            // A zero generation is an initialization hint. It is only consumed by
            // an ungrouped, unpaged snapshot; committed entries replace it with a
            // generation-bound display slot before later viewport changes.
            _rangeDetailsSlotHints[rowKey] = (0, ungroupedWindowSlotHint);
        }
    }

    internal void NotifyRowDetailsVisibilityPropertyChanged(DataGridRowKey rowKey, bool isVisible)
    {
        if (!rowKey.IsValid)
        {
            throw new ArgumentException("The row key must be valid.", nameof(rowKey));
        }

        _rangeDetailsVisibility[rowKey] = isVisible;
        if (!isVisible)
        {
            _rangeMeasuredDetailsHeights.Remove(rowKey);
            _rangeDetailsSlotHints.Remove(rowKey);
        }
    }

    internal double GetRangeRowDetailsHeightEstimate(DataGridRowKey rowKey)
    {
        if (!GetRowDetailsVisibility(rowKey))
        {
            return 0;
        }
        return _rangeMeasuredDetailsHeights.TryGetValue(rowKey, out var measuredHeight) &&
               MathUtils.GreaterThanOrClose(measuredHeight, RowDetailsHeightEstimate)
            ? measuredHeight
            : RowDetailsHeightEstimate;
    }

    internal bool GetRowDetailsVisibility(DataGridRowKey rowKey)
    {
        if (!rowKey.IsValid)
        {
            throw new ArgumentException("The row key must be valid.", nameof(rowKey));
        }
        if (_rangeDetailsVisibility.TryGetValue(rowKey, out var explicitVisibility))
        {
            return explicitVisibility;
        }
        if (RowDetailsVisibilityMode == DataGridRowDetailsVisibilityMode.Visible)
        {
            return true;
        }
        if (RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.VisibleWhenSelected ||
            _rangePresentationIndex?.FindSlot(rowKey) is not { } slot ||
            !TryGetCommittedRangeEntry(slot, out var entry) ||
            GetCommittedSelectionScope() is not { } scope)
        {
            return false;
        }
        return Selection.Contains(rowKey, entry.DataIndex, scope);
    }

    internal bool GetRowDetailsVisibility(int rowIndex)
    {
        return GetRowDetailsVisibility(rowIndex, RowDetailsVisibilityMode);
    }

    internal bool GetRowDetailsVisibility(int rowIndex, DataGridRowDetailsVisibilityMode gridLevelRowDetailsVisibility)
    {
        Debug.Assert(rowIndex != -1);
        var slot = GetCommittedRangeSlot(rowIndex);
        if (slot < 0 || !TryGetCommittedRangeEntry(slot, out var entry))
        {
            return false;
        }
        if (_rangeDetailsVisibility.TryGetValue(entry.RowKey, out var explicitVisibility))
        {
            return explicitVisibility;
        }
        return gridLevelRowDetailsVisibility == DataGridRowDetailsVisibilityMode.Visible ||
               (gridLevelRowDetailsVisibility == DataGridRowDetailsVisibilityMode.VisibleWhenSelected &&
                GetCommittedSelectionScope() is { } scope &&
                Selection.Contains(entry.RowKey, entry.DataIndex, scope));
    }

    /// <summary>
    /// Raises the <see cref="E:Avalonia.Controls.DataGrid.RowDetailsVisibilityChanged" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected internal virtual void OnRowDetailsVisibilityChanged(DataGridRowDetailsEventArgs e)
    {
        RowDetailsVisibilityChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 是否全部选中
    /// </summary>
    /// <returns></returns>
    internal bool IsAllRowSelected()
    {
        var scope = GetCommittedSelectionScope();
        if (TotalItemCount <= 0 || scope is null)
        {
            return false;
        }
        if (Selection.AllMatchingQuery?.Equals(scope) == true &&
            Selection.ExcludedKeys.IsEmpty)
        {
            return true;
        }
        if (_rangePresentationIndex is null || TotalItemCount > int.MaxValue)
        {
            return false;
        }

        var loadedKeys = new HashSet<DataGridRowKey>();
        foreach (var block in _rangePresentationIndex.Snapshot.Blocks)
        {
            foreach (var entry in block.Entries)
            {
                if (entry.Kind == DataGridSourceEntryKind.Data &&
                    loadedKeys.Add(entry.RowKey) &&
                    !Selection.Contains(entry.RowKey, entry.DataIndex, scope))
                {
                    return false;
                }
            }
        }
        return loadedKeys.Count == TotalItemCount;
    }

    
    private void HandleIsRowGroupHeadersFrozenChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var value = (bool)(change.NewValue ?? false);
        ProcessFrozenColumnCount();

        // Update elements in the RowGroupHeader that were previously frozen
        if (value)
        {
            if (_rowsPresenter != null)
            {
                foreach (Control element in _rowsPresenter.Children)
                {
                    if (element is DataGridRowGroupHeader groupHeader)
                    {
                        groupHeader.ClearFrozenStates();
                    }
                }
            }
        }
    }
    
    private void HandleRowDetailsTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // Update the RowDetails templates if necessary
        if (_rowsPresenter != null)
        {
            foreach (DataGridRow row in GetAllRows())
            {
                if (GetRowDetailsVisibility(row.Index))
                {
                    // DetailsPreferredHeight is initialized when the DetailsElement's size changes.
                    row.ApplyDetailsTemplate(initializeDetailsPreferredHeight: false);
                }
            }
        }

        UpdateRowDetailsHeightEstimate();
        InvalidateMeasure();
    }

    private void HandleRowHeaderContentTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_rowsPresenter != null)
        {
            foreach (DataGridRow row in GetAllRows())
            {
                row.ApplyHeaderContentTemplate();
            }
        }
        InvalidateMeasure();
    }
    
    private void HandleRowDetailsVisibilityModeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        UpdateRowDetailsVisibilityMode((DataGridRowDetailsVisibilityMode)(change.NewValue ?? DataGridRowDetailsVisibilityMode.Collapsed));
    }

    private void UpdateRowDetailsVisibilityMode(DataGridRowDetailsVisibilityMode newDetailsMode)
    {
        _rangeDetailsVisibility.Clear();
        _rangeMeasuredDetailsHeights.Clear();
        _rangeDetailsSlotHints.Clear();
        RebaseRangeHeightIndex(discardMeasuredDataHeights: true);
        var updated = false;
        foreach (var row in DisplayData.GetScrollingRows().Cast<DataGridRow>())
        {
            var newVisibility = GetRowDetailsVisibility(row.Index, newDetailsMode);
            if (row.IsDetailsVisible != newVisibility)
            {
                updated = true;
                row.SetDetailsVisibilityInternal(
                    newVisibility,
                    raiseNotification: true,
                    animate: false);
            }
        }
        if (updated)
        {
            UpdateDisplayedRows(DisplayData.FirstScrollingSlot, CellsEstimatedHeight);
            InvalidateRowsMeasure(invalidateIndividualElements: false);
        }
    }

    private void ReConfigurePagination()
    {
        var currentPageRequest = PageRequest;
        var currentPageSize = currentPageRequest?.DataCount ?? 0;
        if (PageSize == currentPageSize)
        {
            ConfigurePaginationVisibility();
            SyncRangePaginationState();
            return;
        }

        DataGridPageRequest? nextPageRequest = null;
        if (PageSize > 0)
        {
            var pageIndex = currentPageRequest is { } request
                ? request.DataStartIndex / request.DataCount
                : 0;
            var dataStartIndex = checked(pageIndex * PageSize);
            nextPageRequest = new DataGridPageRequest(dataStartIndex, PageSize);
        }

        SetPageRequest(nextPageRequest);
        if (PageRequest != nextPageRequest)
        {
            SetCurrentValue(PageSizeProperty, currentPageSize);
        }
    }

    private void SyncRangePaginationState()
    {
        var pageRequest = AppliedPageRequest;
        var total = TotalItemCount > int.MaxValue
            ? int.MaxValue
            : (int)TotalItemCount;
        var pageSize = pageRequest?.DataCount ?? 0;
        var currentPage = GetRangePaginationPage(pageRequest);

        _synchronizingRangePagination = true;
        try
        {
            if (_topPagination != null)
            {
                _topPagination.Total = total;
                _topPagination.PageSize = pageSize;
                _topPagination.CurrentPage = currentPage;
            }
            if (_bottomPagination != null)
            {
                _bottomPagination.Total = total;
                _bottomPagination.PageSize = pageSize;
                _bottomPagination.CurrentPage = currentPage;
            }
        }
        finally
        {
            _synchronizingRangePagination = false;
        }
        ConfigurePaginationVisibility();
    }

    private void HandlePageChangeRequest(object? sender, PageChangedEventArgs args)
    {
        if (_synchronizingRangePagination)
        {
            return;
        }
        if (ItemsSource is not null && (AppliedPageRequest ?? PageRequest) is { } pageRequest)
        {
            if (args.PageSize != pageRequest.DataCount)
            {
                var targetPageIndex = Math.Max(0, args.PageIndex - 1);
                var targetDataStartIndex = checked((long)targetPageIndex * args.PageSize);
                SetPageRequest(new DataGridPageRequest(targetDataStartIndex, args.PageSize));
                return;
            }

            // Pagination also raises CurrentPageChanged while applying a delayed template update.
            // Treat the currently projected applied page as state synchronization, not user intent.
            if (args.PageIndex == GetRangePaginationPage(pageRequest))
            {
                return;
            }
            var pageIndex = Math.Max(0, args.PageIndex - 1);
            var dataStartIndex = checked((long)pageIndex * pageRequest.DataCount);
            PageRequest = new DataGridPageRequest(dataStartIndex, pageRequest.DataCount);
            SyncRangePaginationState();
            return;
        }
    }

    private static int GetRangePaginationPage(DataGridPageRequest? pageRequest) =>
        pageRequest is null
            ? Pagination.DefaultCurrentPage
            : (int)Math.Min(
                int.MaxValue - 4L,
                pageRequest.Value.DataStartIndex / pageRequest.Value.DataCount + 1);

    private void HandlePageChanging(object? sender, PageChangingEventArgs args)
    {
        var targetPage = args.NewPageIndex + 1;
        if (_topPagination != null && _topPagination.CurrentPage != targetPage)
        {
            _topPagination.CurrentPage = targetPage;
        }
        
        if (_bottomPagination != null && _bottomPagination.CurrentPage != targetPage)
        {
            _bottomPagination.CurrentPage = targetPage;
        }
        PageChanging?.Invoke(this, args);
    }
    
    private void HandlePageChanged(object? sender, PageChangedEventArgs args)
    {
        PageChanged?.Invoke(this, args);
    }
}
