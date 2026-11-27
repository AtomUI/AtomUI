using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    /// <summary>
    /// Commits new values for one row through the source, then repopulates the cells of that row.
    /// </summary>
    /// <param name="rowKey">
    /// The key of the row to edit.
    /// </param>
    /// <param name="values">
    /// The values to commit, keyed by field id.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the commit while the source is still working on it. A change the source has already finished
    /// is not reverted.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the edit counts as applied: the grid already shows the result or a reload
    /// that shows it is running or has been started. <see langword="false" /> when the grid shows no loaded,
    /// up-to-date snapshot of an <see cref="IDataGridEditableSource" />, or when the source, its query or the
    /// presented snapshot was replaced while the commit was running.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="values" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="rowKey" /> or a field id in <paramref name="values" /> is invalid.
    /// </exception>
    /// <remarks>
    /// The grid calls this itself when a row edit is committed. When the source changes the values in place
    /// without changing its snapshot, the cells of the row are rebuilt so that items without change
    /// notification show the committed values.
    /// </remarks>
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

    /// <summary>
    /// Deletes a row through the source and refreshes the grid.
    /// </summary>
    /// <param name="rowKey">
    /// The key of the row to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the deletion while the source is still working on it. A change the source has already
    /// finished is not reverted.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the deletion counts as applied: the grid already shows the result or a
    /// reload that shows it is running or has been started. <see langword="false" /> when the grid shows no
    /// loaded, up-to-date snapshot of an <see cref="IDataGridEditableSource" />, or when the source, its
    /// query or the presented snapshot was replaced while the deletion was running.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="rowKey" /> is invalid.
    /// </exception>
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

    /// <summary>
    /// Adds a row with the given values through the source and refreshes the grid.
    /// </summary>
    /// <param name="values">
    /// The values of the new row, keyed by field id.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the insertion while the source is still working on it. A change the source has already
    /// finished is not reverted.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the insertion counts as applied: the grid already shows the result or a
    /// reload that shows it is running or has been started. <see langword="false" /> when the grid shows no
    /// loaded, up-to-date snapshot of an <see cref="IDataGridEditableSource" />, or when the source, its
    /// query or the presented snapshot was replaced while the insertion was running.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="values" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A field id in <paramref name="values" /> is invalid.
    /// </exception>
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

    /// <summary>
    /// Runs a bulk operation on the rows that are currently selected.
    /// </summary>
    /// <param name="operation">
    /// The identity of the operation. Its meaning is defined by the source.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the operation while the source is still working on it. A change the source has already
    /// finished is not reverted.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the operation counts as applied: the grid already shows the result or a
    /// reload that shows it is running or has been started. <see langword="false" /> when the selection is
    /// empty, when the grid shows no loaded, up-to-date snapshot of an
    /// <see cref="IDataGridBulkSelectionSource" />, or when the source, its query or the presented snapshot
    /// was replaced while the operation was running.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="operation" /> is <see langword="null" />, empty or white space.
    /// </exception>
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

    /// <summary>
    /// Moves a row to a new position in the source and refreshes the grid with the viewport position kept.
    /// </summary>
    /// <param name="rowKey">
    /// The key of the row to move.
    /// </param>
    /// <param name="beforeKey">
    /// The key of the row the moved row is placed in front of. Give exactly one of
    /// <paramref name="beforeKey" /> and <paramref name="afterKey" />.
    /// </param>
    /// <param name="afterKey">
    /// The key of the row the moved row is placed behind.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the move while the source is still working on it. A change the source has already finished
    /// is not reverted.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the move counts as applied: the grid already shows the result or a
    /// reload that shows it is running or has been started. <see langword="false" /> when the grid shows no
    /// loaded, up-to-date snapshot of an <see cref="IDataGridMovableSource" />, when the source reports that
    /// moves are unavailable, when the query sorts, filters or groups, or when the source, its query or the
    /// presented snapshot was replaced while the move was running.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="rowKey" /> or a neighbor key is invalid, both or neither neighbor key is given, or a
    /// neighbor key is <paramref name="rowKey" /> itself.
    /// </exception>
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

        // The source returned the snapshot the request was made against: the presented order and members are
        // still valid, so a reload would change nothing.
        if (resultSnapshot == snapshot && presentation.Snapshot.Snapshot == snapshot)
        {
            return true;
        }

        // The source invalidated itself during the mutation and a reload is already running. That reload
        // reflects the mutation, so it counts as applied and is not loaded again.
        if (IsDataStale || LoadState == DataGridLoadState.Refreshing)
        {
            return true;
        }

        if (presentation.Snapshot.Snapshot != snapshot || LoadState != DataGridLoadState.Ready)
        {
            return false;
        }

        // An explicit move keeps the viewport position. Every other mutation uses the content anchor, which
        // keeps the complete rows at their screen position.
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
