using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

internal sealed class DataGridRangeCoordinator : IDisposable
{
    private readonly object _gate = new();
    private readonly DataGridRangeBlockCache _rangeCache;
    private readonly SemaphoreSlim _requestSemaphore = new(2, 2);
    private readonly Dictionary<BlockRequestKey, RangeRequestWorkItem> _inflight = [];
    private readonly HashSet<RangeRequestWorkItem> _trackedWorkItems = [];
    private CancellationTokenSource _generationCancellation = new();
    private DataGridGeneration? _activeGeneration;
    private ViewportRequestScope? _activeViewportScope;
    private Task<DataGridValidatedRangeBlock?>? _bootstrapTask;
    private long _bootstrapEpoch = -1;
    private DataGridPresentationSnapshot? _appliedSnapshot;
    private DataGridQuery? _lastQuery;
    private Exception? _lastError;
    private Exception? _lastPrefetchError;
    private long _queryRevision;
    private long _dataGeneration;
    private long _epoch;
    private long _viewportIntent;
    private int _activeRequestCount;
    private int _snapshotExpiryRestartCount;
    private bool _isDisposed;
    private bool _semaphoreDisposed;

    public DataGridRangeCoordinator(int cacheCapacity)
    {
        _rangeCache = new DataGridRangeBlockCache(cacheCapacity);
    }

    public DataGridPresentationSnapshot? AppliedSnapshot
    {
        get
        {
            lock (_gate)
            {
                return _appliedSnapshot;
            }
        }
    }

    public Exception? LastError
    {
        get
        {
            lock (_gate)
            {
                return _lastError;
            }
        }
    }

    public int ActiveRequestCount
    {
        get
        {
            lock (_gate)
            {
                return _activeRequestCount;
            }
        }
    }

    public int InFlightBlockCount
    {
        get
        {
            lock (_gate)
            {
                return _inflight.Count;
            }
        }
    }

    public int TrackedRequestCount
    {
        get
        {
            lock (_gate)
            {
                return _trackedWorkItems.Count;
            }
        }
    }

    public int CachePinCount => _rangeCache.TotalPinCount;

    public int CacheCount => _rangeCache.Count;

    public Exception? LastPrefetchError
    {
        get
        {
            lock (_gate)
            {
                return _lastPrefetchError;
            }
        }
    }

    public int SnapshotExpiryRestartCount => Volatile.Read(ref _snapshotExpiryRestartCount);

    public int StaleCommitCount => 0;

    // 快照过期重启会在协调器内部换上新的代际；表格据此接管，否则后续视口请求都会因代际不是当前而被丢弃
    internal DataGridGeneration? ActiveGeneration
    {
        get
        {
            lock (_gate)
            {
                return _activeGeneration;
            }
        }
    }

    public DataGridGeneration BeginGeneration(
        IDataGridSource source,
        DataGridQuery query,
        DataGridPageRequest? pageRequest,
        DataGridGroupExpansion groupExpansion,
        DataGridPresentationSnapshot? fallback,
        DataGridViewportAnchor? viewportAnchor = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(groupExpansion);
        var schema = source.Schema;
        ArgumentNullException.ThrowIfNull(schema);
        ValidateRequestScope(schema, query, pageRequest, groupExpansion);

        CancellationTokenSource previousCancellation;
        DataGridPresentationSnapshot? previousSnapshot;
        ViewportRequestScope? previousScope;
        var orphanedWorkItems = new List<RangeRequestWorkItem>();
        DataGridGeneration generation;
        lock (_gate)
        {
            ThrowIfDisposed();
            if (fallback is not null && !ReferenceEquals(fallback, _appliedSnapshot))
            {
                throw new ArgumentException(
                    "The fallback must be the coordinator's currently applied snapshot.",
                    nameof(fallback));
            }
            if (_lastQuery is null || _lastQuery != query)
            {
                _queryRevision = checked(_queryRevision + 1);
                _lastQuery = query;
            }
            _dataGeneration = checked(_dataGeneration + 1);
            _epoch = checked(_epoch + 1);
            _viewportIntent = checked(_viewportIntent + 1);
            previousScope = DetachActiveViewportScopeLocked(orphanedWorkItems);
            _bootstrapTask = null;
            _bootstrapEpoch = -1;
            previousCancellation = _generationCancellation;
            _generationCancellation = new CancellationTokenSource();
            previousSnapshot = ReferenceEquals(_appliedSnapshot, fallback)
                ? null
                : _appliedSnapshot;
            _appliedSnapshot = fallback;
            _lastError = null;
            _lastPrefetchError = null;
            generation = new DataGridGeneration(
                source,
                schema,
                query,
                pageRequest,
                groupExpansion,
                _queryRevision,
                _dataGeneration,
                _epoch,
                0,
                fallback,
                _generationCancellation.Token,
                viewportAnchor);
            _activeGeneration = generation;
        }

        CancelViewportScope(previousScope, orphanedWorkItems);
        previousCancellation.Cancel();
        previousCancellation.Dispose();
        previousSnapshot?.Dispose();
        _rangeCache.ClearPassive();
        return generation;
    }

