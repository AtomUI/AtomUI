// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.ColumnReordering" /> event, raised when a column drag
/// starts, before any column moves. Cancelling the event cancels the drag.
/// </summary>
public class DataGridColumnReorderingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridColumnReorderingEventArgs" /> class.
    /// </summary>
    public DataGridColumnReorderingEventArgs(DataGridColumn dataGridColumn)
    {
        Column = dataGridColumn;
    }

    /// <summary>
    /// The column being moved.
    /// </summary>
    public DataGridColumn Column { get; private set; }

    /// <summary>
    /// Gets or sets the popup indicator displayed while the column is dragged. The default is a preview copy of
    /// the column header; a handler can replace it with its own control.
    /// </summary>
    public Control? DragIndicator { get; set; }
}