namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the filter values requested for a column, for the notification that the column's filter indicator
/// sends to the column header.
/// </summary>
public class DataGridColumnFilterEventArgs : EventArgs
{
    /// <summary>
    /// Gets the values to filter the column by.
    /// </summary>
    public List<object> FilterValues { get; }

    /// <summary>
    /// Gets the column the filter request applies to.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Initializes an instance for a filter request on a column.
    /// </summary>
    /// <param name="column">
    /// The column the filter request applies to.
    /// </param>
    /// <param name="filterValues">
    /// The values to filter the column by.
    /// </param>
    public DataGridColumnFilterEventArgs(DataGridColumn column, List<object> filterValues)
    {
        Column       = column;
        FilterValues = filterValues;
    }
}
