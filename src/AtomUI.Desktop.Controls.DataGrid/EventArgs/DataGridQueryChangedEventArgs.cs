namespace AtomUI.Desktop.Controls;

/// <summary>
/// What caused one change of the data grid's query. The grid reports it as
/// <see cref="DataGridQueryChangedEventArgs.Reason" /> of the <see cref="DataGrid.QueryChanged" /> event.
/// </summary>
public enum DataGridQueryChangeReason
{
    /// <summary>
    /// The query was assigned through <see cref="DataGrid.Query" />.
    /// </summary>
    External,
    /// <summary>
    /// The sorts of the query changed through <c>SetSort</c>, <c>ClearSorts</c> or a sort gesture on a column.
    /// </summary>
    SortGesture,
    /// <summary>
    /// The filters of the query changed through a filter gesture on a column.
    /// </summary>
    FilterGesture,
    /// <summary>
    /// The groups of the query changed. The grid does not pass this value in the current implementation:
    /// a query assigned through <see cref="DataGrid.Query" /> with other groups is reported as
    /// <see cref="External" />.
    /// </summary>
    GroupChange,
    /// <summary>
    /// A failed load restored the query that was current before the load.
    /// </summary>
    LoadRollback
}

/// <summary>
/// Carries one change of <see cref="DataGrid.Query" />: the query before and after the change, the revision of the
/// new query and what caused the change. The grid raises it with <see cref="DataGrid.QueryChanged" /> after the new
/// query is committed and projected onto the columns, and before the rows are reloaded.
/// </summary>
public sealed class DataGridQueryChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the arguments of one query change. <paramref name="revision" /> is the revision the grid gave to
    /// <paramref name="newQuery" /> and must be greater than zero.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="oldQuery" /> or <paramref name="newQuery" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="revision" /> is zero or negative.
    /// </exception>
    public DataGridQueryChangedEventArgs(
        DataGridQuery oldQuery,
        DataGridQuery newQuery,
        long revision,
        DataGridQueryChangeReason reason)
    {
        ArgumentNullException.ThrowIfNull(oldQuery);
        ArgumentNullException.ThrowIfNull(newQuery);
        if (revision <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(revision));
        }
        OldQuery = oldQuery;
        NewQuery = newQuery;
        Revision = revision;
        Reason = reason;
    }

    /// <summary>
    /// The query that was current before the change.
    /// </summary>
    public DataGridQuery OldQuery { get; }

    /// <summary>
    /// The query that replaced <see cref="OldQuery" />. It is the grid's current query when the event is raised.
    /// </summary>
    public DataGridQuery NewQuery { get; }

    /// <summary>
    /// Tells one query change from another. The grid increases the value by one for each change it reports, including
    /// a rollback, so a change made inside a handler gets a value greater than the event it is handling.
    /// </summary>
    public long Revision { get; }

    /// <summary>
    /// What caused the change; <see cref="DataGridQueryChangeReason" /> lists the causes the grid reports.
    /// </summary>
    public DataGridQueryChangeReason Reason { get; }
}
