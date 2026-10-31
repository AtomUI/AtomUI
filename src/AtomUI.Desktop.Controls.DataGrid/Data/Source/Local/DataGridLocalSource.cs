using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace AtomUI.Desktop.Controls;

public sealed class DataGridLocalSource<T> : IDataGridMovableSource, IDataGridMoveAvailability, IDisposable
{
    private readonly object _gate = new();
    private readonly IReadOnlyList<T> _items;
    private readonly IList<T>? _mutableItems;
    private readonly Func<T, DataGridRowKey> _rowKey;
    private readonly Dictionary<DataGridFieldId, IDataGridLocalField<T>> _fields;
    private readonly int _maximumProjectionCount;
    private readonly string _sourceIdentity = Guid.NewGuid().ToString("N");
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly ProjectionCache _projectionCache;
    private readonly WeakCollectionChangedSubscription? _collectionSubscription;
    private CancellationTokenSource _versionCancellation = new();
    private CapturedSource? _capturedSource;
    private DataGridSnapshotId _snapshot;
    private long _sourceVersion;
    private bool _isDisposed;

    private DataGridLocalSource(
        IReadOnlyList<T> items,
        DataGridLocalSourceDescriptor<T> descriptor,
        DataGridLocalSourceOptions options)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        _items = items;
        _mutableItems = items is IList<T> { IsReadOnly: false } list ? list : null;
        _rowKey = descriptor.RowKey;
        _fields = new Dictionary<DataGridFieldId, IDataGridLocalField<T>>(descriptor.Fields.Count);
        var schemas = ImmutableArray.CreateBuilder<DataGridFieldSchema>(descriptor.Fields.Count);
        foreach (var field in descriptor.Fields)
        {
            if (!_fields.TryAdd(field.Schema.Id, field))
            {
                throw new ArgumentException(
                    $"Field '{field.Schema.Id}' occurs more than once.", nameof(descriptor));
            }
            schemas.Add(field.Schema);
        }

        _maximumProjectionCount = options.MaximumProjectionCount;
        _projectionCache = new ProjectionCache(_maximumProjectionCount);
        Schema = new DataGridSourceSchema(
            typeof(T),
            schemas.MoveToImmutable(),
            options.PreferredRangeSize,
            options.MaximumRangeSize);
        _snapshot = CreateSnapshot(_sourceVersion);

