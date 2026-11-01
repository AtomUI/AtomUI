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
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    internal const bool DefaultCanUserReorderColumns = true;
    internal const bool DefaultCanUserResizeColumns = true;
    internal const bool DefaultCanUserFilterColumns = false;
    internal const bool DefaultIsSorterTooltipVisible = false;

    #region 内部属性定义

    internal static readonly DirectProperty<DataGrid, bool> IsGroupHeaderModeProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, bool>(
            nameof(IsGroupHeaderMode),
            o => o.IsGroupHeaderMode,
            (o, v) => o.IsGroupHeaderMode = v);

    internal static readonly DirectProperty<DataGrid, Thickness> FrameBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, Thickness>(
            nameof(FrameBorderThickness),
            o => o.FrameBorderThickness,
            (o, v) => o.FrameBorderThickness = v);

    internal static readonly DirectProperty<DataGrid, CornerRadius> FrameCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, CornerRadius>(
            nameof(FrameCornerRadius),
            o => o.FrameCornerRadius,
            (o, v) => o.FrameCornerRadius = v);

    internal static readonly DirectProperty<DataGrid, CornerRadius> HeaderCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, CornerRadius>(
            nameof(HeaderCornerRadius),
            o => o.HeaderCornerRadius,
            (o, v) => o.HeaderCornerRadius = v);

    internal static readonly DirectProperty<DataGrid, bool> IsEmptyDataSourceProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, bool>(
            nameof(IsEmptyDataSource),
            o => o.IsEmptyDataSource,
            (o, v) => o.IsEmptyDataSource = v);
    
    internal static readonly DirectProperty<DataGrid, DataGridPaginationVisibility> EffectivePaginationVisibilityProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridPaginationVisibility>(
            nameof(EffectivePaginationVisibility),
            o => o.EffectivePaginationVisibility,
            (o, v) => o.EffectivePaginationVisibility = v);

    internal static readonly DirectProperty<DataGrid, bool> EffectiveIsOperatingProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, bool>(
            nameof(EffectiveIsOperating),
            o => o.EffectiveIsOperating);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<DataGrid>();
    

    internal bool IsGroupHeaderMode
    {
        get => _isGroupHeaderMode;
        set => SetAndRaise(IsGroupHeaderModeProperty, ref _isGroupHeaderMode, value);
    }

    private bool _isGroupHeaderMode;

    internal Thickness FrameBorderThickness
    {
        get => _frameBorderThickness;
        set => SetAndRaise(FrameBorderThicknessProperty, ref _frameBorderThickness, value);
    }

    private Thickness _frameBorderThickness;

    internal CornerRadius FrameCornerRadius
    {
        get => _frameCornerRadius;
        set => SetAndRaise(FrameCornerRadiusProperty, ref _frameCornerRadius, value);
    }

    private CornerRadius _frameCornerRadius;

    internal CornerRadius HeaderCornerRadius
    {
        get => _headerCornerRadius;
        set => SetAndRaise(HeaderCornerRadiusProperty, ref _headerCornerRadius, value);
    }

    private CornerRadius _headerCornerRadius;
    
    internal DataGridPaginationVisibility EffectivePaginationVisibility
    {
        get => _effectivePaginationVisibility;
        set => SetAndRaise(EffectivePaginationVisibilityProperty, ref _effectivePaginationVisibility, value);
    }

    private DataGridPaginationVisibility _effectivePaginationVisibility;

    internal bool EffectiveIsOperating => _effectiveIsOperating;

    private bool _effectiveIsOperating;

    internal bool IsEmptyDataSource
    {
        get => _isEmptyDataSource;
        set => SetAndRaise(IsEmptyDataSourceProperty, ref _isEmptyDataSource, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    private bool _isEmptyDataSource = false;

    internal DataGridDisplayData DisplayData { get; private set; }

    internal double RowHeightEstimate { get; private set; }

    internal double RowDetailsHeightEstimate { get; private set; }

    internal DataGridRangeDataAccess RangeDataAccess { get; private set; }

    internal int AnchorSlot { get; private set; }

    private int _noCurrentCellChangeCount;

    internal int NoCurrentCellChangeCount
    {
        get => _noCurrentCellChangeCount;

        set
        {
            _noCurrentCellChangeCount = value;
            if (value == 0)
            {
                FlushCurrentCellChanged();
            }
        }
    }

    private DataGridCellCoordinates CurrentCellCoordinates { get; set; }

    internal double RowGroupHeaderHeightEstimate { get; private set; }

    internal bool ContainsFocus { get; private set; }

    internal double NegVerticalOffset { get; private set; }

    internal double HorizontalMaximizeOffset => _hScrollBar?.Maximum ?? 0.0d;

    // 冻结列阴影的唯一判定：表头、分组表头与单元格共用，按 3 位小数比较以忽略浮点残差，
    // 避免各自用不同数据源与精度判断而在滚动边缘出现表头与单元格不一致。
    internal bool IsLeftFrozenShadowVisible =>
        DataGridHelper.AreLessAt3Decimals(0, HorizontalOffset);

    internal bool IsRightFrozenShadowVisible =>
        _hScrollBar is { IsVisible: true } &&
        DataGridHelper.AreLessAt3Decimals(HorizontalOffset, HorizontalMaximizeOffset);

    /// <summary>
    /// Indicates whether or not to use star-sizing logic.  If the DataGrid has infinite available space,
    /// then star sizing doesn't make sense.  In this case, all star columns grow to a predefined size of
    /// 10,000 pixels in order to show the developer that star columns shouldn't be used.
    /// </summary>
    internal bool UsesStarSizing => ColumnsInternal.VisibleStarColumnCount > 0 &&
                                    (!_columnViewportWidth.HasValue ||
                                     !double.IsPositiveInfinity(_columnViewportWidth.Value));

    /// <summary>
    /// Indicates whether or not at least one auto-sizing column is waiting for all the rows
    /// to be measured before its final width is determined.
    /// </summary>
    internal bool AutoSizingColumns
    {
        get => _autoSizingColumns;
        private set => _autoSizingColumns = value;
    }

    internal int? MouseOverRowIndex
    {
        get => _mouseOverRowIndex;
        set
        {
            if (_mouseOverRowIndex != value)
            {
                DataGridRow? oldMouseOverRow = null;
                if (_mouseOverRowIndex.HasValue)
                {
                    int oldSlot = SlotFromRowIndex(_mouseOverRowIndex.Value);
                    if (IsSlotVisible(oldSlot))
                    {
                        oldMouseOverRow = DisplayData.GetDisplayedElement(oldSlot) as DataGridRow;
                    }
                }

                _mouseOverRowIndex = value;

                // State for the old row needs to be applied after setting the new value
                if (oldMouseOverRow != null)
                {
                    oldMouseOverRow.ApplyState();
                }

                if (_mouseOverRowIndex.HasValue)
                {
                    int newSlot = SlotFromRowIndex(_mouseOverRowIndex.Value);
                    if (IsSlotVisible(newSlot))
                    {
                        DataGridRow? newMouseOverRow = DisplayData.GetDisplayedElement(newSlot) as DataGridRow;
                        Debug.Assert(newMouseOverRow != null);
                        newMouseOverRow.ApplyState();
                    }
                }
            }
        }
    }

    internal ScrollBar? VerticalScrollBar => _vScrollBar;
    internal ScrollBar? HorizontalScrollBar => _hScrollBar;

    #endregion

    private const double MinimumRowHeaderWidth = 4;
    private const double MinimumColumnHeaderHeight = 4;
    internal const double MaximumStarColumnWidth = 10000;
    internal const double MinimumStarColumnWidth = 0.001;
    private const double MouseWheelDelta = 50.0;
    private const double MaxHeadersThickness = 32768;
    private const double DefaultRowHeight = 22;
    internal const double DefaultRowGroupSublevelIndent = 20;
    private const double DefaultMinColumnWidth = 20;
    private const double DefaultMaxColumnWidth = double.PositiveInfinity;

    private ContentControl? _clipboardContentControl;

    private Visual? _bottomRightCorner;
    private DataGridRowsPresenter? _rowsPresenter;
    private ScrollBar? _vScrollBar;
    private ScrollBar? _hScrollBar;

    private ContentControl? _topLeftCornerHeader;
    private ContentControl? _topRightCornerHeader;

    // Nth row of rows 0..N that make up the RowHeightEstimate
    private int _lastEstimatedRow;
    private List<DataGridRow> _loadedRows;

    // prevents reentry into the VerticalScroll event handler
    private Queue<Action> _lostFocusActions;
    private readonly Dictionary<DataGridRowKey, bool> _rangeDetailsVisibility = [];
    private readonly Dictionary<DataGridRowKey, double> _rangeMeasuredDetailsHeights = [];
    private readonly Dictionary<DataGridRowKey, (long DataGeneration, int Slot)> _rangeDetailsSlotHints = [];
    private IndexToValueTable<double> _rowDetailsHeightEstimateTable;
    private double _rowHeaderDesiredWidth;
    private int? _mouseOverRowIndex; // -1 is used for the 'new row'
    private bool _makeFirstDisplayedCellCurrentCellPending;
    private bool _measured;
    private bool _scrollingByHeight;
    private bool _temporarilyResetCurrentCell;
    private DataGridColumn? _previousCurrentColumn;
    private object? _previousCurrentItem;
    private double[] _rowGroupHeightsByLevel;
    private object? _uneditedValue; // Represents the original current cell value at the time it enters editing mode.

    // the sum of the widths in pixels of the scrolling columns preceding
    // the first displayed scrolling column
    private double _horizontalOffset;

    // the number of pixels of the firstDisplayedScrollingCol which are not displayed
    private double _negHorizontalOffset;
    private byte _autoGeneratingColumnOperationCount;
    private bool _autoSizingColumns;
    private Control? _clickedElement;

    // used to store the current column during a Reset
    private int _desiredCurrentColumnIndex;
    private int _editingColumnIndex;

    // this is a workaround only for the scenarios where we need it, it is not all encompassing nor always updated
    private RoutedEventArgs? _editingEventArgs;
    private bool _executingLostFocusActions;
    private bool _focusEditingControl;
    private Visual? _focusedObject;
    private byte _horizontalScrollChangesIgnored;
    private DataGridRow? _focusedRow;
    private bool _successfullyUpdatedSelection;

    // An approximation of the sum of the heights in pixels of the scrolling rows preceding
    // the first displayed scrolling row.  Since the scrolled off rows are discarded, the grid
    // does not know their actual height. The heights used for the approximation are the ones
    // set as the rows were scrolled off.
    private double _verticalOffset;

    internal double VerticalOffset => _verticalOffset;

    private byte _verticalScrollChangesIgnored;
    private bool _templatedApplied;
    private Pagination? _topPagination;
    private Pagination? _bottomPagination;
    private bool _synchronizingRangePagination;

    internal void InitializeElements(bool recycleRows)
    {
        try
        {
            _noCurrentCellChangeCount++;

            // The underlying collection has changed and our editing row (if there is one)
            // is no longer relevant, so we should force a cancel edit.
            CancelEdit(DataGridEditingUnit.Row, raiseEvents: false);

            if (recycleRows)
            {
                RefreshRows(recycleRows, clearRows: true);
            }
            else
            {
                RefreshRowsAndColumns(clearRows: true);
            }

            if (RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.Collapsed)
            {
                UpdateRowDetailsVisibilityMode(RowDetailsVisibilityMode);
            }

            // The currently displayed rows may have incorrect visual states because of the selection change
            ApplyDisplayedRowsState(DisplayData.FirstScrollingSlot, DisplayData.LastScrollingSlot);
        }
        finally
        {
            NoCurrentCellChangeCount--;
        }
    }

    // Returns the committed item or group entry for a display slot.

    internal void RefreshRowsAndColumns(bool clearRows)
    {
        if (_measured)
        {
            try
            {
                _noCurrentCellChangeCount++;

                if (clearRows)
                {
                    ClearRows(false);
                    ClearRowGroupHeadersTable();
                    PopulateRowGroupHeadersTable();
                }

                if (AutoGenerateColumns)
                {
                    //Column auto-generation refreshes the rows too
                    AutoGenerateColumnsPrivate();
                }

                foreach (DataGridColumn column in ColumnsItemsInternal)
                {
                    //We don't need to refresh the state of AutoGenerated column headers because they're up-to-date
                    if (!column.IsAutoGenerated && column.HasHeaderCell)
                    {
                        column.HeaderCell.UpdatePseudoClasses();
                    }
                }

                RefreshRows(recycleRows: false, clearRows: false);

                if (Columns.Count > 0 && CurrentColumnIndex == -1)
                {
                    MakeFirstDisplayedCellCurrentCell();
                }
                else
                {
                    _makeFirstDisplayedCellCurrentCellPending = false;
                    _desiredCurrentColumnIndex                = -1;
                    FlushCurrentCellChanged();
                }
            }
            finally
            {
                NoCurrentCellChangeCount--;
            }
        }
        else
        {
            if (clearRows)
            {
                ClearRows(recycle: false);
            }

            ClearRowGroupHeadersTable();
            PopulateRowGroupHeadersTable();
        }
    }

    private void HandleColumnsInternalCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            UpdatePseudoClasses();
            if (_templatedApplied)
            {
                if (IsGroupHeaderMode)
                {
                    SetupColumnGroupFrozenState();
                }

                CheckFrozenColumnCount();
            }
        }

        RefreshPopupPinnedOpenFilterTarget();
    }

    private void SetupColumnGroupFrozenState()
    {
        if (LeftFrozenColumnCount > 0)
        {
            var stop = Math.Min(LeftFrozenColumnCount, Columns.Count);
            for (var i = 0; i < stop; ++i)
            {
                RecursiveSetupGroupHeaderItemFrozenState(Columns[i]);
            }
        }

        if (RightFrozenColumnCount > 0)
        {
            var stop  = Math.Min(RightFrozenColumnCount, Columns.Count);
            var count = Columns.Count;
            for (var i = 0; i < stop; ++i)
            {
                RecursiveSetupGroupHeaderItemFrozenState(Columns[count - i - 1]);
            }
        }
    }

    private void RecursiveSetupGroupHeaderItemFrozenState(IDataGridColumnGroupItem item)
    {
        var current = item;
        while (current != null)
        {
            if (current is DataGridColumnGroupItem gridColumnGroupItem)
            {
                gridColumnGroupItem.HeaderCell.IsFrozen = true;
                gridColumnGroupItem.IsFrozen            = true;
            }

            if (current is IDataGridColumnGroupItemInternal gridColumnGroupItemInternal)
            {
                if (gridColumnGroupItemInternal.GroupHeaderViewItem != null)
                {
                    gridColumnGroupItemInternal.GroupHeaderViewItem.IsFrozen = true;
                }
            }

            current = current.GroupParent;
        }
    }

    internal void UpdatePseudoClasses()
    {
        int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
        int visibleColumnCount = 0;
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            if (ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex).IsVisible)
            {
                visibleColumnCount++;
            }
        }

        int visibleColumnIndex = 0;
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible)
            {
                continue;
            }

            if (visibleColumnIndex == 0)
            {
                column.HeaderCell.IsFirstVisible = true;
            }
            else if (visibleColumnIndex == visibleColumnCount - 1)
            {
                column.HeaderCell.IsLastVisible = true;
            }
            else
            {
                column.HeaderCell.IsFirstVisible  = false;
                column.HeaderCell.IsLastVisible   = false;
                column.HeaderCell.IsMiddleVisible = true;
            }

            column.HeaderCell.CanUserSort       = column.EffectiveCanUserSort;
            column.HeaderCell.SupportedSortDirections = column.EffectiveSupportedSortDirections;
            column.HeaderCell.CanUserFilter     = column.CanUserFilter;
            column.HeaderCell.IsSorterTooltipVisible = column.IsSorterTooltipVisible;
            visibleColumnIndex++;
        }

        PseudoClasses.Set(DataGridPseudoClass.EmptyColumns, visibleColumnCount == 0);
        PseudoClasses.Set(DataGridPseudoClass.EmptyRows, TotalEntryCount == 0);
    }

    private void EnsureVerticalGridLines()
    {
        if (IsColumnHeadersVisible)
        {
            double totalColumnsWidth = 0;
            foreach (DataGridColumn column in ColumnsInternal)
            {
                totalColumnsWidth += column.ActualWidth;

                column.HeaderCell.IsSeparatorsVisible =
                    (column != ColumnsInternal.LastVisibleColumn || totalColumnsWidth < CellsWidth);
                column.HeaderCell.IsSeparatorFullHeight = AreVerticalGridLinesVisible;
            }
        }

        foreach (DataGridRow row in GetAllRows())
        {
            row.EnsureGridLines();
        }
    }

    internal void NotifyRowDetailsChanged()
    {
        if (!_scrollingByHeight)
        {
            // Update layout when RowDetails are expanded or collapsed, just updating the vertical scroll bar is not enough
            // since rows could be added or removed
            InvalidateMeasure();
        }
    }

    private static void NotifyDataContextPropertyForAllRowCells(DataGridRowsEnumerable rowSource, bool arg2)
    {
        foreach (DataGridRow row in rowSource)
        {
            foreach (DataGridCell cell in row.Cells)
            {
                if (cell.Content is StyledElement cellContent)
                {
                    // TODO need review 需要审查是否正常
                    DataContextProperty.InvokeNotifying(cellContent, arg2);
                }
            }
        }
    }

    private void AddNewCellPrivate(DataGridRow row, DataGridColumn column)
    {
        DataGridCell newCell = new DataGridCell();
        PopulateCellContent(
            isCellEdited: false,
            dataGridColumn: column,
            dataGridRow: row,
            dataGridCell: newCell);
        if (row.OwningGrid != null)
        {
            newCell.OwningColumn       = column;
            newCell.IsVisible          = column.IsVisible;
        }

        row.Cells.Insert(column.Index, newCell);
        // 新建单元格在选中行等场景下没有机会经过回收/当前格刷新路径，
        // 创建时按行与网格的当前状态初始化伪类，保证 :selected 等状态不缺失
        newCell.UpdatePseudoClasses();
    }

    private void CompleteCellsCollection(DataGridRow dataGridRow)
    {
        Debug.Assert(dataGridRow != null);
        int cellsInCollection = dataGridRow.Cells.Count;
        if (ColumnsItemsInternal.Count > cellsInCollection)
        {
            for (int columnIndex = cellsInCollection; columnIndex < ColumnsItemsInternal.Count; columnIndex++)
            {
                AddNewCellPrivate(dataGridRow, ColumnsItemsInternal[columnIndex]);
            }
        }

        // Recycled rows retain their cells and OwningColumn references, so assigning the same column
        // does not invoke DataGridCell.OnOwningColumnSet again. Reproject transient column state here
        // before the row is presented for a new query generation.
        for (int columnIndex = 0; columnIndex < dataGridRow.Cells.Count; columnIndex++)
        {
            var cell = dataGridRow.Cells[columnIndex];
            cell.IsSorting = cell.OwningColumn?.SortState.Direction is not null;
        }
    }

    // Makes sure horizontal layout is updated to reflect any changes that affect it
    private void EnsureHorizontalLayout()
    {
        ColumnsInternal.EnsureVisibleEdgedColumnsWidth();
        InvalidateColumnHeadersMeasure();
        InvalidateRowsMeasure(true);
        InvalidateMeasure();
    }

    private void HandleCanUserResizeColumnsChanged(AvaloniaPropertyChangedEventArgs change)
    {
        EnsureHorizontalLayout();
    }

    private void EnsureRowHeaderWidth()
    {
        if (IsRowHeadersVisible)
        {
            if (IsColumnHeadersVisible)
            {
                EnsureTopLeftCornerHeader();
            }

            if (_rowsPresenter != null)
            {
                bool updated = false;

                foreach (Control element in _rowsPresenter.Children)
                {
                    if (element is DataGridRow row)
                    {
                        // If the RowHeader resulted in a different width the last time it was measured, we need
                        // to re-measure it
                        if (row.HeaderCell != null &&
                            !MathUtils.AreClose(row.HeaderCell.DesiredSize.Width, ActualRowHeaderWidth))
                        {
                            row.HeaderCell.InvalidateMeasure();
                            updated = true;
                        }
                    }
                    else if (element is DataGridRowGroupHeader groupHeader && groupHeader.HeaderCell != null &&
                             !MathUtils.AreClose(groupHeader.HeaderCell.DesiredSize.Width, ActualRowHeaderWidth))
                    {
                        groupHeader.HeaderCell.InvalidateMeasure();
                        updated = true;
                    }
                }

                if (updated)
                {
                    // We need to update the width of the horizontal scrollbar if the rowHeaders' width actually changed
                    InvalidateMeasure();
                }
            }
        }
    }

    private void EnsureRowsPresenterVisibility()
    {
        if (_rowsPresenter != null)
        {
            // RowCount doesn't need to be considered, doing so might cause extra Visibility changes
            _rowsPresenter.IsVisible = (ColumnsInternal.FirstVisibleNonFillerColumn != null);
        }
    }

    private void EnsureTopLeftCornerHeader()
    {
        if (_topLeftCornerHeader != null)
        {
            _topLeftCornerHeader.IsVisible = (HeadersVisibility == DataGridHeadersVisibility.All);

            if (_topLeftCornerHeader.IsVisible)
            {
                if (!double.IsNaN(RowHeaderWidth))
                {
                    // RowHeaderWidth is set explicitly so we should use that
                    _topLeftCornerHeader.Width = RowHeaderWidth;
                }
                else if (VisibleSlotCount > 0)
                {
                    // RowHeaders AutoSize and we have at least 1 row so take the desired width
                    _topLeftCornerHeader.Width = RowHeadersDesiredWidth;
                }
            }
        }
    }

    private void InvalidateCellsArrange()
    {
        foreach (DataGridRow row in GetAllRows())
        {
            row.InvalidateHorizontalArrange();
        }
    }

    private void InvalidateColumnHeadersArrange()
    {
        if (IsGroupHeaderMode)
        {
            if (_groupColumnHeadersPresenter != null)
            {
                _groupColumnHeadersPresenter.InvalidateArrange();
            }
        }
        else
        {
            if (_columnHeadersPresenter != null)
            {
                _columnHeadersPresenter.InvalidateArrange();
            }
        }
    }

    private void InvalidateColumnHeadersMeasure()
    {
        if (IsGroupHeaderMode)
        {
            if (_groupColumnHeadersPresenter != null)
            {
                EnsureColumnHeadersVisibility();
                _groupColumnHeadersPresenter.InvalidateMeasure();
            }
        }
        else
        {
            if (_columnHeadersPresenter != null)
            {
                EnsureColumnHeadersVisibility();
                _columnHeadersPresenter.InvalidateMeasure();
            }
        }
    }

    private void InvalidateRowsArrange()
    {
        if (_rowsPresenter != null)
        {
            _rowsPresenter.InvalidateArrange();
        }
    }

    private void InvalidateRowsMeasure(bool invalidateIndividualElements)
    {
        if (_rowsPresenter != null)
        {
            _rowsPresenter.InvalidateMeasure();

            if (invalidateIndividualElements)
            {
                foreach (Control element in _rowsPresenter.Children)
                {
                    element.InvalidateMeasure();
                }
            }
        }
    }

    //TODO: Check
    private void HandleIsEnabledChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!IsEnabled)
        {
            CancelRowReorder();
            SuspendPopupPinnedOpenFilterTarget();
        }
        else
        {
            RefreshPopupPinnedOpenFilterTarget();
        }
    }

    private void HandleCanUserReorderRowsChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!CanUserReorderRows)
        {
            CancelRowReorder();
        }
    }

    private void HandleFrozenColumnCountChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ProcessFrozenColumnCount();
    }

    private void HandleColumnWidthChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var value = (DataGridLength)(change.NewValue ?? DataGridLength.Auto);

        int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (column.InheritsWidth)
            {
                column.SetWidthInternalNoCallback(value);
            }
        }

        EnsureHorizontalLayout();
    }

    private void HandleGridLinesVisibilityChanged(AvaloniaPropertyChangedEventArgs change)
    {
        EnsureVerticalGridLines();
        foreach (DataGridRow row in GetAllRows())
        {
            row.InvalidateHorizontalArrange();
        }
    }

    private void HandleHeadersVisibilityChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // TODO 需要审查
        var oldValue = (DataGridHeadersVisibility)(change.OldValue ?? DataGridHeadersVisibility.All);
        var newValue = (DataGridHeadersVisibility)(change.NewValue ?? DataGridHeadersVisibility.All);
        bool HasFlags(DataGridHeadersVisibility value, DataGridHeadersVisibility flags) => ((value & flags) == flags);

        bool newValueCols = HasFlags(newValue, DataGridHeadersVisibility.Column);
        bool newValueRows = HasFlags(newValue, DataGridHeadersVisibility.Row);
        bool oldValueCols = HasFlags(oldValue, DataGridHeadersVisibility.Column);
        bool oldValueRows = HasFlags(oldValue, DataGridHeadersVisibility.Row);

        // Columns
        if (newValueCols != oldValueCols)
        {
            if (IsGroupHeaderMode)
            {
                if (_groupColumnHeadersPresenter != null)
                {
                    if (!newValueCols)
                    {
                        _groupColumnHeadersPresenter.Measure(default);
                    }
                    else
                    {
                        EnsureVerticalGridLines();
                    }

                    InvalidateMeasure();
                }
            }
            else
            {
                if (_columnHeadersPresenter != null)
                {
                    EnsureColumnHeadersVisibility();
                    if (!newValueCols)
                    {
                        _columnHeadersPresenter.Measure(default);
                    }
                    else
                    {
                        EnsureVerticalGridLines();
                    }

                    InvalidateMeasure();
                }
            }
        }

        // Rows
        if (newValueRows != oldValueRows)
        {
            if (_rowsPresenter != null)
            {
                foreach (Control element in _rowsPresenter.Children)
                {
                    if (element is DataGridRow row)
                    {
                        row.EnsureHeaderStyleAndVisibility(null);
                        if (newValueRows)
                        {
                            row.ApplyState();
                            row.EnsureHeaderVisibility();
                            if (IsRowHeadersVisible)
                            {
                                row.ApplyHeaderContentTemplate();
                            }
                        }
                    }
                    else if (element is DataGridRowGroupHeader rowGroupHeader)
                    {
                        rowGroupHeader.EnsureHeaderVisibility();
                    }
                }

                InvalidateRowHeightEstimate();
                InvalidateRowsMeasure(invalidateIndividualElements: true);
            }
        }

        if (_topLeftCornerHeader != null)
        {
            _topLeftCornerHeader.IsVisible = newValueRows && newValueCols;
            if (_topLeftCornerHeader.IsVisible)
            {
                _topLeftCornerHeader.Measure(default);
            }
        }
    }

    private void HandleIsReadOnlyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var value = (bool)(change.NewValue ?? false);
        if (value && !CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
        {
            CancelEdit(DataGridEditingUnit.Row, raiseEvents: false);
        }
    }

    private void HandleMaxColumnWidthChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var oldValue = (double)(change.OldValue ?? 0);
        int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
            HandleColumnMaxWidthChanged(column, Math.Min(column.MaxWidth, oldValue));
        }
    }

    private void HandleMinColumnWidthChanged(AvaloniaPropertyChangedEventArgs change)
    {
        double oldValue = (double)(change.OldValue ?? 0);
        int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
            HandleColumnMinWidthChanged(column, Math.Max(column.MinWidth, oldValue));
        }
    }

    private void HandleRowHeightChanged(AvaloniaPropertyChangedEventArgs change)
    {
        InvalidateRowHeightEstimate();
        // Re-measure all the rows due to the Height change
        InvalidateRowsMeasure(invalidateIndividualElements: true);
        // DataGrid needs to update the layout information and the ScrollBars
        InvalidateMeasure();
    }

    private void HandleRowHeaderWidthChanged(AvaloniaPropertyChangedEventArgs change)
    {
        EnsureRowHeaderWidth();
    }

    private void EnsureColumnHeadersVisibility()
    {
        if (IsGroupHeaderMode)
        {
            if (_groupColumnHeadersPresenter != null)
            {
                _groupColumnHeadersPresenter.IsVisible = IsColumnHeadersVisible;
            }
        }
        else
        {
            if (_columnHeadersPresenter != null)
            {
                _columnHeadersPresenter.IsVisible = IsColumnHeadersVisible;
            }
        }
    }

    private bool IsColumnOutOfBounds(int columnIndex)
    {
        return columnIndex >= ColumnsItemsInternal.Count || columnIndex < 0;
    }

    private bool IsInnerCellOutOfBounds(int columnIndex, int slot)
    {
        return IsColumnOutOfBounds(columnIndex) || IsSlotOutOfBounds(slot);
    }

    private bool IsInnerCellOutOfSelectionBounds(int columnIndex, int slot)
    {
        return IsColumnOutOfBounds(columnIndex) || IsSlotOutOfSelectionBounds(slot);
    }

    private bool IsSlotOutOfBounds(int slot)
    {
        return slot >= SlotCount || slot < -1;
    }

    private bool IsSlotOutOfSelectionBounds(int slot)
    {
        return slot < 0 ||
               slot >= SlotCount ||
               !TryGetCommittedRangeEntry(slot, out _);
    }

    private void PopulateCellContent(bool isCellEdited,
                                     DataGridColumn dataGridColumn,
                                     DataGridRow dataGridRow,
                                     DataGridCell dataGridCell)
    {
        Debug.Assert(dataGridColumn != null);
        Debug.Assert(dataGridRow != null);
        Debug.Assert(dataGridCell != null);

        Control? element = null;
        Debug.Assert(dataGridRow.DataContext != null);
        if (isCellEdited)
        {
            // Generate EditingElement and apply column style if available
            element = dataGridColumn.GenerateEditingElementInternal(dataGridCell, dataGridRow.DataContext);
            if (element != null)
            {
                dataGridCell.Content = element;
                if (element.IsInitialized)
                {
                    PreparingCellForEditPrivate(element);
                }
                else
                {
                    // Subscribe to the new element's events
                    element.Initialized += HandleEditingElementInitialized;
                }
            }
        }
        else
        {
            // Generate Element and apply column style if available
            element              = dataGridColumn.GenerateElementInternal(dataGridCell, dataGridRow.DataContext);
            dataGridCell.Content = element;
        }
    }

    private bool ProcessF2Key(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);

        if (!shift && !ctrl &&
            _editingColumnIndex == -1 && CurrentColumnIndex != -1 && GetRowSelection(CurrentSlot) &&
            !GetColumnEffectiveReadOnlyState(CurrentColumn))
        {
            if (ScrollSlotIntoView(CurrentColumnIndex, CurrentSlot, forCurrentCellChange: false,
                    forceHorizontalScroll: true))
            {
                BeginCellEdit(e);
            }

            return true;
        }

        return false;
    }

    private void RemoveDisplayedColumnHeader(DataGridColumn dataGridColumn)
    {
        if (_columnHeadersPresenter != null)
        {
            _columnHeadersPresenter.Children.Remove(dataGridColumn.HeaderCell);
        }
    }

    private void RemoveDisplayedColumnHeaders()
    {
        if (_columnHeadersPresenter != null)
        {
            _columnHeadersPresenter.Children.Clear();
        }

        Debug.Assert(ColumnsInternal.FillerColumn != null);
        ColumnsInternal.FillerColumn.IsRepresented = false;
    }

    private void HandleSelectionModeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        AnchorSlot = -1;
        _rangeSelectionAnchorDataIndex = -1;
        Selection = Selection.NormalizeForMode(SelectionMode);
    }

    private void HandleAutoGenerateColumnsChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var value = (bool)(change.NewValue ?? false);
        if (value)
        {
            InitializeElements(recycleRows: false);
        }
        else
        {
            RemoveAutoGeneratedColumns();
        }
    }


    /// <summary>
    /// This is an empty content control that's used during the DataGrid's copy procedure
    /// to determine the value of a ClipboardContentBinding for a particular column and item.
    /// </summary>
    internal ContentControl ClipboardContentControl
    {
        get
        {
            if (_clipboardContentControl == null)
            {
                _clipboardContentControl = new ContentControl();
            }

            return _clipboardContentControl;
        }
    }

    private void ConfigureFrameBorderThickness()
    {
        if (!IsFrameBorderVisible)
        {
            SetValue(FrameBorderThicknessProperty, new Thickness(0), BindingPriority.Template);
        }
        else
        {
            SetValue(FrameBorderThicknessProperty, BorderThickness);
        }
    }

    private void ConfigureFrameCornerRadius()
    {
        if (IsFrameBorderVisible)
        {
            FrameCornerRadius = CornerRadius;
        }
        else
        {
            FrameCornerRadius = new CornerRadius(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0);
        }
    }

    private void ConfigureHeaderCornerRadius()
    {
        if (Title == null)
        {
            if (!IsRowHeadersVisible)
            {
                HeaderCornerRadius = new CornerRadius(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0);
            }
            else
            {
                HeaderCornerRadius = new CornerRadius(0, CornerRadius.TopRight, 0, 0);
            }
        }
        else
        {
            HeaderCornerRadius = new CornerRadius(0);
        }
    }

    private void CheckFrozenColumnCount()
    {
        // The total number of left frozen and right frozen columns must be less than or equal to the total number of columns
        var totalFrozenCount = LeftFrozenColumnCount + RightFrozenColumnCount;
        if (totalFrozenCount > Columns.Count)
        {
            throw DataGridError.DataGrid.ValueMustBeLessThanOrEqualTo("LeftFrozenColumnCount + RightFrozenColumnCount",
                "Columns.Count", Columns.Count);
        }
    }

    private void ConfigurePaginationVisibility()
    {
        var pageSize = PageRequest?.DataCount ?? PageSize;
        if (pageSize == 0 ||
            TotalItemCount == 0 ||
            (IsHideOnSinglePage && TotalItemCount <= pageSize))
        {
            EffectivePaginationVisibility = DataGridPaginationVisibility.None;
        }
        else
        {
            EffectivePaginationVisibility = PaginationVisibility;
        }
    }
}
