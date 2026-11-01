// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Immutable;
using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

// 编辑：单元格/行的进入、提交、取消与编辑元素焦点；编辑器只读取绑定值，提交统一经 IDataGridEditableSource 写回
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

    internal bool IsEditingWholeRow => _isEditingWholeRow;

    /// <summary>
    /// Enters whole-row editing for the operation column: every editable cell of the row gets its editor.
    /// </summary>
    internal bool BeginWholeRowEdit(DataGridRow dataGridRow)
    {
        if (_isEditingWholeRow && EditingRow == dataGridRow)
        {
            return true;
        }
        if (dataGridRow.OwningGrid != this || !dataGridRow.RowKey.IsValid || dataGridRow.Slot < 0)
        {
            return false;
        }

        var firstEditableColumn = ColumnsItemsInternal.FirstOrDefault(column =>
            column.IsVisible && !GetColumnEffectiveReadOnlyState(column));
        if (firstEditableColumn is null)
        {
            return false;
        }

        // Moving currency to the row commits the edit in progress, exactly as a pointer press would.
        if (CurrentSlot != dataGridRow.Slot)
        {
            UpdateSelectionAndCurrency(
                firstEditableColumn.Index,
                dataGridRow.Slot,
                DataGridSelectionAction.SelectCurrent,
                scrollIntoView: false);
            if (!_successfullyUpdatedSelection || CurrentSlot != dataGridRow.Slot)
            {
                return false;
            }
        }
        else if (!CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
        {
            return false;
        }

        if (EditingRow == null && !BeginRowEdit(dataGridRow))
        {
            return false;
        }

        _isEditingWholeRow = true;
        foreach (var column in ColumnsItemsInternal)
        {
            if (column.IsVisible && !GetColumnEffectiveReadOnlyState(column))
            {
                PopulateCellContent(
                    isCellEdited: true,
                    dataGridColumn: column,
                    dataGridRow: dataGridRow,
                    dataGridCell: dataGridRow.Cells[column.Index]);
            }
        }
        dataGridRow.ApplyState();
        return true;
    }

    /// <summary>
    /// Saves or discards the whole-row edit started by <see cref="BeginWholeRowEdit"/>.
    /// </summary>
    internal bool EndWholeRowEdit(DataGridRow dataGridRow, DataGridEditAction editAction)
    {
        if (!_isEditingWholeRow || EditingRow != dataGridRow)
        {
            return false;
        }
        return editAction == DataGridEditAction.Commit
            ? CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true)
            : CancelEdit(DataGridEditingUnit.Row);
    }

    private bool IsColumnSourceEditable(DataGridColumn column) =>
        ItemsSource is IDataGridEditableSource source &&
        column is DataGridBoundColumn { CanEditThroughSource: true } &&
        column.FieldId is { IsValid: true } fieldId &&
        source.Schema.TryGetField(fieldId, out var field) &&
        field.IsEditable;

    private bool BeginCellEdit(RoutedEventArgs editingEventArgs)
    {
        // Whole-row editing already shows every editor; cells are not entered one by one.
        if (CurrentColumnIndex == -1 || _isEditingWholeRow)
        {
            return false;
        }

        Debug.Assert(CurrentColumn != null);
        Debug.Assert(CurrentColumnIndex >= 0);
        Debug.Assert(CurrentColumnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(CurrentSlot >= -1);
        Debug.Assert(CurrentSlot < SlotCount);
        Debug.Assert(EditingRow == null || EditingRow.Slot == CurrentSlot);
        Debug.Assert(CurrentColumn.IsVisible);

        if (GetColumnEffectiveReadOnlyState(CurrentColumn) || IsRangeGroupSlot(CurrentSlot))
        {
            return false;
        }

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

    private bool BeginRowEdit(DataGridRow dataGridRow)
    {
        // Edited values stay in the editors until committed by row key, so no item buffer is opened.
        if (!dataGridRow.RowKey.IsValid)
        {
            return false;
        }

        EditingRow = dataGridRow;
        if (IsSlotVisible(dataGridRow.Slot))
        {
            dataGridRow.ApplyState();
        }
        return true;
    }

    private bool CancelRowEdit(bool exitEditingMode)
    {
        // Editors never write to the data item, so discarding them is the whole cancellation.
        return true;
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

    private bool CommitRowEdit(bool exitEditingMode)
    {
        // Cell edits commit as each cell ends; only whole-row editing still holds values here.
        if (EditingRow is not { } editingRow || !_isEditingWholeRow)
        {
            return true;
        }

        Debug.Assert(editingRow.Index >= -1);
        Debug.Assert(editingRow.Slot < SlotCount);

        var values = ImmutableDictionary.CreateBuilder<DataGridFieldId, DataGridScalar>();
        var isValid = true;
        foreach (var column in ColumnsItemsInternal)
        {
            var cell = editingRow.Cells[column.Index];
            if (column.EditingElement is { } editingElement &&
                ReferenceEquals(cell.Content, editingElement) &&
                !TryReadEditedCell(editingRow, cell, editingElement, values))
            {
                isValid = false;
            }
        }
        if (!isValid)
        {
            return false;
        }

        if (values.Count > 0)
        {
            CommitEditedValues(editingRow.RowKey, values.ToImmutable());
        }
        return true;
    }

    /// <summary>
    /// Converts one editor value to the scalar of its field, marking the cell invalid when it cannot be written.
    /// Unchanged values are left out so a no-op edit does not reach the source.
    /// </summary>
    private bool TryReadEditedCell(
        DataGridRow editingRow,
        DataGridCell editingCell,
        Control editingElement,
        ImmutableDictionary<DataGridFieldId, DataGridScalar>.Builder values)
    {
        if (editingCell.OwningColumn is not DataGridBoundColumn { FieldId: { IsValid: true } fieldId } column ||
            ItemsSource is not { } source ||
            !source.Schema.TryGetField(fieldId, out var field))
        {
            return true;
        }

        if (column.TryGetEditedValue(editingElement, field.ValueType, out var value, out var error))
        {
            if (DataGridScalar.TryFromValue(value, out var scalar))
            {
                DataValidationErrors.ClearErrors(editingElement);
                if (!IsCurrentFieldValue(field, editingRow.DataContext, scalar))
                {
                    values[fieldId] = scalar;
                }
                return true;
            }
            error = new InvalidCastException(
                $"Values of type '{field.ValueType.Name}' cannot be committed as a data source scalar.");
        }

        DataValidationErrors.SetError(editingElement, error);
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
        return false;
    }

    private static bool IsCurrentFieldValue(DataGridFieldSchema field, object? item, DataGridScalar value) =>
        field.DisplayAccessor is { } accessor &&
        item is not null &&
        DataGridScalar.TryFromValue(accessor.GetValue(item), out var current) &&
        current == value;

    /// <summary>
    /// Commits edited values after the current input has been handled, so a source that reloads
    /// synchronously never rebuilds rows while currency or editing state is still changing.
    /// Failures are reported through <see cref="EditCommitFailed"/> because the editors have already closed.
    /// </summary>
    private void CommitEditedValues(
        DataGridRowKey rowKey,
        ImmutableDictionary<DataGridFieldId, DataGridScalar> values)
    {
        var ownerReference = new WeakReference<DataGrid>(this);
        Dispatcher.UIThread.Post(() => CommitEditedValuesAsync(ownerReference, rowKey, values));
    }

    private static async void CommitEditedValuesAsync(
        WeakReference<DataGrid> ownerReference,
        DataGridRowKey rowKey,
        ImmutableDictionary<DataGridFieldId, DataGridScalar> values)
    {
        if (!ownerReference.TryGetTarget(out var owner))
        {
            return;
        }

        Exception? failure = null;
        try
        {
            if (!await owner.EditRowAsync(rowKey, values))
            {
                failure = new InvalidOperationException(
                    "The edit was not committed because the data source changed or is not ready.");
            }
        }
        catch (OperationCanceledException)
        {
            // The presentation generation ended; the reload that replaced it shows the source state.
        }
        catch (Exception exception)
        {
            failure = exception;
        }

        if (failure is not null && ownerReference.TryGetTarget(out owner))
        {
            owner.NotifyEditCommitFailed(new DataGridEditCommitFailedEventArgs(rowKey, values, failure));
        }
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

        // If we're committing, convert the editor value first; an invalid value keeps the cell in edit
        var committedValues = ImmutableDictionary.CreateBuilder<DataGridFieldId, DataGridScalar>();
        if (editAction == DataGridEditAction.Commit &&
            !TryReadEditedCell(editingRow, editingCell, editingElement, committedValues))
        {
            ScrollSlotIntoView(CurrentColumnIndex, CurrentSlot, forCurrentCellChange: false,
                forceHorizontalScroll: true);
            return false;
        }

        ResetValidationStatus();
        Debug.Assert(CurrentColumn != null);
        if (exitEditingMode)
        {
            DataValidationErrors.ClearErrors(editingElement);
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

        if (committedValues.Count > 0)
        {
            CommitEditedValues(editingRow.RowKey, committedValues.ToImmutable());
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
            RestoreWholeRowCells(editingRow);
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

    /// <summary>
    /// Puts display elements back into the cells that whole-row editing filled with editors.
    /// </summary>
    private void RestoreWholeRowCells(DataGridRow editingRow)
    {
        if (!_isEditingWholeRow)
        {
            return;
        }

        foreach (var column in ColumnsItemsInternal)
        {
            var cell = editingRow.Cells[column.Index];
            if (column.EditingElement is { } editingElement && ReferenceEquals(cell.Content, editingElement))
            {
                DataValidationErrors.ClearErrors(editingElement);
                PopulateCellContent(
                    isCellEdited: false,
                    dataGridColumn: column,
                    dataGridRow: editingRow,
                    dataGridCell: cell);
            }
        }
    }

    private void ResetEditingRow()
    {
        var editingRow = EditingRow;
        if (editingRow != null
            && editingRow != _focusedRow
            && !IsSlotVisible(editingRow.Slot))
        {
            // Unload the old editing row if it's off screen
            editingRow.Clip = null;
            UnloadRow(editingRow);
            DisplayData.FullyRecycleElements();
        }

        _isEditingWholeRow = false;
        EditingRow = null;
        if (editingRow is { OwningGrid: not null } && IsSlotVisible(editingRow.Slot))
        {
            // Leave the editing visual state (pseudo-class, operation buttons) now that the row is no longer edited.
            editingRow.ApplyState();
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
}
