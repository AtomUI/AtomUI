namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data for the <see cref="DataGrid.ColumnDraggingOver" /> event, raised while a column is
/// dragged across the column headers, and once with both columns null when the drag-over state is cleared.
/// </summary>
public class DataGridColumnDraggingOverEventArgs : EventArgs
{
    /// <summary>
    /// Gets the column the dragged column is currently over, or null when there is none.
    /// </summary>
    public DataGridColumn? DraggingOverColumn { get; }

    /// <summary>
    /// Gets the column being dragged, or null when the drag-over state is cleared.
    /// </summary>
    public DataGridColumn? DraggedColumn { get; }

    /// <summary>
    /// Initializes an instance for a column drag.
    /// </summary>
    /// <param name="draggedColumn">
    /// The column being dragged, or null when the drag-over state is cleared.
    /// </param>
    /// <param name="draggingOverColumn">
    /// The column the dragged column is over, or null when there is none.
    /// </param>
    public DataGridColumnDraggingOverEventArgs(DataGridColumn? draggedColumn, DataGridColumn? draggingOverColumn)
    {
        DraggedColumn      = draggedColumn;
        DraggingOverColumn = draggingOverColumn;
    }
}