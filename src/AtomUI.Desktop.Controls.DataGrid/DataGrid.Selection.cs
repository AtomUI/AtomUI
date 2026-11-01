using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    private long _rangeSelectionAnchorDataIndex = -1;
    private RangeNavigationIntent? _pendingRangeNavigation;
    private long _rangeBringIntoViewIntent;

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

        // 与其他当前格变更路径一致，在批次结束时统一比较并触发 CurrentCellChanged
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

    // 当前行由 CurrentRowKey 拥有，CurrentSlot 只是它在已提交快照中的投影。
    // 排序、过滤、失效刷新后同一槽位可能对应另一行或已越界，因此提交新快照后按 key 重新投影；
    // 纯视口滚动时槽位编号不变，key 不在已提交块内也保留原槽位。
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
                // 旧行空间的坐标已无意义（元素已在提交时卸载），直接作废后再按 key 投影；
                // CurrentCellChanged 由批次结束时的状态比较统一触发。
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
        // 直接作用于已加载行的选择/当前格变更是更新的用户意图，挂起的导航随之作废，
        // 否则其目标块加载完成后会覆盖这次操作
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
                    // Shift 选择以锚点到当前行的区间替换整个选区，回退时区间随之收缩。
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
