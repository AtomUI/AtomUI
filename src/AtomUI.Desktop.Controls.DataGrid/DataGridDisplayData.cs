// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class DataGridDisplayData
{
    public int FirstDisplayedScrollingCol { get; set; }
    public int FirstScrollingSlot { get; set; }
    public int LastScrollingSlot { get; set; }
    public int LastTotallyDisplayedScrollingCol { get; set; }
    public int NumDisplayedScrollingElements => _scrollingElements.Count;
    public int NumTotallyDisplayedScrollingElements { get; set; }
    internal double PendingVerticalScrollHeight { get; set; }

    internal int RecycledRowCount => _recyclableRows.Count + _fullyRecycledRows.Count;

    internal int RecycledGroupHeaderCount =>
        _recyclableGroupHeaders.Count + _fullyRecycledGroupHeaders.Count;
    
    private Stack<DataGridRow> _fullyRecycledRows; // list of Rows that have been fully recycled (Collapsed)
    private int _headScrollingElements; // index of the row in _scrollingRows that is the first displayed row
    private DataGrid _owner;
    private Stack<DataGridRow> _recyclableRows; // list of Rows which have not been fully recycled (avoids Measure in several cases)
    private List<Control> _scrollingElements; // circular list of displayed elements
    private Stack<DataGridRowGroupHeader> _fullyRecycledGroupHeaders; // list of GroupHeaders that have been fully recycled (Collapsed)
    private Stack<DataGridRowGroupHeader> _recyclableGroupHeaders; // list of GroupHeaders which have not been fully recycled (avoids Measure in several cases)
    
    public DataGridDisplayData(DataGrid owner)
    {
        _owner                           = owner;
        FirstDisplayedScrollingCol       = -1;
        LastTotallyDisplayedScrollingCol = -1;

        _scrollingElements         = new List<Control>();
        _recyclableRows            = new Stack<DataGridRow>();
        _fullyRecycledRows         = new Stack<DataGridRow>();
        _recyclableGroupHeaders    = new Stack<DataGridRowGroupHeader>();
        _fullyRecycledGroupHeaders = new Stack<DataGridRowGroupHeader>();
        ResetSlotIndexes();
    }
    
    internal void AddRecyclableRow(DataGridRow row)
    {
        Debug.Assert(!_recyclableRows.Contains(row));
        row.DetachFromDataGrid(true);
        row.IsVisible = false;
        if (_owner.IsRangePresentationActive)
        {
            _owner.RowsPresenter?.Children.Remove(row);
        }
        _recyclableRows.Push(row);
    }
    
    internal DataGridRowGroupHeader? GetUsedGroupHeader()
    {
        if (_recyclableGroupHeaders.Count > 0)
        {
            DataGridRowGroupHeader groupHeader = _recyclableGroupHeaders.Pop();
            PrepareRecycledElementForDisplay(groupHeader);
            return groupHeader;
        }
        if (_fullyRecycledGroupHeaders.Count > 0)
        {
            // For fully recycled rows, we need to set the Visibility back to Visible
            DataGridRowGroupHeader groupHeader = _fullyRecycledGroupHeaders.Pop();
            PrepareRecycledElementForDisplay(groupHeader);
            return groupHeader;
        }
        return null;
    }
    
    internal void AddRecyclableRowGroupHeader(DataGridRowGroupHeader groupHeader)
    {
        Debug.Assert(!_recyclableGroupHeaders.Contains(groupHeader));
        if (_owner.IsRangePresentationActive)
        {
            groupHeader.DetachFromDataGrid();
            _owner.RowsPresenter?.Children.Remove(groupHeader);
        }
        else
        {
            groupHeader.IsRecycled = true;
        }
        groupHeader.IsVisible = false;
        _recyclableGroupHeaders.Push(groupHeader);
    }
    
    internal void ClearElements(bool recycle)
    {
        ResetSlotIndexes();
        if (recycle)
        {
            foreach (Control element in _scrollingElements)
            {
                if (element is DataGridRow row)
                {
                    if (row.IsRecyclable)
                    {
                        AddRecyclableRow(row);
                    }
                    else
                    {
                        row.ApplyHiddenClipGeometry();
                    }
                }
                else if (element is DataGridRowGroupHeader groupHeader)
                {
                    AddRecyclableRowGroupHeader(groupHeader);
                }
            }
        }
        else
        {
            // 回收池中的行不再被复用，丢弃前解除其对表格的绑定
            foreach (var row in _recyclableRows.Concat(_fullyRecycledRows))
            {
                row.ReleaseOwningGridBindings();
            }
            _recyclableRows.Clear();
            _fullyRecycledRows.Clear();
            _recyclableGroupHeaders.Clear();
            _fullyRecycledGroupHeaders.Clear();
        }
        _scrollingElements.Clear();
    }
    
    internal void CorrectSlotsAfterDeletion(int slot, bool wasCollapsed)
    {
        if (wasCollapsed)
        {
            if (slot > FirstScrollingSlot)
            {
                LastScrollingSlot--;
            }
        }
        else if (_owner.IsSlotVisible(slot))
        {
            UnloadScrollingElement(slot, true /*updateSlotInformation*/, true /*wasDeleted*/);
        }
        // This cannot be an else condition because if there are 2 rows left, and you delete the first one
        // then these indexes need to be updated as well
        if (slot < FirstScrollingSlot)
        {
            FirstScrollingSlot--;
            LastScrollingSlot--;
        }
    }

    internal void CorrectSlotsAfterInsertion(int slot, Control? element, bool isCollapsed)
    {
        if (slot < FirstScrollingSlot)
        {
            // The row was inserted above our viewport, just update our indexes
            FirstScrollingSlot++;
            LastScrollingSlot++;
        }
        else if (isCollapsed && (slot <= LastScrollingSlot))
        {
            LastScrollingSlot++;
        }
        else if ((_owner.GetPreviousVisibleSlot(slot) <= LastScrollingSlot) || (LastScrollingSlot == -1))
        {
            if (element != null)
            {
                // The row was inserted in our viewport, add it as a scrolling row
                LoadScrollingSlot(slot, element, true /*updateSlotInformation*/);
            }
        }
    }
    
    private int GetCircularListIndex(int slot, bool wrap)
    {
        Debug.Assert(_scrollingElements.Count > 0);

        int index = slot - FirstScrollingSlot - _headScrollingElements - _owner.GetCollapsedSlotCount(FirstScrollingSlot, slot);
        return wrap ? index % _scrollingElements.Count : index;
    }
    
    internal void FullyRecycleElements()
    {
        // Fully recycle Recyclable rows and transfer them to Recycled rows
        while (_recyclableRows.Count > 0)
        {
            DataGridRow row = _recyclableRows.Pop();
            row.IsVisible = false;
            // TODO 拖动的时候滚动翻页了会触发这个，暂时改成动态判断
            // Debug.Assert(!_fullyRecycledRows.Contains(row));
            if (!_fullyRecycledRows.Contains(row))
            {
                _fullyRecycledRows.Push(row);
            }
        }
        // Fully recycle Recyclable GroupHeaders and transfer them to Recycled GroupHeaders
        while (_recyclableGroupHeaders.Count > 0)
        {
            DataGridRowGroupHeader groupHeader = _recyclableGroupHeaders.Pop();
            groupHeader.IsVisible = false;
            // TODO 拖动的时候滚动翻页了会触发这个，暂时改成动态判断
            // Debug.Assert(!_fullyRecycledGroupHeaders.Contains(groupHeader));
            if (!_fullyRecycledGroupHeaders.Contains(groupHeader))
            {
                _fullyRecycledGroupHeaders.Push(groupHeader);
            }
        }
    }
    
    internal Control GetDisplayedElement(int slot)
    {
        Debug.Assert(slot >= FirstScrollingSlot);
        Debug.Assert(slot <= LastScrollingSlot);

        return _scrollingElements[GetCircularListIndex(slot, true /*wrap*/)];
    }

    internal Control GetScrollingElementAtDisplayIndex(int displayIndex)
    {
        Debug.Assert(displayIndex >= 0);
        Debug.Assert(displayIndex < _scrollingElements.Count);

        return _scrollingElements[(_headScrollingElements + displayIndex) % _scrollingElements.Count];
    }
    
    internal DataGridRow? GetDisplayedRow(int rowIndex)
    {
        return GetDisplayedElement(_owner.SlotFromRowIndex(rowIndex)) as DataGridRow;
    }
    
    // Returns an enumeration of the displayed scrolling rows in order starting with the FirstDisplayedScrollingRow
    internal IEnumerable<Control> GetScrollingElements(Predicate<object>? filter = null)
    {
        for (int i = 0; i < _scrollingElements.Count; i++)
        {
            Control element = GetScrollingElementAtDisplayIndex(i);
            if (filter == null || filter(element))
            {
                // _scrollingRows is a circular list that wraps
                yield return element;
            }
        }
    }

    internal IEnumerable<Control> GetScrollingRows()
    {
        return GetScrollingElements(element => element is DataGridRow);
    }

    internal DataGridRow? GetUsedRow()
    {
        if (_recyclableRows.Count > 0)
        {
            DataGridRow row = _recyclableRows.Pop();
            PrepareRecycledElementForDisplay(row);
            return row;
        }
        if (_fullyRecycledRows.Count > 0)
        {
            // For fully recycled rows, we need to set the Visibility back to Visible
            DataGridRow row = _fullyRecycledRows.Pop();
            PrepareRecycledElementForDisplay(row);
            return row;
        }
        return null;
    }

    private static void PrepareRecycledElementForDisplay(Control element)
    {
        element.IsVisible = true;
        element.InvalidateMeasure();
        element.InvalidateArrange();
    }
    
    // Tracks the row at index rowIndex as a scrolling row
    internal void LoadScrollingSlot(int slot, Control element, bool updateSlotInformation)
    {
        if (_scrollingElements.Count == 0)
        {
            SetScrollingSlots(slot);
            _scrollingElements.Add(element);
        }
        else
        {
            // The slot should be adjacent to the other slots being displayed
            Debug.Assert(slot >= _owner.GetPreviousVisibleSlot(FirstScrollingSlot) && slot <= _owner.GetNextVisibleSlot(LastScrollingSlot));
            if (updateSlotInformation)
            {
                if (slot < FirstScrollingSlot)
                {
                    FirstScrollingSlot = slot;
                }
                else
                {
                    LastScrollingSlot = _owner.GetNextVisibleSlot(LastScrollingSlot);
                }
            }
            int insertIndex = GetCircularListIndex(slot, false /*wrap*/);
            if (insertIndex > _scrollingElements.Count)
            {
                // We need to wrap around from the bottom to the top of our circular list; as a result the head of the list moves forward
                insertIndex -= _scrollingElements.Count;
                _headScrollingElements++;
            }
            _scrollingElements.Insert(insertIndex, element);
        }
    }
    
    private void ResetSlotIndexes()
    {
        SetScrollingSlots(-1);
        NumTotallyDisplayedScrollingElements = 0;
        _headScrollingElements               = 0;
    }
    
    private void SetScrollingSlots(int newValue)
    {
        FirstScrollingSlot = newValue;
        LastScrollingSlot  = newValue;
    }
    
    // Stops tracking the element at the given slot as a scrolling element
    internal void UnloadScrollingElement(int slot, bool updateSlotInformation, bool wasDeleted)
    {
        Debug.Assert(_owner.IsSlotVisible(slot));
        int elementIndex = GetCircularListIndex(slot, false /*wrap*/);
        if (elementIndex > _scrollingElements.Count)
        {
            // We need to wrap around from the top to the bottom of our circular list
            elementIndex -= _scrollingElements.Count;
            _headScrollingElements--;
        }
        _scrollingElements.RemoveAt(elementIndex);

        if (updateSlotInformation)
        {
            if (slot == FirstScrollingSlot && !wasDeleted)
            {
                FirstScrollingSlot = _owner.GetNextVisibleSlot(FirstScrollingSlot);
            }
            else
            {
                LastScrollingSlot = _owner.GetPreviousVisibleSlot(LastScrollingSlot);
            }
            if (LastScrollingSlot < FirstScrollingSlot)
            {
                ResetSlotIndexes();
            }
        }
    }

#if DEBUG
    internal void PrintDisplay()
    {
        foreach (Control element in GetScrollingElements())
        {
            if (element is DataGridRow row)
            {
                Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Slot: {0} Row: {1} ", row.Slot, row.Index));
            }
            else if (element is DataGridRowGroupHeader groupHeader)
            {
                Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Slot: {0} GroupHeader: {1}", groupHeader.SourceSlot, groupHeader.SourceGroup?.Key));
            }
        }
    }
#endif
}
