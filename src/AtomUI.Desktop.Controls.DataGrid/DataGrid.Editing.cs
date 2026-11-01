// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

// 编辑：单元格/行的进入、提交、取消与编辑元素焦点（当前范围数据源下 BeginEdit 返回 false）
public partial class DataGrid
{
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

    private void HandleEditingElementInitialized(object? sender, EventArgs e)
    {
        if (sender is Control element)
        {
            element.Initialized -= HandleEditingElementInitialized;
            PreparingCellForEditPrivate(element);
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
}
