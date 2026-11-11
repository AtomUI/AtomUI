namespace AtomUI.Controls;

internal sealed class ImageRequestCoordinator : IDisposable
{
    private readonly object _gate = new();
    private readonly Dictionary<ImageSourceOperationKey, SharedOperation<ImageValidatedContent>> _sources = [];
    private readonly Dictionary<ImageDecodedOperationKey, SharedOperation<ImageDecodedCacheEntry>> _decoded = [];
    private bool _disposed;

    internal Task<ImageValidatedContent> GetSourceAsync(
        ImageSourceOperationKey key,
        ImageRequestPriorityState priorityState,
        Func<SharedOperationContext, CancellationToken, Task<ImageValidatedContent>> factory,
        IProgress<ImageLoadProgress>? progress,
        CancellationToken cancellationToken)
    {
        return JoinAsync(
            _sources,
            key,
            priorityState,
            factory,
            static value => value,
            progress,
            cancellationToken,
            null);
    }

    internal Task<TResult> GetDecodedAsync<TResult>(
        ImageDecodedOperationKey key,
        ImageRequestPriorityState priorityState,
        Func<SharedOperationContext, CancellationToken, Task<ImageDecodedCacheEntry>> factory,
        Func<ImageDecodedCacheEntry, TResult> resultSelector,
        IProgress<ImageLoadProgress>? progress,
        CancellationToken cancellationToken)
    {
        return JoinAsync(
            _decoded,
            key,
            priorityState,
            factory,
            resultSelector,
            progress,
            cancellationToken,
            entry => entry.ReleaseOperation());
    }

    internal void Cancel(string? partitionHash)
    {
        SharedOperationBase[] operations;
        lock (_gate)
        {
            operations = _sources
                .Where(pair => partitionHash is null || pair.Key.SourceKey.PartitionHash == partitionHash)
                .Select(pair => (SharedOperationBase)pair.Value)
                .Concat(_decoded
                    .Where(pair => partitionHash is null || pair.Key.DecodeKey.PartitionHash == partitionHash)
                    .Select(pair => (SharedOperationBase)pair.Value))
                .Distinct()
                .ToArray();
        }
        foreach (var operation in operations)
        {
            operation.Cancel();
        }
    }

    internal bool HasInFlightOperations
    {
        get
        {
            lock (_gate)
            {
                return _sources.Count != 0 || _decoded.Count != 0;
            }
        }
    }

