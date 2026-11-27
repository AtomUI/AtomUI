using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A request for one row of the data the grid presented when the request was created. It names the row whose edit
/// <see cref="IDataGridEditableSource.CancelAsync" /> ends and the row that
/// <see cref="IDataGridEditableSource.DeleteAsync" /> removes.
/// </summary>
public readonly struct DataGridRowMutationRequest
{
    /// <summary>
    /// Creates a request for the row with the key <paramref name="rowKey" />.
    /// </summary>
    /// <param name="query">
    /// The query the grid presented when the request was created.
    /// </param>
    /// <param name="snapshot">
    /// The snapshot of the data the grid presented when the request was created.
    /// </param>
    /// <param name="rowKey">
    /// The key of the row the request applies to.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> or <paramref name="rowKey" /> is not valid.
    /// </exception>
    public DataGridRowMutationRequest(
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        DataGridRowKey rowKey)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateSnapshot(snapshot);
        ValidateRowKey(rowKey);
        Query = query;
        Snapshot = snapshot;
        RowKey = rowKey;
    }

    /// <summary>
    /// Gets the query the grid presented when the request was created: the sorts, filters and group levels that
    /// the source applied to the rows.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// Gets the identity of the snapshot the grid presented when the request was created. A source rejects a
    /// request whose snapshot is no longer current, so pass the snapshot the grid shows and not an older one.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }

    /// <summary>
    /// Gets the key of the row the request applies to.
    /// </summary>
    public DataGridRowKey RowKey { get; }

    internal static void ValidateSnapshot(DataGridSnapshotId snapshot)
    {
        if (!snapshot.IsValid)
        {
            throw new ArgumentException("The snapshot identity must be valid.", nameof(snapshot));
        }
    }

    internal static void ValidateRowKey(DataGridRowKey rowKey)
    {
        if (!rowKey.IsValid)
        {
            throw new ArgumentException("The row key must be valid.", nameof(rowKey));
        }
    }
}

/// <summary>
/// A request for an <see cref="IDataGridEditableSource" /> to write a set of field values into one row of the
/// snapshot the grid presented when the request was created.
/// </summary>
public readonly struct DataGridEditRequest
{
    /// <summary>
    /// Creates an edit request for the row with the key <paramref name="rowKey" />.
    /// </summary>
    /// <param name="query">
    /// The query the grid presented when the request was created.
    /// </param>
    /// <param name="snapshot">
    /// The snapshot of the data the grid presented when the request was created.
    /// </param>
    /// <param name="rowKey">
    /// The key of the row to edit.
    /// </param>
    /// <param name="values">
    /// The field values to write, keyed by field id. <see langword="null" /> creates a request without values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> or <paramref name="rowKey" /> is not valid, or a field id in
    /// <paramref name="values" /> is not valid.
    /// </exception>
    public DataGridEditRequest(
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        DataGridRowKey rowKey,
        ImmutableDictionary<DataGridFieldId, DataGridScalar>? values = null)
    {
        var target = new DataGridRowMutationRequest(query, snapshot, rowKey);
        Query = target.Query;
        Snapshot = target.Snapshot;
        RowKey = target.RowKey;
        Values = values ?? ImmutableDictionary<DataGridFieldId, DataGridScalar>.Empty;
        foreach (var field in Values.Keys)
        {
            if (!field.IsValid)
            {
                throw new ArgumentException("Every edited field identity must be valid.", nameof(values));
            }
        }
    }

    /// <summary>
    /// Gets the query the grid presented when the request was created: the sorts, filters and group levels that
    /// the source applied to the rows. A source compares the edited fields with this query to decide whether the
    /// edit changes the order or the membership of the presented rows.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// Gets the identity of the snapshot the grid presented when the request was created. A source rejects a
    /// request whose snapshot is no longer current, so pass the snapshot the grid shows and not an older one.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }

    /// <summary>
    /// Gets the key of the row to edit.
    /// </summary>
    public DataGridRowKey RowKey { get; }

    /// <summary>
    /// Gets the field values to write, keyed by field id. Fields that are absent keep their value.
    /// The dictionary is never <see langword="null" /> and is empty when the request was created without values.
    /// </summary>
    public ImmutableDictionary<DataGridFieldId, DataGridScalar> Values { get; }
}

