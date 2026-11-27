namespace AtomUI.Desktop.Controls;

/// <summary>
/// Lets a <see cref="DataGrid" /> load rows by range instead of reading a whole collection into memory.
/// </summary>
public interface IDataGridSource
{
    /// <summary>
    /// The item type, the fields a query may name and the range sizes that requests and results have to follow.
    /// </summary>
    DataGridSourceSchema Schema { get; }

    /// <summary>
    /// Returns the rows and group headers of the range of <paramref name="request" />, in the order produced by
    /// its query, page request and group expansion. The task completes when the whole range is available, or
    /// with <see cref="OperationCanceledException" /> when <paramref name="cancellationToken" /> is canceled.
    /// </summary>
    /// <returns>
    /// The entries of the range from its start index, with the totals and their snapshot.
    /// </returns>
    /// <exception cref="DataGridSourceContractException">
    /// The request does not follow <see cref="Schema" />, for example a range longer than its maximum range size.
    /// </exception>
    /// <exception cref="DataGridSnapshotExpiredException">
    /// The request names an expected snapshot that this source no longer serves.
    /// </exception>
    ValueTask<DataGridRangeResult> FetchAsync(
        DataGridFetchRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Raised after the data changed under the snapshot the grid is using, so that ranges loaded earlier can no
    /// longer be trusted. The grid then cancels its row edit and its pending navigation and loads the viewport
    /// again. It may be raised on any thread; <c>sender</c> is the source.
    /// </summary>
    event EventHandler? Invalidated;
}
