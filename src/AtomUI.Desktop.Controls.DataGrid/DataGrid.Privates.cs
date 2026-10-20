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

public partial class DataGrid
{
    internal const bool DefaultCanUserReorderColumns = true;
    internal const bool DefaultCanUserResizeColumns = true;
    internal const bool DefaultCanUserSortColumns = false;
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

    private int NoSelectionChangeCount
    {
        get => _noSelectionChangeCount;
        set => _noSelectionChangeCount = value;
    }

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

    /// <summary>
    /// The default order to use for columns when there is no <see cref="DisplayAttribute.Order"/>
    /// value available for the property.
    /// </summary>
    /// <remarks>
    /// The value of 10,000 comes from the DataAnnotations spec, allowing
    /// some properties to be ordered at the beginning and some at the end.
    /// </remarks>
    private const int DefaultColumnDisplayOrder = 10000;

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
    private IndexToValueTable<bool> _showDetailsTable;
    private readonly Dictionary<DataGridRowKey, bool> _rangeDetailsVisibility = [];
    private readonly Dictionary<DataGridRowKey, double> _rangeMeasuredDetailsHeights = [];
    private readonly Dictionary<DataGridRowKey, (long DataGeneration, int Slot)> _rangeDetailsSlotHints = [];
    private IndexToValueTable<double> _rowDetailsHeightEstimateTable;
    private double _rowHeaderDesiredWidth;
    private int? _mouseOverRowIndex; // -1 is used for the 'new row'
    private bool _makeFirstDisplayedCellCurrentCellPending;
    private bool _measured;
    private int _noSelectionChangeCount;
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
    private bool _areHandlersSuspended;
    private bool _autoSizingColumns;
    private IndexToValueTable<bool> _collapsedSlotsTable;
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
    private bool _ignoreNextScrollBarsLayout;
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