/// <summary>
/// A request for an <see cref="IDataGridEditableSource" /> to add one row with the given field values.
/// </summary>
public readonly struct DataGridAddRequest
{
    /// <summary>
    /// Creates an add request.
    /// </summary>
    /// <param name="query">
    /// The query the grid presented when the request was created.
    /// </param>
    /// <param name="snapshot">
    /// The snapshot of the data the grid presented when the request was created.
    /// </param>
    /// <param name="values">
    /// The values of the new row, keyed by field id. <see langword="null" /> creates a request without values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> or a field id in <paramref name="values" /> is not valid.
    /// </exception>
    public DataGridAddRequest(
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        ImmutableDictionary<DataGridFieldId, DataGridScalar>? values = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        DataGridRowMutationRequest.ValidateSnapshot(snapshot);
        Query = query;
        Snapshot = snapshot;
        Values = values ?? ImmutableDictionary<DataGridFieldId, DataGridScalar>.Empty;
        foreach (var field in Values.Keys)
        {
            if (!field.IsValid)
            {
                throw new ArgumentException("Every added field identity must be valid.", nameof(values));
            }
        }
    }

    /// <summary>
    /// Gets the query the grid presented when the request was created: the sorts, filters and group levels that
    /// the source applied to the rows.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// Gets the identity of the snapshot the grid presented when the request was created. A source rejects a
    /// request whose snapshot is no longer current, so pass the snapshot the grid shows and not an older one.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }

    /// <summary>
    /// Gets the values of the new row, keyed by field id. The dictionary is never <see langword="null" /> and is
    /// empty when the request was created without values.
    /// </summary>
    public ImmutableDictionary<DataGridFieldId, DataGridScalar> Values { get; }
}

/// <summary>
/// A request for an <see cref="IDataGridMovableSource" /> to move one row of the data the grid presented when
/// the request was created to the position directly before or directly after a neighbor row.
/// </summary>
public readonly struct DataGridMoveRequest
{
    /// <summary>
    /// Creates a move request. Exactly one of <paramref name="beforeKey" /> and <paramref name="afterKey" /> must
    /// be set, and neither may be the moved row.
    /// </summary>
    /// <param name="query">
    /// The query the grid presented when the request was created.
    /// </param>
    /// <param name="snapshot">
    /// The snapshot of the data the grid presented when the request was created.
    /// </param>
    /// <param name="rowKey">
    /// The key of the row to move.
    /// </param>
    /// <param name="beforeKey">
    /// The row the moved row is placed directly before, or <see langword="null" /> when the move is relative to
    /// <paramref name="afterKey" />.
    /// </param>
    /// <param name="afterKey">
    /// The row the moved row is placed directly after, or <see langword="null" /> when the move is relative to
    /// <paramref name="beforeKey" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> or <paramref name="rowKey" /> is not valid; a neighbor key is set but not
    /// valid; both or neither neighbor key is set; or a neighbor key is the moved row.
    /// </exception>
    public DataGridMoveRequest(
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        DataGridRowKey rowKey,
        DataGridRowKey? beforeKey,
        DataGridRowKey? afterKey)
    {
        var target = new DataGridRowMutationRequest(query, snapshot, rowKey);
        if (beforeKey is { IsValid: false } || afterKey is { IsValid: false })
        {
            throw new ArgumentException("Neighbor row keys must be valid.");
        }
        if (beforeKey.HasValue == afterKey.HasValue)
        {
            throw new ArgumentException("A move must specify exactly one before or after key.");
        }
        if (beforeKey == rowKey || afterKey == rowKey)
        {
            throw new ArgumentException("A row cannot be moved relative to itself.");
        }
        Query = target.Query;
        Snapshot = target.Snapshot;
        RowKey = target.RowKey;
        BeforeKey = beforeKey;
        AfterKey = afterKey;
    }

    /// <summary>
    /// Gets the query the grid presented when the request was created: the sorts, filters and group levels that
    /// the source applied to the rows. A source may refuse the move while this query has a sort, filter or
    /// group, because the presented order then does not follow the order of the source.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// Gets the identity of the snapshot the grid presented when the request was created. A source rejects a
    /// request whose snapshot is no longer current, so pass the snapshot the grid shows and not an older one.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }

    /// <summary>
    /// Gets the key of the row to move.
    /// </summary>
    public DataGridRowKey RowKey { get; }

    /// <summary>
    /// Gets the row the moved row is placed directly before, or <see langword="null" /> when the move is relative
    /// to <see cref="AfterKey" />.
    /// </summary>
    public DataGridRowKey? BeforeKey { get; }

    /// <summary>
    /// Gets the row the moved row is placed directly after, or <see langword="null" /> when the move is relative
    /// to <see cref="BeforeKey" />.
    /// </summary>
    public DataGridRowKey? AfterKey { get; }
}