        if (items is INotifyCollectionChanged observable)
        {
            _collectionSubscription = new WeakCollectionChangedSubscription(observable, this);
        }
    }

    public DataGridSourceSchema Schema { get; }

    public event EventHandler? Invalidated;

    /// <summary>
    /// Whether rows can be reordered, which requires the backing items to be a writable <see cref="IList{T}"/>.
    /// </summary>
    public bool CanMove => _mutableItems is not null;

    internal int CachedProjectionCount
    {
        get
        {
            lock (_gate)
            {
                return _projectionCache.Count;
            }
        }
    }

    public static DataGridLocalSource<T> Create(
        IReadOnlyList<T> items,
        DataGridLocalSourceDescriptor<T> descriptor,
        DataGridLocalSourceOptions? options = null) =>
        new(items, descriptor, options ?? new DataGridLocalSourceOptions());

    public ValueTask<DataGridRangeResult> FetchAsync(
        DataGridFetchRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DataGridSourceContractValidator.ValidateRequest(request, Schema);

        if (request.Query.Sorts.IsEmpty &&
            request.Query.Filters.IsEmpty &&
            request.Query.Groups.IsEmpty)
        {
            return new ValueTask<DataGridRangeResult>(
                CreateIdentityRangeResult(request, cancellationToken));
        }

        CapturedSource capturedSource;
        CancellationTokenSource linkedCancellation;
        lock (_gate)
        {
            ThrowIfDisposed();
            capturedSource = _capturedSource ??= CaptureSource();
            if (request.ExpectedSnapshot is { } expectedSnapshot &&
                expectedSnapshot != capturedSource.Snapshot)
            {
                throw new DataGridSnapshotExpiredException(
                    $"Snapshot '{expectedSnapshot}' has expired; current snapshot is '{capturedSource.Snapshot}'.");
            }
            if (TryCreateCachedResult(
                    request, capturedSource, cancellationToken, out var cachedResult))
            {
                return new ValueTask<DataGridRangeResult>(cachedResult);
            }
            linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _disposeCancellation.Token,
                _versionCancellation.Token);
        }

        return new ValueTask<DataGridRangeResult>(
            FetchCoreAsync(request, capturedSource, linkedCancellation));
    }

    private DataGridRangeResult CreateIdentityRangeResult(
        DataGridFetchRequest request,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            if (request.ExpectedSnapshot is { } expectedSnapshot && expectedSnapshot != _snapshot)
            {
                throw new DataGridSnapshotExpiredException(
                    $"Snapshot '{expectedSnapshot}' has expired; current snapshot is '{_snapshot}'.");
            }

            var totalCount = _items.Count;
            var pageStart = GetPageStart(request.PageRequest, totalCount);
            var windowCount = GetWindowCount(request.PageRequest, totalCount, pageStart);
            var entryCount = GetRequestedEntryCount(
                request.Range.StartIndex,
                request.Range.Count,
                windowCount);
            var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(entryCount);
            for (var offset = 0; offset < entryCount; offset++)
            {
                CheckCancellation(offset, cancellationToken);
                var windowDataIndex = request.Range.StartIndex + offset;
                var dataIndex = checked(pageStart + windowDataIndex);
                var item = _items[dataIndex];
                entries.Add(DataGridSourceEntry.CreateData(
                    _rowKey(item),
                    item,
                    windowDataIndex,
                    dataIndex));
            }

            return new DataGridRangeResult(
                request.Range.StartIndex,
                entries.MoveToImmutable(),
                windowCount,
                windowCount,
                totalCount,
                _snapshot);
        }
    }

    private bool TryCreateCachedResult(
        DataGridFetchRequest request,
        CapturedSource capturedSource,
        CancellationToken cancellationToken,
        out DataGridRangeResult result)
    {
        var baseKey = new BaseProjectionKey(capturedSource.Version, request.Query);
        if (!_projectionCache.TryGet(baseKey, out BaseProjection? baseProjection))
        {
            result = null!;
            return false;
        }

        var pageStart = GetPageStart(request.PageRequest, baseProjection!.Count);
        var windowCount = GetWindowCount(request.PageRequest, baseProjection.Count, pageStart);
        if (request.Query.Groups.IsEmpty)
        {
            result = CreateUngroupedResult(
                request,
                baseProjection,
                pageStart,
                windowCount,
                cancellationToken);
            return true;
        }

        var groupedKey = new GroupedProjectionKey(
            capturedSource.Version,
            request.Query,
            request.PageRequest,
            request.GroupExpansion);
        if (!_projectionCache.TryGet(groupedKey, out GroupedProjection? groupedProjection))
        {
            result = null!;
            return false;
        }
        result = CreateGroupedResult(
            request, groupedProjection!, baseProjection, cancellationToken);
        return true;
    }

    public ValueTask<DataGridMutationResult> MoveAsync(
        DataGridMoveRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_mutableItems is not { } items)
        {
            throw new NotSupportedException("The local items are read-only and cannot be reordered.");
        }
        if (!request.Query.Sorts.IsEmpty ||
            !request.Query.Filters.IsEmpty ||
            !request.Query.Groups.IsEmpty)
        {
            throw new NotSupportedException("Local rows can only be moved while they are shown in source order.");
        }

        int sourceIndex;
        int insertIndex;
        lock (_gate)
        {
            ThrowIfDisposed();
            if (request.Snapshot != _snapshot)
            {
                throw new DataGridSnapshotExpiredException(
                    $"Snapshot '{request.Snapshot}' has expired; current snapshot is '{_snapshot}'.");
            }
            sourceIndex = IndexOfRowKey(request.RowKey);
            var targetIndex = IndexOfRowKey(request.BeforeKey ?? request.AfterKey!.Value);
            // The insert position is expressed in the list after the moved row has been removed.
            insertIndex = targetIndex > sourceIndex ? targetIndex - 1 : targetIndex;
            if (request.AfterKey.HasValue)
            {
                insertIndex++;
            }
            if (insertIndex == sourceIndex)
            {
                return ValueTask.FromResult(new DataGridMutationResult(_snapshot));
            }
        }

        if (items is ObservableCollection<T> observable)
        {
            observable.Move(sourceIndex, insertIndex);
        }
        else
        {
            var item = items[sourceIndex];
            items.RemoveAt(sourceIndex);
            items.Insert(insertIndex, item);
        }

        // Observable items already advanced the snapshot through their change notification.
        if (_collectionSubscription is null)
        {
            HandleCollectionChanged();
        }
        lock (_gate)
        {
            return ValueTask.FromResult(new DataGridMutationResult(_snapshot));
        }
    }

    private int IndexOfRowKey(DataGridRowKey rowKey)
    {
        for (var index = 0; index < _items.Count; index++)
        {
            if (_rowKey(_items[index]) == rowKey)
            {
                return index;
            }
        }
        throw new ArgumentException($"Row key '{rowKey}' does not exist in the local source.");
    }

    public void Dispose()
    {
        CancellationTokenSource? versionCancellation;
        lock (_gate)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            versionCancellation = _versionCancellation;
            _projectionCache.Clear();
            _capturedSource = null;
            Invalidated = null;
        }

        _collectionSubscription?.Dispose();
        versionCancellation.Cancel();
        _disposeCancellation.Cancel();
        versionCancellation.Dispose();
        _disposeCancellation.Dispose();
    }

    private async Task<DataGridRangeResult> FetchCoreAsync(
        DataGridFetchRequest request,
        CapturedSource capturedSource,
        CancellationTokenSource linkedCancellation)
    {
        using (linkedCancellation)
        {
            var cancellationToken = linkedCancellation.Token;
            var result = await Task.Run(
                    () => FetchCore(request, capturedSource, cancellationToken),
                    cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                ThrowIfDisposed();
                if (_sourceVersion != capturedSource.Version)
                {
                    throw new OperationCanceledException(
                        "The local source changed while the range was being projected.", cancellationToken);
                }
            }
            return result;
        }
    }

    private DataGridRangeResult FetchCore(
        DataGridFetchRequest request,
        CapturedSource capturedSource,
        CancellationToken cancellationToken)
    {
        var baseProjection = GetOrCreateBaseProjection(
            capturedSource, request.Query, cancellationToken);
        var pageStart = GetPageStart(request.PageRequest, baseProjection.Count);
        var windowCount = GetWindowCount(request.PageRequest, baseProjection.Count, pageStart);

        if (request.Query.Groups.IsEmpty)
        {
            return CreateUngroupedResult(
                request,
                baseProjection,
                pageStart,
                windowCount,
                cancellationToken);
        }

        var groupedProjection = GetOrCreateGroupedProjection(
            capturedSource.Version,
            request,
            baseProjection,
            pageStart,
            windowCount,
            cancellationToken);
        return CreateGroupedResult(request, groupedProjection, baseProjection, cancellationToken);
    }

    private BaseProjection GetOrCreateBaseProjection(
        CapturedSource capturedSource,
        DataGridQuery query,
        CancellationToken cancellationToken)
    {
        var key = new BaseProjectionKey(capturedSource.Version, query);
        lock (_gate)
        {
            if (_projectionCache.TryGet(key, out BaseProjection? cached))
            {
                return cached!;
            }
        }

        var created = BuildBaseProjection(capturedSource, query, cancellationToken);
        lock (_gate)
        {
            ThrowIfDisposed();
            if (_sourceVersion != capturedSource.Version)
            {
                throw new OperationCanceledException(cancellationToken);
            }
            return _projectionCache.GetOrAdd(key, created);
        }
    }

    private BaseProjection BuildBaseProjection(
        CapturedSource capturedSource,
        DataGridQuery query,
        CancellationToken cancellationToken)
    {
        var rowKeys = capturedSource.GetOrCreateRowKeys(_rowKey, cancellationToken);
        var indices = new int[capturedSource.Items.Length];
        var activeCount = 0;
        for (var ordinal = 0; ordinal < capturedSource.Items.Length; ordinal++)
        {
            CheckCancellation(ordinal, cancellationToken);
            var include = true;
            for (var filterIndex = 0; filterIndex < query.Filters.Length; filterIndex++)
            {
                var filter = query.Filters[filterIndex];
                if (!_fields[filter.Field].Evaluate(capturedSource.Items[ordinal], filter))
                {
                    include = false;
                    break;
                }
            }
            if (include)
            {
                indices[activeCount++] = ordinal;
            }
        }

        if (activeCount > 1 && (!query.Groups.IsEmpty || !query.Sorts.IsEmpty))
        {
            var comparer = new ProjectionComparer(
                capturedSource.Items,
                GetOrderedFields(query.Groups),
                GetDirections(query.Groups),
                GetOrderedFields(query.Sorts),
                GetDirections(query.Sorts),
                cancellationToken);
            try
            {
                Array.Sort(indices, 0, activeCount, comparer);
            }
            catch (InvalidOperationException exception)
                when (exception.InnerException is OperationCanceledException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new BaseProjection(capturedSource, rowKeys, indices, activeCount);
    }

    private GroupedProjection GetOrCreateGroupedProjection(
        long sourceVersion,
        DataGridFetchRequest request,
        BaseProjection baseProjection,
        int pageStart,
        int windowCount,
        CancellationToken cancellationToken)
    {
        var key = new GroupedProjectionKey(
            sourceVersion,
            request.Query,
            request.PageRequest,
            request.GroupExpansion);
        lock (_gate)
        {
            if (_projectionCache.TryGet(key, out GroupedProjection? cached))
            {
                return cached!;
            }
        }

        var fields = GetOrderedFields(request.Query.Groups);
        var slots = new List<ProjectedSlot>(windowCount);
        HashSet<DataGridGroupKey>? collapsed = request.GroupExpansion.CollapsedGroups.IsEmpty
            ? null
            : [.. request.GroupExpansion.CollapsedGroups];
        AppendGroupLevel(
            baseProjection,
            fields,
            pageStart,
            pageStart + windowCount,
            pageStart,
            level: 0,
            parentPath: null,
            collapsed,
            slots,
            cancellationToken);
        var created = new GroupedProjection(
            slots.ToArray(),
            windowCount,
            baseProjection.Count,
            baseProjection.CapturedSource.Snapshot);

        lock (_gate)
        {
            ThrowIfDisposed();
            if (_sourceVersion != sourceVersion)
            {
                throw new OperationCanceledException(cancellationToken);
            }
            return _projectionCache.GetOrAdd(key, created);
        }
    }

    private void AppendGroupLevel(
        BaseProjection projection,
        IDataGridLocalField<T>[] fields,
        int start,
        int end,
        int pageStart,
        int level,
        string? parentPath,
        HashSet<DataGridGroupKey>? collapsed,
        List<ProjectedSlot> slots,
        CancellationToken cancellationToken)
    {
        var field = fields[level];
        var runStart = start;
        while (runStart < end)
        {
            CheckCancellation(runStart - start, cancellationToken);
            var firstOrdinal = projection.Indices[runStart];
            var runEnd = runStart + 1;
            while (runEnd < end &&
                   field.Compare(
                       projection.CapturedSource.Items[firstOrdinal],
                       projection.CapturedSource.Items[projection.Indices[runEnd]]) == 0)
            {
                CheckCancellation(runEnd - runStart, cancellationToken);
                runEnd++;
            }

            var value = field.GetScalar(projection.CapturedSource.Items[firstOrdinal]);
            var path = BuildGroupPath(parentPath, field.Schema.Id, value);
            var key = new DataGridGroupKey(path);
            AddSlot(slots, ProjectedSlot.GroupHeader(new DataGridGroupEntry(
                key,
                field.Schema.Id,
                value,
                level,
                runEnd - runStart)));

            if (collapsed is null || !collapsed.Contains(key))
            {
                if (level + 1 < fields.Length)
                {
                    AppendGroupLevel(
                        projection,
                        fields,
                        runStart,
                        runEnd,
                        pageStart,
                        level + 1,
                        path,
                        collapsed,
                        slots,
                        cancellationToken);
                }
                else
                {
                    for (var position = runStart; position < runEnd; position++)
                    {
                        CheckCancellation(position - runStart, cancellationToken);
                        AddSlot(slots, ProjectedSlot.Data(
                            projection.Indices[position],
                            position - pageStart,
                            position));
                    }
                }
            }
            runStart = runEnd;
        }
    }

    private DataGridRangeResult CreateUngroupedResult(
        DataGridFetchRequest request,
        BaseProjection projection,
        int pageStart,
        int windowCount,
        CancellationToken cancellationToken)
    {
        var entryCount = GetRequestedEntryCount(
            request.Range.StartIndex, request.Range.Count, windowCount);
        var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(entryCount);
        for (var offset = 0; offset < entryCount; offset++)
        {
            CheckCancellation(offset, cancellationToken);
            var windowDataIndex = request.Range.StartIndex + offset;
            var projectionIndex = pageStart + windowDataIndex;
            var ordinal = projection.Indices[projectionIndex];
            entries.Add(DataGridSourceEntry.CreateData(
                projection.RowKeys[ordinal],
                projection.CapturedSource.Items[ordinal],
                windowDataIndex,
                projectionIndex));
        }
        return new DataGridRangeResult(
            request.Range.StartIndex,
            entries.MoveToImmutable(),
            windowCount,
            windowCount,
            projection.Count,
            projection.CapturedSource.Snapshot);
    }

    private static DataGridRangeResult CreateGroupedResult(
        DataGridFetchRequest request,
        GroupedProjection groupedProjection,
        BaseProjection baseProjection,
        CancellationToken cancellationToken)
    {
        var entryCount = GetRequestedEntryCount(
            request.Range.StartIndex,
            request.Range.Count,
            groupedProjection.Slots.Length);
        var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(entryCount);
        for (var offset = 0; offset < entryCount; offset++)
        {
            CheckCancellation(offset, cancellationToken);
            var slot = groupedProjection.Slots[request.Range.StartIndex + offset];
            if (slot.Group is not null)
            {
                entries.Add(DataGridSourceEntry.CreateGroupHeader(slot.Group));
            }
            else
            {
                entries.Add(DataGridSourceEntry.CreateData(
                    baseProjection.RowKeys[slot.SourceOrdinal],
                    baseProjection.CapturedSource.Items[slot.SourceOrdinal],
                    slot.WindowDataIndex,
                    slot.DataIndex));
            }
        }
        return new DataGridRangeResult(
            request.Range.StartIndex,
            entries.MoveToImmutable(),
            groupedProjection.Slots.Length,
            groupedProjection.WindowDataCount,
            groupedProjection.TotalDataCount,
            groupedProjection.Snapshot);
    }

    private CapturedSource CaptureSource()
    {
        var items = new T[_items.Count];
        for (var index = 0; index < items.Length; index++)
        {
            items[index] = _items[index];
        }
        return new CapturedSource(_sourceVersion, _snapshot, items);
    }

    private IDataGridLocalField<T>[] GetOrderedFields(ImmutableArray<DataGridGroup> groups)
    {
        var fields = new IDataGridLocalField<T>[groups.Length];
        for (var index = 0; index < groups.Length; index++)
        {
            fields[index] = _fields[groups[index].Field];
        }
        return fields;
    }

    private IDataGridLocalField<T>[] GetOrderedFields(ImmutableArray<DataGridSort> sorts)
    {
        var fields = new IDataGridLocalField<T>[sorts.Length];
        for (var index = 0; index < sorts.Length; index++)
        {
            fields[index] = _fields[sorts[index].Field];
        }
        return fields;
    }

    private static DataGridSortDirection[] GetDirections(ImmutableArray<DataGridGroup> groups)
    {
        var directions = new DataGridSortDirection[groups.Length];
        for (var index = 0; index < groups.Length; index++)
        {
            directions[index] = groups[index].Direction;
        }
        return directions;
    }

    private static DataGridSortDirection[] GetDirections(ImmutableArray<DataGridSort> sorts)
    {
        var directions = new DataGridSortDirection[sorts.Length];
        for (var index = 0; index < sorts.Length; index++)
        {
            directions[index] = sorts[index].Direction;
        }
        return directions;
    }

    private static int GetPageStart(DataGridPageRequest? pageRequest, int totalCount)
    {
        if (pageRequest is null)
        {
            return 0;
        }
        return pageRequest.Value.DataStartIndex >= totalCount
            ? totalCount
            : checked((int)pageRequest.Value.DataStartIndex);
    }

    private static int GetWindowCount(
        DataGridPageRequest? pageRequest,
        int totalCount,
        int pageStart) =>
        pageRequest is null
            ? totalCount
            : Math.Min(pageRequest.Value.DataCount, totalCount - pageStart);

    private static int GetRequestedEntryCount(int start, int count, int total)
    {
        var remaining = total - start;
        return remaining <= 0 ? 0 : Math.Min(count, remaining);
    }

    private static string BuildGroupPath(
        string? parentPath,
        DataGridFieldId field,
        DataGridScalar value)
    {
        var scalar = value.ToCanonicalString();
        var builder = new StringBuilder(
            (parentPath?.Length ?? 0) + field.Value.Length + scalar.Length + 32);
        if (parentPath is not null)
        {
            builder.Append(parentPath).Append('|');
        }
        builder.Append(field.Value.Length)
               .Append(':')
               .Append(field.Value)
               .Append('=')
               .Append((int)value.Kind)
               .Append(':')
               .Append(scalar.Length)
               .Append(':')
               .Append(scalar);
        return builder.ToString();
    }

    private static void AddSlot(List<ProjectedSlot> slots, ProjectedSlot slot)
    {
        if (slots.Count == int.MaxValue)
        {
            throw new DataGridPresentationLimitExceededException(
                "The grouped local presentation exceeds the supported display-entry domain.");
        }
        slots.Add(slot);
    }

    private static void CheckCancellation(int iteration, CancellationToken cancellationToken)
    {
        if ((iteration & 1023) == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private DataGridSnapshotId CreateSnapshot(long version) =>
        new($"{_sourceIdentity}:{version}");

    private void HandleCollectionChanged()
    {
        CancellationTokenSource previousCancellation;
        lock (_gate)
        {
            if (_isDisposed)
            {
                return;
            }
            previousCancellation = _versionCancellation;
            _versionCancellation = new CancellationTokenSource();
            _sourceVersion = checked(_sourceVersion + 1);
            _snapshot = CreateSnapshot(_sourceVersion);
            _capturedSource = null;
            _projectionCache.Clear();
        }

        previousCancellation.Cancel();
        previousCancellation.Dispose();
        Invalidated?.Invoke(this, EventArgs.Empty);
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    private sealed class WeakCollectionChangedSubscription : IDisposable
    {
        private readonly WeakReference<DataGridLocalSource<T>> _owner;
        private INotifyCollectionChanged? _source;

        public WeakCollectionChangedSubscription(
            INotifyCollectionChanged source,
            DataGridLocalSource<T> owner)
        {
            _source = source;
            _owner = new WeakReference<DataGridLocalSource<T>>(owner);
            source.CollectionChanged += HandleCollectionChanged;
        }

        public void Dispose()
        {
            var source = Interlocked.Exchange(ref _source, null);
            if (source is not null)
            {
                source.CollectionChanged -= HandleCollectionChanged;
            }
        }

        private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (_owner.TryGetTarget(out var owner))
            {
                owner.HandleCollectionChanged();
            }
            else
            {
                Dispose();
            }
        }
    }

    private sealed class CapturedSource
    {
        private readonly object _rowKeyGate = new();
        private DataGridRowKey[]? _rowKeys;

        public CapturedSource(long version, DataGridSnapshotId snapshot, T[] items)
        {
            Version = version;
            Snapshot = snapshot;
            Items = items;
        }

        public long Version { get; }

        public DataGridSnapshotId Snapshot { get; }

        public T[] Items { get; }

        public DataGridRowKey[] GetOrCreateRowKeys(
            Func<T, DataGridRowKey> getRowKey,
            CancellationToken cancellationToken)
        {
            lock (_rowKeyGate)
            {
                if (_rowKeys is not null)
                {
                    return _rowKeys;
                }
            }

            var keys = new DataGridRowKey[Items.Length];
            var uniqueKeys = new HashSet<DataGridRowKey>(Items.Length);
            for (var index = 0; index < Items.Length; index++)
            {
                CheckCancellation(index, cancellationToken);
                var key = getRowKey(Items[index]);
                if (!key.IsValid)
                {
                    throw new DataGridSourceContractException(
                        $"Local row at source ordinal {index} has an invalid key.");
                }
                if (!uniqueKeys.Add(key))
                {
                    throw new DataGridSourceContractException(
                        $"Local row key '{key}' occurs more than once.");
                }
                keys[index] = key;
            }

            lock (_rowKeyGate)
            {
                return _rowKeys ??= keys;
            }
        }
    }

    private sealed class BaseProjection
    {
        public BaseProjection(
            CapturedSource capturedSource,
            DataGridRowKey[] rowKeys,
            int[] indices,
            int count)
        {
            CapturedSource = capturedSource;
            RowKeys = rowKeys;
            Indices = indices;
            Count = count;
        }

        public CapturedSource CapturedSource { get; }

        public DataGridRowKey[] RowKeys { get; }

        public int[] Indices { get; }

        public int Count { get; }
    }

    private sealed class GroupedProjection
    {
        public GroupedProjection(
            ProjectedSlot[] slots,
            int windowDataCount,
            long totalDataCount,
            DataGridSnapshotId snapshot)
        {
            Slots = slots;
            WindowDataCount = windowDataCount;
            TotalDataCount = totalDataCount;
            Snapshot = snapshot;
        }

        public ProjectedSlot[] Slots { get; }

        public int WindowDataCount { get; }

        public long TotalDataCount { get; }

        public DataGridSnapshotId Snapshot { get; }
    }

    private readonly struct ProjectedSlot
    {
        private ProjectedSlot(
            int sourceOrdinal,
            int windowDataIndex,
            long dataIndex,
            DataGridGroupEntry? group)
        {
            SourceOrdinal = sourceOrdinal;
            WindowDataIndex = windowDataIndex;
            DataIndex = dataIndex;
            Group = group;
        }

        public int SourceOrdinal { get; }

        public int WindowDataIndex { get; }

        public long DataIndex { get; }

        public DataGridGroupEntry? Group { get; }

        public static ProjectedSlot Data(int sourceOrdinal, int windowDataIndex, long dataIndex) =>
            new(sourceOrdinal, windowDataIndex, dataIndex, null);

        public static ProjectedSlot GroupHeader(DataGridGroupEntry group) =>
            new(-1, -1, -1, group);
    }

    private sealed class ProjectionComparer : IComparer<int>
    {
        private readonly T[] _items;
        private readonly IDataGridLocalField<T>[] _groupFields;
        private readonly DataGridSortDirection[] _groupDirections;
        private readonly IDataGridLocalField<T>[] _sortFields;
        private readonly DataGridSortDirection[] _sortDirections;
        private readonly CancellationToken _cancellationToken;
        private int _comparisonCount;

        public ProjectionComparer(
            T[] items,
            IDataGridLocalField<T>[] groupFields,
            DataGridSortDirection[] groupDirections,
            IDataGridLocalField<T>[] sortFields,
            DataGridSortDirection[] sortDirections,
            CancellationToken cancellationToken)
        {
            _items = items;
            _groupFields = groupFields;
            _groupDirections = groupDirections;
            _sortFields = sortFields;
            _sortDirections = sortDirections;
            _cancellationToken = cancellationToken;
        }

        public int Compare(int leftOrdinal, int rightOrdinal)
        {
            CheckCancellation(_comparisonCount++, _cancellationToken);
            var comparison = CompareFields(
                leftOrdinal, rightOrdinal, _groupFields, _groupDirections);
            if (comparison != 0)
            {
                return comparison;
            }
            comparison = CompareFields(
                leftOrdinal, rightOrdinal, _sortFields, _sortDirections);
            return comparison != 0 ? comparison : leftOrdinal.CompareTo(rightOrdinal);
        }

        private int CompareFields(
            int leftOrdinal,
            int rightOrdinal,
            IDataGridLocalField<T>[] fields,
            DataGridSortDirection[] directions)
        {
            for (var index = 0; index < fields.Length; index++)
            {
                var comparison = fields[index].Compare(
                    _items[leftOrdinal], _items[rightOrdinal]);
                if (comparison == 0)
                {
                    continue;
                }
                if (directions[index] == DataGridSortDirection.Descending)
                {
                    return comparison < 0 ? 1 : -1;
                }
                return comparison < 0 ? -1 : 1;
            }
            return 0;
        }
    }

    private readonly record struct BaseProjectionKey(long SourceVersion, DataGridQuery Query);

    private readonly record struct GroupedProjectionKey(
        long SourceVersion,
        DataGridQuery Query,
        DataGridPageRequest? PageRequest,
        DataGridGroupExpansion GroupExpansion);

    private sealed class ProjectionCache
    {
        private readonly int _capacity;
        private readonly Dictionary<object, CacheEntry> _entries = [];
        private readonly LinkedList<object> _lru = [];

        public ProjectionCache(int capacity)
        {
            _capacity = capacity;
        }

        public int Count => _entries.Count;

        public bool TryGet<TProjection>(object key, out TProjection? projection)
            where TProjection : class
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                _lru.Remove(entry.Node);
                _lru.AddFirst(entry.Node);
                projection = (TProjection)entry.Projection;
                return true;
            }
            projection = null;
            return false;
        }

        public TProjection GetOrAdd<TProjection>(object key, TProjection projection)
            where TProjection : class
        {
            if (TryGet(key, out TProjection? existing))
            {
                return existing!;
            }
            var node = _lru.AddFirst(key);
            _entries.Add(key, new CacheEntry(projection, node));
            while (_entries.Count > _capacity)
            {
                var oldest = _lru.Last!;
                _lru.RemoveLast();
                _entries.Remove(oldest.Value);
            }
            return projection;
        }

        public void Clear()
        {
            _entries.Clear();
            _lru.Clear();
        }

        private sealed record CacheEntry(object Projection, LinkedListNode<object> Node);
    }
}

public static class DataGridLocalSource
{
    public static DataGridLocalSource<T> Create<T>(
        IReadOnlyList<T> items,
        DataGridLocalSourceDescriptor<T> descriptor,
        DataGridLocalSourceOptions? options = null) =>
        DataGridLocalSource<T>.Create(items, descriptor, options);
}
