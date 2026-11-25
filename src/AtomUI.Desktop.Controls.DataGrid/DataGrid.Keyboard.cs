// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

// 键盘与焦点：按键分派、方向/翻页/Home/End/Tab 导航与焦点进出
public partial class DataGrid
{
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

    internal bool ProcessUpKey(KeyEventArgs e)
    {
        KeyboardHelper.GetMetaKeyState(this, e.KeyModifiers, out bool ctrl, out bool shift);
        return ProcessUpKey(shift, ctrl);
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

        return _successfullyUpdatedSelection;
    }

    // Ctrl Left <==> Home
    private bool ProcessLeftMost(int firstVisibleColumnIndex, int firstVisibleSlot)
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

        return _successfullyUpdatedSelection;
    }

    // Ctrl Right <==> End
    private bool ProcessRightMost(int lastVisibleColumnIndex, int firstVisibleSlot)
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

        return _successfullyUpdatedSelection;
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
}