    private void FlushCurrentCellChanged()
    {
        if (_makeFirstDisplayedCellCurrentCellPending)
        {
            return;
        }

        if (CurrentColumn != _previousCurrentColumn
            || CurrentItem != _previousCurrentItem)
        {
            _previousCurrentColumn = CurrentColumn;
            _previousCurrentItem   = CurrentItem;

            NotifyCurrentCellChanged(EventArgs.Empty);
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = ProcessDataGridKey(e);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled)
        {
            HandleKeyDown(this, e);
        }
    }

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

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab && CurrentColumnIndex != -1 && e.Source == this)
        {
            bool success =
                ScrollSlotIntoView(
                    CurrentColumnIndex, CurrentSlot,
                    forCurrentCellChange: false,
                    forceHorizontalScroll: true);
            Debug.Assert(success);
            if (CurrentColumnIndex != -1 && Selection.IsEmpty)
            {
                SetRowSelection(CurrentSlot, isSelected: true, setAnchorSlot: true);
            }
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (!e.Handled)
        {
            HandleKeyUp(this, e);
        }
    }

    private void HandleGotFocus(object? sender, RoutedEventArgs e)
    {
        if (!ContainsFocus)
        {
            ContainsFocus = true;
            ApplyDisplayedRowsState(DisplayData.FirstScrollingSlot, DisplayData.LastScrollingSlot);
            if (CurrentColumnIndex != -1 && IsSlotVisible(CurrentSlot))
            {
                if (DisplayData.GetDisplayedElement(CurrentSlot) is DataGridRow row)
                {
                    row.Cells[CurrentColumnIndex].UpdatePseudoClasses();
                }
            }
        }

        // Keep track of which row contains the newly focused element.
        // 使用 FindAncestorOfType 替代手写 visual tree walk，避免在 Avalonia 12 下
        // 撞到窗口装饰系统形成的循环边导致 CPU spin（参见 datagrid-cross-instance-rowdetails-hang.md）。
        var focusedElement = e.Source as Visual;
        _focusedObject = focusedElement;
        var focusedRow = focusedElement?.FindAncestorOfType<DataGridRow>(includeSelf: true);
        if (focusedRow != null && focusedRow.OwningGrid == this && _focusedRow != focusedRow)
        {
            ResetFocusedRow();
            _focusedRow = focusedRow.IsVisible ? focusedRow : null;
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (!e.Handled)
        {
            HandleGotFocus(this, e);
        }
    }

    private void HandleLostFocus(object? sender, RoutedEventArgs e)
    {
        _focusedObject = null;
        if (ContainsFocus)
        {
            Visual?         focusedObject     = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
            bool            focusLeftDataGrid = !this.IsVisualAncestorOf(focusedObject);
            DataGridColumn? editingColumn     = null;

            if (EditingRow != null && EditingColumnIndex != -1)
            {
                editingColumn = ColumnsItemsInternal[EditingColumnIndex];
            }

            if (focusLeftDataGrid && !(editingColumn is DataGridTemplateColumn))
            {
                ContainsFocus = false;
                if (EditingRow != null)
                {
                    CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
                }

                ResetFocusedRow();
                ApplyDisplayedRowsState(DisplayData.FirstScrollingSlot, DisplayData.LastScrollingSlot);
                if (CurrentColumnIndex != -1 && IsSlotVisible(CurrentSlot))
                {
                    if (DisplayData.GetDisplayedElement(CurrentSlot) is DataGridRow row)
                    {
                        row.Cells[CurrentColumnIndex].UpdatePseudoClasses();
                    }
                }
            }
            else if (focusLeftDataGrid)
            {
                if (focusedObject is Control focusedElement)
                {
                    focusedElement.LostFocus += HandleExternalEditingElementLostFocus;
                }
            }
        }
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (!e.Handled)
        {
            HandleLostFocus(this, e);
        }
    }

    /// <summary>
    /// Cancels editing mode for the specified DataGridEditingUnit and restores its original value.
    /// </summary>
    /// <param name="editingUnit">Specifies whether to cancel edit for a Cell or Row.</param>
    /// <param name="raiseEvents">Specifies whether or not to raise editing events</param>
    /// <returns>True if operation was successful. False otherwise.</returns>
    internal bool CancelEdit(DataGridEditingUnit editingUnit, bool raiseEvents)
    {
        if (!EndCellEdit(
                DataGridEditAction.Cancel,
                exitEditingMode: true,
                keepFocus: ContainsFocus,
                raiseEvents: raiseEvents))
        {
            return false;
        }

        if (editingUnit == DataGridEditingUnit.Row)
        {
            return EndRowEdit(DataGridEditAction.Cancel, true, raiseEvents);
        }

        return true;
    }

    internal IEnumerable<object?> GetSelectionInclusive(int startRowIndex, int endRowIndex)
    {
        if (_rangePresentationIndex is null ||
            GetCommittedSelectionScope() is not { } scope)
        {
            yield break;
        }
        foreach (var block in _rangePresentationIndex.Snapshot.Blocks)
        {
            foreach (var entry in block.Entries)
            {
                if (entry.Kind == DataGridSourceEntryKind.Data &&
                    entry.WindowDataIndex >= startRowIndex &&
                    entry.WindowDataIndex <= endRowIndex &&
                    Selection.Contains(entry.RowKey, entry.DataIndex, scope))
                {
                    yield return entry.Item;
                }
            }
        }
    }

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

    internal bool IsDoubleClickRecordsClickOnCall(Control element)
    {
        if (_clickedElement == element)
        {
            _clickedElement = null;
            return true;
        }

        _clickedElement = element;
        return false;
    }

    // Returns the committed item or group entry for a display slot.
    internal object? ItemFromSlot(int slot, ref int rowIndex)
    {
        if (!TryGetCommittedRangeEntry(slot, out var entry))
        {
            return null;
        }
        rowIndex = entry.Kind == DataGridSourceEntryKind.Data
            ? entry.WindowDataIndex
            : -1;
        return entry.Kind == DataGridSourceEntryKind.Data ? entry.Item : entry.Group;
    }

    internal bool ProcessDownKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessDownKeyInternal(shift, ctrl);
    }

    internal bool ProcessEndKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessEndKey(shift, ctrl);
    }

    internal bool ProcessEnterKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessEnterKey(shift, ctrl);
    }

    internal bool ProcessHomeKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessHomeKey(shift, ctrl);
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

    internal bool ProcessLeftKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessLeftKey(shift, ctrl);
    }

    internal bool ProcessNextKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessNextKey(shift, ctrl);
    }

    internal bool ProcessPriorKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessPriorKey(shift, ctrl);
    }

    internal bool ProcessRightKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessRightKey(shift, ctrl);
    }

    /// <summary>
    /// Selects items and updates currency based on parameters
    /// </summary>
    /// <param name="columnIndex">column index to make current</param>
    /// <param name="item">A committed data item to make current.</param>
    /// <param name="backupSlot">slot to use in case the item is no longer valid</param>
    /// <param name="action">selection action to perform</param>
    /// <param name="scrollIntoView">whether or not the new current item should be scrolled into view</param>
    internal void ProcessSelectionAndCurrency(int columnIndex, object item, int backupSlot,
                                              DataGridSelectionAction action, bool scrollIntoView)
    {
        _noSelectionChangeCount++;
        _noCurrentCellChangeCount++;
        try
        {
            var slot = FindCommittedRangeSlot(item);

            if (slot == -1)
            {
                slot = backupSlot;
            }

            if (slot < 0 || slot > SlotCount)
            {
                return;
            }

            switch (action)
            {
                case DataGridSelectionAction.AddCurrentToSelection:
                    SetRowSelection(slot, isSelected: true, setAnchorSlot: true);
                    break;
                case DataGridSelectionAction.RemoveCurrentFromSelection:
                    SetRowSelection(slot, isSelected: false, setAnchorSlot: false);
                    break;
                case DataGridSelectionAction.SelectFromAnchorToCurrent:
                    if (SelectionMode == DataGridSelectionMode.Extended && AnchorSlot != -1)
                    {
                        int anchorSlot = AnchorSlot;
                        if (slot <= anchorSlot)
                        {
                            SetRowsSelection(slot, anchorSlot);
                        }
                        else
                        {
                            SetRowsSelection(anchorSlot, slot);
                        }
                    }
                    else
                    {
                        goto case DataGridSelectionAction.SelectCurrent;
                    }

                    break;
                case DataGridSelectionAction.SelectCurrent:
                    ClearRowSelection(slot, setAnchorSlot: true);
                    break;
                case DataGridSelectionAction.None:
                    break;
            }

            if (CurrentSlot != slot || (CurrentColumnIndex != columnIndex && columnIndex != -1))
            {
                if (columnIndex == -1)
                {
                    if (CurrentColumnIndex != -1)
                    {
                        columnIndex = CurrentColumnIndex;
                    }
                    else
                    {
                        DataGridColumn? firstVisibleColumn = ColumnsInternal.FirstVisibleNonFillerColumn;
                        if (firstVisibleColumn != null)
                        {
                            columnIndex = firstVisibleColumn.Index;
                        }
                    }
                }

                if (columnIndex != -1)
                {
                    if (!SetCurrentCellCore(
                            columnIndex, slot,
                            commitEdit: true,
                            endRowEdit: CurrentSlot != slot)
                        || (scrollIntoView &&
                            !ScrollSlotIntoView(
                                columnIndex, slot,
                                forCurrentCellChange: true,
                                forceHorizontalScroll: false)))
                    {
                        return;
                    }
                }
            }

            _successfullyUpdatedSelection = true;
        }
        finally
        {
            NoCurrentCellChangeCount--;
            NoSelectionChangeCount--;
        }
    }

    internal bool ProcessUpKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessUpKey(shift, ctrl);
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

    // Convenient overload that commits the current edit.
    internal bool SetCurrentCellCore(int columnIndex, int slot)
    {
        return SetCurrentCellCore(columnIndex, slot, commitEdit: true, endRowEdit: true);
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

    internal bool UpdateSelectionAndCurrency(int columnIndex, int slot, DataGridSelectionAction action,
                                             bool scrollIntoView)
    {
        _successfullyUpdatedSelection = false;

        _noSelectionChangeCount++;
        _noCurrentCellChangeCount++;
        try
        {
            Debug.Assert(ColumnsInternal.RowGroupSpacerColumn != null);
            if (ColumnsInternal.RowGroupSpacerColumn.IsRepresented &&
                columnIndex == ColumnsInternal.RowGroupSpacerColumn.Index)
            {
                columnIndex = -1;
            }

            if (IsRangePresentationActive &&
                slot >= 0 &&
                slot < SlotCount &&
                !TryGetCommittedRangeEntry(slot, out _))
            {
                return QueueRangeNavigation(columnIndex, slot, action);
            }

            if (IsSlotOutOfSelectionBounds(slot) || (columnIndex != -1 && IsColumnOutOfBounds(columnIndex)))
            {
                return false;
            }

            if (EditingRow != null && slot != EditingRow.Slot &&
                !CommitEdit(DataGridEditingUnit.Row, true))
            {
                return false;
            }
            ApplyRangeSelectionAndCurrency(columnIndex, slot, action, scrollIntoView);
            return _successfullyUpdatedSelection;
        }
        finally
        {
            NoCurrentCellChangeCount--;
            NoSelectionChangeCount--;
        }
    }

    //TODO: Ensure right button is checked for
    internal bool UpdateStateOnMouseRightButtonDown(PointerPressedEventArgs pointerPressedEventArgs, int columnIndex,
                                                    int slot, bool allowEdit)
    {
        KeyboardHelper.GetMetaKeyState(this, pointerPressedEventArgs.KeyModifiers, out bool ctrl, out bool shift);
        return UpdateStateOnMouseRightButtonDown(pointerPressedEventArgs, columnIndex, slot, allowEdit, shift, ctrl);
    }

    //TODO: Ensure left button is checked for
    internal bool UpdateStateOnMouseLeftButtonDown(PointerPressedEventArgs pointerPressedEventArgs, int columnIndex,
                                                   int slot, bool allowEdit)
    {
        KeyboardHelper.GetMetaKeyState(this, pointerPressedEventArgs.KeyModifiers, out bool ctrl, out bool shift);
        return UpdateStateOnMouseLeftButtonDown(pointerPressedEventArgs, columnIndex, slot, allowEdit, shift, ctrl);
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

    /// <summary>
    /// If the editing element has focus, this method will set focus to the DataGrid itself
    /// in order to force the element to lose focus.  It will then wait for the editing element's
    /// LostFocus event, at which point it will perform the specified action.
    ///
    /// NOTE: It is important to understand that the specified action will be performed when the editing
    /// element loses focus only if this method returns true.  If it returns false, then the action
    /// will not be performed later on, and should instead be performed by the caller, if necessary.
    /// </summary>
    /// <param name="action">Action to perform after the editing element loses focus</param>
    /// <returns>True if the editing element had focus and the action was cached away; false otherwise</returns>
    //TODO TabStop
    internal bool WaitForLostFocus(Action action)
    {
        if (EditingRow != null && EditingColumnIndex != -1 && !_executingLostFocusActions)
        {
            DataGridColumn editingColumn  = ColumnsItemsInternal[EditingColumnIndex];
            Control?       editingElement = editingColumn.GetCellContent(EditingRow);
            if (editingElement != null && _focusedObject is Visual focusedVisual && editingElement.IsVisualAncestorOf(focusedVisual))
            {
                Debug.Assert(_lostFocusActions != null);
                _lostFocusActions.Enqueue(action);
                editingElement.LostFocus += HandleEditingElementLostFocus;
                //IsTabStop = true;
                Focus();
                return true;
            }
        }

        return false;
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

    private void SetValueNoCallback<T>(AvaloniaProperty<T> property, T value,
                                       BindingPriority priority = BindingPriority.LocalValue)
    {
        _areHandlersSuspended = true;
        try
        {
            SetValue(property, value, priority);
        }
        finally
        {
            _areHandlersSuspended = false;
        }
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
            newCell[!DataGridCell.SizeTypeProperty] = this[!SizeTypeProperty];
        }

        row.Cells.Insert(column.Index, newCell);
        // 新建单元格在选中行等场景下没有机会经过回收/当前格刷新路径，
        // 创建时按行与网格的当前状态初始化伪类，保证 :selected 等状态不缺失
        newCell.UpdatePseudoClasses();
    }

    private bool BeginCellEdit(RoutedEventArgs editingEventArgs)
    {
        if (CurrentColumnIndex == -1)
        {
            return false;
        }

        Debug.Assert(CurrentColumn != null);
        Debug.Assert(CurrentColumnIndex >= 0);
        Debug.Assert(CurrentColumnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(CurrentSlot >= -1);
        Debug.Assert(CurrentSlot < SlotCount);
        Debug.Assert(EditingRow == null || EditingRow.Slot == CurrentSlot);
        Debug.Assert(!GetColumnEffectiveReadOnlyState(CurrentColumn));
        Debug.Assert(CurrentColumn.IsVisible);

        if (_editingColumnIndex != -1)
        {
            // Current cell is already in edit mode
            Debug.Assert(_editingColumnIndex == CurrentColumnIndex);
            return true;
        }

        // Get or generate the editing row if it doesn't exist
        DataGridRow? dataGridRow = EditingRow;
        if (dataGridRow == null)
        {
            if (IsSlotVisible(CurrentSlot))
            {
                dataGridRow = DisplayData.GetDisplayedElement(CurrentSlot) as DataGridRow;
                Debug.Assert(dataGridRow != null);
            }
            else
            {
                dataGridRow = GenerateRow(RowIndexFromSlot(CurrentSlot), CurrentSlot);
            }
        }

        Debug.Assert(dataGridRow != null);

        // Cache these to see if they change later
        int currentRowIndex    = CurrentSlot;
        int currentColumnIndex = CurrentColumnIndex;

        // Raise the BeginningEdit event
        DataGridCell dataGridCell = dataGridRow.Cells[CurrentColumnIndex];
        DataGridBeginningEditEventArgs e =
            new DataGridBeginningEditEventArgs(CurrentColumn, dataGridRow, editingEventArgs);
        NotifyBeginningEdit(e);
        if (e.Cancel
            || currentRowIndex != CurrentSlot
            || currentColumnIndex != CurrentColumnIndex
            || (EditingRow == null && !BeginRowEdit(dataGridRow)))
        {
            // If either BeginningEdit was canceled, currency/selection was changed in the event handler,
            // or we failed opening the row for edit, then we can no longer continue BeginCellEdit
            return false;
        }

        Debug.Assert(EditingRow != null);
        Debug.Assert(EditingRow.Slot == CurrentSlot);

        // Finally, we can prepare the cell for editing
        _editingColumnIndex = CurrentColumnIndex;
        _editingEventArgs   = editingEventArgs;
        EditingRow.Cells[CurrentColumnIndex].UpdatePseudoClasses();
        PopulateCellContent(
            isCellEdited: true,
            dataGridColumn: CurrentColumn,
            dataGridRow: dataGridRow,
            dataGridCell: dataGridCell);
        return true;
    }

    //TODO Validation
    private bool BeginRowEdit(DataGridRow dataGridRow)
    {
        // Inline editing requires a key-based edit buffer and is not entered by
        // the range-backed control; mutations use EditRowAsync instead.
        return false;
    }

    private bool CancelRowEdit(bool exitEditingMode)
    {
        if (EditingRow == null)
        {
            return true;
        }

        return false;
    }

    private bool CommitEditForOperation(int columnIndex, int slot, bool forCurrentCellChange)
    {
        if (forCurrentCellChange)
        {
            if (!EndCellEdit(DataGridEditAction.Commit, exitEditingMode: true, keepFocus: true, raiseEvents: true))
            {
                return false;
            }

            if (CurrentSlot != slot &&
                !EndRowEdit(DataGridEditAction.Commit, exitEditingMode: true, raiseEvents: true))
            {
                return false;
            }
        }

        if (IsColumnOutOfBounds(columnIndex))
        {
            return false;
        }

        if (slot >= SlotCount)
        {
            // Current cell was reset because the commit deleted row(s).
            // Since the user wants to change the current cell, we don't
            // want to end up with no current cell. We pick the last row
            // in the grid which may be the 'new row'.
            int lastSlot = LastVisibleSlot;
            if (forCurrentCellChange &&
                CurrentColumnIndex == -1 &&
                lastSlot != -1)
            {
                SetAndSelectCurrentCell(columnIndex, lastSlot, forceCurrentCellSelection: false);
            }

            // Interrupt operation because it has become invalid.
            return false;
        }

        return true;
    }

    //TODO Validation
    private bool CommitRowEdit(bool exitEditingMode)
    {
        if (EditingRow == null)
        {
            return true;
        }

        Debug.Assert(EditingRow != null && EditingRow.Index >= -1);
        Debug.Assert(EditingRow.Slot < SlotCount);

        //if (!ValidateEditingRow(scrollIntoView: true, wireEvents: false))
        if (!EditingRow.IsValid)
        {
            return false;
        }

        return false;
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

    private void ComputeScrollBarsLayout()
    {
        if (_ignoreNextScrollBarsLayout)
        {
            _ignoreNextScrollBarsLayout = false;
        }

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

    /// <summary>
    /// Handles the current editing element's LostFocus event by performing any actions that
    /// were cached by the WaitForLostFocus method.
    /// </summary>
    /// <param name="sender">Editing element</param>
    /// <param name="e">RoutedEventArgs</param>
    private void HandleEditingElementLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is Control editingElement)
        {
            editingElement.LostFocus -= HandleEditingElementLostFocus;
            if (EditingRow != null && _editingColumnIndex != -1)
            {
                FocusEditingCell(true);
            }

            Debug.Assert(_lostFocusActions != null);
            try
            {
                _executingLostFocusActions = true;
                while (_lostFocusActions.Count > 0)
                {
                    _lostFocusActions.Dequeue()();
                }
            }
            finally
            {
                _executingLostFocusActions = false;
            }
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

    private void HandleEditingElementInitialized(object? sender, EventArgs e)
    {
        if (sender is Control element)
        {
            element.Initialized -= HandleEditingElementInitialized;
            PreparingCellForEditPrivate(element);
        }
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

    private void HandleHorizontalGridLinesBrushChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended && _rowsPresenter != null)
        {
            foreach (DataGridRow row in GetAllRows())
            {
                row.EnsureGridLines();
            }
        }
    }

    private void HandleIsReadOnlyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            var value = (bool)(change.NewValue ?? false);
            if (value && !CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
            {
                CancelEdit(DataGridEditingUnit.Row, raiseEvents: false);
            }
        }
    }

    private void HandleMaxColumnWidthChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            var oldValue = (double)(change.OldValue ?? 0);
            int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
            for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
            {
                DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
                HandleColumnMaxWidthChanged(column, Math.Min(column.MaxWidth, oldValue));
            }
        }
    }

    private void HandleMinColumnWidthChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            double oldValue = (double)(change.OldValue ?? 0);
            int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
            for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
            {
                DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
                HandleColumnMinWidthChanged(column, Math.Max(column.MinWidth, oldValue));
            }
        }
    }

    private void HandleRowHeightChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            InvalidateRowHeightEstimate();
            // Re-measure all the rows due to the Height change
            InvalidateRowsMeasure(invalidateIndividualElements: true);
            // DataGrid needs to update the layout information and the ScrollBars
            InvalidateMeasure();
        }
    }

    private void HandleRowHeaderWidthChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            EnsureRowHeaderWidth();
        }
    }

    //TODO Validation
    //TODO Binding
    //TODO TabStop
    private bool EndCellEdit(DataGridEditAction editAction, bool exitEditingMode, bool keepFocus, bool raiseEvents)
    {
        if (_editingColumnIndex == -1)
        {
            return true;
        }

        var editingRow = EditingRow;
        if (editingRow is null)
        {
            return true;
        }

        Debug.Assert(_editingColumnIndex >= 0);
        Debug.Assert(_editingColumnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(_editingColumnIndex == CurrentColumnIndex);
        Debug.Assert(CurrentColumn != null);

        // Cache these to see if they change later
        int currentSlot        = CurrentSlot;
        int currentColumnIndex = CurrentColumnIndex;

        // We're ready to start ending, so raise the event
        DataGridCell editingCell    = editingRow.Cells[_editingColumnIndex];
        var          editingElement = editingCell.Content as Control;
        if (editingElement == null)
        {
            return false;
        }

        if (raiseEvents)
        {
            Debug.Assert(CurrentColumn != null);
            DataGridCellEditEndingEventArgs e =
                new DataGridCellEditEndingEventArgs(CurrentColumn, editingRow, editingElement, editAction);
            NotifyCellEditEnding(e);
            if (e.Cancel)
            {
                // CellEditEnding has been cancelled
                return false;
            }

            // Ensure that the current cell wasn't changed in the user's CellEditEnding handler
            if (_editingColumnIndex == -1 ||
                currentSlot != CurrentSlot ||
                currentColumnIndex != CurrentColumnIndex)
            {
                return true;
            }

            Debug.Assert(EditingRow != null);
            Debug.Assert(EditingRow.Slot == currentSlot);
            Debug.Assert(_editingColumnIndex != -1);
            Debug.Assert(_editingColumnIndex == CurrentColumnIndex);
        }

        // If we're canceling, let the editing column repopulate its old value if it wants
        if (editAction == DataGridEditAction.Cancel)
        {
            Debug.Assert(_uneditedValue != null);
            CurrentColumn.CancelCellEditInternal(editingElement, _uneditedValue);

            // Ensure that the current cell wasn't changed in the user column's CancelCellEdit
            if (_editingColumnIndex == -1 ||
                currentSlot != CurrentSlot ||
                currentColumnIndex != CurrentColumnIndex)
            {
                return true;
            }

            Debug.Assert(EditingRow != null);
            Debug.Assert(EditingRow.Slot == currentSlot);
            Debug.Assert(_editingColumnIndex != -1);
            Debug.Assert(_editingColumnIndex == CurrentColumnIndex);
        }

        // If we're committing, explicitly update the source but watch out for any validation errors
        if (editAction == DataGridEditAction.Commit)
        {
            void SetValidationStatus(BindingExpressionBase binding)
            {
                if (!DataValidationErrors.GetHasErrors(editingElement))
                {
                    ResetValidationStatus();
                }
                else
                {
                    if (editingCell.IsValid)
                    {
                        editingCell.IsValid = false;
                        editingCell.UpdatePseudoClasses();
                    }

                    if (editingRow.IsValid)
                    {
                        editingRow.IsValid = false;
                        editingRow.ApplyState();
                    }
                }
            }

            var editBinding = CurrentColumn?.CellEditBinding;
            if (editBinding != null && DataValidationErrors.GetHasErrors(editingElement))
            {
                editBinding.UpdateSource();
                SetValidationStatus(editBinding);
                ScrollSlotIntoView(CurrentColumnIndex, CurrentSlot, forCurrentCellChange: false,
                    forceHorizontalScroll: true);
                return false;
            }
        }

        ResetValidationStatus();
        Debug.Assert(CurrentColumn != null);
        if (exitEditingMode)
        {
            CurrentColumn.EndCellEditInternal(editingElement);
            _editingColumnIndex = -1;
            editingCell.UpdatePseudoClasses();

            //IsTabStop = true;
            if (keepFocus && editingElement.ContainsFocusedElement())
            {
                Focus();
            }

            PopulateCellContent(
                isCellEdited: !exitEditingMode,
                dataGridColumn: CurrentColumn,
                dataGridRow: editingRow,
                dataGridCell: editingCell);

            editingRow.InvalidateDesiredHeight();
            var column = editingCell.OwningColumn;
            Debug.Assert(column != null);
            if (column.Width.IsSizeToCells || column.Width.IsAuto)
            {
                // Invalidate desired width and force recalculation
                column.SetWidthDesiredValue(0);
                editingRow.OwningGrid?.AutoSizeColumn(column, editingCell.DesiredSize.Width);
            }
        }

        // We're done, so raise the CellEditEnded event
        if (raiseEvents)
        {
            NotifyCellEditEnded(new DataGridCellEditEndedEventArgs(CurrentColumn, editingRow, editAction));
        }

        // There's a chance that somebody reopened this cell for edit within the CellEditEnded handler,
        // so we should return false if we were supposed to exit editing mode, but we didn't
        return !(exitEditingMode && currentColumnIndex == _editingColumnIndex);
    }

    //TODO Validation
    private bool EndRowEdit(DataGridEditAction editAction, bool exitEditingMode, bool raiseEvents)
    {
        if (EditingRow == null)
        {
            return true;
        }

        if (_editingColumnIndex != -1)
        {
            // Ending the row edit will fail immediately under the following conditions:
            // 1. We haven't ended the cell edit yet.
            // 2. We're trying to cancel edit when the underlying DataType is not an IEditableObject,
            //    because we have no way to properly restore the old value.  We will only allow this to occur
            //    if raiseEvents == false, which means we're internally forcing a cancel.
            return false;
        }

        DataGridRow editingRow = EditingRow;

        if (raiseEvents)
        {
            DataGridRowEditEndingEventArgs e = new DataGridRowEditEndingEventArgs(EditingRow, editAction);
            NotifyRowEditEnding(e);
            if (e.Cancel)
            {
                // RowEditEnding has been cancelled
                return false;
            }

            // Editing states might have been changed in the RowEditEnding handlers
            if (_editingColumnIndex != -1)
            {
                return false;
            }

            if (editingRow != EditingRow)
            {
                return true;
            }
        }

        // Call the appropriate commit or cancel methods
        if (editAction == DataGridEditAction.Commit)
        {
            if (!CommitRowEdit(exitEditingMode))
            {
                return false;
            }
        }
        else
        {
            if (!CancelRowEdit(exitEditingMode) && raiseEvents)
            {
                // We failed to cancel edit so we should abort unless we're forcing a cancel
                return false;
            }
        }

        ResetValidationStatus();

        // Update the previously edited row's state
        if (exitEditingMode && editingRow == EditingRow)
        {
            RemoveEditingElements();
            ResetEditingRow();
        }

        // Raise the RowEditEnded event
        if (raiseEvents)
        {
            NotifyRowEditEnded(new DataGridRowEditEndedEventArgs(editingRow, editAction));
        }

        return true;
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

    /// <summary>
    /// Exits editing mode without trying to commit or revert the editing, and
    /// without repopulating the edited row's cell.
    /// </summary>
    //TODO TabStop
    private void ExitEdit(bool keepFocus)
    {
        if (EditingRow == null)
        {
            Debug.Assert(_editingColumnIndex == -1);
            return;
        }

        if (_editingColumnIndex != -1)
        {
            Debug.Assert(_editingColumnIndex >= 0);
            Debug.Assert(_editingColumnIndex < ColumnsItemsInternal.Count);
            Debug.Assert(_editingColumnIndex == CurrentColumnIndex);
            Debug.Assert(EditingRow != null && EditingRow.Slot == CurrentSlot);

            _editingColumnIndex = -1;
            EditingRow.Cells[CurrentColumnIndex].UpdatePseudoClasses();
        }

        //IsTabStop = true;
        if (IsSlotVisible(EditingRow.Slot))
        {
            EditingRow.ApplyState();
        }

        ResetEditingRow();
        if (keepFocus)
        {
            Focus();
        }
    }

    private void HandleExternalEditingElementLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is Control element)
        {
            element.LostFocus -= HandleExternalEditingElementLostFocus;
            HandleLostFocus(sender, e);
        }
    }

    //TODO TabStop
    private bool FocusEditingCell(bool setFocus)
    {
        Debug.Assert(CurrentColumnIndex >= 0);
        Debug.Assert(CurrentColumnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(CurrentSlot >= -1);
        Debug.Assert(CurrentSlot < SlotCount);
        Debug.Assert(EditingRow != null && EditingRow.Slot == CurrentSlot);
        Debug.Assert(_editingColumnIndex != -1);

        //IsTabStop = false;
        _focusEditingControl = false;

        bool         success      = false;
        DataGridCell dataGridCell = EditingRow.Cells[_editingColumnIndex];
        if (setFocus)
        {
            if (dataGridCell.ContainsFocusedElement())
            {
                success = true;
            }
            else
            {
                dataGridCell.Focus();
                success = dataGridCell.ContainsFocusedElement();
            }

            _focusEditingControl = !success;
        }

        return success;
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
        return slot >= SlotCount || slot < -1 || _collapsedSlotsTable.Contains(slot);
    }

    private bool IsSlotOutOfSelectionBounds(int slot)
    {
        return slot < 0 ||
               slot >= SlotCount ||
               !TryGetCommittedRangeEntry(slot, out _);
    }

    private void MakeFirstDisplayedCellCurrentCell()
    {
        if (CurrentColumnIndex != -1)
        {
            _makeFirstDisplayedCellCurrentCellPending = false;
            _desiredCurrentColumnIndex                = -1;
            FlushCurrentCellChanged();
            return;
        }

        var slot = -1;
        for (var candidate = 0; candidate < SlotCount; candidate++)
        {
            if (!TryGetCommittedRangeEntry(candidate, out var entry))
            {
                break;
            }
            if (entry.Kind == DataGridSourceEntryKind.Data)
            {
                slot = candidate;
                break;
            }
        }

        int columnIndex = FirstDisplayedNonFillerColumnIndex;
        if (_desiredCurrentColumnIndex >= 0 && _desiredCurrentColumnIndex < ColumnsItemsInternal.Count)
        {
            columnIndex = _desiredCurrentColumnIndex;
        }

        if (slot >= 0 && columnIndex >= 0)
        {
            SetAndSelectCurrentCell(columnIndex, slot, forceCurrentCellSelection: false);
        }
        AnchorSlot                                = slot;
        _makeFirstDisplayedCellCurrentCellPending = false;
        _desiredCurrentColumnIndex                = -1;
        FlushCurrentCellChanged();
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

    private void PreparingCellForEditPrivate(Control editingElement)
    {
        Debug.Assert(EditingRow != null);
        if (_editingColumnIndex == -1 ||
            CurrentColumnIndex == -1 ||
            EditingRow.Cells[CurrentColumnIndex].Content != editingElement)
        {
            // The current cell has changed since the call to BeginCellEdit, so the fact
            // that this element has loaded is no longer relevant
            return;
        }

        Debug.Assert(EditingRow != null);
        Debug.Assert(_editingColumnIndex >= 0);
        Debug.Assert(_editingColumnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(_editingColumnIndex == CurrentColumnIndex);
        Debug.Assert(EditingRow != null && EditingRow.Slot == CurrentSlot);

        FocusEditingCell(setFocus: ContainsFocus || _focusEditingControl);

        // Prepare the cell for editing and raise the PreparingCellForEdit event for all columns
        DataGridColumn? dataGridColumn = CurrentColumn;
        Debug.Assert(dataGridColumn != null);
        Debug.Assert(_editingEventArgs != null);
        _uneditedValue = dataGridColumn.PrepareCellForEditInternal(editingElement, _editingEventArgs);
        NotifyPreparingCellForEdit(new DataGridPreparingCellForEditEventArgs(dataGridColumn, EditingRow,
            _editingEventArgs,
            editingElement));
    }

    private bool ProcessAKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift, out bool alt);

        if (ctrl && !shift && !alt && SelectionMode == DataGridSelectionMode.Extended)
        {
            SelectAll();
            return true;
        }

        return false;
    }

    //TODO TabStop
    //TODO FlowDirection
    private bool ProcessDataGridKey(KeyEventArgs e)
    {
        bool focusDataGrid = false;
        switch (e.Key)
        {
            case Key.Tab:
                return ProcessTabKey(e);

            case Key.Up:
                focusDataGrid = ProcessUpKey(e);
                break;

            case Key.Down:
                focusDataGrid = ProcessDownKey(e);
                break;

            case Key.PageDown:
                focusDataGrid = ProcessNextKey(e);
                break;

            case Key.PageUp:
                focusDataGrid = ProcessPriorKey(e);
                break;

            case Key.Left:
                focusDataGrid = ProcessLeftKey(e);
                break;

            case Key.Right:
                focusDataGrid = ProcessRightKey(e);
                break;

            case Key.F2:
                return ProcessF2Key(e);

            case Key.Home:
                focusDataGrid = ProcessHomeKey(e);
                break;

            case Key.End:
                focusDataGrid = ProcessEndKey(e);
                break;

            case Key.Enter:
                focusDataGrid = ProcessEnterKey(e);
                break;

            case Key.Escape:
                return ProcessEscapeKey();

            case Key.A:
                return ProcessAKey(e);

            case Key.C:
                return ProcessCopyKey(e.KeyModifiers);

            case Key.Insert:
                return ProcessCopyKey(e.KeyModifiers);
        }

        if (focusDataGrid)
        {
            Focus();
        }

        return focusDataGrid;
    }

    private bool ProcessDownKeyInternal(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn          = ColumnsInternal.FirstVisibleColumn;
        int             firstVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        int             lastSlot                = LastVisibleSlot;
        if (firstVisibleColumnIndex == -1 || lastSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(() => ProcessDownKeyInternal(shift, ctrl)))
        {
            return true;
        }

        int nextSlot = -1;
        if (CurrentSlot != -1)
        {
            nextSlot = GetNextVisibleSlot(CurrentSlot);
            if (nextSlot >= SlotCount)
            {
                nextSlot = -1;
            }
        }

        _noSelectionChangeCount++;
        try
        {
            int                     desiredSlot;
            int                     columnIndex;
            DataGridSelectionAction action;
            if (CurrentColumnIndex == -1)
            {
                desiredSlot = FirstVisibleSlot;
                columnIndex = firstVisibleColumnIndex;
                action      = DataGridSelectionAction.SelectCurrent;
            }
            else if (ctrl)
            {
                if (shift)
                {
                    // Both Ctrl and Shift
                    desiredSlot = lastSlot;
                    columnIndex = CurrentColumnIndex;
                    action = (SelectionMode == DataGridSelectionMode.Extended)
                        ? DataGridSelectionAction.SelectFromAnchorToCurrent
                        : DataGridSelectionAction.SelectCurrent;
                }
                else
                {
                    // Ctrl without Shift
                    desiredSlot = lastSlot;
                    columnIndex = CurrentColumnIndex;
                    action      = DataGridSelectionAction.SelectCurrent;
                }
            }
            else
            {
                if (nextSlot == -1)
                {
                    return true;
                }

                if (shift)
                {
                    // Shift without Ctrl
                    desiredSlot = nextSlot;
                    columnIndex = CurrentColumnIndex;
                    action      = DataGridSelectionAction.SelectFromAnchorToCurrent;
                }
                else
                {
                    // Neither Ctrl nor Shift
                    desiredSlot = nextSlot;
                    columnIndex = CurrentColumnIndex;
                    action      = DataGridSelectionAction.SelectCurrent;
                }
            }

            UpdateSelectionAndCurrency(columnIndex, desiredSlot, action, scrollIntoView: true);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    private bool ProcessEndKey(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn         = ColumnsInternal.LastVisibleColumn;
        int             lastVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        int             firstVisibleSlot       = FirstVisibleSlot;
        int             lastVisibleSlot        = LastVisibleSlot;
        if (lastVisibleColumnIndex == -1 || firstVisibleSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(() => ProcessEndKey(shift, ctrl)))
        {
            return true;
        }

        _noSelectionChangeCount++;
        try
        {
            if (!ctrl)
            {
                return ProcessRightMost(lastVisibleColumnIndex, firstVisibleSlot);
            }
            else
            {
                DataGridSelectionAction action = (shift && SelectionMode == DataGridSelectionMode.Extended)
                    ? DataGridSelectionAction.SelectFromAnchorToCurrent
                    : DataGridSelectionAction.SelectCurrent;

                var targetSlot = IsRangePresentationActive ? SlotCount - 1 : lastVisibleSlot;
                UpdateSelectionAndCurrency(lastVisibleColumnIndex, targetSlot, action, scrollIntoView: true);
            }
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    private bool ProcessEnterKey(bool shift, bool ctrl)
    {
        int oldCurrentSlot = CurrentSlot;

        if (!ctrl)
        {
            // If Enter was used by a TextBox, we shouldn't handle the key
            if (TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement() is Avalonia.Controls.TextBox focusedTextBox
                && focusedTextBox.AcceptsReturn)
            {
                return false;
            }

            if (WaitForLostFocus(() => ProcessEnterKey(shift, ctrl)))
            {
                return true;
            }

            // Enter behaves like down arrow - it commits the potential editing and goes down one cell.
            if (!ProcessDownKeyInternal(false, ctrl))
            {
                return false;
            }
        }
        else if (WaitForLostFocus(() => ProcessEnterKey(shift, ctrl)))
        {
            return true;
        }

        // Try to commit the potential editing
        if (oldCurrentSlot == CurrentSlot &&
            EndCellEdit(DataGridEditAction.Commit, exitEditingMode: true, keepFocus: true, raiseEvents: true) &&
            EditingRow != null)
        {
            EndRowEdit(DataGridEditAction.Commit, exitEditingMode: true, raiseEvents: true);
            ScrollIntoView(CurrentItem, CurrentColumn);
        }

        return true;
    }

    private bool ProcessEscapeKey()
    {
        if (WaitForLostFocus(() => ProcessEscapeKey()))
        {
            return true;
        }

        if (_editingColumnIndex != -1)
        {
            // Revert the potential cell editing and exit cell editing.
            EndCellEdit(DataGridEditAction.Cancel, exitEditingMode: true, keepFocus: true, raiseEvents: true);
            return true;
        }

        if (EditingRow != null)
        {
            // Revert the potential row editing and exit row editing.
            EndRowEdit(DataGridEditAction.Cancel, exitEditingMode: true, raiseEvents: true);
            return true;
        }

        return false;
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

    private bool ProcessHomeKey(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn          = ColumnsInternal.FirstVisibleNonFillerColumn;
        int             firstVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        int             firstVisibleSlot        = FirstVisibleSlot;
        if (firstVisibleColumnIndex == -1 || firstVisibleSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(() => ProcessHomeKey(shift, ctrl)))
        {
            return true;
        }

        _noSelectionChangeCount++;
        try
        {
            if (!ctrl)
            {
                return ProcessLeftMost(firstVisibleColumnIndex, firstVisibleSlot);
            }
            else
            {
                DataGridSelectionAction action = (shift && SelectionMode == DataGridSelectionMode.Extended)
                    ? DataGridSelectionAction.SelectFromAnchorToCurrent
                    : DataGridSelectionAction.SelectCurrent;

                var targetSlot = IsRangePresentationActive ? 0 : firstVisibleSlot;
                UpdateSelectionAndCurrency(firstVisibleColumnIndex, targetSlot, action, scrollIntoView: true);
            }
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    private bool ProcessLeftKey(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn          = ColumnsInternal.FirstVisibleNonFillerColumn;
        int             firstVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        int             firstVisibleSlot        = FirstVisibleSlot;
        if (firstVisibleColumnIndex == -1 || firstVisibleSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(() => ProcessLeftKey(shift, ctrl)))
        {
            return true;
        }

        int previousVisibleColumnIndex = -1;
        if (CurrentColumnIndex != -1)
        {
            dataGridColumn =
                ColumnsInternal.GetPreviousVisibleNonFillerColumn(ColumnsItemsInternal[CurrentColumnIndex]);
            if (dataGridColumn != null)
            {
                previousVisibleColumnIndex = dataGridColumn.Index;
            }
        }

        _noSelectionChangeCount++;
        try
        {
            if (ctrl)
            {
                return ProcessLeftMost(firstVisibleColumnIndex, firstVisibleSlot);
            }
            else
            {
                if (IsRangeGroupSlot(CurrentSlot))
                {
                    CollapseGroup(GetCommittedRangeEntry(CurrentSlot).Group!.Key);
                }
                else if (CurrentColumnIndex == -1)
                {
                    UpdateSelectionAndCurrency(
                        firstVisibleColumnIndex,
                        firstVisibleSlot,
                        DataGridSelectionAction.SelectCurrent,
                        scrollIntoView: true);
                }
                else
                {
                    if (previousVisibleColumnIndex == -1)
                    {
                        return true;
                    }

                    UpdateSelectionAndCurrency(
                        previousVisibleColumnIndex,
                        CurrentSlot,
                        DataGridSelectionAction.None,
                        scrollIntoView: true);
                }
            }
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    // Ctrl Left <==> Home
    private bool ProcessLeftMost(int firstVisibleColumnIndex, int firstVisibleSlot)
    {
        _noSelectionChangeCount++;
        try
        {
            int                     desiredSlot;
            DataGridSelectionAction action;
            if (CurrentColumnIndex == -1)
            {
                desiredSlot = firstVisibleSlot;
                action      = DataGridSelectionAction.SelectCurrent;
                Debug.Assert(Selection.IsEmpty);
            }
            else
            {
                desiredSlot = CurrentSlot;
                action      = DataGridSelectionAction.None;
            }

            UpdateSelectionAndCurrency(firstVisibleColumnIndex, desiredSlot, action, scrollIntoView: true);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    private bool ProcessNextKey(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn          = ColumnsInternal.FirstVisibleNonFillerColumn;
        int             firstVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        if (firstVisibleColumnIndex == -1 || DisplayData.FirstScrollingSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(() => ProcessNextKey(shift, ctrl)))
        {
            return true;
        }

        int nextPageSlot = CurrentSlot == -1 ? DisplayData.FirstScrollingSlot : CurrentSlot;
        Debug.Assert(nextPageSlot != -1);
        int slot = GetNextVisibleSlot(nextPageSlot);

        int scrollCount = DisplayData.NumTotallyDisplayedScrollingElements;
        while (scrollCount > 0 && slot < SlotCount)
        {
            nextPageSlot = slot;
            scrollCount--;
            slot = GetNextVisibleSlot(slot);
        }

        _noSelectionChangeCount++;
        try
        {
            DataGridSelectionAction action;
            int                     columnIndex;
            if (CurrentColumnIndex == -1)
            {
                columnIndex = firstVisibleColumnIndex;
                action      = DataGridSelectionAction.SelectCurrent;
            }
            else
            {
                columnIndex = CurrentColumnIndex;
                action = (shift && SelectionMode == DataGridSelectionMode.Extended)
                    ? action = DataGridSelectionAction.SelectFromAnchorToCurrent
                    : IsRangePresentationActive
                        ? DataGridSelectionAction.None
                        : DataGridSelectionAction.SelectCurrent;
            }

            UpdateSelectionAndCurrency(columnIndex, nextPageSlot, action, scrollIntoView: true);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    private bool ProcessPriorKey(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn          = ColumnsInternal.FirstVisibleNonFillerColumn;
        int             firstVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        if (firstVisibleColumnIndex == -1 || DisplayData.FirstScrollingSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(() => ProcessPriorKey(shift, ctrl)))
        {
            return true;
        }

        int previousPageSlot = (CurrentSlot == -1) ? DisplayData.FirstScrollingSlot : CurrentSlot;
        Debug.Assert(previousPageSlot != -1);

        int scrollCount = DisplayData.NumTotallyDisplayedScrollingElements;
        int slot        = GetPreviousVisibleSlot(previousPageSlot);
        while (scrollCount > 0 && slot != -1)
        {
            previousPageSlot = slot;
            scrollCount--;
            slot = GetPreviousVisibleSlot(slot);
        }

        Debug.Assert(previousPageSlot != -1);

        _noSelectionChangeCount++;
        try
        {
            int                     columnIndex;
            DataGridSelectionAction action;
            if (CurrentColumnIndex == -1)
            {
                columnIndex = firstVisibleColumnIndex;
                action      = DataGridSelectionAction.SelectCurrent;
            }
            else
            {
                columnIndex = CurrentColumnIndex;
                action = (shift && SelectionMode == DataGridSelectionMode.Extended)
                    ? DataGridSelectionAction.SelectFromAnchorToCurrent
                    : IsRangePresentationActive
                        ? DataGridSelectionAction.None
                        : DataGridSelectionAction.SelectCurrent;
            }

            UpdateSelectionAndCurrency(columnIndex, previousPageSlot, action, scrollIntoView: true);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    private bool ProcessRightKey(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn         = ColumnsInternal.LastVisibleColumn;
        int             lastVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        int             firstVisibleSlot       = FirstVisibleSlot;
        if (lastVisibleColumnIndex == -1 || firstVisibleSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(delegate { ProcessRightKey(shift, ctrl); }))
        {
            return true;
        }

        int nextVisibleColumnIndex = -1;
        if (CurrentColumnIndex != -1)
        {
            dataGridColumn = ColumnsInternal.GetNextVisibleColumn(ColumnsItemsInternal[CurrentColumnIndex]);
            if (dataGridColumn != null)
            {
                nextVisibleColumnIndex = dataGridColumn.Index;
            }
        }

        _noSelectionChangeCount++;
        try
        {
            if (ctrl)
            {
                return ProcessRightMost(lastVisibleColumnIndex, firstVisibleSlot);
            }
            else
            {
                if (IsRangeGroupSlot(CurrentSlot))
                {
                    ExpandGroup(GetCommittedRangeEntry(CurrentSlot).Group!.Key);
                }
                else if (CurrentColumnIndex == -1)
                {
                    int firstVisibleColumnIndex = ColumnsInternal.FirstVisibleColumn == null
                        ? -1
                        : ColumnsInternal.FirstVisibleColumn.Index;

                    UpdateSelectionAndCurrency(
                        firstVisibleColumnIndex,
                        firstVisibleSlot,
                        DataGridSelectionAction.SelectCurrent,
                        scrollIntoView: true);
                }
                else
                {
                    if (nextVisibleColumnIndex == -1)
                    {
                        return true;
                    }

                    UpdateSelectionAndCurrency(
                        nextVisibleColumnIndex,
                        CurrentSlot,
                        DataGridSelectionAction.None,
                        scrollIntoView: true);
                }
            }
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    // Ctrl Right <==> End
    private bool ProcessRightMost(int lastVisibleColumnIndex, int firstVisibleSlot)
    {
        _noSelectionChangeCount++;
        try
        {
            int                     desiredSlot;
            DataGridSelectionAction action;
            if (CurrentColumnIndex == -1)
            {
                desiredSlot = firstVisibleSlot;
                action      = DataGridSelectionAction.SelectCurrent;
            }
            else
            {
                desiredSlot = CurrentSlot;
                action      = DataGridSelectionAction.None;
            }

            UpdateSelectionAndCurrency(lastVisibleColumnIndex, desiredSlot, action, scrollIntoView: true);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
    }

    private bool ProcessTabKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessTabKey(e, shift, ctrl);
    }

    private bool ProcessTabKey(KeyEventArgs e, bool shift, bool ctrl)
    {
        if (ctrl || _editingColumnIndex == -1 || IsReadOnly)
        {
            //Go to the next/previous control on the page when
            // - Ctrl key is used
            // - Potential current cell is not edited, or the datagrid is read-only.
            return false;
        }

        // Try to locate a writable cell before/after the current cell
        Debug.Assert(CurrentColumnIndex != -1);
        Debug.Assert(CurrentSlot != -1);

        int             neighborVisibleWritableColumnIndex, neighborSlot;
        DataGridColumn? dataGridColumn;
        if (shift)
        {
            dataGridColumn = ColumnsInternal.GetPreviousVisibleWritableColumn(ColumnsItemsInternal[CurrentColumnIndex]);
            neighborSlot   = GetPreviousVisibleSlot(CurrentSlot);
            if (EditingRow != null)
            {
                while (neighborSlot != -1 && IsRangeGroupSlot(neighborSlot))
                {
                    neighborSlot = GetPreviousVisibleSlot(neighborSlot);
                }
            }
        }
        else
        {
            dataGridColumn = ColumnsInternal.GetNextVisibleWritableColumn(ColumnsItemsInternal[CurrentColumnIndex]);
            neighborSlot   = GetNextVisibleSlot(CurrentSlot);
            if (EditingRow != null)
            {
                while (neighborSlot < SlotCount && IsRangeGroupSlot(neighborSlot))
                {
                    neighborSlot = GetNextVisibleSlot(neighborSlot);
                }
            }
        }

        neighborVisibleWritableColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;

        if (neighborVisibleWritableColumnIndex == -1 && (neighborSlot == -1 || neighborSlot >= SlotCount))
        {
            // There is no previous/next row and no previous/next writable cell on the current row
            return false;
        }

        if (WaitForLostFocus(() => ProcessTabKey(e, shift, ctrl)))
        {
            return true;
        }

        int targetSlot        = -1;
        int targetColumnIndex = -1;

        _noSelectionChangeCount++;
        try
        {
            if (neighborVisibleWritableColumnIndex == -1)
            {
                targetSlot = neighborSlot;
                if (shift)
                {
                    Debug.Assert(ColumnsInternal.LastVisibleWritableColumn != null);
                    targetColumnIndex = ColumnsInternal.LastVisibleWritableColumn.Index;
                }
                else
                {
                    Debug.Assert(ColumnsInternal.FirstVisibleWritableColumn != null);
                    targetColumnIndex = ColumnsInternal.FirstVisibleWritableColumn.Index;
                }
            }
            else
            {
                targetSlot        = CurrentSlot;
                targetColumnIndex = neighborVisibleWritableColumnIndex;
            }

            DataGridSelectionAction action;
            if (targetSlot != CurrentSlot || (SelectionMode == DataGridSelectionMode.Extended))
            {
                if (IsSlotOutOfBounds(targetSlot))
                {
                    return true;
                }

                action = DataGridSelectionAction.SelectCurrent;
            }
            else
            {
                action = DataGridSelectionAction.None;
            }

            UpdateSelectionAndCurrency(targetColumnIndex, targetSlot, action, scrollIntoView: true);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        if (_successfullyUpdatedSelection && !IsRangeGroupSlot(targetSlot))
        {
            BeginCellEdit(e);
        }

        // Return true to say we handled the key event even if the operation was unsuccessful. If we don't
        // say we handled this event, the framework will continue to process the tab key and change focus.
        return true;
    }

    private bool ProcessUpKey(bool shift, bool ctrl)
    {
        DataGridColumn? dataGridColumn          = ColumnsInternal.FirstVisibleNonFillerColumn;
        int             firstVisibleColumnIndex = (dataGridColumn == null) ? -1 : dataGridColumn.Index;
        int             firstVisibleSlot        = FirstVisibleSlot;
        if (firstVisibleColumnIndex == -1 || firstVisibleSlot == -1)
        {
            return false;
        }

        if (WaitForLostFocus(() => ProcessUpKey(shift, ctrl)))
        {
            return true;
        }

        int previousVisibleSlot = (CurrentSlot != -1) ? GetPreviousVisibleSlot(CurrentSlot) : -1;

        _noSelectionChangeCount++;

        try
        {
            int                     slot;
            int                     columnIndex;
            DataGridSelectionAction action;
            if (CurrentColumnIndex == -1)
            {
                slot        = firstVisibleSlot;
                columnIndex = firstVisibleColumnIndex;
                action      = DataGridSelectionAction.SelectCurrent;
            }
            else if (ctrl)
            {
                if (shift)
                {
                    // Both Ctrl and Shift
                    slot        = firstVisibleSlot;
                    columnIndex = CurrentColumnIndex;
                    action = (SelectionMode == DataGridSelectionMode.Extended)
                        ? DataGridSelectionAction.SelectFromAnchorToCurrent
                        : DataGridSelectionAction.SelectCurrent;
                }
                else
                {
                    // Ctrl without Shift
                    slot        = firstVisibleSlot;
                    columnIndex = CurrentColumnIndex;
                    action      = DataGridSelectionAction.SelectCurrent;
                }
            }
            else
            {
                if (previousVisibleSlot == -1)
                {
                    return true;
                }

                if (shift)
                {
                    // Shift without Ctrl
                    slot        = previousVisibleSlot;
                    columnIndex = CurrentColumnIndex;
                    action      = DataGridSelectionAction.SelectFromAnchorToCurrent;
                }
                else
                {
                    // Neither Shift nor Ctrl
                    slot        = previousVisibleSlot;
                    columnIndex = CurrentColumnIndex;
                    action      = DataGridSelectionAction.SelectCurrent;
                }
            }

            UpdateSelectionAndCurrency(columnIndex, slot, action, scrollIntoView: true);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return _successfullyUpdatedSelection;
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

    private bool ResetCurrentCellCore()
    {
        return (CurrentColumnIndex == -1 || SetCurrentCellCore(-1, -1));
    }

    private void ResetEditingRow()
    {
        if (EditingRow != null
            && EditingRow != _focusedRow
            && !IsSlotVisible(EditingRow.Slot))
        {
            // Unload the old editing row if it's off screen
            EditingRow.Clip = null;
            UnloadRow(EditingRow);
            DisplayData.FullyRecycleElements();
        }

        EditingRow = null;
    }

    internal void ResetFocusedRow()
    {
        if (_focusedRow != null
            && _focusedRow != EditingRow
            && !IsSlotVisible(_focusedRow.Slot))
        {
            // Unload the old focused row if it's off screen
            _focusedRow.Clip = null;
            UnloadRow(_focusedRow);
            DisplayData.FullyRecycleElements();
        }

        _focusedRow = null;
    }

    private void SetAndSelectCurrentCell(int columnIndex,
                                         int slot,
                                         bool forceCurrentCellSelection)
    {
        DataGridSelectionAction action = forceCurrentCellSelection
            ? DataGridSelectionAction.SelectCurrent
            : DataGridSelectionAction.None;
        UpdateSelectionAndCurrency(columnIndex, slot, action, scrollIntoView: false);
    }

    // columnIndex = 2, rowIndex = -1 --> current cell belongs to the 'new row'.
    // columnIndex = 2, rowIndex = 2 --> current cell is an inner cell
    // columnIndex = -1, rowIndex = -1 --> current cell is reset
    // columnIndex = -1, rowIndex = 2 --> Unexpected
    private bool SetCurrentCellCore(
        int columnIndex,
        int slot,
        bool commitEdit,
        bool endRowEdit,
        bool updateRangeCurrentKey = true)
    {
        Debug.Assert(columnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(slot < SlotCount);
        Debug.Assert(columnIndex == -1 || ColumnsItemsInternal[columnIndex].IsVisible);
        Debug.Assert(!(columnIndex > -1 && slot == -1));

        if (columnIndex == CurrentColumnIndex &&
            slot == CurrentSlot)
        {
            Debug.Assert(_editingColumnIndex == -1 || _editingColumnIndex == CurrentColumnIndex);
            Debug.Assert(EditingRow == null || EditingRow.Slot == CurrentSlot);
            return true;
        }

        Control?                oldDisplayedElement = null;
        DataGridCellCoordinates oldCurrentCell      = new DataGridCellCoordinates(CurrentCellCoordinates);

        object? newCurrentItem = null;
        if (TryGetCommittedRangeEntry(slot, out var rangeEntry) &&
            rangeEntry.Kind == DataGridSourceEntryKind.Data)
        {
            newCurrentItem = rangeEntry.Item;
        }

        if (CurrentColumnIndex > -1)
        {
            Debug.Assert(CurrentColumnIndex < ColumnsItemsInternal.Count);
            Debug.Assert(CurrentSlot < SlotCount);

            if (!IsInnerCellOutOfBounds(oldCurrentCell.ColumnIndex, oldCurrentCell.Slot) &&
                IsSlotVisible(oldCurrentCell.Slot))
            {
                oldDisplayedElement = DisplayData.GetDisplayedElement(oldCurrentCell.Slot);
            }

            if (!IsRangeGroupSlot(oldCurrentCell.Slot) &&
                !_temporarilyResetCurrentCell)
            {
                bool keepFocus = ContainsFocus;
                if (commitEdit)
                {
                    if (!EndCellEdit(DataGridEditAction.Commit, exitEditingMode: true, keepFocus: keepFocus,
                            raiseEvents: true))
                    {
                        return false;
                    }

                    // Resetting the current cell: setting it to (-1, -1) is not considered setting it out of bounds
                    var isOldCurrentCellOutOfBounds = IsRangePresentationActive
                        ? IsInnerCellOutOfBounds(
                            oldCurrentCell.ColumnIndex,
                            oldCurrentCell.Slot)
                        : IsInnerCellOutOfSelectionBounds(
                            oldCurrentCell.ColumnIndex,
                            oldCurrentCell.Slot);
                    if ((columnIndex != -1 && slot != -1 &&
                         IsInnerCellOutOfSelectionBounds(columnIndex, slot)) ||
                        isOldCurrentCellOutOfBounds)
                    {
                        return false;
                    }

                    if (endRowEdit && !EndRowEdit(DataGridEditAction.Commit, exitEditingMode: true, raiseEvents: true))
                    {
                        return false;
                    }
                }
                else
                {
                    CancelEdit(DataGridEditingUnit.Row, false);
                    ExitEdit(keepFocus);
                }
            }
        }

        if (slot == -1 && columnIndex != -1)
        {
            return false;
        }

        CurrentColumnIndex = columnIndex;
        CurrentSlot        = slot;

        if (updateRangeCurrentKey)
        {
            if (TryGetCommittedRangeEntry(slot, out var currentEntry) &&
                currentEntry.Kind == DataGridSourceEntryKind.Data)
            {
                SetCurrentRowKeyFromCoordinates(
                    currentEntry.RowKey,
                    currentEntry.DataIndex);
            }
            else
            {
                SetCurrentRowKeyFromCoordinates(null, -1);
            }
        }

        if (_temporarilyResetCurrentCell)
        {
            if (columnIndex != -1)
            {
                _temporarilyResetCurrentCell = false;
            }
        }

        if (!_temporarilyResetCurrentCell && _editingColumnIndex != -1)
        {
            _editingColumnIndex = columnIndex;
        }

        if (oldDisplayedElement != null)
        {
            if (oldDisplayedElement is DataGridRow row)
            {
                // Don't reset the state of the current cell if we're editing it because that would put it in an invalid state
                UpdateCurrentState(oldDisplayedElement, oldCurrentCell.ColumnIndex,
                    !(_temporarilyResetCurrentCell && row.IsEditing &&
                      _editingColumnIndex == oldCurrentCell.ColumnIndex));
            }
            else
            {
                UpdateCurrentState(oldDisplayedElement, oldCurrentCell.ColumnIndex, applyCellState: false);
            }
        }

        if (CurrentColumnIndex > -1)
        {
            Debug.Assert(CurrentSlot > -1);
            Debug.Assert(CurrentColumnIndex < ColumnsItemsInternal.Count);
            Debug.Assert(CurrentSlot < SlotCount);
            if (IsSlotVisible(CurrentSlot))
            {
                UpdateCurrentState(DisplayData.GetDisplayedElement(CurrentSlot), CurrentColumnIndex,
                    applyCellState: true);
            }
        }

        return true;
    }

    private void SetVerticalOffset(double newVerticalOffset)
    {
        _verticalOffset = newVerticalOffset;
        if (_vScrollBar != null && !MathUtils.AreClose(newVerticalOffset, _vScrollBar.Value))
        {
            _vScrollBar.Value = _verticalOffset;
        }
    }

    private void UpdateCurrentState(Control displayedElement, int columnIndex, bool applyCellState)
    {
        if (displayedElement is DataGridRow row)
        {
            if (IsRowHeadersVisible)
            {
                row.ApplyHeaderStatus();
            }

            DataGridCell cell = row.Cells[columnIndex];
            if (applyCellState)
            {
                cell.UpdatePseudoClasses();
            }
        }
        else if (displayedElement is DataGridRowGroupHeader groupHeader)
        {
            groupHeader.UpdatePseudoClasses();
            if (IsRowHeadersVisible)
            {
                groupHeader.ApplyHeaderStatus();
            }
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
                    // This will trigger a call to this method via Cells_SizeChanged for
                    _ignoreNextScrollBarsLayout = true;
                    // which no processing is needed.
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
                    // This will trigger a call to this method via Cells_SizeChanged for
                    // which no processing is needed.
                    _hScrollBar.IsVisible       = false;
                    _ignoreNextScrollBarsLayout = true;
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
                }
                else
                {
                    _vScrollBar.Maximum      = 0;
                    _vScrollBar.ViewportSize = 0;
                    _vScrollBar.IsEnabled    = false;
                }

                if (!_vScrollBar.IsVisible)
                {
                    // This will trigger a call to this method via Cells_SizeChanged for
                    // which no processing is needed.
                    _vScrollBar.IsVisible = true;
                    if (_vScrollBar.DesiredSize.Width == 0)
                    {
                        // We need to know the width for the rest of layout to work correctly so measure it now
                        _vScrollBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }

                    _ignoreNextScrollBarsLayout = true;
                }
            }
            else
            {
                _vScrollBar.Maximum = 0;
                if (_vScrollBar.IsVisible)
                {
                    // This will trigger a call to this method via Cells_SizeChanged for
                    // which no processing is needed.
                    _vScrollBar.IsVisible       = false;
                    _ignoreNextScrollBarsLayout = true;
                }
            }
        }
    }

    private void HandleVerticalScrollBarScroll(object? sender, ScrollEventArgs e)
    {
        ProcessVerticalScroll(e.ScrollEventType);
        VerticalScroll?.Invoke(sender, e);
    }

    //TODO: Ensure right button is checked for
    private bool UpdateStateOnMouseRightButtonDown(PointerPressedEventArgs pointerPressedEventArgs, int columnIndex,
                                                   int slot, bool allowEdit, bool shift, bool ctrl)
    {
        Debug.Assert(slot >= 0);

        if (shift || ctrl)
        {
            return true;
        }

        if (IsSlotOutOfBounds(slot))
        {
            return true;
        }

        if (GetRowSelection(slot))
        {
            return true;
        }

        // Unselect everything except the row that was clicked on
        _noSelectionChangeCount++;
        try
        {
            UpdateSelectionAndCurrency(columnIndex, slot, DataGridSelectionAction.SelectCurrent, scrollIntoView: false);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        return true;
    }

    //TODO: Ensure left button is checked for
    private bool UpdateStateOnMouseLeftButtonDown(PointerPressedEventArgs pointerPressedEventArgs, int columnIndex,
                                                  int slot, bool allowEdit, bool shift, bool ctrl)
    {
        bool beginEdit;

        Debug.Assert(slot >= 0);

        // Before changing selection, check if the current cell needs to be committed, and
        // check if the current row needs to be committed. If any of those two operations are required and fail,
        // do not change selection, and do not change current cell.

        bool wasInEdit = EditingColumnIndex != -1;

        if (IsSlotOutOfBounds(slot))
        {
            return true;
        }

        if (wasInEdit && (columnIndex != EditingColumnIndex || slot != CurrentSlot) &&
            WaitForLostFocus(() =>
                UpdateStateOnMouseLeftButtonDown(pointerPressedEventArgs, columnIndex, slot, allowEdit, shift, ctrl)))
        {
            return true;
        }

        try
        {
            _noSelectionChangeCount++;
            beginEdit = allowEdit &&
                        CurrentSlot == slot &&
                        columnIndex != -1 &&
                        (wasInEdit || CurrentColumnIndex == columnIndex) &&
                        !GetColumnEffectiveReadOnlyState(ColumnsItemsInternal[columnIndex]);

            DataGridSelectionAction action = DataGridSelectionAction.None;

            DataGridColumn? column = null;
            if (columnIndex != -1)
            {
                column = ColumnsInternal[columnIndex];
            }
            
            var clickInSelectIndicator = false;
            if (columnIndex != -1)
            {
                if (column is DataGridSelectionColumn selectionColumn)
                {
                    var row = DisplayData.GetDisplayedElement(slot) as DataGridRow;
                    Debug.Assert(row != null);
                    var dataGridCell = row.Cells[columnIndex];
                    clickInSelectIndicator = selectionColumn.NotifyAboutToUpdateSelection(pointerPressedEventArgs, dataGridCell);
                }
            }

            if (SelectTriggerType == DataGridSelectTriggerType.SelectIndicator && clickInSelectIndicator)
            {
                if (columnIndex != -1)
                {
                    if (column is DataGridSelectionColumn selectionColumn)
                    {
                        var row = DisplayData.GetDisplayedElement(slot) as DataGridRow;
                        Debug.Assert(row != null);
                        var dataGridCell = row.Cells[columnIndex];
                        action = selectionColumn.GetSelectionAction(dataGridCell);
                    }
                }
            }
            else
            {
                if (SelectionMode != DataGridSelectionMode.None)
                {
                    if (clickInSelectIndicator)
                    {
                        if (columnIndex != -1)
                        {
                            if (column is DataGridSelectionColumn selectionColumn)
                            {
                                var row = DisplayData.GetDisplayedElement(slot) as DataGridRow;
                                Debug.Assert(row != null);
                                var dataGridCell = row.Cells[columnIndex];
                                action = selectionColumn.GetSelectionAction(dataGridCell);
                            }
                        }
                    }
                    else
                    {
                        if (SelectionMode == DataGridSelectionMode.Extended && shift)
                        {
                            // Shift select multiple rows
                            action = DataGridSelectionAction.SelectFromAnchorToCurrent;
                        }
                        else if (GetRowSelection(slot)) // Unselecting single row or Selecting a previously multi-selected row
                        {
                            if (!ctrl && SelectionMode == DataGridSelectionMode.Extended && !Selection.IsEmpty)
                            {
                                // Unselect everything except the row that was clicked on
                                action = DataGridSelectionAction.SelectCurrent;
                            }
                            else if (ctrl && EditingRow == null)
                            {
                                action = DataGridSelectionAction.RemoveCurrentFromSelection;
                            }
                            else
                            {
                                action = DataGridSelectionAction.None;
                            }
                        }
                        else // Selecting a single row or multi-selecting with Ctrl
                        {
                            if (SelectionMode == DataGridSelectionMode.Single || !ctrl)
                            {
                                // Unselect the currently selected rows except the new selected row
                                action = DataGridSelectionAction.SelectCurrent;
                            }
                            else
                            {
                                action = DataGridSelectionAction.AddCurrentToSelection;
                            }
                        }
                    }
                }
                else
                {
                    action = DataGridSelectionAction.None;
                }
            }
            UpdateSelectionAndCurrency(columnIndex, slot, action, scrollIntoView: false);
        }
        finally
        {
            NoSelectionChangeCount--;
        }

        if (_successfullyUpdatedSelection && beginEdit && BeginCellEdit(pointerPressedEventArgs))
        {
            FocusEditingCell(setFocus: true);
        }

        return true;
    }

    /// <summary>
    /// This method formats a row (specified by a DataGridRowClipboardEventArgs) into
    /// the clipboard text builder when the DataGrid is copying its contents.
    /// </summary>
    /// <param name="text">The destination clipboard text builder.</param>
    /// <param name="e">DataGridRowClipboardEventArgs.</param>
    private static void AppendClipboardContent(StringBuilder text, DataGridRowClipboardEventArgs e)
    {
        var clipboardRowContent = e.ClipboardRowContent;
        var numberOfItem        = clipboardRowContent.Count;
        for (int cellIndex = 0; cellIndex < numberOfItem; cellIndex++)
        {
            AppendEscapedClipboardCellContent(text, clipboardRowContent[cellIndex].Content);
            if (cellIndex < numberOfItem - 1)
            {
                text.Append('\t');
            }
            else
            {
                text.Append('\r');
                text.Append('\n');
            }
        }
    }

    private static void AppendEscapedClipboardCellContent(StringBuilder text, object? content)
    {
        text.Append('"');
        var cellContent = content?.ToString();
        if (!string.IsNullOrEmpty(cellContent))
        {
            var segmentStart = 0;
            for (var charIndex = 0; charIndex < cellContent.Length; charIndex++)
            {
                if (cellContent[charIndex] != '"')
                {
                    continue;
                }

                text.Append(cellContent, segmentStart, charIndex - segmentStart);
                text.Append("\"\"");
                segmentStart = charIndex + 1;
            }

            if (segmentStart < cellContent.Length)
            {
                text.Append(cellContent, segmentStart, cellContent.Length - segmentStart);
            }
        }
        text.Append('"');
    }

    /// <summary>
    /// Handles the case where a 'Copy' key ('C' or 'Insert') has been pressed.  If pressed in combination with
    /// the control key, and the necessary prerequisites are met, the DataGrid will copy its contents
    /// to the Clipboard as text.
    /// </summary>
    /// <returns>Whether or not the DataGrid handled the key press.</returns>
    private bool ProcessCopyKey(KeyModifiers modifiers)
    {
        KeyboardHelper.GetMetaKeyState(this, modifiers, out bool ctrl, out bool shift, out bool alt);

        var selectedItems = GetLoadedSelectionObjects();
        if (ctrl && !shift && !alt && ClipboardCopyMode != DataGridClipboardCopyMode.None && selectedItems.Count > 0)
        {
            var textBuilder = new StringBuilder();

            if (ClipboardCopyMode == DataGridClipboardCopyMode.IncludeHeader)
            {
                DataGridRowClipboardEventArgs headerArgs = new DataGridRowClipboardEventArgs(
                    null,
                    true,
                    ColumnsInternal.VisibleColumnCount);
                int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
                for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
                {
                    DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
                    if (!column.IsVisible)
                    {
                        continue;
                    }

                    headerArgs.ClipboardRowContent.Add(new DataGridClipboardCellContent(null, column, column.Header));
                }

                OnCopyingRowClipboardContent(headerArgs);
                AppendClipboardContent(textBuilder, headerArgs);
            }

            for (int index = 0; index < selectedItems.Count; index++)
            {
                var item = selectedItems[index];
                Debug.Assert(item != null);
                DataGridRowClipboardEventArgs itemArgs = new DataGridRowClipboardEventArgs(
                    item,
                    false,
                    ColumnsInternal.VisibleColumnCount);
                int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
                for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
                {
                    DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
                    if (!column.IsVisible)
                    {
                        continue;
                    }

                    object? content = column.GetCellValue(item, column.ClipboardContentBinding);
                    itemArgs.ClipboardRowContent.Add(new DataGridClipboardCellContent(item, column, content));
                }

                OnCopyingRowClipboardContent(itemArgs);
                AppendClipboardContent(textBuilder, itemArgs);
            }

            string text = textBuilder.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                _ = CopyToClipboardAsync(text);
                return true;
            }
        }

        return false;
    }

    private void HandleSelectionModeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            AnchorSlot = -1;
            _rangeSelectionAnchorDataIndex = -1;
            Selection = Selection.NormalizeForMode(SelectionMode);
        }
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

    private async Task CopyToClipboardAsync(string text)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

            if (clipboard != null && this.IsAttachedToVisualTree())
            {
                await clipboard.SetTextAsync(text);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CopyToClipboardAsync: {ex.Message}");
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

    //TODO Validation UI
    private void ResetValidationStatus()
    {
        // Clear the invalid status of the Cell, Row and DataGrid
        if (EditingRow != null)
        {
            EditingRow.IsValid = true;
            if (EditingRow.Index != -1)
            {
                foreach (DataGridCell cell in EditingRow.Cells)
                {
                    if (!cell.IsValid)
                    {
                        cell.IsValid = true;
                        cell.UpdatePseudoClasses();
                    }
                }

                EditingRow.ApplyState();
            }
        }

        IsValid = true;

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
