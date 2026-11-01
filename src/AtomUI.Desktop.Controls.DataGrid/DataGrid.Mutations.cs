using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
        var ownerReference = new WeakReference<DataGrid>(this);
        return ObserveRangeEditAsync(
            ownerReference,
            request,
            ObserveRangeMutationAsync(
                ownerReference,
                source,
                request.Query,
                request.Snapshot,
                generation.CancellationToken,
                cancellationToken,
                editableSource,
                request,
                static (capability, mutation, token) => capability.CommitAsync(mutation, token)));
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
        var moveContext = _rangeMoveContext;
        return ObserveRangeMutationAsync(
            new WeakReference<DataGrid>(this),
            source,
            request.Query,
            request.Snapshot,
            generation.CancellationToken,
            cancellationToken,
            movableSource,
            request,
            (capability, mutation, token) => ExecuteRangeMoveAsync(moveContext, capability, mutation, token));
    }

    internal bool IsRangeMoveSemanticallyAllowed =>
        Query.Sorts.IsEmpty &&
        Query.Filters.IsEmpty &&
        Query.Groups.IsEmpty;

    private readonly AsyncLocal<DataGridMoveRequest?> _rangeMoveContext = new();

    private bool TryCaptureRangeMutation<TCapability>(
        [NotNullWhen(true)] out IDataGridSource? source,
        [NotNullWhen(true)] out TCapability? capability,
        [NotNullWhen(true)] out DataGridPresentationIndex? presentation,
        [NotNullWhen(true)] out DataGridGeneration? generation)
        where TCapability : class, IDataGridSource
    {
        source = ItemsSource;
        capability = source as TCapability;
        presentation = _rangePresentationIndex;
        generation = _rangeGeneration;
        return _isRangeAttached &&
               IsRangePresentationActive &&
               !IsDataStale &&
               LoadState == DataGridLoadState.Ready &&
               source is not null &&
               capability is not null &&
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
        var moveRequest = request is DataGridMoveRequest move ? move : (DataGridMoveRequest?)null;
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
            return owner.CompleteRangeMutation(source, query, snapshot, result.Snapshot, moveRequest);
        }
        return await Dispatcher.UIThread.InvokeAsync(() =>
            ownerReference.TryGetTarget(out var currentOwner) &&
            currentOwner.CompleteRangeMutation(source, query, snapshot, result.Snapshot, moveRequest));
    }

    private static async ValueTask<DataGridMutationResult> ExecuteRangeMoveAsync(
        AsyncLocal<DataGridMoveRequest?> moveContext,
        IDataGridMovableSource source,
        DataGridMoveRequest request,
        CancellationToken cancellationToken)
    {
        var previous = moveContext.Value;
        moveContext.Value = request;
        try
        {
            // A source may invalidate synchronously or on its async continuation before returning.
            // Execution-local context distinguishes those notifications from unrelated invalidations
            // and from another grid sharing the source, without retaining the grid across the await.
            return await source.MoveAsync(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            moveContext.Value = previous;
        }
    }

    private DataGridViewportAnchor? CaptureRangeMutationAnchor(DataGridMoveRequest? moveRequest)
    {
        if (moveRequest is { } move &&
            _rangePresentationIndex is { SlotCount: > 0 } presentation &&
            presentation.Snapshot.Query == move.Query &&
            presentation.Snapshot.Snapshot == move.Snapshot)
        {
            return presentation.CapturePositionAnchor(_verticalOffset);
        }
        return CaptureFirstCompleteRangeAnchor();
    }

    private static async ValueTask<bool> ObserveRangeEditAsync(
        WeakReference<DataGrid> ownerReference,
        DataGridEditRequest request,
        ValueTask<bool> mutation)
    {
        if (!await mutation.ConfigureAwait(false))
        {
            return false;
        }

        // A source that keeps the snapshot changed values in place; rebuild the edited cells so items
        // without change notification show the committed values.
        if (Dispatcher.UIThread.CheckAccess())
        {
            if (ownerReference.TryGetTarget(out var owner))
            {
                owner.RefreshRangeRowCells(request.RowKey, request.Values);
            }
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ownerReference.TryGetTarget(out var currentOwner))
                {
                    currentOwner.RefreshRangeRowCells(request.RowKey, request.Values);
                }
            });
        }
        return true;
    }

    private void RefreshRangeRowCells(
        DataGridRowKey rowKey,
        ImmutableDictionary<DataGridFieldId, DataGridScalar> values)
    {
        foreach (var element in DisplayData.GetScrollingRows())
        {
            if (element is not DataGridRow { DataContext: not null } row || row.RowKey != rowKey)
            {
                continue;
            }
            foreach (var column in ColumnsItemsInternal)
            {
                var cell = row.Cells[column.Index];
                if (column is DataGridBoundColumn { FieldId: { } fieldId } &&
                    values.ContainsKey(fieldId) &&
                    !ReferenceEquals(cell.Content, column.EditingElement))
                {
                    PopulateCellContent(
                        isCellEdited: false,
                        dataGridColumn: column,
                        dataGridRow: row,
                        dataGridCell: cell);
                }
            }
        }
    }

    private bool CompleteRangeMutation(
        IDataGridSource source,
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        DataGridSnapshotId? resultSnapshot,
        DataGridMoveRequest? moveRequest = null)
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
            presentation.Snapshot.Query != query)
        {
            return false;
        }

        // 数据源返回请求时的 snapshot：当前呈现的顺序与成员仍然有效，重载没有意义
        if (resultSnapshot == snapshot && presentation.Snapshot.Snapshot == snapshot)
        {
            return true;
        }

        // 数据源在变更过程中自行发出失效并已开始重载：该重载会反映本次变更，变更视为成功，不再重复加载
        if (IsDataStale || LoadState == DataGridLoadState.Refreshing)
        {
            return true;
        }

        if (presentation.Snapshot.Snapshot != snapshot || LoadState != DataGridLoadState.Ready)
        {
            return false;
        }

        // 主动移动保持视口位置；其他变更沿用内容锚点，保持原完整行的屏幕位置。
        SetAndRaise(IsDataStaleProperty, ref _isDataStale, true);
        BeginRangeLoad(
            isInvalidation: true,
            CaptureRangeMutationAnchor(moveRequest));
        return true;
    }

    private bool CompleteRangeMove(
        IDataGridSource source,
        DataGridMoveRequest request,
        DataGridMutationResult result) =>
        CompleteRangeMutation(source, request.Query, request.Snapshot, result.Snapshot, request);
}
