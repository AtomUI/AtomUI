// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

// 当前单元格：当前格设置与变更通知、点击/右键时的选择与当前格更新
public partial class DataGrid
{
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

    // Convenient overload that commits the current edit.
    internal bool SetCurrentCellCore(int columnIndex, int slot)
    {
        return SetCurrentCellCore(columnIndex, slot, commitEdit: true, endRowEdit: true);
    }

    internal bool UpdateSelectionAndCurrency(int columnIndex, int slot, DataGridSelectionAction action,
                                             bool scrollIntoView)
    {
        _successfullyUpdatedSelection = false;

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

    private bool ResetCurrentCellCore()
    {
        return (CurrentColumnIndex == -1 || SetCurrentCellCore(-1, -1));
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
                SetCurrentRowKeyFromCoordinates(currentEntry.RowKey);
            }
            else
            {
                SetCurrentRowKeyFromCoordinates(null);
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
        UpdateSelectionAndCurrency(columnIndex, slot, DataGridSelectionAction.SelectCurrent, scrollIntoView: false);

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

        if (_successfullyUpdatedSelection && beginEdit && BeginCellEdit(pointerPressedEventArgs))
        {
            FocusEditingCell(setFocus: true);
        }

        return true;
    }
}