/// <summary>
/// A request for an <see cref="IDataGridKeyLookupSource" /> to locate one row of the data the grid presented
/// when the request was created.
/// </summary>
public readonly struct DataGridKeyLookupRequest
{
    /// <summary>
    /// Creates a lookup request for the row with the key <paramref name="rowKey" />.
    /// </summary>
    /// <param name="query">
    /// The query the grid presented when the request was created.
    /// </param>
    /// <param name="snapshot">
    /// The snapshot of the data the grid presented when the request was created.
    /// </param>
    /// <param name="rowKey">
    /// The key of the row to locate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> or <paramref name="rowKey" /> is not valid.
    /// </exception>
    public DataGridKeyLookupRequest(
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        DataGridRowKey rowKey)
    {
        var target = new DataGridRowMutationRequest(query, snapshot, rowKey);
        Query = target.Query;
        Snapshot = target.Snapshot;
        RowKey = target.RowKey;
    }

    /// <summary>
    /// Gets the query the grid presented when the request was created: the sorts, filters and group levels that
    /// the source applied to the rows.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// Gets the identity of the snapshot the grid presented when the request was created. A source rejects a
    /// request whose snapshot is no longer current, so pass the snapshot the grid shows and not an older one.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }

    /// <summary>
    /// Gets the key of the row to locate.
    /// </summary>
    public DataGridRowKey RowKey { get; }
}

/// <summary>
/// The position of a row that an <see cref="IDataGridKeyLookupSource" /> found for a
/// <see cref="DataGridKeyLookupRequest" />.
/// </summary>
public readonly struct DataGridKeyLookupResult
{
    /// <summary>
    /// Creates a lookup result. Both indexes must be 0 or greater.
    /// </summary>
    /// <param name="displayIndex">
    /// The slot of the row among the entries the grid presents for the request.
    /// </param>
    /// <param name="dataIndex">
    /// The data index of the row in the data the source presents for the query of the request.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="displayIndex" /> or <paramref name="dataIndex" /> is negative.
    /// </exception>
    public DataGridKeyLookupResult(int displayIndex, long dataIndex)
    {
        if (displayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayIndex));
        }
        if (dataIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataIndex));
        }
        DisplayIndex = displayIndex;
        DataIndex = dataIndex;
    }

    /// <summary>
    /// Gets the slot of the row: its position among the entries the grid presents for the request, in which a
    /// group header takes a slot as well. The grid brings this slot into view.
    /// </summary>
    public int DisplayIndex { get; }

    /// <summary>
    /// Gets the data index of the row: its position in the rows the source presents for the query of the request.
    /// Unlike <see cref="DisplayIndex" /> it does not depend on the range the grid has loaded.
    /// </summary>
    public long DataIndex { get; }
}

