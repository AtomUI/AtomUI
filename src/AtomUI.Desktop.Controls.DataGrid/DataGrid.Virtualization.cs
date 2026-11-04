using Avalonia.Controls;
using Avalonia.Threading;
using AtomUI.Utils;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    private DataGridPresentationIndex? _rangePresentationIndex;
    private SparseHeightDeltaIndex? _rangeHeightIndex;
    private IDisposable? _rangeVisualPins;
    private double _rangeDesiredOffset;
    private DataGridDesiredViewport? _pendingRangeViewport;
    private bool _rangeViewportDispatchPending;
    private bool _committingRangePresentation;
    private int _rangeViewportCommitCount;
    private int _rangeMeasurementScrollDirection;
    private double _rangeMeasurementScrollTarget;

    internal bool IsRangePresentationActive =>
        ItemsSource is not null && _rangePresentationIndex is not null;

    internal int RangeWindowDataCount =>
        _rangePresentationIndex?.ChildCount ?? 0;

    internal int RangeCachePinCount => _rangeCoordinator?.CachePinCount ?? 0;

    internal int RangeCacheCount => _rangeCoordinator?.CacheCount ?? 0;

    internal int RangeActiveRequestCount => _rangeCoordinator?.ActiveRequestCount ?? 0;

    internal int RangeInFlightBlockCount => _rangeCoordinator?.InFlightBlockCount ?? 0;

    internal int RangeStaleCommitCount => _rangeCoordinator?.StaleCommitCount ?? 0;

    internal int RangeMeasuredHeightCount => _rangeHeightIndex?.MeasuredCount ?? 0;

    internal int RangeViewportCommitCount => _rangeViewportCommitCount;

    internal bool HasPendingRangeViewport =>
        _pendingRangeViewport.HasValue || _rangeViewportDispatchPending;

    internal void RequestRangeViewport(
        int firstVisibleSlot,
        int visibleCount,
        int scrollDirection)
    {
        if (firstVisibleSlot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(firstVisibleSlot));
        }
        if (visibleCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(visibleCount));
        }
        if (scrollDirection is < -1 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(scrollDirection));
        }
        if (!_isRangeAttached ||
            _rangeCoordinator is null ||
            _rangeGeneration is null ||
            _rangePresentationIndex is null)
        {
            return;
        }
        if (EditingRow is not null && !CommitEdit())
        {
            return;
        }

        var totalEntryCount = _rangePresentationIndex.SlotCount;
        if (totalEntryCount == 0)
        {
            return;
        }
        var legalFirstSlot = Math.Min(firstVisibleSlot, totalEntryCount - 1);
        var legalVisibleCount = Math.Min(visibleCount, totalEntryCount - legalFirstSlot);
        _rangeDesiredOffset = GetRangeOffset(legalFirstSlot);
        _rangeMeasurementScrollDirection = scrollDirection;
        _rangeMeasurementScrollTarget = _rangeDesiredOffset;
        QueueRangeViewport(new DataGridDesiredViewport(
            legalFirstSlot,
            legalVisibleCount,
            scrollDirection));
    }

    private bool QueueRangeScrollOffset(double requestedOffset, int scrollDirection)
    {
        if (_rangeHeightIndex is null || SlotCount == 0)
        {
            return false;
        }

        var legalOffset = Math.Clamp(requestedOffset, 0, RangeMaximumOffset);
        if (MathUtils.AreClose(legalOffset, _rangeDesiredOffset))
        {
            return false;
        }

        RequestRangeScrollOffset(legalOffset, scrollDirection);
        return true;
    }

    private void RequestRangeScrollOffset(double legalOffset, int scrollDirection)
    {
        _rangeDesiredOffset = legalOffset;
        _rangeMeasurementScrollDirection = scrollDirection;
        _rangeMeasurementScrollTarget = legalOffset;
        var firstVisibleSlot = _rangeHeightIndex!.FindSlotAtOffset(legalOffset, SlotCount);
        var visibleCount = Math.Max(
            GetInitialRangeVisibleCount(),
            _rangePresentationIndex!.Snapshot.CommittedViewport.VisibleCount);
        visibleCount = Math.Min(visibleCount, SlotCount - firstVisibleSlot);
        QueueRangeViewport(new DataGridDesiredViewport(
            firstVisibleSlot,
            visibleCount,
            scrollDirection));
    }

    private double RangeMaximumOffset => Math.Max(0, GetRangeExtent() - CellsEstimatedHeight);

    // 视口变高或内容总高缩水都会降低滚动上界，RangeBase 会自行把滚动条 Value 夹到新上界，
    // 但偏移不会跟随；上界在滚动条布局中统一计算，偏移也在这里统一收敛，
    // 否则末行之后会留出与上界降幅等高的空白。只收敛越界部分，不改变界内的滚动意图。
    private void ClampRangeOffsetToMaximum()
    {
        if (!IsRangePresentationActive || SlotCount == 0)
        {
            return;
        }

        if (MathUtils.GreaterThan(Math.Max(_verticalOffset, _rangeDesiredOffset), RangeMaximumOffset))
        {
            ReconcileCommittedRangeOffset(RangeMaximumOffset);
        }
    }

    // 纵向位置的唯一落点：把目标偏移落到已提交视口，使显示的行、_verticalOffset、
    // _rangeDesiredOffset 与滚动条保持同一真相。
    // - 目标在显示首行之后：沿用以显示首行为基准的 NegVerticalOffset，由测量阶段向前归一化；
    // - 目标在显示首行之前但仍在已提交窗口内：从包含目标的槽位重建显示行；
    // - 目标在已提交窗口之前：先按窗口起点显示，再以目标请求包含它的视口。
    private void ReconcileCommittedRangeOffset(double offset)
    {
        var committedFirst = _rangePresentationIndex!.Snapshot.CommittedViewport.FirstVisibleIndex;
        var maximumOffset = RangeMaximumOffset;
        var target = Math.Clamp(offset, 0, maximumOffset);
        var windowStart = GetRangeOffset(committedFirst);
        if (MathUtils.LessThan(target, windowStart))
        {
            ApplyRangeVerticalOffset(Math.Min(windowStart, maximumOffset), committedFirst);
            RequestRangeScrollOffset(target, -1);
            return;
        }

        _rangeDesiredOffset = target;
        var displayedFirst = DisplayData.FirstScrollingSlot;
        if (displayedFirst > committedFirst &&
            MathUtils.LessThan(target, GetRangeOffset(displayedFirst)))
        {
            var firstSlot = Math.Max(committedFirst, _rangeHeightIndex!.FindSlotAtOffset(target, SlotCount));
            NegVerticalOffset = Math.Max(0, target - GetRangeOffset(firstSlot));
            SetVerticalOffset(target);
            // 显示行只能按相邻槽位向上扩展；新首行与当前显示不相邻时先回收全部显示行再从新首行重建
            if (displayedFirst - firstSlot > 1)
            {
                ResetDisplayedRows();
            }
            UpdateDisplayedRows(firstSlot, CellsEstimatedHeight);
            return;
        }

        ApplyRangeVerticalOffset(target, displayedFirst >= committedFirst ? displayedFirst : committedFirst);
    }

    // 滚动意图（方向与目标偏移）只属于发起它的那次滚动及当时的高度空间。
    // 高度整体重建、加载失败或回滚、数据换代后旧目标已无意义，继续生效会把偏移推到未加载区域。
    private void ClearRangeScrollIntent()
    {
        _rangeMeasurementScrollDirection = 0;
        _rangeMeasurementScrollTarget = 0;
    }

    // 未完成的滚动请求作废：期望偏移退回到当前已显示的位置，挂起的视口请求与滚动意图一并清除。
    private void AbandonPendingRangeScroll()
    {
        _rangeDesiredOffset = _verticalOffset;
        _pendingRangeViewport = null;
        ClearRangeScrollIntent();
    }

    private void QueueRangeViewport(DataGridDesiredViewport viewport)
    {
        _pendingRangeViewport = viewport;
        if (_rangeViewportDispatchPending)
        {
            return;
        }

        _rangeViewportDispatchPending = true;
        var ownerReference = new WeakReference<DataGrid>(this);
        Dispatcher.UIThread.Post(
            () => ProcessPendingRangeViewport(ownerReference),
            DispatcherPriority.Input);
    }

    private static void ProcessPendingRangeViewport(WeakReference<DataGrid> ownerReference)
    {
        if (ownerReference.TryGetTarget(out var owner))
        {
            owner.ProcessPendingRangeViewport();
        }
    }

    private void ProcessPendingRangeViewport()
    {
        _rangeViewportDispatchPending = false;
        var viewport = _pendingRangeViewport;
        _pendingRangeViewport = null;
        if (!viewport.HasValue ||
            !_isRangeAttached ||
            _rangeCoordinator is null ||
            _rangeGeneration is null)
        {
            return;
        }

        var committed = _rangePresentationIndex?.Snapshot.CommittedViewport;
        if (committed.HasValue &&
            committed.Value.FirstVisibleIndex == viewport.Value.FirstVisibleIndex &&
            committed.Value.VisibleCount == viewport.Value.VisibleCount)
        {
            ApplyRangeOffsetWithinCommittedViewport();
            return;
        }

        BeginViewportRangeLoad(viewport.Value);
    }

    private void ApplyRangeOffsetWithinCommittedViewport()
    {
        ReconcileCommittedRangeOffset(_rangeDesiredOffset);
        ComputeScrollBarsLayout();
        InvalidateRowsMeasure(invalidateIndividualElements: false);
        InvalidateRowsArrange();
    }

    internal int GetInitialRangeVisibleCount()
    {
        var viewportHeight = RowsPresenterAvailableSize?.Height;
        if (!viewportHeight.HasValue ||
            !double.IsFinite(viewportHeight.Value) ||
            viewportHeight.Value <= 0)
        {
            viewportHeight = Bounds.Height;
        }
        if (!viewportHeight.HasValue ||
            !double.IsFinite(viewportHeight.Value) ||
            viewportHeight.Value <= 0)
        {
            viewportHeight = Height;
        }
        if (!viewportHeight.HasValue ||
            !double.IsFinite(viewportHeight.Value) ||
            viewportHeight.Value <= 0)
        {
            return 1;
        }
        var estimate = GetRangeDefaultHeight();
        var visible = Math.Ceiling(viewportHeight.Value / estimate) + 1;
        return Math.Clamp((int)Math.Min(visible, int.MaxValue), 1, ItemsSource!.Schema.MaximumRangeSize);
    }

    internal DataGridSourceEntry GetCommittedRangeEntry(int slot)
    {
        if (_rangePresentationIndex is null)
        {
            throw new InvalidOperationException("No range presentation is committed.");
        }
        return _rangePresentationIndex.GetEntry(slot);
    }

    internal bool TryGetCommittedRangeEntry(int slot, out DataGridSourceEntry entry)
    {
        if (_rangePresentationIndex is not null)
        {
            return _rangePresentationIndex.TryGetEntry(slot, out entry);
        }
        entry = default;
        return false;
    }

    internal int GetCommittedRangeRowIndex(int slot)
    {
        var entry = GetCommittedRangeEntry(slot);
        if (entry.Kind != DataGridSourceEntryKind.Data)
        {
            return -1;
        }
        return entry.WindowDataIndex;
    }

    internal int GetCommittedRangeSlot(int windowDataIndex)
    {
        if (_rangePresentationIndex is null)
        {
            return -1;
        }
        return _rangePresentationIndex.FindSlotByWindowDataIndex(windowDataIndex) ?? -1;
    }

    internal int FindCommittedRangeSlot(object item) =>
        _rangePresentationIndex?.FindSlot(item) ?? -1;

    internal double GetRangeOffset(int slot) =>
        _rangeHeightIndex?.GetOffset(slot) ??
        SparseHeightDeltaIndex.SafeMultiply(slot, GetRangeDefaultHeight());

    internal double GetRangeExtent() =>
        _rangeHeightIndex?.GetExtent(SlotCount) ??
        SparseHeightDeltaIndex.SafeMultiply(SlotCount, GetRangeDefaultHeight());

    internal double GetRangeHeight(int slot) =>
        _rangeHeightIndex?.GetHeight(slot) ?? GetRangeDefaultHeight();

    internal void RecordRangeMeasuredHeight(int slot, double height, DataGridHeightClass heightClass)
    {
        // A zero height can be reported transiently while a recycled element is
        // detached or before its first completed arrange. It is not a meaningful
        // row-height sample and SparseHeightDeltaIndex deliberately rejects it.
        if (_rangeHeightIndex is not null && double.IsFinite(height) && height > 0)
        {
            if (MathUtils.AreClose(_rangeHeightIndex.GetHeight(slot), height))
            {
                return;
            }

            DataGridViewportAnchor? anchor = null;
            if (!_committingRangePresentation && !_rowMetricsChanging &&
                _rangePresentationIndex is not null &&
                SlotCount > 0)
            {
                var candidate =
                    _rangePresentationIndex.CaptureFirstCompleteAnchor(_verticalOffset);
                // Only height changes above the first complete element can move its
                // screen position. Re-anchoring while measuring that element (or an
                // element below it) can promote the next row because of sub-pixel
                // rounding, producing cumulative downward drift across one layout pass.
                if (slot < candidate.FallbackSlot)
                {
                    anchor = candidate;
                }
            }
            _rangeHeightIndex.SetMeasuredHeight(slot, height, heightClass);
            if (anchor.HasValue && _rangePresentationIndex is not null)
            {
                var maximumOffset = RangeMaximumOffset;
                ReconcileCommittedRangeOffset(PreserveRangeScrollDirection(Math.Clamp(
                    _rangePresentationIndex.RestoreAnchor(anchor.Value),
                    0,
                    maximumOffset), maximumOffset));
            }
        }
    }

    private double PreserveRangeScrollDirection(double offset, double maximumOffset)
    {
        var directionalOffset = _rangeMeasurementScrollDirection switch
        {
            > 0 => Math.Max(offset, _rangeMeasurementScrollTarget),
            < 0 => Math.Min(offset, _rangeMeasurementScrollTarget),
            _ => offset
        };
        return Math.Clamp(directionalOffset, 0, maximumOffset);
    }

    private void BeginViewportRangeLoad(DataGridDesiredViewport viewport)
    {
        if (_rangeCoordinator is null || _rangeGeneration is null)
        {
            return;
        }
        SetAndRaise(LoadErrorProperty, ref _loadError, null);
        var pendingTransition = _rangeCoordinator.EnsureViewportAsync(_rangeGeneration, viewport);
        if (pendingTransition.IsCompletedSuccessfully)
        {
            ApplyRangeTransition(
                _rangeGeneration,
                pendingTransition.Result,
                isInvalidation: false);
            return;
        }
        SetDataLoadState(DataGridLoadState.Refreshing);
        _ = ObserveRangeLoadAsync(
            new WeakReference<DataGrid>(this),
            _rangeGeneration,
            pendingTransition,
            isInvalidation: false);
    }

    private void CommitRangePresentation(
        DataGridPresentationSnapshot snapshot,
        DataGridViewportAnchor? viewportAnchor = null)
    {
        var coordinator = _rangeCoordinator ??
                          throw new InvalidOperationException("The range coordinator is detached.");
        var isNewDataGeneration =
            _rangePresentationIndex?.Snapshot.DataGeneration != snapshot.DataGeneration;
        var defaultHeight = GetRangeDefaultHeight(
            snapshot,
            useDefaultRowHeightEstimate: isNewDataGeneration);
        var heightIndex = _rangeHeightIndex;
        if (heightIndex is null || isNewDataGeneration)
        {
            heightIndex = new SparseHeightDeltaIndex(defaultHeight);
            if (isNewDataGeneration)
            {
                // 滚动意图与期望偏移都属于被替换代际的高度空间；无论后续走完整提交还是复用重叠视口，
                // 都要先作废意图，并把期望偏移按其相对已提交首行的位置换算到新高度空间
                // （快照过期重启后视口仍按槽位加载，不换算会落到未加载的槽位上）。
                ClearRangeScrollIntent();
                if (!viewportAnchor.HasValue && _rangeHeightIndex is { } previousHeights)
                {
                    var committedFirst = snapshot.CommittedViewport.FirstVisibleIndex;
                    var withinFirstRow = _rangeDesiredOffset - previousHeights.GetOffset(committedFirst);
                    _rangeDesiredOffset = heightIndex.GetOffset(committedFirst) +
                                          Math.Clamp(withinFirstRow, 0, heightIndex.GetHeight(committedFirst));
                }
            }
        }
        else
        {
            heightIndex.RebaseDefaultHeight(defaultHeight);
            var previousIndex = _rangePresentationIndex!;
            foreach (var previousBlock in previousIndex.Snapshot.Blocks)
            {
                if (!snapshot.Blocks.Any(nextBlock =>
                        nextBlock.Range.StartIndex == previousBlock.Range.StartIndex))
                {
                    var preservedDetailsHeights = new List<(int Slot, double Height)>();
                    for (var offset = 0; offset < previousBlock.Entries.Length; offset++)
                    {
                        var entry = previousBlock.Entries[offset];
                        if (entry.Kind == DataGridSourceEntryKind.Data &&
                            _rangeDetailsVisibility.TryGetValue(entry.RowKey, out var isVisible) &&
                            isVisible)
                        {
                            var slot = checked(previousBlock.Range.StartIndex + offset);
                            preservedDetailsHeights.Add((slot, heightIndex.GetHeight(slot)));
                        }
                    }
                    heightIndex.RemoveRange(
                        previousBlock.Range.StartIndex,
                        previousBlock.Entries.Length,
                        absorbIntoEstimator: true);
                    foreach (var preserved in preservedDetailsHeights)
                    {
                        heightIndex.SetMeasuredHeight(
                            preserved.Slot,
                            preserved.Height,
                            DataGridHeightClass.Data);
                    }
                }
            }
        }

        SeedRangeDetailsHeights(snapshot, heightIndex, preserveViewportAnchor: false);

        var nextIndex = new DataGridPresentationIndex(snapshot, heightIndex);
        if (viewportAnchor.HasValue)
        {
            _rangeDesiredOffset = nextIndex.RestoreAnchor(viewportAnchor.Value);
        }
        var nextPins = coordinator.AcquireSnapshotPins(
            snapshot,
            DataGridRangeBlockPinReason.Realized);
        if (CanReuseCommittedRangeViewport(nextIndex, snapshot))
        {
            CommitOverlappingRangeViewport(nextIndex, snapshot, heightIndex, nextPins, viewportAnchor);
            return;
        }

        var staged = new List<(int Slot, Control Element)>(snapshot.CommittedViewport.VisibleCount);
        try
        {
            if (_rowsPresenter is not null &&
                ((RowsPresenterAvailableSize is { Height: > 0 } presenterSize &&
                  double.IsFinite(presenterSize.Height)) ||
                 (double.IsNaN(Height) && Bounds.Height <= 0)))
            {
                var endExclusive = checked(
                    snapshot.CommittedViewport.FirstVisibleIndex +
                    snapshot.CommittedViewport.VisibleCount);
                for (var slot = snapshot.CommittedViewport.FirstVisibleIndex;
                     slot < endExclusive;
                     slot++)
                {
                    staged.Add((slot, CreateRangeElement(nextIndex.GetEntry(slot), slot)));
                }
            }
        }
        catch
        {
            ReleaseStagedRangeElements(staged);
            nextPins.Dispose();
            throw;
        }

        if (isNewDataGeneration)
        {
            ResetRowHeightEstimateForDataGeneration();
        }
        var previousPins = _rangeVisualPins;
        UnloadElements(recycle: true);
        _rangePresentationIndex = nextIndex;
        _rangeHeightIndex = heightIndex;
        _rangeVisualPins = nextPins;
        SlotCount = nextIndex.SlotCount;
        VisibleSlotCount = nextIndex.SlotCount;
        IsEmptyDataSource = nextIndex.SlotCount == 0;
        DisplayData.PendingVerticalScrollHeight = 0;
        NegVerticalOffset = 0;
        AvailableSlotElementRoom = CellsEstimatedHeight;

        if (SlotCount == 0)
        {
            SetVerticalOffset(0);
        }
        else
        {
            ReconcileCommittedRangeOffset(viewportAnchor.HasValue
                ? nextIndex.RestoreAnchor(viewportAnchor.Value)
                : _rangeDesiredOffset);
            _committingRangePresentation = true;
            try
            {
                foreach (var element in staged)
                {
                    InsertDisplayedElement(
                        element.Slot,
                        element.Element,
                        wasNewlyAdded: false,
                        updateSlotInformation: true);
                }
            }
            finally
            {
                _committingRangePresentation = false;
            }
            // 插入元素时实测的行高与估值重建可能改变偏移的含义，按最终高度再落一次偏移
            ReconcileCommittedRangeOffset(viewportAnchor.HasValue
                ? nextIndex.RestoreAnchor(viewportAnchor.Value)
                : _rangeDesiredOffset);
            DisplayData.NumTotallyDisplayedScrollingElements =
                CalculateTotallyVisibleRangeElementCount(staged);
        }

        previousPins?.Dispose();
        DisplayData.FullyRecycleElements();
        _rangeViewportCommitCount = checked(_rangeViewportCommitCount + 1);
        ComputeScrollBarsLayout();
        InvalidateRowsMeasure(invalidateIndividualElements: false);
        InvalidateRowsArrange();
    }

    private bool CanReuseCommittedRangeViewport(
        DataGridPresentationIndex nextIndex,
        DataGridPresentationSnapshot nextSnapshot)
    {
        if (_rangePresentationIndex is not { } previousIndex ||
            previousIndex.Snapshot.DataGeneration != nextSnapshot.DataGeneration ||
            DisplayData.FirstScrollingSlot < 0 ||
            DisplayData.LastScrollingSlot < DisplayData.FirstScrollingSlot ||
            nextSnapshot.CommittedViewport.VisibleCount <= 0)
        {
            return false;
        }

        var nextFirst = nextSnapshot.CommittedViewport.FirstVisibleIndex;
        var nextLast = checked(nextFirst + nextSnapshot.CommittedViewport.VisibleCount - 1);
        var overlapFirst = Math.Max(DisplayData.FirstScrollingSlot, nextFirst);
        var overlapLast = Math.Min(DisplayData.LastScrollingSlot, nextLast);
        if (overlapFirst > overlapLast)
        {
            return false;
        }

        for (var slot = overlapFirst; slot <= overlapLast; slot++)
        {
            if (!previousIndex.TryGetEntry(slot, out var previousEntry) ||
                !nextIndex.TryGetEntry(slot, out var nextEntry) ||
                !CanReuseRangeElement(previousEntry, nextEntry))
            {
                return false;
            }
        }
        return true;
    }

    private static bool CanReuseRangeElement(
        DataGridSourceEntry previousEntry,
        DataGridSourceEntry nextEntry)
    {
        if (previousEntry.Kind != nextEntry.Kind)
        {
            return false;
        }
        return previousEntry.Kind switch
        {
            DataGridSourceEntryKind.Data =>
                previousEntry.RowKey == nextEntry.RowKey &&
                ReferenceEquals(previousEntry.Item, nextEntry.Item),
            DataGridSourceEntryKind.GroupHeader =>
                previousEntry.Group?.Key == nextEntry.Group?.Key,
            _ => false
        };
    }

    private void CommitOverlappingRangeViewport(
        DataGridPresentationIndex nextIndex,
        DataGridPresentationSnapshot snapshot,
        SparseHeightDeltaIndex heightIndex,
        IDisposable nextPins,
        DataGridViewportAnchor? viewportAnchor)
    {
        var nextFirst = snapshot.CommittedViewport.FirstVisibleIndex;
        var nextLast = checked(nextFirst + snapshot.CommittedViewport.VisibleCount - 1);
        // 提交期间不做实测重锚；在已显示内容上方插入的行实测高度会推动内容，
        // 因此记录插入前的内容锚点，插入后把它的位移叠加到目标偏移上，保持目标相对内容不变。
        var contentAnchor = viewportAnchor.HasValue || DisplayData.FirstScrollingSlot < 0
            ? null
            : CaptureFirstCompleteRangeAnchor();
        var offsetBeforeInsertion = _verticalOffset;
        RemoveNonDisplayedRows(nextFirst, nextLast);

        var previousPins = _rangeVisualPins;
        _rangePresentationIndex = nextIndex;
        _rangeHeightIndex = heightIndex;
        _rangeVisualPins = nextPins;
        SlotCount = nextIndex.SlotCount;
        VisibleSlotCount = nextIndex.SlotCount;
        IsEmptyDataSource = nextIndex.SlotCount == 0;
        DisplayData.PendingVerticalScrollHeight = 0;
        AvailableSlotElementRoom = CellsEstimatedHeight;

        _committingRangePresentation = true;
        try
        {
            while (DisplayData.FirstScrollingSlot > nextFirst)
            {
                InsertDisplayedElement(
                    GetPreviousVisibleSlot(DisplayData.FirstScrollingSlot),
                    updateSlotInformation: true);
            }
            while (DisplayData.LastScrollingSlot < nextLast)
            {
                InsertDisplayedElement(
                    GetNextVisibleSlot(DisplayData.LastScrollingSlot),
                    updateSlotInformation: true);
            }
            AvailableSlotElementRoom = CellsEstimatedHeight;
            foreach (var element in DisplayData.GetScrollingElements())
            {
                AvailableSlotElementRoom -= GetDisplayedElementHeight(element);
            }
        }
        finally
        {
            _committingRangePresentation = false;
        }

        var targetOffset = viewportAnchor.HasValue
            ? nextIndex.RestoreAnchor(viewportAnchor.Value)
            : _rangeDesiredOffset;
        if (contentAnchor.HasValue)
        {
            targetOffset += nextIndex.RestoreAnchor(contentAnchor.Value) - offsetBeforeInsertion;
        }
        ReconcileCommittedRangeOffset(targetOffset);

        DisplayData.NumTotallyDisplayedScrollingElements =
            CalculateTotallyVisibleRangeElementCount(
                DisplayData.GetScrollingElements()
                    .Select(element => (GetRangeElementSlot(element), element))
                    .ToList());
        previousPins?.Dispose();
        DisplayData.FullyRecycleElements();
        _rangeViewportCommitCount = checked(_rangeViewportCommitCount + 1);
        ComputeScrollBarsLayout();
        InvalidateRowsMeasure(invalidateIndividualElements: false);
        InvalidateRowsArrange();
    }

    private static int GetRangeElementSlot(Control element) => element switch
    {
        DataGridRow row => row.Slot,
        DataGridRowGroupHeader groupHeader => groupHeader.DisplaySlot,
        _ => -1
    };

    private Control CreateRangeElement(DataGridSourceEntry entry, int slot)
    {
        if (entry.Kind == DataGridSourceEntryKind.GroupHeader)
        {
            return GenerateRangeRowGroupHeader(entry.Group!, slot);
        }

        var row = DisplayData.GetUsedRow() ?? new DataGridRow();
        row.OwningGrid = this;
        row.RowKey = entry.RowKey;
        row.DataIndex = entry.DataIndex;
        row.IsRangeBacked = true;
        row.Index = entry.WindowDataIndex;
        row.Slot = slot;
        row.DataContext = entry.Item;
        row.BindOwningGridProperties(this);
        CompleteCellsCollection(row);
        NotifyLoadingRow(new DataGridRowEventArgs(row));
        row.ApplyState();
        return row;
    }

    private DataGridRowGroupHeader GenerateRangeRowGroupHeader(
        DataGridGroupEntry group,
        int slot)
    {
        EnsureRangeGroupMetrics(group.Level + 1);
        var header = DisplayData.GetUsedGroupHeader() ?? new DataGridRowGroupHeader();
        header.OwningGrid = this;
        header.SourceGroup = group;
        header.SourceSlot = slot;
        header.DataContext = group;
        header.Level = group.Level;
        header.PropertyName = group.Field.Value;
        if (RowGroupTheme is { } rowGroupTheme)
        {
            header.SetValue(ThemeProperty, rowGroupTheme, Avalonia.Data.BindingPriority.Template);
        }
        header.UpdateTitleElements();
        NotifyLoadingRowGroup(new DataGridRowGroupHeaderEventArgs(header));
        return header;
    }

    private void EnsureRangeGroupMetrics(int levelCount)
    {
        if (_rowGroupHeightsByLevel.Length >= levelCount)
        {
            return;
        }
        var heights = new double[levelCount];
        var indents = new double[levelCount];
        Array.Fill(heights, DefaultRowHeight);
        for (var level = 0; level < levelCount; level++)
        {
            indents[level] = DefaultRowGroupSublevelIndent * (level + 1);
        }
        Array.Copy(_rowGroupHeightsByLevel, heights, _rowGroupHeightsByLevel.Length);
        Array.Copy(RowGroupSublevelIndents, indents, RowGroupSublevelIndents.Length);
        _rowGroupHeightsByLevel = heights;
        RowGroupSublevelIndents = indents;
    }

    private int CalculateTotallyVisibleRangeElementCount(
        List<(int Slot, Control Element)> elements)
    {
        var available = CellsEstimatedHeight + NegVerticalOffset;
        var used = 0d;
        var count = 0;
        foreach (var element in elements)
        {
            used += GetDisplayedElementHeight(element.Element);
            if (used <= available)
            {
                count++;
            }
        }
        return count;
    }

    private static void ReleaseStagedRangeElements(List<(int Slot, Control Element)> elements)
    {
        foreach (var element in elements)
        {
            if (element.Element is DataGridRow row && row.OwningGrid is not null)
            {
                row.DetachFromDataGrid(recycle: false);
            }
            else if (element.Element is DataGridRowGroupHeader groupHeader)
            {
                groupHeader.DetachFromDataGrid();
            }
        }
    }

    private void ClearRangePresentation(bool clearRows)
    {
        if (_rangePresentationIndex is null && _rangeVisualPins is null)
        {
            return;
        }
        if (clearRows)
        {
            UnloadElements(recycle: false);
        }
        _rangeVisualPins?.Dispose();
        _rangeVisualPins = null;
        _rangePresentationIndex = null;
        _rangeHeightIndex = null;
        _rangeDesiredOffset = 0;
        ClearRangeScrollIntent();
        _pendingRangeViewport = null;
        SlotCount = 0;
        VisibleSlotCount = 0;
        IsEmptyDataSource = true;
        NegVerticalOffset = 0;
        SetVerticalOffset(0);
    }

    private void RestoreRangePresentationAfterTemplate()
    {
        if (IsRangePresentationActive &&
            _rowsPresenter is not null &&
            (DisplayData.NumDisplayedScrollingElements > 0 ||
             (double.IsNaN(Height) && Bounds.Height <= 0)))
        {
            CommitRangePresentation(_rangePresentationIndex!.Snapshot);
        }
    }

    private DataGridViewportAnchor? CaptureFirstCompleteRangeAnchor()
    {
        if (_rangePresentationIndex is null || SlotCount == 0)
        {
            return null;
        }
        return _rangePresentationIndex.CaptureFirstCompleteAnchor(_verticalOffset);
    }

    private DataGridViewportAnchor? CaptureRangeGroupAnchor(DataGridGroupKey key)
    {
        if (_rangePresentationIndex is null ||
            _rangePresentationIndex.FindSlot(key) is not { } slot)
        {
            return CaptureFirstCompleteRangeAnchor();
        }
        return _rangePresentationIndex.CaptureAnchorAtSlot(slot, _verticalOffset);
    }

    private void RebaseRangeHeightIndex(bool discardMeasuredDataHeights)
    {
        if (_rangeHeightIndex is null || _rangePresentationIndex is null)
        {
            return;
        }
        // 提交过程中插入元素也会触发重建，此时显示行尚未完整、不能据显示捕获锚点；
        // 只把期望偏移按其所在槽位的相对位置换算到新高度空间，由提交结束时统一落点
        var committing = _committingRangePresentation;
        var anchor = committing ? null : CaptureFirstCompleteRangeAnchor();
        var desiredSlot = SlotCount > 0 ? _rangeHeightIndex.FindSlotAtOffset(_rangeDesiredOffset, SlotCount) : 0;
        var desiredWithinSlot = _rangeDesiredOffset - _rangeHeightIndex.GetOffset(desiredSlot);
        if (discardMeasuredDataHeights)
        {
            _rangeHeightIndex.RemoveHeightClass(DataGridHeightClass.Data);
        }
        _rangeHeightIndex.RebaseDefaultHeight(
            GetRangeDefaultHeight(_rangePresentationIndex.Snapshot));
        // 整体重建后高度空间已变，旧滚动目标不能再参与锚点恢复
        ClearRangeScrollIntent();
        if (committing)
        {
            _rangeDesiredOffset = _rangeHeightIndex.GetOffset(desiredSlot) +
                                  Math.Clamp(desiredWithinSlot, 0, _rangeHeightIndex.GetHeight(desiredSlot));
            return;
        }
        if (!anchor.HasValue)
        {
            return;
        }
        ReconcileCommittedRangeOffset(_rangePresentationIndex.RestoreAnchor(anchor.Value));
    }

    private void SeedRangeDetailsHeights(
        DataGridPresentationSnapshot snapshot,
        SparseHeightDeltaIndex heightIndex,
        bool preserveViewportAnchor)
    {
        if (RowDetailsVisibilityMode == DataGridRowDetailsVisibilityMode.Visible ||
            !double.IsFinite(RowDetailsHeightEstimate) ||
            RowDetailsHeightEstimate <= 0)
        {
            return;
        }

        var anchor = preserveViewportAnchor ? CaptureFirstCompleteRangeAnchor() : null;
        var baseHeight = GetRangeBaseRowHeight();
        var changed = false;
        foreach (var block in snapshot.Blocks)
        {
            for (var offset = 0; offset < block.Entries.Length; offset++)
            {
                var entry = block.Entries[offset];
                if (entry.Kind != DataGridSourceEntryKind.Data ||
                    !_rangeDetailsVisibility.TryGetValue(entry.RowKey, out var isVisible) ||
                    !isVisible)
                {
                    continue;
                }

                _rangeDetailsSlotHints[entry.RowKey] = (
                    snapshot.DataGeneration,
                    checked(block.Range.StartIndex + offset));
            }
        }

        foreach (var state in _rangeDetailsVisibility)
        {
            if (!state.Value ||
                !_rangeDetailsSlotHints.TryGetValue(state.Key, out var hint) ||
                (hint.DataGeneration != snapshot.DataGeneration &&
                 (hint.DataGeneration != 0 ||
                  !snapshot.Query.Groups.IsEmpty ||
                  snapshot.PageRequest is not null)) ||
                hint.Slot < 0 ||
                hint.Slot >= snapshot.TotalEntryCount)
            {
                continue;
            }

            var detailsHeight = _rangeMeasuredDetailsHeights.TryGetValue(
                state.Key,
                out var measuredDetailsHeight)
                ? measuredDetailsHeight
                : RowDetailsHeightEstimate;
            var targetHeight = SparseHeightDeltaIndex.SafeAdd(baseHeight, detailsHeight);
            if (heightIndex.TryGetMeasuredHeight(hint.Slot, out var currentHeight) &&
                MathUtils.GreaterThanOrClose(currentHeight, targetHeight))
            {
                continue;
            }
            heightIndex.SetMeasuredHeight(
                hint.Slot,
                targetHeight,
                DataGridHeightClass.Data);
            changed = true;
        }

        if (!changed || !anchor.HasValue || _rangePresentationIndex is null)
        {
            return;
        }
        // 详情高度批量写入后高度空间已变，旧滚动目标不能再参与锚点恢复
        ClearRangeScrollIntent();
        ReconcileCommittedRangeOffset(_rangePresentationIndex.RestoreAnchor(anchor.Value));
    }

    private void ApplyRangeVerticalOffset(double offset, int firstVisibleSlot)
    {
        NegVerticalOffset = Math.Max(0, offset - GetRangeOffset(firstVisibleSlot));
        SetVerticalOffset(offset);
    }

    private double GetRangeBaseRowHeight()
    {
        if (!double.IsNaN(RowHeight) && double.IsFinite(RowHeight) && RowHeight > 0)
        {
            return RowHeight;
        }
        return double.IsFinite(RowHeightEstimate) && RowHeightEstimate > 0
            ? RowHeightEstimate
            : DefaultRowHeight;
    }

    private double GetRangeDefaultHeight(
        DataGridPresentationSnapshot? snapshot = null,
        bool useDefaultRowHeightEstimate = false)
    {
        var baseHeight = useDefaultRowHeightEstimate && double.IsNaN(RowHeight)
            ? DefaultRowHeight
            : GetRangeBaseRowHeight();
        if (RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.Visible ||
            !double.IsFinite(RowDetailsHeightEstimate) ||
            RowDetailsHeightEstimate <= 0)
        {
            return baseHeight;
        }

        snapshot ??= _rangePresentationIndex?.Snapshot;
        if (snapshot is null)
        {
            return Query.Groups.IsEmpty
                ? SparseHeightDeltaIndex.SafeAdd(baseHeight, RowDetailsHeightEstimate)
                : baseHeight;
        }
        if (snapshot.TotalEntryCount == 0 || snapshot.WindowDataCount == 0)
        {
            return baseHeight;
        }
        var totalDetailsHeight = SparseHeightDeltaIndex.SafeMultiply(
            snapshot.WindowDataCount,
            RowDetailsHeightEstimate);
        return SparseHeightDeltaIndex.SafeAdd(
            baseHeight,
            totalDetailsHeight / snapshot.TotalEntryCount);
    }
}