    public ValueTask<DataGridPresentationTransition> EnsureViewportAsync(
        DataGridGeneration generation,
        DataGridDesiredViewport viewport,
        CancellationToken cancellationToken = default)
    {
        var scope = BeginViewportScope(generation, viewport);
        return scope is null
            ? ValueTask.FromResult(DataGridPresentationTransition.Superseded())
            : new ValueTask<DataGridPresentationTransition>(
                EnsureViewportCoreAsync(generation, scope, cancellationToken));
    }

    public ValueTask PrefetchAsync(
        DataGridGeneration generation,
        DataGridDesiredViewport viewport,
        CancellationToken cancellationToken = default) =>
        new(PrefetchCoreAsync(generation, viewport, cancellationToken));

    public void RecordPrefetchFailure(
        DataGridGeneration generation,
        DataGridDesiredViewport viewport,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(generation);
        ArgumentNullException.ThrowIfNull(exception);
        lock (_gate)
        {
            if (IsActiveViewportScopeLocked(generation, viewport))
            {
                _lastPrefetchError = exception;
            }
        }
    }

    public IDisposable AcquireSnapshotPins(
        DataGridPresentationSnapshot snapshot,
        DataGridRangeBlockPinReason reason)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        lock (_gate)
        {
            ThrowIfDisposed();
        }

