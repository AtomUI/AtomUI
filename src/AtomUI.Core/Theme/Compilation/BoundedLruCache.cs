namespace AtomUI.Theme.Compilation;

internal sealed class BoundedLruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly object _gate = new();
    private readonly int _entryLimit;
    private readonly long _retainedBytesLimit;
    private readonly Func<TValue, long> _retainedBytesEstimator;
    private readonly Func<TKey, long>? _keyRetainedBytesEstimator;
    private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _entries = new();
    private readonly LinkedList<CacheEntry> _lru = new();
    private readonly Dictionary<TKey, Task<TValue>> _inFlight = new();
    private long _retainedBytes;

    internal BoundedLruCache(
        int entryLimit,
        long retainedBytesLimit,
        Func<TValue, long> retainedBytesEstimator,
        Func<TKey, long>? keyRetainedBytesEstimator = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(entryLimit, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(retainedBytesLimit, 1);
        ArgumentNullException.ThrowIfNull(retainedBytesEstimator);

        _entryLimit             = entryLimit;
        _retainedBytesLimit     = retainedBytesLimit;
        _retainedBytesEstimator = retainedBytesEstimator;
        _keyRetainedBytesEstimator = keyRetainedBytesEstimator;
    }

    internal int Count
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count;
            }
        }
    }

    internal long RetainedBytes
    {
        get
        {
            lock (_gate)
            {
                return _retainedBytes;
            }
        }
    }

    internal int InFlightCount
    {
        get
        {
            lock (_gate)
            {
                return _inFlight.Count;
            }
        }
    }

    internal ValueTask<TValue> GetOrCreateAsync(
        TKey key,
        Func<CancellationToken, ValueTask<TValue>> factory)
    {
        return GetOrCreateAsync(key, factory, CancellationToken.None);
    }

    internal ValueTask<TValue> GetOrCreateAsync(
        TKey key,
        Func<CancellationToken, ValueTask<TValue>> factory,
        CancellationToken callerCancellation)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Task<TValue> production;
        TaskCompletionSource<TValue>? completion = null;
        lock (_gate)
        {
            if (_entries.TryGetValue(key, out var node))
            {
                Touch(node);
                return ValueTask.FromResult(node.Value.Value);
            }

            if (!_inFlight.TryGetValue(key, out production!))
            {
                completion = new TaskCompletionSource<TValue>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                production = completion.Task;
                _inFlight.Add(key, production);
            }
        }

        if (completion is not null)
        {
            _ = ProduceAsync(key, factory, completion);
        }

        return callerCancellation.CanBeCanceled
            ? new ValueTask<TValue>(production.WaitAsync(callerCancellation))
            : new ValueTask<TValue>(production);
    }

    private async Task ProduceAsync(
        TKey key,
        Func<CancellationToken, ValueTask<TValue>> factory,
        TaskCompletionSource<TValue> completion)
    {
        try
        {
            var value = await factory(CancellationToken.None).ConfigureAwait(false);
            lock (_gate)
            {
                // 与物化后的重新计费使用同一锁，避免插入旧尺寸覆盖先到达的物化通知。
                var retainedBytes = EstimateEntry(key, value);
                ArgumentOutOfRangeException.ThrowIfNegative(retainedBytes);
                if (retainedBytes <= _retainedBytesLimit)
                {
                    Add(key, value, retainedBytes);
                }
                // Publish completion before releasing the in-flight slot. This keeps
                // oversized values and failed productions joinable through the
                // completion boundary instead of opening a duplicate-production
                // window between Remove and TrySetResult/TrySetException.
                completion.TrySetResult(value);
                _inFlight.Remove(key);
            }
        }
        catch (OperationCanceledException exception)
        {
            lock (_gate)
            {
                completion.TrySetCanceled(exception.CancellationToken);
                _inFlight.Remove(key);
            }
        }
        catch (Exception exception)
        {
            lock (_gate)
            {
                completion.TrySetException(exception);
                _inFlight.Remove(key);
            }
        }
    }

    private long EstimateEntry(TKey key, TValue value)
    {
        var valueBytes = _retainedBytesEstimator(value);
        var keyBytes = _keyRetainedBytesEstimator?.Invoke(key) ?? 0;
        ArgumentOutOfRangeException.ThrowIfNegative(valueBytes);
        ArgumentOutOfRangeException.ThrowIfNegative(keyBytes);
        return ThemeRetainedBytesEstimator.Add(valueBytes, keyBytes);
    }

    private void Add(TKey key, TValue value, long retainedBytes)
    {
        var node = _lru.AddFirst(new CacheEntry(key, value, retainedBytes));
        _entries.Add(key, node);
        _retainedBytes += retainedBytes;

        Trim();
    }

    internal void RefreshRetainedBytes()
    {
        lock (_gate)
        {
            _retainedBytes = 0;
            foreach (var entry in _lru)
            {
                var bytes = EstimateEntry(entry.Key, entry.Value);
                ArgumentOutOfRangeException.ThrowIfNegative(bytes);
                entry.RetainedBytes = bytes;
                _retainedBytes = ThemeRetainedBytesEstimator.Add(_retainedBytes, bytes);
            }
            Trim();
        }
    }

    private void Trim()
    {
        while (_entries.Count > _entryLimit || _retainedBytes > _retainedBytesLimit)
        {
            var oldest = _lru.Last!;
            _lru.RemoveLast();
            _entries.Remove(oldest.Value.Key);
            _retainedBytes -= oldest.Value.RetainedBytes;
        }
    }

    private void Touch(LinkedListNode<CacheEntry> node)
    {
        if (!ReferenceEquals(node, _lru.First))
        {
            _lru.Remove(node);
            _lru.AddFirst(node);
        }
    }

    private sealed class CacheEntry(TKey key, TValue value, long retainedBytes)
    {
        internal TKey Key { get; } = key;
        internal TValue Value { get; } = value;
        internal long RetainedBytes { get; set; } = retainedBytes;
    }
}