/// <summary>
/// Reports the snapshot that a source holds after it executed a mutation for the grid.
/// </summary>
public readonly struct DataGridMutationResult
{
    /// <summary>
    /// Creates a result that reports the snapshot the source holds after the mutation.
    /// </summary>
    /// <param name="snapshot">
    /// The snapshot the source holds after the mutation, or <see langword="null" /> to report none.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> is set but not valid.
    /// </exception>
    public DataGridMutationResult(DataGridSnapshotId? snapshot)
    {
        if (snapshot is { IsValid: false })
        {
            throw new ArgumentException("The returned snapshot identity must be valid.", nameof(snapshot));
        }
        Snapshot = snapshot;
    }

    /// <summary>
    /// Gets the snapshot the source holds after the mutation. When it is the snapshot of the request and the grid
    /// still shows it, the grid keeps the rows it shows. When it is another snapshot or
    /// <see langword="null" />, the grid reloads the source, unless it already shows that snapshot or a reload is
    /// already running.
    /// </summary>
    public DataGridSnapshotId? Snapshot { get; }
}

/// <summary>
/// A request for an <see cref="IDataGridBulkSelectionSource" /> to run one bulk operation on the rows the grid
/// presented and had selected when the request was created.
/// </summary>
public readonly struct DataGridBulkSelectionRequest
{
    /// <summary>
    /// Creates a bulk selection request.
    /// </summary>
    /// <param name="operation">
    /// The identity of the bulk operation. Its meaning is defined by the source.
    /// </param>
    /// <param name="query">
    /// The query the grid presented when the request was created.
    /// </param>
    /// <param name="snapshot">
    /// The snapshot of the data the grid presented when the request was created.
    /// </param>
    /// <param name="selection">
    /// The selected rows the operation applies to.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="operation" /> is <see langword="null" />, empty or white space, or
    /// <paramref name="snapshot" /> is not valid.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> or <paramref name="selection" /> is <see langword="null" />.
    /// </exception>
    public DataGridBulkSelectionRequest(
        string operation,
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        DataGridSelectionState selection)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new ArgumentException("The bulk operation identity cannot be empty.", nameof(operation));
        }
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(selection);
        DataGridRowMutationRequest.ValidateSnapshot(snapshot);
        Operation = operation;
        Query = query;
        Snapshot = snapshot;
        Selection = selection;
    }

    /// <summary>
    /// Gets the identity of the bulk operation to run. Its meaning is defined by the source; the grid passes it on
    /// unchanged. It is never <see langword="null" />, empty or white space.
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Gets the query the grid presented when the request was created: the sorts, filters and group levels that
    /// the source applied to the rows.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// Gets the identity of the snapshot the grid presented when the request was created. A source rejects a
    /// request whose snapshot is no longer current, so pass the snapshot the grid shows and not an older one.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }

    /// <summary>
    /// Gets the selected rows the operation applies to, as the grid selected them when the request was created.
    /// It is never empty when the grid creates the request.
    /// </summary>
    public DataGridSelectionState Selection { get; }
}

