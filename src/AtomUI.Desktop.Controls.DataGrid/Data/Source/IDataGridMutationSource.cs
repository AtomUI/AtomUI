using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

public readonly struct DataGridRowMutationRequest
{
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

    public DataGridQuery Query { get; }

    public DataGridSnapshotId Snapshot { get; }

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

public readonly struct DataGridEditRequest
{
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

    public DataGridQuery Query { get; }

    public DataGridSnapshotId Snapshot { get; }

    public DataGridRowKey RowKey { get; }

    public ImmutableDictionary<DataGridFieldId, DataGridScalar> Values { get; }
}

public readonly struct DataGridAddRequest
{
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

    public DataGridQuery Query { get; }

    public DataGridSnapshotId Snapshot { get; }

    public ImmutableDictionary<DataGridFieldId, DataGridScalar> Values { get; }
}

public readonly struct DataGridMoveRequest
{
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

    public DataGridQuery Query { get; }

    public DataGridSnapshotId Snapshot { get; }

    public DataGridRowKey RowKey { get; }

    public DataGridRowKey? BeforeKey { get; }

    public DataGridRowKey? AfterKey { get; }
}

public readonly struct DataGridKeyLookupRequest
{
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

    public DataGridQuery Query { get; }

    public DataGridSnapshotId Snapshot { get; }

    public DataGridRowKey RowKey { get; }
}

public readonly struct DataGridKeyLookupResult
{
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

    public int DisplayIndex { get; }

    public long DataIndex { get; }
}

public readonly struct DataGridMutationResult
{
    public DataGridMutationResult(DataGridSnapshotId? snapshot)
    {
        if (snapshot is { IsValid: false })
        {
            throw new ArgumentException("The returned snapshot identity must be valid.", nameof(snapshot));
        }
        Snapshot = snapshot;
    }

    public DataGridSnapshotId? Snapshot { get; }
}

public readonly struct DataGridBulkSelectionRequest
{
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

    public string Operation { get; }

    public DataGridQuery Query { get; }

    public DataGridSnapshotId Snapshot { get; }

    public DataGridSelectionState Selection { get; }
}

public interface IDataGridEditableSource : IDataGridSource
{
    ValueTask<DataGridMutationResult> CommitAsync(
        DataGridEditRequest request,
        CancellationToken cancellationToken);

    ValueTask<DataGridMutationResult> CancelAsync(
        DataGridRowMutationRequest request,
        CancellationToken cancellationToken);

    ValueTask<DataGridMutationResult> AddAsync(
        DataGridAddRequest request,
        CancellationToken cancellationToken);

    ValueTask<DataGridMutationResult> DeleteAsync(
        DataGridRowMutationRequest request,
        CancellationToken cancellationToken);
}

public interface IDataGridMovableSource : IDataGridSource
{
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

public interface IDataGridKeyLookupSource : IDataGridSource
{
    ValueTask<DataGridKeyLookupResult?> LookupAsync(
        DataGridKeyLookupRequest request,
        CancellationToken cancellationToken);
}

public interface IDataGridBulkSelectionSource : IDataGridSource
{
    ValueTask<DataGridMutationResult> ExecuteAsync(
        DataGridBulkSelectionRequest request,
        CancellationToken cancellationToken);
}
