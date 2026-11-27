// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.CellPointerPressed" /> event: the pressed cell with its row
/// and column, and the pointer event that pressed it.
/// </summary>
public class DataGridCellPointerPressedEventArgs : EventArgs
{
    /// <summary>
    /// Instantiates a new instance of this class.
    /// </summary>
    /// <param name="cell">
    /// The cell that has been pressed.
    /// </param>
    /// <param name="row">
    /// The row container of the cell that has been pressed.
    /// </param>
    /// <param name="column">
    /// The column of the cell that has been pressed.
    /// </param>
    /// <param name="e">
    /// The pointer action that has been taken.
    /// </param>
    public DataGridCellPointerPressedEventArgs(DataGridCell cell, 
                                               DataGridRow row,
                                               DataGridColumn column,
                                               PointerPressedEventArgs e)
    {
        Cell                    = cell;
        Row                     = row;
        Column                  = column;
        PointerPressedEventArgs = e;
    }

    /// <summary>
    /// The cell that has been pressed.
    /// </summary> 
    public DataGridCell Cell { get; }

    /// <summary>
    /// The row container of the cell that has been pressed.
    /// </summary> 
    public DataGridRow Row { get; }

    /// <summary>
    /// The column of the cell that has been pressed.
    /// </summary> 
    public DataGridColumn Column { get; }

    /// <summary>
    /// The pointer action that has been taken.
    /// </summary> 
    public PointerPressedEventArgs PointerPressedEventArgs { get; }
}