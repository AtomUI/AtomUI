namespace AtomUI.Theme.Compilation;

internal sealed class BoundedLruCache<TKey, TValue> : IDisposable
    where TKey : notnull
{
    private readonly object _gate = new();
    private readonly int _entryLimit;
    private readonly long _retainedBytesLimit;
    private readonly Func<TValue, long> _retainedBytesEstimator;
    private readonly Func<TKey, long>? _keyRetainedBytesEstimator;
    private readonly int _productionEntryLimit;
    private readonly long _productionRetainedBytesLimit;
    private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _entries = new();
    private readonly LinkedList<CacheEntry> _lru = new();
    private readonly Dictionary<TKey, Task<TValue>> _inFlight = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly CancellationToken _lifetimeToken;
    private long _retainedBytes;
    private long _productionRetainedBytes;
    private bool _disposed;
    private bool _cancellationCompleted;

    internal BoundedLruCache(
        int entryLimit,
        long retainedBytesLimit,
        Func<TValue, long> retainedBytesEstimator,
        Func<TKey, long>? keyRetainedBytesEstimator = null,
        int productionEntryLimit = ThemeCacheOptions.DefaultProductionEntryLimit,
        long productionRetainedBytesLimit = ThemeCacheOptions.DefaultProductionRetainedBytesLimit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(entryLimit, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(retainedBytesLimit, 1);
        ArgumentNullException.ThrowIfNull(retainedBytesEstimator);
        ArgumentOutOfRangeException.ThrowIfLessThan(productionEntryLimit, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(productionRetainedBytesLimit, 1);

        _entryLimit             = entryLimit;
        _retainedBytesLimit     = retainedBytesLimit;
        _retainedBytesEstimator = retainedBytesEstimator;
        _keyRetainedBytesEstimator = keyRetainedBytesEstimator;
        _lifetimeToken = _lifetime.Token;
        _productionEntryLimit = productionEntryLimit;
        _productionRetainedBytesLimit = productionRetainedBytesLimit;
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
        CancellationToken callerCancellation,
        bool allowUncachedAfterDispose = false,
        Func<long>? productionInputRetainedBytesEstimator = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        callerCancellation.ThrowIfCancellationRequested();

        Task<TValue> production;
        TaskCompletionSource<TValue>? completion = null;
        var uncached = false;
        var productionBytes = 0L;
        lock (_gate)
        {
            if (_disposed)
            {
                ObjectDisposedException.ThrowIf(!allowUncachedAfterDispose, this);
                uncached = true;
                production = null!;
            }
            else if (_entries.TryGetValue(key, out var node))
            {
                Touch(node);
                return ValueTask.FromResult(node.Value.Value);
            }
            else if (!_inFlight.TryGetValue(key, out production!))
            {
                var keyBytes = _keyRetainedBytesEstimator?.Invoke(key) ?? 0;
                ArgumentOutOfRangeException.ThrowIfNegative(keyBytes);
                // Factories retain more than their lookup key. Only a new producer owns these
                // inputs; cache hits and equivalent waiters must not rescan or charge them.
                var inputBytes = productionInputRetainedBytesEstimator?.Invoke() ?? 0;
                ArgumentOutOfRangeException.ThrowIfNegative(inputBytes);
                productionBytes = ThemeRetainedBytesEstimator.Add(
                    ThemeRetainedBytesEstimator.Add(128, keyBytes), inputBytes);
                if (_inFlight.Count >= _productionEntryLimit ||
                    productionBytes > _productionRetainedBytesLimit - _productionRetainedBytes)
                {
                    throw new InvalidOperationException("The theme cache production capacity has been reached.");
                }
                completion = new TaskCompletionSource<TValue>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                production = completion.Task;
                _inFlight.Add(key, production);
                _productionRetainedBytes += productionBytes;
            }
        }

        if (uncached)
        {
            // A detached read view may still compute pure values, but cannot reopen this cache domain.
            return factory(callerCancellation);
        }

        if (completion is not null)
        {
            _ = ProduceAsync(key, factory, completion, productionBytes);
        }

        return callerCancellation.CanBeCanceled
            ? new ValueTask<TValue>(production.WaitAsync(callerCancellation))
            : new ValueTask<TValue>(production);
    }

    private async Task ProduceAsync(
        TKey key,
        Func<CancellationToken, ValueTask<TValue>> factory,
        TaskCompletionSource<TValue> completion,
        long productionBytes)
    {
        try
        {
            _lifetimeToken.ThrowIfCancellationRequested();
            var value = await factory(_lifetimeToken).ConfigureAwait(false);
            lock (_gate)
            {
                _lifetimeToken.ThrowIfCancellationRequested();
                if (_disposed)
                {
                    throw new OperationCanceledException(new CancellationToken(true));
                }
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
                FinishProduction(key, productionBytes);
            }
        }
        catch (OperationCanceledException exception)
        {
            lock (_gate)
            {
                completion.TrySetCanceled(exception.CancellationToken);
                FinishProduction(key, productionBytes);
            }
        }
        catch (Exception exception)
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    completion.TrySetCanceled(new CancellationToken(true));
                }
                else
                {
                    completion.TrySetException(exception);
                }
                FinishProduction(key, productionBytes);
            }
        }
        finally
        {
            lock (_gate)
            {
                if (_disposed && _cancellationCompleted && _inFlight.Count == 0)
                {
                    _lifetime.Dispose();
                }
            }
        }
    }

    private void FinishProduction(TKey key, long bytes)
    {
        _inFlight.Remove(key);
        _productionRetainedBytes -= bytes;
    }

    internal bool TryGetValue(TKey key, out TValue? value)
    {
        lock (_gate)
        {
            if (!_disposed && _entries.TryGetValue(key, out var node))
            {
                Touch(node);
                value = node.Value.Value;
                return true;
            }
            value = default;
            return false;
        }
    }

    internal void StoreValue(TKey key, TValue value)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            var bytes = EstimateEntry(key, value);
            if (_entries.Remove(key, out var previous))
            {
                _lru.Remove(previous);
                _retainedBytes -= previous.Value.RetainedBytes;
            }
            if (bytes <= _retainedBytesLimit)
            {
                Add(key, value, bytes);
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _entries.Clear();
            _lru.Clear();
            _retainedBytes = 0;
        }
        // Cancellation callbacks are external code. Never run them under the cache lock;
        // live production records remain until their factories actually exit.
        try
        {
            _lifetime.Cancel();
        }
        finally
        {
            lock (_gate)
            {
                _cancellationCompleted = true;
                if (_inFlight.Count == 0)
                {
                    _lifetime.Dispose();
                }
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
            if (_disposed)
            {
                return;
            }
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