/// <summary>
/// A source that the grid writes changes back to: it commits or cancels the edit of a row, and it adds or removes
/// a row. Every request carries the query and the snapshot the grid presented when the request was created.
/// </summary>
public interface IDataGridEditableSource : IDataGridSource
{
    /// <summary>
    /// Writes the field values of the request into the row the request names.
    /// </summary>
    /// <param name="request">
    /// The row to edit and the field values to write, with the query and the snapshot the grid presented when the
    /// request was created.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the commit while the source is still working on it: the caller canceled, or the grid stopped
    /// presenting the source, the query or the snapshot of the request. A change the source has already
    /// finished is not reverted.
    /// </param>
    /// <returns>
    /// The snapshot the source holds after the commit. The task completes when the values are committed or the
    /// source rejected the request.
    /// </returns>
    ValueTask<DataGridMutationResult> CommitAsync(
        DataGridEditRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Discards the edit of the row the request names without writing its values. A source that applies an
    /// edit only in <see cref="CommitAsync" /> has nothing to discard.
    /// </summary>
    /// <param name="request">
    /// The row whose edit is discarded, with the query and the snapshot the grid presented when the request was
    /// created.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the request while the source is still working on it.
    /// </param>
    /// <returns>
    /// The snapshot the source holds afterwards. The task completes when the edit is discarded.
    /// </returns>
    ValueTask<DataGridMutationResult> CancelAsync(
        DataGridRowMutationRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a row with the field values of the request.
    /// </summary>
    /// <param name="request">
    /// The values of the new row, with the query and the snapshot the grid presented when the request was created.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the insertion while the source is still working on it. A change the source has already finished is
    /// not reverted.
    /// </param>
    /// <returns>
    /// The snapshot the source holds after the insertion. The task completes when the row is added.
    /// </returns>
    ValueTask<DataGridMutationResult> AddAsync(
        DataGridAddRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes the row the request names from the source.
    /// </summary>
    /// <param name="request">
    /// The row to remove, with the query and the snapshot the grid presented when the request was created.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the deletion while the source is still working on it. A change the source has already finished is
    /// not reverted.
    /// </param>
    /// <returns>
    /// The snapshot the source holds after the deletion. The task completes when the row is removed.
    /// </returns>
    ValueTask<DataGridMutationResult> DeleteAsync(
        DataGridRowMutationRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// A source that changes the order of its own rows when the grid asks it to move one of them.
/// </summary>
public interface IDataGridMovableSource : IDataGridSource
{
    /// <summary>
    /// Moves the row the request names to the position directly before
    /// <see cref="DataGridMoveRequest.BeforeKey" /> or directly after
    /// <see cref="DataGridMoveRequest.AfterKey" /> of the request.
    /// </summary>
    /// <param name="request">
    /// The row to move and the row it is placed next to, with the query and the snapshot the grid presented when
    /// the request was created.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the move while the source is still working on it. A change the source has already finished is not
    /// reverted.
    /// </param>
    /// <returns>
    /// The snapshot the source holds after the move. The task completes when the row is moved.
    /// </returns>
    ValueTask<DataGridMutationResult> MoveAsync(
        DataGridMoveRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Lets a movable source report that moves are currently unavailable, so the grid does not start a row drag.
/// </summary>
internal interface IDataGridMoveAvailability
{
    bool CanMove { get; }
}

/// <summary>
/// A source that finds the position of a row by its key for the grid, which asks for it when it brings a row
/// into view that is not among the rows it has loaded.
/// </summary>
public interface IDataGridKeyLookupSource : IDataGridSource
{
    /// <summary>
    /// Finds the row the request names in the data the grid presented.
    /// </summary>
    /// <param name="request">
    /// The row to locate, with the query and the snapshot the grid presented when the request was created.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the lookup while the source is still working on it.
    /// </param>
    /// <returns>
    /// The position of the row, or <see langword="null" /> when the source cannot report one; the grid then
    /// leaves the view as it is.
    /// </returns>
    ValueTask<DataGridKeyLookupResult?> LookupAsync(
        DataGridKeyLookupRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// A source that runs one bulk operation on the selected rows of the grid in a single request.
/// </summary>
public interface IDataGridBulkSelectionSource : IDataGridSource
{
    /// <summary>
    /// Runs the operation the request names on the rows the request selects.
    /// </summary>
    /// <param name="request">
    /// The operation, the selected rows, the query and the snapshot the grid presented when the request was
    /// created.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the operation while the source is still working on it. A change the source has already finished
    /// is not reverted.
    /// </param>
    /// <returns>
    /// The snapshot the source holds after the operation. The task completes when the operation has finished.
    /// </returns>
    ValueTask<DataGridMutationResult> ExecuteAsync(
        DataGridBulkSelectionRequest request,
        CancellationToken cancellationToken);
}
