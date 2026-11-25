// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Diagnostics;

namespace AtomUI.Desktop.Controls;

internal class DataGridCellCollection
{
    public int Count => _cells.Count;
    private List<DataGridCell> _cells;
    private DataGridRow _owningRow;
    
    internal event EventHandler<DataGridCellEventArgs>? CellAdded;
    internal event EventHandler<DataGridCellEventArgs>? CellRemoved;

    public DataGridCellCollection(DataGridRow owningRow)
    {
        _owningRow = owningRow;
        _cells     = new List<DataGridCell>();
    }
    
    public IEnumerator GetEnumerator()
    {
        return _cells.GetEnumerator();
    }
    
    public void Insert(int cellIndex, DataGridCell cell)
    {
        Debug.Assert(cellIndex >= 0 && cellIndex <= _cells.Count);

        cell.OwningRow = _owningRow;
        cell.BindOwningRowProperties(_owningRow);
        _cells.Insert(cellIndex, cell);

        CellAdded?.Invoke(this, new DataGridCellEventArgs(cell));
    }
    
    public void RemoveAt(int cellIndex)
    {
        DataGridCell dataGridCell = _cells[cellIndex];
        _cells.RemoveAt(cellIndex);
        dataGridCell.ReleaseOwningRowBinding();
        dataGridCell.OwningRow = null;
        CellRemoved?.Invoke(this, new DataGridCellEventArgs(dataGridCell));
    }
    
    public DataGridCell this[int index]
    {
        get
        {
            if (index < 0 || index >= _cells.Count)
            {
                throw DataGridError.DataGrid.ValueMustBeBetween("index", "Index", 0, true, _cells.Count, false);
            }
            return _cells[index];
        }
    }
}