        var pins = new List<IDisposable>(snapshot.Blocks.Length);
        try
        {
            foreach (var block in snapshot.Blocks)
            {
                pins.Add(_rangeCache.AcquirePin(
                    block.CreateCacheIdentity(snapshot.Source),
                    reason));
            }
            return new SnapshotPinSet(pins);
        }
        catch
        {
            foreach (var pin in pins)
            {
                pin.Dispose();
            }
            throw;
        }
    }

    public void CancelAndClear()
    {
        CancellationTokenSource previousCancellation;
        DataGridPresentationSnapshot? previousSnapshot;
        ViewportRequestScope? previousScope;
        var orphanedWorkItems = new List<RangeRequestWorkItem>();
        lock (_gate)
        {
            ThrowIfDisposed();
            _dataGeneration = checked(_dataGeneration + 1);
            _epoch = checked(_epoch + 1);
            _viewportIntent = checked(_viewportIntent + 1);
            previousScope = DetachActiveViewportScopeLocked(orphanedWorkItems);
            _bootstrapTask = null;
            _bootstrapEpoch = -1;
            previousCancellation = _generationCancellation;
            _generationCancellation = new CancellationTokenSource();
            _activeGeneration = null;
            previousSnapshot = _appliedSnapshot;
            _appliedSnapshot = null;
            _lastError = null;
            _lastPrefetchError = null;
        }

        CancelViewportScope(previousScope, orphanedWorkItems);
        previousCancellation.Cancel();
        previousCancellation.Dispose();
        previousSnapshot?.Dispose();
        _rangeCache.Clear();
    }

    public void Dispose()
    {
        CancellationTokenSource generationCancellation;
        DataGridPresentationSnapshot? previousSnapshot;
        ViewportRequestScope? previousScope;
        var orphanedWorkItems = new List<RangeRequestWorkItem>();
        var disposeSemaphore = false;
        lock (_gate)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _dataGeneration = checked(_dataGeneration + 1);
            _epoch = checked(_epoch + 1);
            _viewportIntent = checked(_viewportIntent + 1);
            previousScope = DetachActiveViewportScopeLocked(orphanedWorkItems);
            _bootstrapTask = null;
            _bootstrapEpoch = -1;
            generationCancellation = _generationCancellation;
            _activeGeneration = null;
            previousSnapshot = _appliedSnapshot;
            _appliedSnapshot = null;
            _lastError = null;
            _lastPrefetchError = null;
            if (_trackedWorkItems.Count == 0)
            {
                _semaphoreDisposed = true;
                disposeSemaphore = true;
            }
        }

        CancelViewportScope(previousScope, orphanedWorkItems);
        generationCancellation.Cancel();
        generationCancellation.Dispose();
        previousSnapshot?.Dispose();
        _rangeCache.Clear();
        if (disposeSemaphore)
        {
            _requestSemaphore.Dispose();
        }
    }

    private async Task<DataGridPresentationTransition> EnsureViewportCoreAsync(
        DataGridGeneration generation,
        ViewportRequestScope scope,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource? linkedCancellation = null;
        var waitCancellation = scope.Token;
        if (cancellationToken.CanBeCanceled)
        {
            linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                waitCancellation,
                cancellationToken);
            waitCancellation = linkedCancellation.Token;
        }

        try
        {
            if (TryGetCommittedSnapshot(generation, scope, out var committed))
            {
                return new DataGridPresentationTransition(
                    DataGridPresentationTransitionKind.Committed,
                    committed,
                    null);
            }

            if (generation.ResultIdentity is null)
            {
                var bootstrapStart = AlignBlockStart(
                    scope.Viewport.FirstVisibleIndex,
                    generation.Schema.PreferredRangeSize);
                var bootstrapTask = GetOrFetchBootstrapBlock(generation, bootstrapStart);
                var bootstrap = await bootstrapTask.WaitAsync(waitCancellation).ConfigureAwait(false);
                if (bootstrap is null || !IsActiveViewportScope(generation, scope))
                {
                    return DataGridPresentationTransition.Superseded();
                }
            }

            var identity = GetCurrentResultIdentity(generation);
            if (identity is null)
            {
                return DataGridPresentationTransition.Superseded();
            }
            if (generation.PageRequest is { } pageRequest &&
                pageRequest.DataStartIndex > 0 &&
                pageRequest.DataStartIndex >= identity.TotalDataCount)
            {
                var legalStart = identity.TotalDataCount == 0
                    ? 0
                    : ((identity.TotalDataCount - 1) / pageRequest.DataCount) * pageRequest.DataCount;
                return DataGridPresentationTransition.Redirected(
                    new DataGridPageRequest(legalStart, pageRequest.DataCount));
            }
            if (identity.TotalEntryCount == 0)
            {
                return Commit(
                    generation,
                    scope,
                    new DataGridDesiredViewport(0, 0, scope.Viewport.ScrollDirection),
                    ImmutableArray<DataGridValidatedRangeBlock>.Empty,
                    identity);
            }

            var loadPlan = PrepareVisibleBlocks(generation, scope, identity);
            StartWorkItems(loadPlan.WorkItemsToStart);

            var loadedBlocks = await Task.WhenAll(loadPlan.BlockTasks)
                                         .WaitAsync(waitCancellation)
                                         .ConfigureAwait(false);
            if (!IsActiveViewportScope(generation, scope))
            {
                return DataGridPresentationTransition.Superseded();
            }

            var blocks = ImmutableArray.CreateBuilder<DataGridValidatedRangeBlock>(loadedBlocks.Length);
            foreach (var block in loadedBlocks)
            {
                if (block is null)
                {
                    return DataGridPresentationTransition.Superseded();
                }
                blocks.Add(block);
            }
            return Commit(
                generation,
                scope,
                loadPlan.CommittedViewport,
                blocks.MoveToImmutable(),
                identity);
        }
        catch (DataGridSnapshotExpiredException exception)
        {
            var restarted = RestartAfterSnapshotExpiry(generation);
            if (restarted is null)
            {
                return IsCurrent(generation)
                    ? Fail(generation, scope, exception)
                    : DataGridPresentationTransition.Superseded();
            }
            return await EnsureViewportAsync(restarted, scope.Viewport, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return DataGridPresentationTransition.Superseded();
        }
        catch (Exception exception)
        {
            return IsActiveViewportScope(generation, scope)
                ? Fail(generation, scope, exception)
                : DataGridPresentationTransition.Superseded();
        }
        finally
        {
            linkedCancellation?.Dispose();
        }
    }

    private async Task PrefetchCoreAsync(
        DataGridGeneration generation,
        DataGridDesiredViewport viewport,
        CancellationToken cancellationToken)
    {
        DataGridSourceResultIdentity? resultIdentity;
        ViewportRequestScope? scope;
        lock (_gate)
        {
            if (!IsCurrentLocked(generation) ||
                _activeViewportScope is not { CommittedSnapshot: not null } activeScope ||
                !IsSameViewportTarget(activeScope, viewport, generation.ResultIdentity))
            {
                return;
            }
            scope = activeScope;
            resultIdentity = generation.ResultIdentity;
        }
        if (resultIdentity is null || resultIdentity.TotalEntryCount == 0 || viewport.VisibleCount == 0)
        {
            return;
        }

        var totalEntryCount = resultIdentity.TotalEntryCount;
        var visibleStart = Math.Min(viewport.FirstVisibleIndex, totalEntryCount - 1);
        var visibleEnd = Math.Min(
            totalEntryCount,
            checked(visibleStart + viewport.VisibleCount));
        var beforeStart = Math.Max(0, visibleStart - viewport.VisibleCount);
        var afterEnd = Math.Min(
            totalEntryCount,
            checked(visibleEnd + viewport.VisibleCount));
        var blockSize = generation.Schema.PreferredRangeSize;
        var blockStarts = new List<int>();
        if (viewport.ScrollDirection >= 0)
        {
            AppendBlockStarts(blockStarts, visibleEnd, afterEnd, blockSize);
            AppendBlockStarts(blockStarts, beforeStart, visibleStart, blockSize);
        }
        else
        {
            AppendBlockStarts(blockStarts, beforeStart, visibleStart, blockSize);
            AppendBlockStarts(blockStarts, visibleEnd, afterEnd, blockSize);
        }
        if (blockStarts.Count == 0)
        {
            return;
        }

        Task<DataGridValidatedRangeBlock?>[] tasks;
        RangeRequestWorkItem[] workItemsToStart;
        lock (_gate)
        {
            if (!IsActiveViewportScopeLocked(generation, scope) ||
                scope.CommittedSnapshot is null)
            {
                return;
            }
            var newWorkItems = new List<RangeRequestWorkItem>();
            tasks = new Task<DataGridValidatedRangeBlock?>[blockStarts.Count];
            for (var index = 0; index < blockStarts.Count; index++)
            {
                tasks[index] = GetOrCreateBlockTaskLocked(
                    generation,
                    scope,
                    blockStarts[index],
                    isBootstrap: false,
                    newWorkItems);
            }
            workItemsToStart = newWorkItems.ToArray();
        }
        StartWorkItems(workItemsToStart);

        CancellationTokenSource? linkedCancellation = null;
        var waitCancellation = scope.Token;
        if (cancellationToken.CanBeCanceled)
        {
            linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                waitCancellation,
                cancellationToken);
            waitCancellation = linkedCancellation.Token;
        }
        try
        {
            await Task.WhenAll(tasks).WaitAsync(waitCancellation).ConfigureAwait(false);
        }
        finally
        {
            linkedCancellation?.Dispose();
        }
    }

    private static void AppendBlockStarts(
        List<int> destination,
        int rangeStart,
        int rangeEndExclusive,
        int blockSize)
    {
        if (rangeStart >= rangeEndExclusive)
        {
            return;
        }
        var first = AlignBlockStart(rangeStart, blockSize);
        var last = AlignBlockStart(rangeEndExclusive - 1, blockSize);
        for (var blockStart = first;; blockStart = checked(blockStart + blockSize))
        {
            if (!destination.Contains(blockStart))
            {
                destination.Add(blockStart);
            }
            if (blockStart == last)
            {
                break;
            }
        }
    }

    private ViewportRequestScope? BeginViewportScope(
        DataGridGeneration generation,
        DataGridDesiredViewport viewport)
    {
        ViewportRequestScope? previousScope = null;
        var orphanedWorkItems = new List<RangeRequestWorkItem>();
        var newWorkItems = new List<RangeRequestWorkItem>();
        ViewportRequestScope scope;
        lock (_gate)
        {
            if (!IsCurrentLocked(generation))
            {
                return null;
            }
            if (_activeViewportScope is { IsSuperseded: false, HasTerminalFailure: false } activeScope &&
                IsSameViewportTarget(activeScope, viewport, generation.ResultIdentity))
            {
                activeScope.Viewport = viewport;
                return activeScope;
            }

            scope = new ViewportRequestScope(checked(++_viewportIntent), viewport);
            if (generation.ResultIdentity is { } identity)
            {
                PrepareVisibleBlocksLocked(generation, scope, identity, newWorkItems);
            }

            previousScope = _activeViewportScope;
            _activeViewportScope = scope;
            if (previousScope is not null)
            {
                ReleaseScopeLeasesLocked(previousScope, orphanedWorkItems);
            }
        }

        CancelViewportScope(previousScope, orphanedWorkItems);
        StartWorkItems(newWorkItems);
        return scope;
    }

    private Task<DataGridValidatedRangeBlock?> GetOrFetchBootstrapBlock(
        DataGridGeneration generation,
        int blockStart)
    {
        RangeRequestWorkItem[] workItemsToStart;
        Task<DataGridValidatedRangeBlock?> task;
        lock (_gate)
        {
            if (!IsCurrentLocked(generation))
            {
                return Task.FromResult<DataGridValidatedRangeBlock?>(null);
            }
            if (_bootstrapEpoch == generation.Epoch && _bootstrapTask is not null)
            {
                return _bootstrapTask;
            }

            var newWorkItems = new List<RangeRequestWorkItem>(1);
            task = GetOrCreateBlockTaskLocked(
                generation,
                scope: null,
                blockStart,
                isBootstrap: true,
                newWorkItems);
            _bootstrapEpoch = generation.Epoch;
            _bootstrapTask = task;
            workItemsToStart = newWorkItems.ToArray();
        }
        StartWorkItems(workItemsToStart);
        return task;
    }

    private VisibleBlockLoadPlan PrepareVisibleBlocks(
        DataGridGeneration generation,
        ViewportRequestScope scope,
        DataGridSourceResultIdentity identity)
    {
        RangeRequestWorkItem[] workItemsToStart;
        lock (_gate)
        {
            if (!IsActiveViewportScopeLocked(generation, scope))
            {
                throw new OperationCanceledException(scope.Token);
            }
            if (!scope.BlocksPrepared)
            {
                var newWorkItems = new List<RangeRequestWorkItem>();
                PrepareVisibleBlocksLocked(generation, scope, identity, newWorkItems);
                workItemsToStart = newWorkItems.ToArray();
            }
            else
            {
                workItemsToStart = [];
            }
            return new VisibleBlockLoadPlan(
                scope.CommittedViewport,
                scope.BlockTasks,
                workItemsToStart);
        }
    }

    private void PrepareVisibleBlocksLocked(
        DataGridGeneration generation,
        ViewportRequestScope scope,
        DataGridSourceResultIdentity identity,
        List<RangeRequestWorkItem> newWorkItems)
    {
        if (scope.BlocksPrepared)
        {
            return;
        }
        if (identity.TotalEntryCount == 0)
        {
            scope.CommittedViewport = new DataGridDesiredViewport(
                0,
                0,
                scope.Viewport.ScrollDirection);
            scope.BlockTasks = [];
            scope.BlocksPrepared = true;
            return;
        }

        var visibleStart = Math.Min(scope.Viewport.FirstVisibleIndex, identity.TotalEntryCount - 1);
        var visibleEnd = (int)Math.Min(
            identity.TotalEntryCount,
            (long)visibleStart + Math.Max(1, scope.Viewport.VisibleCount));
        var blockSize = generation.Schema.PreferredRangeSize;
        var firstBlockStart = AlignBlockStart(visibleStart, blockSize);
        var lastBlockStart = AlignBlockStart(visibleEnd - 1, blockSize);
        var blockCount = ((lastBlockStart - firstBlockStart) / blockSize) + 1;
        var blockTasks = new Task<DataGridValidatedRangeBlock?>[blockCount];
        for (var index = 0; index < blockCount; index++)
        {
            var blockStart = checked(firstBlockStart + index * blockSize);
            blockTasks[index] = GetOrCreateBlockTaskLocked(
                generation,
                scope,
                blockStart,
                isBootstrap: false,
                newWorkItems);
        }

        scope.CommittedViewport = new DataGridDesiredViewport(
            visibleStart,
            Math.Max(1, visibleEnd - visibleStart),
            scope.Viewport.ScrollDirection);
        scope.BlockTasks = blockTasks;
        scope.BlocksPrepared = true;
    }

    private Task<DataGridValidatedRangeBlock?> GetOrCreateBlockTaskLocked(
        DataGridGeneration generation,
        ViewportRequestScope? scope,
        int blockStart,
        bool isBootstrap,
        List<RangeRequestWorkItem> newWorkItems)
    {
        if (!IsCurrentLocked(generation))
        {
            return Task.FromResult<DataGridValidatedRangeBlock?>(null);
        }
        if (generation.ResultIdentity is { } resultIdentity)
        {
            var identity = new DataGridRangeBlockIdentity(
                generation.Source,
                generation.QueryRevision,
                generation.DataGeneration,
                resultIdentity.Snapshot,
                blockStart);
            if (_rangeCache.TryGetBlock(identity, out var cached))
            {
                return Task.FromResult(cached);
            }
        }

        var key = new BlockRequestKey(generation.Epoch, blockStart);
        if (!_inflight.TryGetValue(key, out var workItem))
        {
            var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                generation.CancellationToken,
                CancellationToken.None);
            workItem = new RangeRequestWorkItem(
                key,
                generation,
                blockStart,
                generation.ResultIdentity,
                requestCancellation,
                isBootstrap);
            _inflight.Add(key, workItem);
            _trackedWorkItems.Add(workItem);
            newWorkItems.Add(workItem);
        }

        if (scope is not null && workItem.LeaseOwners.Add(scope.Intent))
        {
            scope.Leases.Add(workItem);
        }
        return workItem.Completion.Task;
    }

    private void StartWorkItems(IEnumerable<RangeRequestWorkItem> workItems)
    {
        foreach (var workItem in workItems)
        {
            lock (_gate)
            {
                if (workItem.IsStarted)
                {
                    continue;
                }
                workItem.IsStarted = true;
            }

            // Never invoke user Source code while holding the coordinator gate. A
            // genuinely synchronous Source may still complete in this call.
            _ = FetchTrackedBlockAsync(workItem);
        }
    }

    private async Task FetchTrackedBlockAsync(RangeRequestWorkItem workItem)
    {
        try
        {
            var block = await FetchBlockAsync(workItem).ConfigureAwait(false);
            workItem.Completion.TrySetResult(block);
        }
        catch (OperationCanceledException exception)
        {
            workItem.Completion.TrySetCanceled(exception.CancellationToken);
        }
        catch (Exception exception)
        {
            if (IsWorkItemUsable(workItem))
            {
                workItem.Completion.TrySetException(exception);
            }
            else
            {
                workItem.Completion.TrySetCanceled(workItem.Cancellation.Token);
            }
        }
        finally
        {
            var disposeSemaphore = false;
            lock (_gate)
            {
                if (_inflight.TryGetValue(workItem.Key, out var current) &&
                    ReferenceEquals(current, workItem))
                {
                    _inflight.Remove(workItem.Key);
                }
                _trackedWorkItems.Remove(workItem);
                if (_isDisposed && _trackedWorkItems.Count == 0 && !_semaphoreDisposed)
                {
                    _semaphoreDisposed = true;
                    disposeSemaphore = true;
                }
            }
            workItem.Cancellation.Dispose();
            if (disposeSemaphore)
            {
                _requestSemaphore.Dispose();
            }
        }
    }

    private async Task<DataGridValidatedRangeBlock?> FetchBlockAsync(
        RangeRequestWorkItem workItem)
    {
        var generation = workItem.Generation;
        var blockStart = workItem.BlockStart;
        var expectedIdentity = workItem.ExpectedIdentity;
        if (!IsWorkItemUsable(workItem))
        {
            return null;
        }

        var maximumCount = int.MaxValue - blockStart;
        var count = Math.Min(generation.Schema.PreferredRangeSize, maximumCount);
        if (count <= 0)
        {
            throw new DataGridPresentationLimitExceededException(
                "The requested display block exceeds the supported index domain.");
        }
        var request = new DataGridFetchRequest(
            generation.Query,
            generation.PageRequest,
            generation.GroupExpansion,
            new DataGridRange(blockStart, count),
            expectedIdentity?.Snapshot,
            generation.QueryRevision,
            generation.DataGeneration);
        DataGridSourceContractValidator.ValidateRequest(request, generation.Schema);

        var requestCancellation = workItem.Cancellation.Token;
        var enteredSemaphore = false;
        var activeRequestRegistered = false;
        try
        {
            await _requestSemaphore.WaitAsync(requestCancellation).ConfigureAwait(false);
            enteredSemaphore = true;
            lock (_gate)
            {
                if (!IsWorkItemUsableLocked(workItem))
                {
                    return null;
                }
                _activeRequestCount++;
                activeRequestRegistered = true;
            }

            var result = await generation.Source.FetchAsync(request, requestCancellation)
                                                .ConfigureAwait(false);
            if (!IsWorkItemUsable(workItem))
            {
                return null;
            }

            var validated = DataGridSourceContractValidator.Validate(
                request,
                result,
                generation.Schema,
                expectedIdentity);
            lock (_gate)
            {
                if (!IsWorkItemUsableLocked(workItem))
                {
                    return null;
                }
                if (generation.ResultIdentity is null)
                {
                    generation.ResultIdentity = validated.Identity;
                }
                else if (!ReferenceEquals(generation.ResultIdentity, validated.Identity))
                {
                    throw new DataGridSourceContractException(
                        "Validated block identity changed during the active generation.");
                }

                if (_rangeCache.TryAdd(generation.Source, validated))
                {
                    return validated;
                }
                var identity = validated.CreateCacheIdentity(generation.Source);
                if (_rangeCache.TryGetBlock(identity, out var cached))
                {
                    return cached;
                }
                throw new DataGridSourceContractException(
                    "The bounded range cache could not accept a required block.");
            }
        }
        finally
        {
            if (activeRequestRegistered)
            {
                lock (_gate)
                {
                    _activeRequestCount--;
                }
            }
            if (enteredSemaphore)
            {
                _requestSemaphore.Release();
            }
        }
    }

    private DataGridPresentationTransition Commit(
        DataGridGeneration generation,
        ViewportRequestScope scope,
        DataGridDesiredViewport committedViewport,
        ImmutableArray<DataGridValidatedRangeBlock> blocks,
        DataGridSourceResultIdentity identity)
    {
        lock (_gate)
        {
            if (!IsActiveViewportScopeLocked(generation, scope))
            {
                return DataGridPresentationTransition.Superseded();
            }
            if (scope.CommittedSnapshot is { } committed)
            {
                return new DataGridPresentationTransition(
                    DataGridPresentationTransitionKind.Committed,
                    committed,
                    null);
            }
        }

        var pins = ImmutableArray.CreateBuilder<IDisposable>(blocks.Length);
        DataGridPresentationSnapshot? created = null;
        try
        {
            foreach (var block in blocks)
            {
                pins.Add(_rangeCache.AcquirePin(
                    block.CreateCacheIdentity(generation.Source),
                    DataGridRangeBlockPinReason.Applied));
            }
            created = new DataGridPresentationSnapshot(
                generation,
                committedViewport,
                blocks,
                pins.MoveToImmutable(),
                identity);
        }
        catch
        {
            foreach (var pin in pins)
            {
                pin.Dispose();
            }
            throw;
        }

        DataGridPresentationSnapshot? previous;
        lock (_gate)
        {
            if (!IsActiveViewportScopeLocked(generation, scope))
            {
                created.Dispose();
                return DataGridPresentationTransition.Superseded();
            }
            if (scope.CommittedSnapshot is { } committed)
            {
                created.Dispose();
                return new DataGridPresentationTransition(
                    DataGridPresentationTransitionKind.Committed,
                    committed,
                    null);
            }
            previous = _appliedSnapshot;
            _appliedSnapshot = created;
            scope.CommittedSnapshot = created;
            generation.Fallback = null;
            _lastError = null;
        }
        if (!ReferenceEquals(previous, created))
        {
            previous?.Dispose();
        }
        return new DataGridPresentationTransition(
            DataGridPresentationTransitionKind.Committed, created, null);
    }

    private DataGridPresentationTransition Fail(
        DataGridGeneration generation,
        ViewportRequestScope scope,
        Exception exception)
    {
        DataGridPresentationTransition transition;
        lock (_gate)
        {
            if (!IsActiveViewportScopeLocked(generation, scope))
            {
                return DataGridPresentationTransition.Superseded();
            }
            scope.HasTerminalFailure = true;
            _lastError = exception;
            _rangeCache.ClearPassive();
            transition = generation.Fallback is null
                ? new DataGridPresentationTransition(
                    DataGridPresentationTransitionKind.Failed, null, exception)
                : new DataGridPresentationTransition(
                    DataGridPresentationTransitionKind.RolledBack,
                    generation.Fallback,
                    exception);
        }
        return transition;
    }

    private DataGridGeneration? RestartAfterSnapshotExpiry(DataGridGeneration generation)
    {
        CancellationTokenSource previousCancellation;
        ViewportRequestScope? previousScope;
        var orphanedWorkItems = new List<RangeRequestWorkItem>();
        DataGridGeneration restarted;
        lock (_gate)
        {
            if (!IsCurrentLocked(generation) || generation.SnapshotExpiryRestarts >= 1)
            {
                return null;
            }
            _dataGeneration = checked(_dataGeneration + 1);
            _epoch = checked(_epoch + 1);
            _viewportIntent = checked(_viewportIntent + 1);
            previousScope = DetachActiveViewportScopeLocked(orphanedWorkItems);
            _bootstrapTask = null;
            _bootstrapEpoch = -1;
            previousCancellation = _generationCancellation;
            _generationCancellation = new CancellationTokenSource();
            restarted = new DataGridGeneration(
                generation.Source,
                generation.Schema,
                generation.Query,
                generation.PageRequest,
                generation.GroupExpansion,
                generation.QueryRevision,
                _dataGeneration,
                _epoch,
                generation.SnapshotExpiryRestarts + 1,
                generation.Fallback,
                _generationCancellation.Token,
                generation.ViewportAnchor);
            _activeGeneration = restarted;
            _snapshotExpiryRestartCount++;
        }

        CancelViewportScope(previousScope, orphanedWorkItems);
        previousCancellation.Cancel();
        previousCancellation.Dispose();
        _rangeCache.ClearPassive();
        return restarted;
    }

    private DataGridSourceResultIdentity? GetCurrentResultIdentity(DataGridGeneration generation)
    {
        lock (_gate)
        {
            return IsCurrentLocked(generation) ? generation.ResultIdentity : null;
        }
    }

    private bool IsCurrent(DataGridGeneration generation)
    {
        lock (_gate)
        {
            return IsCurrentLocked(generation);
        }
    }

    private bool IsCurrentLocked(DataGridGeneration generation) =>
        !_isDisposed &&
        ReferenceEquals(_activeGeneration, generation) &&
        ReferenceEquals(_activeGeneration.Source, generation.Source) &&
        _activeGeneration.QueryRevision == generation.QueryRevision &&
        _activeGeneration.DataGeneration == generation.DataGeneration &&
        _activeGeneration.Epoch == generation.Epoch;

    private bool TryGetCommittedSnapshot(
        DataGridGeneration generation,
        ViewportRequestScope scope,
        out DataGridPresentationSnapshot? snapshot)
    {
        lock (_gate)
        {
            if (IsActiveViewportScopeLocked(generation, scope) &&
                scope.CommittedSnapshot is { } committed)
            {
                snapshot = committed;
                return true;
            }
            snapshot = null;
            return false;
        }
    }

    private bool IsActiveViewportScope(
        DataGridGeneration generation,
        ViewportRequestScope scope)
    {
        lock (_gate)
        {
            return IsActiveViewportScopeLocked(generation, scope);
        }
    }

    private bool IsActiveViewportScope(
        DataGridGeneration generation,
        DataGridDesiredViewport viewport)
    {
        lock (_gate)
        {
            return IsActiveViewportScopeLocked(generation, viewport);
        }
    }

    private bool IsActiveViewportScopeLocked(
        DataGridGeneration generation,
        ViewportRequestScope scope) =>
        IsCurrentLocked(generation) &&
        !scope.IsSuperseded &&
        ReferenceEquals(_activeViewportScope, scope);

    private bool IsActiveViewportScopeLocked(
        DataGridGeneration generation,
        DataGridDesiredViewport viewport) =>
        IsCurrentLocked(generation) &&
        _activeViewportScope is { IsSuperseded: false } scope &&
        IsSameViewportTarget(scope, viewport, generation.ResultIdentity);

    private bool IsWorkItemUsable(RangeRequestWorkItem workItem)
    {
        lock (_gate)
        {
            return IsWorkItemUsableLocked(workItem);
        }
    }

    private bool IsWorkItemUsableLocked(RangeRequestWorkItem workItem) =>
        !workItem.IsOrphaned &&
        IsCurrentLocked(workItem.Generation) &&
        _inflight.TryGetValue(workItem.Key, out var current) &&
        ReferenceEquals(current, workItem);

    private ViewportRequestScope? DetachActiveViewportScopeLocked(
        List<RangeRequestWorkItem> orphanedWorkItems)
    {
        var scope = _activeViewportScope;
        _activeViewportScope = null;
        if (scope is not null)
        {
            ReleaseScopeLeasesLocked(scope, orphanedWorkItems);
        }
        return scope;
    }

    private void ReleaseScopeLeasesLocked(
        ViewportRequestScope scope,
        List<RangeRequestWorkItem> orphanedWorkItems)
    {
        scope.IsSuperseded = true;
        foreach (var workItem in scope.Leases)
        {
            workItem.LeaseOwners.Remove(scope.Intent);
            if (workItem.LeaseOwners.Count == 0 &&
                !workItem.IsBootstrap &&
                _inflight.TryGetValue(workItem.Key, out var current) &&
                ReferenceEquals(current, workItem))
            {
                workItem.IsOrphaned = true;
                _inflight.Remove(workItem.Key);
                orphanedWorkItems.Add(workItem);
            }
        }
        scope.Leases.Clear();
    }

    private static void CancelViewportScope(
        ViewportRequestScope? scope,
        IEnumerable<RangeRequestWorkItem> orphanedWorkItems)
    {
        if (scope is not null)
        {
            TryCancel(scope.Cancellation);
            scope.Cancellation.Dispose();
        }
        foreach (var workItem in orphanedWorkItems)
        {
            TryCancel(workItem.Cancellation);
        }
    }

    private static void TryCancel(CancellationTokenSource cancellation)
    {
        try
        {
            cancellation.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private static bool IsSameViewportTarget(
        ViewportRequestScope scope,
        DataGridDesiredViewport viewport,
        DataGridSourceResultIdentity? identity)
    {
        var currentTarget = NormalizeViewportTarget(scope.Viewport, identity);
        var requestedTarget = NormalizeViewportTarget(viewport, identity);
        return currentTarget.FirstVisibleIndex == requestedTarget.FirstVisibleIndex &&
               currentTarget.VisibleCount == requestedTarget.VisibleCount;
    }

    private static DataGridDesiredViewport NormalizeViewportTarget(
        DataGridDesiredViewport viewport,
        DataGridSourceResultIdentity? identity)
    {
        if (identity is null)
        {
            return viewport;
        }
        if (identity.TotalEntryCount == 0)
        {
            return new DataGridDesiredViewport(0, 0, viewport.ScrollDirection);
        }
        var visibleStart = Math.Min(viewport.FirstVisibleIndex, identity.TotalEntryCount - 1);
        var visibleEnd = (int)Math.Min(
            identity.TotalEntryCount,
            (long)visibleStart + Math.Max(1, viewport.VisibleCount));
        return new DataGridDesiredViewport(
            visibleStart,
            Math.Max(1, visibleEnd - visibleStart),
            viewport.ScrollDirection);
    }

    private static int AlignBlockStart(int displayIndex, int blockSize) =>
        displayIndex - displayIndex % blockSize;

    private static void ValidateRequestScope(
        DataGridSourceSchema schema,
        DataGridQuery query,
        DataGridPageRequest? pageRequest,
        DataGridGroupExpansion groupExpansion)
    {
        var rangeSize = schema.PreferredRangeSize;
        var request = new DataGridFetchRequest(
            query,
            pageRequest,
            groupExpansion,
            new DataGridRange(0, rangeSize),
            null,
            0,
            0);
        DataGridSourceContractValidator.ValidateRequest(request, schema);
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(DataGridRangeCoordinator));
        }
    }

    private readonly record struct BlockRequestKey(long Epoch, int BlockStart);

    private readonly record struct VisibleBlockLoadPlan(
        DataGridDesiredViewport CommittedViewport,
        Task<DataGridValidatedRangeBlock?>[] BlockTasks,
        RangeRequestWorkItem[] WorkItemsToStart);

    private sealed class ViewportRequestScope
    {
        public ViewportRequestScope(long intent, DataGridDesiredViewport viewport)
        {
            Intent = intent;
            Viewport = viewport;
            Cancellation = new CancellationTokenSource();
            Token = Cancellation.Token;
        }

        public long Intent { get; }

        public DataGridDesiredViewport Viewport { get; set; }

        public CancellationTokenSource Cancellation { get; }

        public CancellationToken Token { get; }

        public HashSet<RangeRequestWorkItem> Leases { get; } = [];

        public Task<DataGridValidatedRangeBlock?>[] BlockTasks { get; set; } = [];

        public DataGridDesiredViewport CommittedViewport { get; set; }

        public DataGridPresentationSnapshot? CommittedSnapshot { get; set; }

        public bool BlocksPrepared { get; set; }

        public bool IsSuperseded { get; set; }

        public bool HasTerminalFailure { get; set; }
    }

    private sealed class RangeRequestWorkItem
    {
        public RangeRequestWorkItem(
            BlockRequestKey key,
            DataGridGeneration generation,
            int blockStart,
            DataGridSourceResultIdentity? expectedIdentity,
            CancellationTokenSource cancellation,
            bool isBootstrap)
        {
            Key = key;
            Generation = generation;
            BlockStart = blockStart;
            ExpectedIdentity = expectedIdentity;
            Cancellation = cancellation;
            IsBootstrap = isBootstrap;
        }

        public BlockRequestKey Key { get; }

        public DataGridGeneration Generation { get; }

        public int BlockStart { get; }

        public DataGridSourceResultIdentity? ExpectedIdentity { get; }

        public CancellationTokenSource Cancellation { get; }

        public TaskCompletionSource<DataGridValidatedRangeBlock?> Completion { get; } = new();

        public HashSet<long> LeaseOwners { get; } = [];

        public bool IsBootstrap { get; }

        public bool IsStarted { get; set; }

        public bool IsOrphaned { get; set; }
    }

    private sealed class SnapshotPinSet(List<IDisposable> pins) : IDisposable
    {
        private List<IDisposable>? _pins = pins;

        public void Dispose()
        {
            var pinsToRelease = Interlocked.Exchange(ref _pins, null);
            if (pinsToRelease is null)
            {
                return;
            }
            foreach (var pin in pinsToRelease)
            {
                pin.Dispose();
            }
        }
    }
}
