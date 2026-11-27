using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    private long _rangeSelectionAnchorDataIndex = -1;
    private RangeNavigationIntent? _pendingRangeNavigation;
    private long _rangeBringIntoViewIntent;

    /// <summary>
    /// Scrolls the row with the given key, and with it the cell of the given column, into view under range
    /// presentation. A row inside the committed range is brought into view before the method returns; any other
    /// row is looked up in the source first, and the returned task completes when that lookup has finished.
    /// </summary>
    /// <param name="rowKey">
    /// The key of the row to bring into view.
    /// </param>
    /// <param name="column">
    /// The column of the cell to bring into view, or <see langword="null" /> to scroll for the row only.
    /// </param>
    /// <param name="cancellationToken">
    /// Passed to the key lookup of a row that is not loaded yet.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a viewport containing the row was requested.
    /// <see langword="false" /> when range presentation is inactive, the source cannot look up a key or no data
    /// generation is active, the row was not found, a newer request replaced this one, the query or the snapshot
    /// changed in the meantime, or the column could not be scrolled into view.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="rowKey" /> is not valid, or <paramref name="column" /> belongs to another DataGrid.
    /// </exception>
    public ValueTask<bool> ScrollIntoViewAsync(
        DataGridRowKey rowKey,
        DataGridColumn? column = null,
        CancellationToken cancellationToken = default)
    {
        if (!rowKey.IsValid)
        {
            throw new ArgumentException("The row key must be valid.", nameof(rowKey));
        }
        if (column is not null && !ReferenceEquals(column.OwningGrid, this))
        {
            throw new ArgumentException("The column must belong to this DataGrid.", nameof(column));
        }
        if (!IsRangePresentationActive ||
            ItemsSource is not { } source ||
            _rangePresentationIndex is not { } presentation)
        {
            return ValueTask.FromResult(false);
        }

        var intent = checked(++_rangeBringIntoViewIntent);
        if (TryFindCommittedRangeSlot(rowKey, out var slot, out _))
        {
            return ValueTask.FromResult(ApplyRangeBringIntoView(
                intent,
                source,
                presentation.Snapshot.Query,
                presentation.Snapshot.Snapshot,
                slot,
                column?.Index ?? -1));
        }
        if (source is not IDataGridKeyLookupSource lookup ||
            _rangeGeneration is not { } generation)
        {
            return ValueTask.FromResult(false);
        }

        var request = new DataGridKeyLookupRequest(
            presentation.Snapshot.Query,
            presentation.Snapshot.Snapshot,
            rowKey);
        return new ValueTask<bool>(ObserveRangeKeyLookupAsync(
            new WeakReference<DataGrid>(this),
            intent,
            source,
            lookup,
            request,
            column?.Index ?? -1,
            generation.CancellationToken,
            cancellationToken));
    }

    private static async Task<bool> ObserveRangeKeyLookupAsync(
        WeakReference<DataGrid> ownerReference,
        long intent,
        IDataGridSource source,
        IDataGridKeyLookupSource lookup,
        DataGridKeyLookupRequest request,
        int columnIndex,
        CancellationToken generationCancellationToken,
        CancellationToken cancellationToken)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            generationCancellationToken,
            cancellationToken);
        var result = await lookup.LookupAsync(request, linkedCancellation.Token)
                                 .ConfigureAwait(false);
        if (result is null || !ownerReference.TryGetTarget(out var owner))
        {
            return false;
        }
        if (Dispatcher.UIThread.CheckAccess())
        {
            return owner.ApplyRangeBringIntoView(
                intent,
                source,
                request.Query,
                request.Snapshot,
                result.Value.DisplayIndex,
                columnIndex);
        }
        return await Dispatcher.UIThread.InvokeAsync(() =>
            ownerReference.TryGetTarget(out var currentOwner) &&
            currentOwner.ApplyRangeBringIntoView(
                intent,
                source,
                request.Query,
                request.Snapshot,
                result.Value.DisplayIndex,
                columnIndex));
    }

    private bool ApplyRangeBringIntoView(
        long intent,
        IDataGridSource source,
        DataGridQuery query,
        DataGridSnapshotId snapshot,
        int slot,
        int columnIndex)
    {
        if (intent != _rangeBringIntoViewIntent ||
            !ReferenceEquals(ItemsSource, source) ||
            _rangePresentationIndex is not { } presentation ||
            presentation.Snapshot.Query != query ||
            presentation.Snapshot.Snapshot != snapshot ||
            slot < 0 ||
            slot >= SlotCount)
        {
            return false;
        }
        if (columnIndex >= 0 && !ScrollColumnIntoView(columnIndex))
        {
            return false;
        }
        RequestRangeViewport(
            slot,
            Math.Min(GetInitialRangeVisibleCount(), SlotCount - slot),
            DisplayData.FirstScrollingSlot < 0
                ? 0
                : Math.Sign(slot - DisplayData.FirstScrollingSlot));
        return true;
    }

    private void SetSelectionState(DataGridSelectionState value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var normalized = value.NormalizeForMode(SelectionMode);
        if (_selection.Equals(normalized))
        {
            return;
        }

        var previous = _selection;
        SetAndRaise(SelectionProperty, ref _selection, normalized);
        ApplyRangeSelectionToRealizedRows();
        SelectionChanged?.Invoke(
            this,
            new DataGridSelectionChangedEventArgs(previous, normalized));
    }

    private void SetCurrentRowKey(DataGridRowKey? value)
    {
        if (value is { IsValid: false })
        {
            throw new ArgumentException("The current row key must be valid.", nameof(value));
        }
        if (_currentRowKey == value)
        {
            return;
        }

        // As in the other current cell change paths, the comparison and the CurrentCellChanged event are
        // deferred to the end of the batch.
        NoCurrentCellChangeCount++;
        try
        {
            SetCurrentRowKeyCore(value);
        }
        finally
        {
            NoCurrentCellChangeCount--;
        }
    }

    private void SetCurrentRowKeyCore(DataGridRowKey? value)
    {
        if (IsRangePresentationActive)
        {
            if (value is { } key &&
                TryFindCommittedRangeSlot(key, out var slot, out _))
            {
                SetAndRaise(CurrentRowKeyProperty, ref _currentRowKey, value);
                var columnIndex = CurrentColumnIndex != -1
                    ? CurrentColumnIndex
                    : ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? -1;
                if (columnIndex != -1 && CurrentSlot != slot)
                {
                    SetCurrentCellCore(
                        columnIndex,
                        slot,
                        commitEdit: true,
                        endRowEdit: true,
                        updateRangeCurrentKey: false);
                }
            }
            else
            {
                if (CurrentSlot != -1)
                {
                    SetCurrentCellCore(
                        -1,
                        -1,
                        commitEdit: true,
                        endRowEdit: true,
                        updateRangeCurrentKey: false);
                }
                SetAndRaise(CurrentRowKeyProperty, ref _currentRowKey, value);
            }
        }
        else
        {
            SetAndRaise(CurrentRowKeyProperty, ref _currentRowKey, value);
        }
        ApplyRangeCurrentStateToRealizedRows();
    }

    private void SetCurrentRowKeyFromCoordinates(DataGridRowKey? value)
    {
        if (_currentRowKey != value)
        {
            SetAndRaise(CurrentRowKeyProperty, ref _currentRowKey, value);
        }
    }

    private DataGridSelectionScope? GetCommittedSelectionScope()
    {
        if (ItemsSource is null || _rangePresentationIndex is null)
        {
            return null;
        }
        var snapshot = _rangePresentationIndex.Snapshot;
        return new DataGridSelectionScope(ItemsSource, snapshot.Query, snapshot.Snapshot);
    }

    private bool TryGetRangeSelectionOperand(
        int slot,
        out DataGridSourceEntry entry,
        out DataGridSelectionScope scope)
    {
        var currentScope = GetCommittedSelectionScope();
        if (currentScope is not null &&
            TryGetCommittedRangeEntry(slot, out entry) &&
            entry.Kind == DataGridSourceEntryKind.Data)
        {
            scope = currentScope;
            return true;
        }
        entry = default;
        scope = null!;
        return false;
    }

    private void ApplyRangeSelectionToRealizedRows()
    {
        if (!IsRangePresentationActive)
        {
            return;
        }
        foreach (var row in GetAllRows())
        {
            if (row.IsRangeBacked)
            {
                row.ApplyState();
            }
        }
    }

    private void ApplyRangeCurrentStateToRealizedRows()
    {
        if (!IsRangePresentationActive)
        {
            return;
        }
        foreach (var row in GetAllRows())
        {
            if (row.IsRangeBacked)
            {
                row.ApplyCellsState();
                row.ApplyHeaderStatus();
            }
        }
    }

    private IReadOnlyList<object> GetLoadedSelectionObjects()
    {
        if (_rangePresentationIndex is null ||
            GetCommittedSelectionScope() is not { } scope ||
            Selection.IsEmpty)
        {
            return Array.Empty<object>();
        }

        var selected = new List<object>();
        foreach (var block in _rangePresentationIndex.Snapshot.Blocks)
        {
            foreach (var entry in block.Entries)
            {
                if (entry.Kind == DataGridSourceEntryKind.Data &&
                    Selection.Contains(entry.RowKey, entry.DataIndex, scope) &&
                    entry.Item is { } item)
                {
                    selected.Add(item);
                }
            }
        }
        return selected;
    }

    private void TransitionSelectionForCommittedSnapshot(
        DataGridPresentationSnapshot? previousSnapshot,
        DataGridPresentationSnapshot nextSnapshot,
        bool isInvalidation)
    {
        if (Selection.IsEmpty || ItemsSource is null)
        {
            return;
        }

        var transition = GetSelectionTransition(
            previousSnapshot?.Query,
            nextSnapshot.Query,
            isInvalidation);
        if (transition != DataGridSelectionTransition.Preserve)
        {
            AnchorSlot = -1;
            _rangeSelectionAnchorDataIndex = -1;
        }
        var nextScope = new DataGridSelectionScope(
            ItemsSource,
            nextSnapshot.Query,
            nextSnapshot.Snapshot);
        Selection = Selection.TransitionTo(nextScope, transition);
    }

    // The current row is owned by CurrentRowKey; CurrentSlot is only its projection in the committed snapshot.
    // After a sort, a filter or an invalidation refresh the same slot may hold another row or be out of range,
    // so the slot is reprojected from the key once the new snapshot is committed. A viewport-only scroll keeps
    // the slot numbers, so the original slot is kept even when the key is not in the committed block.
    private void ReprojectCurrentCellForCommittedSnapshot(
        DataGridPresentationSnapshot? previousSnapshot,
        DataGridPresentationSnapshot nextSnapshot,
        bool isInvalidation)
    {
        var isSameRowSpace = previousSnapshot is not null &&
                             !isInvalidation &&
                             previousSnapshot.DataGeneration == nextSnapshot.DataGeneration &&
                             previousSnapshot.Query == nextSnapshot.Query;
        int? slot = _currentRowKey is { } key ? _rangePresentationIndex?.FindSlot(key) : null;
        if (isSameRowSpace && slot is null)
        {
            return;
        }

        var columnIndex = CurrentColumnIndex != -1
            ? CurrentColumnIndex
            : ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? -1;
        NoCurrentCellChangeCount++;
        try
        {
            if (!isSameRowSpace)
            {
                // Coordinates in the old row space are meaningless (the element was unloaded at commit), so they
                // are invalidated and then reprojected from the key. CurrentCellChanged is raised by the state
                // comparison at the end of the batch.
                CurrentColumnIndex = -1;
                CurrentSlot = -1;
            }
            if (slot is { } currentSlot)
            {
                if (columnIndex != -1)
                {
                    SetCurrentCellCore(
                        columnIndex,
                        currentSlot,
                        commitEdit: false,
                        endRowEdit: false,
                        updateRangeCurrentKey: false);
                }
            }
        }
        finally
        {
            NoCurrentCellChangeCount--;
        }
    }

    private bool IsRangeGroupSlot(int slot) =>
        IsRangePresentationActive &&
        TryGetCommittedRangeEntry(slot, out var entry) &&
        entry.Kind == DataGridSourceEntryKind.GroupHeader;

    private bool TryFindCommittedRangeSlot(
        DataGridRowKey key,
        out int slot,
        out long dataIndex)
    {
        if (_rangePresentationIndex is { } index)
        {
            foreach (var block in index.Snapshot.Blocks)
            {
                for (var offset = 0; offset < block.Entries.Length; offset++)
                {
                    var entry = block.Entries[offset];
                    if (entry.Kind == DataGridSourceEntryKind.Data && entry.RowKey == key)
                    {
                        slot = checked(block.Range.StartIndex + offset);
                        dataIndex = entry.DataIndex;
                        return true;
                    }
                }
            }
        }
        slot = -1;
        dataIndex = -1;
        return false;
    }

    private void ApplyRangeSelectionAndCurrency(
        int columnIndex,
        int slot,
        DataGridSelectionAction action,
        bool scrollIntoView)
    {
        if (!TryGetRangeSelectionOperand(slot, out var entry, out var scope))
        {
            return;
        }
        // A selection or current cell change that acts directly on the loaded rows is the newer user intent, so
        // the pending navigation is dropped; otherwise it would override this operation once its target block
        // has loaded.
        _pendingRangeNavigation = null;

        switch (action)
        {
            case DataGridSelectionAction.AddCurrentToSelection:
                SetRowSelection(slot, isSelected: true, setAnchorSlot: true);
                break;
            case DataGridSelectionAction.RemoveCurrentFromSelection:
                SetRowSelection(slot, isSelected: false, setAnchorSlot: false);
                break;
            case DataGridSelectionAction.SelectFromAnchorToCurrent:
                if (SelectionMode == DataGridSelectionMode.Extended &&
                    _rangeSelectionAnchorDataIndex >= 0)
                {
                    // A Shift selection replaces the whole selection with the interval from the anchor to the
                    // current row; the interval shrinks when stepping back.
                    Selection = DataGridSelectionState.Empty.WithInterval(
                        Math.Min(_rangeSelectionAnchorDataIndex, entry.DataIndex),
                        checked(Math.Max(_rangeSelectionAnchorDataIndex, entry.DataIndex) + 1),
                        scope);
                }
                else
                {
                    ClearRowSelection(slot, setAnchorSlot: true);
                }
                break;
            case DataGridSelectionAction.SelectCurrent:
                ClearRowSelection(slot, setAnchorSlot: true);
                break;
            case DataGridSelectionAction.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown selection action.");
        }

        if (columnIndex == -1)
        {
            columnIndex = CurrentColumnIndex != -1
                ? CurrentColumnIndex
                : ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? -1;
        }
        if (columnIndex == -1 ||
            !SetCurrentCellCore(columnIndex, slot, commitEdit: true, endRowEdit: true))
        {
            return;
        }

        if (scrollIntoView && !IsSlotVisible(slot))
        {
            RequestRangeViewport(
                slot,
                Math.Min(GetInitialRangeVisibleCount(), SlotCount - slot),
                slot.CompareTo(DisplayData.FirstScrollingSlot));
        }
        _successfullyUpdatedSelection = true;
    }

    private bool QueueRangeNavigation(
        int columnIndex,
        int slot,
        DataGridSelectionAction action)
    {
        if (!IsRangePresentationActive || slot < 0 || slot >= SlotCount)
        {
            return false;
        }
        if (columnIndex == -1)
        {
            columnIndex = CurrentColumnIndex != -1
                ? CurrentColumnIndex
                : ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? -1;
        }
        if (columnIndex == -1)
        {
            return false;
        }

        _pendingRangeNavigation = new RangeNavigationIntent(columnIndex, slot, action);
        var firstDisplayed = DisplayData.FirstScrollingSlot;
        RequestRangeViewport(
            slot,
            Math.Min(GetInitialRangeVisibleCount(), SlotCount - slot),
            firstDisplayed < 0 ? 0 : Math.Sign(slot - firstDisplayed));
        _successfullyUpdatedSelection = true;
        return true;
    }

    private void ApplyPendingRangeNavigation()
    {
        if (_pendingRangeNavigation is not { } pending ||
            !TryGetCommittedRangeEntry(pending.Slot, out var entry))
        {
            return;
        }

        _pendingRangeNavigation = null;
        var slot = pending.Slot;
        if (entry.Kind != DataGridSourceEntryKind.Data)
        {
            var viewport = _rangePresentationIndex!.Snapshot.CommittedViewport;
            var endExclusive = checked(viewport.FirstVisibleIndex + viewport.VisibleCount);
            while (++slot < endExclusive)
            {
                if (TryGetCommittedRangeEntry(slot, out entry) &&
                    entry.Kind == DataGridSourceEntryKind.Data)
                {
                    break;
                }
            }
            if (slot >= endExclusive || entry.Kind != DataGridSourceEntryKind.Data)
            {
                return;
            }
        }
        UpdateSelectionAndCurrency(
            pending.ColumnIndex,
            slot,
            pending.Action,
            scrollIntoView: false);
    }

    private readonly record struct RangeNavigationIntent(
        int ColumnIndex,
        int Slot,
        DataGridSelectionAction Action);

    private static DataGridSelectionTransition GetSelectionTransition(
        DataGridQuery? previousQuery,
        DataGridQuery nextQuery,
        bool isInvalidation)
    {
        if (isInvalidation)
        {
            return DataGridSelectionTransition.Invalidation;
        }
        if (previousQuery is null)
        {
            return DataGridSelectionTransition.Preserve;
        }
        if (!DataGridQueryValidation.SequenceEqual(
                previousQuery.Filters,
                nextQuery.Filters))
        {
            return DataGridSelectionTransition.Filter;
        }
        if (!DataGridQueryValidation.SequenceEqual(
                previousQuery.Sorts,
                nextQuery.Sorts) ||
            !DataGridQueryValidation.SequenceEqual(
                previousQuery.Groups,
                nextQuery.Groups))
        {
            return DataGridSelectionTransition.SortOrGroup;
        }
        return DataGridSelectionTransition.Preserve;
    }
}
