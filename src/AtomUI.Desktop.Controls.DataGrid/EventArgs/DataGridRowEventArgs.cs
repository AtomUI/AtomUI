// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries a row for the <see cref="DataGrid.LoadingRow" />, <see cref="DataGrid.UnloadingRow" /> and
/// <see cref="DataGrid.RowReordered" /> events.
/// </summary>
public class DataGridRowEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridRowEventArgs" /> class.
    /// </summary>
    /// <param name="dataGridRow">
    /// The row that the event occurs for.
    /// </param>
    public DataGridRowEventArgs(DataGridRow dataGridRow)
    {
        Row = dataGridRow;
    }

    /// <summary>
    /// Gets the row that the event occurs for.
    /// </summary>
    public DataGridRow Row { get; private set; }
}