    public void Dispose()
    {
        SharedOperationBase[] operations;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            operations = _sources.Values.Cast<SharedOperationBase>()
                .Concat(_decoded.Values)
                .Distinct()
                .ToArray();
            _sources.Clear();
            _decoded.Clear();
        }
        foreach (var operation in operations)
        {
            operation.Cancel();
        }
    }

    private Task<TResult> JoinAsync<TKey, TValue, TResult>(
        Dictionary<TKey, SharedOperation<TValue>> operations,
        TKey key,
        ImageRequestPriorityState priorityState,
        Func<SharedOperationContext, CancellationToken, Task<TValue>> factory,
        Func<TValue, TResult> resultSelector,
        IProgress<ImageLoadProgress>? progress,
        CancellationToken cancellationToken,
        Action<TValue>? releaseValue)
        where TKey : notnull
    {
        SharedOperation<TValue> operation;
        long waiterId = 0;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (operations.TryGetValue(key, out operation!) &&
                !operation.TryAddWaiter(priorityState, progress, out waiterId))
            {
                operations.Remove(key);
                operation = null!;
            }
            if (operation is null)
            {
                operation = new SharedOperation<TValue>(factory, releaseValue);
                operations.Add(key, operation);
                operation.Completed += (_, _) =>
                {
                    lock (_gate)
                    {
                        if (operations.TryGetValue(key, out var current) && ReferenceEquals(current, operation))
                        {
                            operations.Remove(key);
                        }
                    }
                };
                if (!operation.TryAddWaiter(priorityState, progress, out waiterId))
                {
                    throw new InvalidOperationException("A new image operation rejected its first waiter.");
                }
                operation.Start();
                return WaitAsync(operation, waiterId, resultSelector, cancellationToken);
            }
        }
        return WaitAsync(operation, waiterId, resultSelector, cancellationToken);
    }

    private static async Task<TResult> WaitAsync<TValue, TResult>(
        SharedOperation<TValue> operation,
        long waiterId,
        Func<TValue, TResult> resultSelector,
        CancellationToken cancellationToken)
    {
        try
        {
            var value = await operation.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return resultSelector(value);
        }
        finally
        {
            operation.RemoveWaiter(waiterId);
        }
    }

    internal sealed class SharedOperationContext
    {
        private readonly object _gate = new();
        private readonly Dictionary<long, IProgress<ImageLoadProgress>> _progress = [];
        private int _priority = (int)ImageRequestPriority.Preload;

        internal ImageRequestPriority Priority => (ImageRequestPriority)Volatile.Read(ref _priority);

        internal void SetPriority(ImageRequestPriority priority)
        {
            Volatile.Write(ref _priority, (int)priority);
        }

        internal void AddProgress(long waiterId, IProgress<ImageLoadProgress>? progress)
        {
            if (progress is null)
            {
                return;
            }
            lock (_gate)
            {
                _progress[waiterId] = progress;
            }
        }

        internal void RemoveProgress(long waiterId)
        {
            lock (_gate)
            {
                _progress.Remove(waiterId);
            }
        }

        internal void Report(ImageLoadProgress progress)
        {
            IProgress<ImageLoadProgress>[] reporters;
            lock (_gate)
            {
                reporters = _progress.Values.ToArray();
            }
            foreach (var reporter in reporters)
            {
                ImageProgressDispatcher.Report(reporter, progress);
            }
        }
    }

    private abstract class SharedOperationBase
    {
        internal abstract Task Completion { get; }

        internal abstract void Cancel();
    }

    private sealed class SharedOperation<T> : SharedOperationBase
    {
        private readonly object _gate = new();
        private readonly Func<SharedOperationContext, CancellationToken, Task<T>> _factory;
        private readonly Action<T>? _releaseValue;
        private readonly CancellationTokenSource _cancellation = new();
        private readonly object _cancellationGate = new();
        private readonly Dictionary<long, ImageRequestPriorityState> _waiters = [];
        private readonly SharedOperationContext _context = new();
        private Task<T>? _task;
        private T? _completedValue;
        private bool _hasCompletedValue;
        private bool _released;
        private bool _completed;
        private bool _cancelRequested;
        private bool _cancellationDisposed;
        private long _nextWaiterId;

        internal SharedOperation(
            Func<SharedOperationContext, CancellationToken, Task<T>> factory,
            Action<T>? releaseValue)
        {
            _factory = factory;
            _releaseValue = releaseValue;
        }

        internal event EventHandler? Completed;

        internal Task<T> Task => _task ?? throw new InvalidOperationException("Operation has not started.");

        internal override Task Completion => Task;

        internal void Start()
        {
            _task = RunAsync();
        }

        internal bool TryAddWaiter(
            ImageRequestPriorityState priorityState,
            IProgress<ImageLoadProgress>? progress,
            out long waiterId)
        {
            lock (_gate)
            {
                // A cancel-requested operation is still in flight but can only end canceled; a new waiter must
                // start fresh work instead of inheriting a cancellation it never asked for.
                if (_completed || _cancelRequested)
                {
                    waiterId = 0;
                    return false;
                }
                waiterId = ++_nextWaiterId;
                _waiters.Add(waiterId, priorityState);
                priorityState.PriorityChanged += HandlePriorityChanged;
                _context.AddProgress(waiterId, progress);
                UpdatePriorityCore();
                return true;
            }
        }

        internal void RemoveWaiter(long waiterId)
        {
            var cancel = false;
            var release = false;
            var disposeCancellation = false;
            lock (_gate)
            {
                if (_waiters.Remove(waiterId, out var priorityState))
                {
                    priorityState.PriorityChanged -= HandlePriorityChanged;
                }
                _context.RemoveProgress(waiterId);
                UpdatePriorityCore();
                if (_waiters.Count == 0)
                {
                    cancel = !_completed && !_cancelRequested;
                    _cancelRequested |= cancel;
                    release = _hasCompletedValue && !_released;
                    _released |= release;
                    disposeCancellation = _completed;
                }
            }
            if (cancel)
            {
                CancelCancellation();
            }
            if (release)
            {
                _releaseValue?.Invoke(_completedValue!);
            }
            if (disposeCancellation)
            {
                DisposeCancellation();
            }
        }

        internal override void Cancel()
        {
            var cancel = false;
            lock (_gate)
            {
                cancel = !_completed && !_cancelRequested;
                _cancelRequested |= cancel;
            }
            if (cancel)
            {
                CancelCancellation();
            }
        }

        private async Task<T> RunAsync()
        {
            T? factoryValue = default;
            var hasFactoryValue = false;
            try
            {
                factoryValue = await _factory(_context, _cancellation.Token).ConfigureAwait(false);
                hasFactoryValue = true;
                _cancellation.Token.ThrowIfCancellationRequested();
                var release = false;
                lock (_gate)
                {
                    _completedValue = factoryValue;
                    _hasCompletedValue = true;
                    hasFactoryValue = false;
                    release = _waiters.Count == 0 && !_released;
                    _released |= release;
                }
                if (release)
                {
                    _releaseValue?.Invoke(factoryValue);
                }
                return factoryValue;
            }
            catch
            {
                if (hasFactoryValue)
                {
                    _releaseValue?.Invoke(factoryValue!);
                }
                throw;
            }
            finally
            {
                var release = false;
                var disposeCancellation = false;
                lock (_gate)
                {
                    _completed = true;
                    release = _hasCompletedValue && _waiters.Count == 0 && !_released;
                    _released |= release;
                    disposeCancellation = _waiters.Count == 0;
                }
                if (release)
                {
                    _releaseValue?.Invoke(_completedValue!);
                }
                Completed?.Invoke(this, EventArgs.Empty);
                if (disposeCancellation)
                {
                    DisposeCancellation();
                }
            }
        }

        private void UpdatePriorityCore()
        {
            _context.SetPriority(_waiters.Count == 0
                ? ImageRequestPriority.Preload
                : _waiters.Values.Min(state => state.Priority));
        }

        private void HandlePriorityChanged()
        {
            lock (_gate)
            {
                if (!_completed)
                {
                    UpdatePriorityCore();
                }
            }
        }

        private void CancelCancellation()
        {
            lock (_cancellationGate)
            {
                if (!_cancellationDisposed)
                {
                    _cancellation.Cancel();
                }
            }
        }

        private void DisposeCancellation()
        {
            lock (_cancellationGate)
            {
                if (_cancellationDisposed)
                {
                    return;
                }
                _cancellationDisposed = true;
                _cancellation.Dispose();
            }
        }
    }
}
