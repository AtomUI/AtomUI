// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries a cell for the notifications that the internal cell collection raises when a cell is added to it
/// or removed from it.
/// </summary>
public class DataGridCellEventArgs : EventArgs
{
    internal DataGridCellEventArgs(DataGridCell dataGridCell)
    {
        Debug.Assert(dataGridCell != null);
        Cell = dataGridCell;
    }

    internal DataGridCell Cell
    {
        get;
        private set;
    }
}