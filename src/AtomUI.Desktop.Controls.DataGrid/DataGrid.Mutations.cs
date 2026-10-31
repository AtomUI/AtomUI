using System.Collections.Immutable;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    public ValueTask<bool> EditRowAsync(
        DataGridRowKey rowKey,
        ImmutableDictionary<DataGridFieldId, DataGridScalar> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (!TryCaptureRangeMutation<IDataGridEditableSource>(
                out var source,
                out var editableSource,
                out var presentation,
                out var generation))
        {
            return ValueTask.FromResult(false);
        }

        var request = new DataGridEditRequest(
            presentation.Snapshot.Query,
            presentation.Snapshot.Snapshot,
            rowKey,
            values);
        return ObserveRangeMutationAsync(
            new WeakReference<DataGrid>(this),
            source,
            request.Query,
            request.Snapshot,
            generation.CancellationToken,
            cancellationToken,
            editableSource,
            request,
            static (capability, mutation, token) => capability.CommitAsync(mutation, token));
    }

    public ValueTask<bool> DeleteRowAsync(
        DataGridRowKey rowKey,
        CancellationToken cancellationToken = default)
    {
        if (!TryCaptureRangeMutation<IDataGridEditableSource>(
                out var source,
                out var editableSource,
                out var presentation,
                out var generation))
        {
            return ValueTask.FromResult(false);
        }

        var request = new DataGridRowMutationRequest(
            presentation.Snapshot.Query,
            presentation.Snapshot.Snapshot,
            rowKey);
        return ObserveRangeMutationAsync(
            new WeakReference<DataGrid>(this),
            source,
            request.Query,
            request.Snapshot,
            generation.CancellationToken,
            cancellationToken,
            editableSource,
            request,
            static (capability, mutation, token) => capability.DeleteAsync(mutation, token));
    }

    public ValueTask<bool> AddRowAsync(
        ImmutableDictionary<DataGridFieldId, DataGridScalar> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (!TryCaptureRangeMutation<IDataGridEditableSource>(
                out var source,
                out var editableSource,
                out var presentation,
                out var generation))
        {
            return ValueTask.FromResult(false);
        }

        var request = new DataGridAddRequest(
            presentation.Snapshot.Query,
            presentation.Snapshot.Snapshot,
            values);
        return ObserveRangeMutationAsync(
            new WeakReference<DataGrid>(this),
            source,
            request.Query,
            request.Snapshot,
            generation.CancellationToken,
            cancellationToken,
            editableSource,
            request,
            static (capability, mutation, token) => capability.AddAsync(mutation, token));
    }

    public ValueTask<bool> ExecuteBulkSelectionAsync(
        string operation,
        CancellationToken cancellationToken = default)
    {
        if (!TryCaptureRangeMutation<IDataGridBulkSelectionSource>(
                out var source,
                out var bulkSource,
                out var presentation,
                out var generation) ||
            Selection.IsEmpty)
        {
            return ValueTask.FromResult(false);
        }

        var request = new DataGridBulkSelectionRequest(
            operation,
            presentation.Snapshot.Query,
            presentation.Snapshot.Snapshot,
            Selection);
        return ObserveRangeMutationAsync(
            new WeakReference<DataGrid>(this),
            source,
            request.Query,
            request.Snapshot,
            generation.CancellationToken,
            cancellationToken,
            bulkSource,
            request,
            static (capability, mutation, token) => capability.ExecuteAsync(mutation, token));
    }

    public ValueTask<bool> MoveRowAsync(
        DataGridRowKey rowKey,
        DataGridRowKey? beforeKey,
        DataGridRowKey? afterKey,
        CancellationToken cancellationToken = default)
    {
        if (!TryCaptureRangeMutation<IDataGridMovableSource>(
                out var source,
                out var movableSource,
                out var presentation,
                out var generation) ||
            movableSource is IDataGridMoveAvailability { CanMove: false } ||
            !IsRangeMoveSemanticallyAllowed)
        {
            return ValueTask.FromResult(false);
        }

        var request = new DataGridMoveRequest(
            presentation.Snapshot.Query,
            presentation.Snapshot.Snapshot,
            rowKey,
            beforeKey,
            afterKey);
        return ObserveRangeMutationAsync(
            new WeakReference<DataGrid>(this),
            source,
            request.Query,
            request.Snapshot,
            generation.CancellationToken,
            cancellationToken,
            movableSource,
            request,
            static (capability, mutation, token) => capability.MoveAsync(mutation, token));
    }

    internal bool IsRangeMoveSemanticallyAllowed =>
        Query.Sorts.IsEmpty &&
        Query.Filters.IsEmpty &&
        Query.Groups.IsEmpty;

    private bool TryCaptureRangeMutation<TCapability>(
        out IDataGridSource source,
        out TCapability capability,
        out DataGridPresentationIndex presentation,
        out DataGridGeneration generation)
        where TCapability : class, IDataGridSource
    {
        source = ItemsSource!;
        capability = ItemsSource as TCapability ?? null!;
        presentation = _rangePresentationIndex!;
        generation = _rangeGeneration!;
        return _isRangeAttached &&
               IsRangePresentationActive &&
               !IsDataStale &&
               LoadState == DataGridLoadState.Ready &&
               source is TCapability &&
               presentation is not null &&
               generation is not null &&
               ReferenceEquals(generation.Source, source) &&
               presentation.Snapshot.Query == generation.Query;
    }

    private static async ValueTask<bool> ObserveRangeMutationAsync<TCapability, TRequest>(
        WeakReference<DataGrid> ownerReference,
        IDataGridSource source,
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        CancellationToken generationCancellationToken,
        CancellationToken callerCancellationToken,
        TCapability capability,
        TRequest request,
        Func<TCapability, TRequest, CancellationToken, ValueTask<DataGridMutationResult>> execute)
        where TCapability : IDataGridSource
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            generationCancellationToken,
            callerCancellationToken);
        DataGridMutationResult result;
        try
        {
            result = await execute(capability, request, linkedCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (
            generationCancellationToken.IsCancellationRequested &&
            !callerCancellationToken.IsCancellationRequested)
        {
            return false;
        }
        if (!ownerReference.TryGetTarget(out var owner))
        {
            return false;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            return owner.CompleteRangeMutation(source, query, snapshot, result.Snapshot);
        }
        return await Dispatcher.UIThread.InvokeAsync(() =>
            ownerReference.TryGetTarget(out var currentOwner) &&
            currentOwner.CompleteRangeMutation(source, query, snapshot, result.Snapshot));
    }

    private bool CompleteRangeMutation(
        IDataGridSource source,
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        DataGridSnapshotId? resultSnapshot)
    {
        // A source that invalidates itself while mutating (for example an observable local collection)
        // may already have been reloaded to the snapshot the mutation produced.
        if (resultSnapshot is { } mutatedSnapshot &&
            mutatedSnapshot != snapshot &&
            ReferenceEquals(ItemsSource, source) &&
            _rangePresentationIndex is { } current &&
            current.Snapshot.Query == query &&
            current.Snapshot.Snapshot == mutatedSnapshot)
        {
            return true;
        }

        if (!ReferenceEquals(ItemsSource, source) ||
            _rangePresentationIndex is not { } presentation ||
            presentation.Snapshot.Query != query ||
            presentation.Snapshot.Snapshot != snapshot ||
            IsDataStale ||
            LoadState != DataGridLoadState.Ready)
        {
            return false;
        }

        if (!IsDataStale && LoadState != DataGridLoadState.Refreshing)
        {
            SetAndRaise(IsDataStaleProperty, ref _isDataStale, true);
            BeginRangeLoad(isInvalidation: true);
        }
        return true;
    }

    private bool CompleteRangeMove(
        IDataGridSource source,
        DataGridMoveRequest request,
        DataGridMutationResult result) =>
        CompleteRangeMutation(source, request.Query, request.Snapshot, result.Snapshot);
}
