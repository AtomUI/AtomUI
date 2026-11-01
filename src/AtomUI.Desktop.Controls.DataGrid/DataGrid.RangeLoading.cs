using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    private const int RangeCacheCapacity = 16;
    private DataGridRangeCoordinator? _rangeCoordinator;
    private DataGridGeneration? _rangeGeneration;
    private bool _isRangeAttached;
    private bool _rangeSourceEventsWired;

    private void SetItemsSource(IDataGridSource? value)
    {
        if (ReferenceEquals(_source, value))
        {
            return;
        }
        ValidateRangeSourceScope(value, Query, PageRequest, GroupExpansion);
        if (EditingRow is not null && !CommitEdit())
        {
            return;
        }

        CancelRowReorder();
        UnwireRangeSource(_source);
        _rangeGeneration = null;
        _rangeCoordinator?.CancelAndClear();
        _rangeDetailsVisibility.Clear();
        _rangeMeasuredDetailsHeights.Clear();
        _rangeDetailsSlotHints.Clear();
        _pendingRangeNavigation = null;
        _rangeBringIntoViewIntent = checked(_rangeBringIntoViewIntent + 1);
        ClearRangePresentation(clearRows: true);
        SetAndRaise(ItemsSourceProperty, ref _source, value);
        RefreshColumnSortCapabilities();
        SetSelectionState(DataGridSelectionState.Empty);
        SetAndRaise(CurrentRowKeyProperty, ref _currentRowKey, null);
        _rangeSelectionAnchorDataIndex = -1;
        ResetAppliedRangeState();

        if (!_isRangeAttached || value is null)
        {
            return;
        }
        try
        {
            WireRangeSource(value);
            BeginRangeLoad(isInvalidation: false);
        }
        catch (Exception exception)
        {
            SetRangeError(exception);
        }
    }

    private void AttachRangeSource()
    {
        if (_isRangeAttached)
        {
            return;
        }
        _isRangeAttached = true;
        _rangeCoordinator = new DataGridRangeCoordinator(RangeCacheCapacity);
        if (ItemsSource is null)
        {
            return;
        }
        try
        {
            WireRangeSource(ItemsSource);
            BeginRangeLoad(isInvalidation: false);
        }
        catch (Exception exception)
        {
            SetRangeError(exception);
        }
    }

    private void DetachRangeSource()
    {
        if (!_isRangeAttached)
        {
            return;
        }
        _isRangeAttached = false;
        _pendingRangeNavigation = null;
        _rangeBringIntoViewIntent = checked(_rangeBringIntoViewIntent + 1);
        UnwireRangeSource(ItemsSource);
        ClearRangePresentation(clearRows: true);
        _rangeGeneration = null;
        var coordinator = _rangeCoordinator;
        _rangeCoordinator = null;
        coordinator?.Dispose();
        SetDataLoadState(DataGridLoadState.Idle);
    }

    private void WireRangeSource(IDataGridSource source)
    {
        if (_rangeSourceEventsWired)
        {
            return;
        }
        source.Invalidated += HandleRangeSourceInvalidated;
        _rangeSourceEventsWired = true;
    }

    private void UnwireRangeSource(IDataGridSource? source)
    {
        if (!_rangeSourceEventsWired || source is null)
        {
            return;
        }
        source.Invalidated -= HandleRangeSourceInvalidated;
        _rangeSourceEventsWired = false;
    }

    private void HandleRangeSourceInvalidated(object? sender, EventArgs args)
    {
        if (sender is not IDataGridSource source)
        {
            return;
        }
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyRangeSourceInvalidation(source);
            return;
        }
        var ownerReference = new WeakReference<DataGrid>(this);
        Dispatcher.UIThread.Post(() => ApplyRangeSourceInvalidation(ownerReference, source));
    }

    private static void ApplyRangeSourceInvalidation(
        WeakReference<DataGrid> ownerReference,
        IDataGridSource source)
    {
        if (ownerReference.TryGetTarget(out var owner))
        {
            owner.ApplyRangeSourceInvalidation(source);
        }
    }

    private void ApplyRangeSourceInvalidation(IDataGridSource source)
    {
        if (!_isRangeAttached || !ReferenceEquals(ItemsSource, source))
        {
            return;
        }
        CancelRowReorder();
        // 失效后槽位编号会变化，按旧槽位记录的导航意图不再指向原目标
        _pendingRangeNavigation = null;
        if (EditingRow is not null)
        {
            CancelEdit(DataGridEditingUnit.Row, raiseEvents: false);
            ResetEditingRow();
        }
        SetAndRaise(IsDataStaleProperty, ref _isDataStale, _rangeCoordinator?.AppliedSnapshot is not null);
        BeginRangeLoad(
            isInvalidation: true,
            CaptureFirstCompleteRangeAnchor());
    }

    private void BeginRangeLoad(
        bool isInvalidation,
        DataGridViewportAnchor? viewportAnchor = null)
    {
        if (!_isRangeAttached || ItemsSource is null || _rangeCoordinator is null)
        {
            return;
        }
        // 挂起的视口请求属于上一代数据，按新代际的槽位解释会把新结果定位到旧位置
        _pendingRangeViewport = null;
        var fallback = _rangeCoordinator.AppliedSnapshot;
        var generation = _rangeCoordinator.BeginGeneration(
            ItemsSource,
            Query,
            PageRequest,
            GroupExpansion,
            fallback,
            viewportAnchor);
        _rangeGeneration = generation;
        SetAndRaise(LoadErrorProperty, ref _loadError, null);
        var viewport = viewportAnchor.HasValue && fallback is not null
            ? fallback.CommittedViewport
            : new DataGridDesiredViewport(
                0,
                GetInitialRangeVisibleCount(),
                0);
        var pendingTransition = _rangeCoordinator.EnsureViewportAsync(generation, viewport);
        if (pendingTransition.IsCompletedSuccessfully)
        {
            ApplyRangeTransition(
                generation,
                pendingTransition.Result,
                isInvalidation);
            return;
        }
        SetDataLoadState(fallback is null
            ? DataGridLoadState.Loading
            : DataGridLoadState.Refreshing);
        _ = ObserveRangeLoadAsync(
            new WeakReference<DataGrid>(this),
            generation,
            pendingTransition,
            isInvalidation);
    }

    private static async Task ObserveRangeLoadAsync(
        WeakReference<DataGrid> ownerReference,
        DataGridGeneration generation,
        ValueTask<DataGridPresentationTransition> pendingTransition,
        bool isInvalidation)
    {
        var transition = await pendingTransition.ConfigureAwait(false);
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyRangeTransition(
                ownerReference,
                generation,
                transition,
                isInvalidation);
            return;
        }
        await Dispatcher.UIThread.InvokeAsync(
            () => ApplyRangeTransition(
                ownerReference,
                generation,
                transition,
                isInvalidation));
    }

    private static void ApplyRangeTransition(
        WeakReference<DataGrid> ownerReference,
        DataGridGeneration generation,
        DataGridPresentationTransition transition,
        bool isInvalidation)
    {
        if (ownerReference.TryGetTarget(out var owner))
        {
            owner.ApplyRangeTransition(generation, transition, isInvalidation);
        }
    }

    private void ApplyRangeTransition(
        DataGridGeneration generation,
        DataGridPresentationTransition transition,
        bool isInvalidation)
    {
        if (!_isRangeAttached ||
            !ReferenceEquals(_rangeGeneration, generation) ||
            !ReferenceEquals(ItemsSource, generation.Source))
        {
            return;
        }

        switch (transition.Kind)
        {
            case DataGridPresentationTransitionKind.Committed:
                AdoptRestartedRangeGeneration(generation);
                try
                {
                    ApplyRangeSnapshot(transition.Snapshot!, isInvalidation);
                }
                catch (Exception exception)
                {
                    SetRangeError(exception);
                }
                break;
            case DataGridPresentationTransitionKind.RolledBack:
                // 回滚后继续显示旧快照，滚动请求退回到当前显示位置
                AbandonPendingRangeScroll();
                if (transition.Snapshot is { } fallback)
                {
                    RollBackRangeIntent(fallback);
                    ApplyRangeSnapshotMetadata(fallback);
                }
                SetAndRaise(LoadErrorProperty, ref _loadError, transition.Error);
                SetAndRaise(IsDataStaleProperty, ref _isDataStale, isInvalidation);
                SetDataLoadState(DataGridLoadState.Error);
                break;
            case DataGridPresentationTransitionKind.Redirected:
                ApplyRedirectedPageRequest(transition.RedirectedPageRequest!.Value, isInvalidation);
                break;
            case DataGridPresentationTransitionKind.Failed:
                SetRangeError(transition.Error!);
                break;
            case DataGridPresentationTransitionKind.Superseded:
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown presentation transition '{transition.Kind}'.");
        }
    }

    // 同一查询在快照过期后由协调器重启为新代际并完成提交时，表格改为持有新代际
    private void AdoptRestartedRangeGeneration(DataGridGeneration committedGeneration)
    {
        if (_rangeCoordinator?.ActiveGeneration is { } active &&
            !ReferenceEquals(active, committedGeneration) &&
            ReferenceEquals(active.Source, committedGeneration.Source) &&
            active.QueryRevision == committedGeneration.QueryRevision &&
            active.SnapshotExpiryRestarts > committedGeneration.SnapshotExpiryRestarts)
        {
            _rangeGeneration = active;
        }
    }

    private void ApplyRangeSnapshot(
        DataGridPresentationSnapshot snapshot,
        bool isInvalidation)
    {
        var previousSnapshot = _rangePresentationIndex?.Snapshot;
        var committingGeneration = _rangeGeneration;
        CommitRangePresentation(snapshot, committingGeneration?.ViewportAnchor);
        if (ReferenceEquals(_rangeGeneration, committingGeneration) && committingGeneration is not null)
        {
            // An anchor belongs to the atomic generation transition only. Reapplying it to
            // later viewport commits would fight explicit scrolling and cause cumulative drift.
            committingGeneration.ViewportAnchor = null;
        }
        TransitionSelectionForCommittedSnapshot(
            previousSnapshot,
            snapshot,
            isInvalidation);
        ReprojectCurrentCellForCommittedSnapshot(
            previousSnapshot,
            snapshot,
            isInvalidation);
        ApplyRangeSnapshotMetadata(snapshot);
        ApplyPendingRangeNavigation();
        SetAndRaise(LoadErrorProperty, ref _loadError, null);
        SetAndRaise(IsDataStaleProperty, ref _isDataStale, false);
        SetDataLoadState(DataGridLoadState.Ready);
        if (_rangeCoordinator is { } coordinator && _rangeGeneration is { } generation)
        {
            _ = ObserveRangePrefetchAsync(
                coordinator,
                generation,
                snapshot.CommittedViewport);
        }
    }

    private static async Task ObserveRangePrefetchAsync(
        DataGridRangeCoordinator coordinator,
        DataGridGeneration generation,
        DataGridDesiredViewport viewport)
    {
        try
        {
            await coordinator.PrefetchAsync(
                generation,
                viewport,
                generation.CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            coordinator.RecordPrefetchFailure(generation, viewport, exception);
        }
    }

    private void ApplyRangeSnapshotMetadata(DataGridPresentationSnapshot snapshot)
    {
        SetAndRaise(AppliedQueryProperty, ref _appliedQuery, snapshot.Query);
        SetAndRaise(
            AppliedPageRequestProperty,
            ref _appliedPageRequest,
            snapshot.PageRequest);
        SetAndRaise(TotalItemCountProperty, ref _totalItemCount, snapshot.TotalDataCount);
        SetAndRaise(TotalEntryCountProperty, ref _totalEntryCount, snapshot.TotalEntryCount);
        SyncRangePaginationState();
    }

    private void ResetAppliedRangeState()
    {
        ClearRangePresentation(clearRows: true);
        SetAndRaise(AppliedQueryProperty, ref _appliedQuery, DataGridQuery.Empty);
        SetAndRaise(AppliedPageRequestProperty, ref _appliedPageRequest, null);
        SetAndRaise(LoadErrorProperty, ref _loadError, null);
        SetAndRaise(TotalItemCountProperty, ref _totalItemCount, 0);
        SetAndRaise(TotalEntryCountProperty, ref _totalEntryCount, 0);
        SetAndRaise(IsDataStaleProperty, ref _isDataStale, false);
        SetDataLoadState(DataGridLoadState.Idle);
    }

    private void SetRangeError(Exception exception)
    {
        AbandonPendingRangeScroll();
        SetAndRaise(LoadErrorProperty, ref _loadError, exception);
        SetDataLoadState(DataGridLoadState.Error);
    }

    private void SetDataLoadState(DataGridLoadState value)
    {
        SetAndRaise(LoadStateProperty, ref _dataLoadState, value);
        UpdateEffectiveIsOperating();
    }

    private void UpdateEffectiveIsOperating()
    {
        var value = IsOperating ||
                    LoadState == DataGridLoadState.Loading;
        SetAndRaise(
            EffectiveIsOperatingProperty,
            ref _effectiveIsOperating,
            value);
    }

    private void RefreshColumnSortCapabilities()
    {
        foreach (var column in ColumnsItemsInternal)
        {
            column.RefreshSortProjection();
        }
    }

    private static void ValidateRangeSourceScope(
        IDataGridSource? source,
        DataGridQuery query,
        DataGridPageRequest? pageRequest,
        DataGridGroupExpansion expansion)
    {
        if (source is null)
        {
            return;
        }
        var request = new DataGridFetchRequest(
            query,
            pageRequest,
            expansion,
            new DataGridRange(0, source.Schema.PreferredRangeSize),
            null,
            0,
            0);
        DataGridSourceContractValidator.ValidateRequest(request, source.Schema);
    }

    private void ApplyRedirectedPageRequest(
        DataGridPageRequest pageRequest,
        bool isInvalidation)
    {
        if (PageRequest == pageRequest)
        {
            return;
        }
        SetPageRequestValue(pageRequest);
        _rangeDesiredOffset = 0;
        SyncRangePaginationState();
        BeginRangeLoad(isInvalidation);
    }
}
