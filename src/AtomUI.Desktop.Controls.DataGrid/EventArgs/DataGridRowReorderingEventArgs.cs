using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.RowReordering" /> event, raised when a row drag starts,
/// before any row moves. Cancelling the event cancels the drag.
/// </summary>
public class DataGridRowReorderingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridRowReorderingEventArgs" /> class.
    /// </summary>
    public DataGridRowReorderingEventArgs(DataGridRow dataGridRow)
    {
        Row = dataGridRow;
    }

    /// <summary>
    /// The row being moved.
    /// </summary>
    public DataGridRow Row { get; private set; }
}