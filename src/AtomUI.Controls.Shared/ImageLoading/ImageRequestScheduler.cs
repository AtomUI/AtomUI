namespace AtomUI.Controls;

internal sealed class ImageRequestScheduler : IDisposable
{
    private readonly PriorityScheduler _downloadScheduler;
    private readonly PriorityScheduler _localReadScheduler;
    private readonly PriorityScheduler _decodeScheduler;

    internal ImageRequestScheduler(
        int maxConcurrentDownloads,
        int maxConcurrentLocalReads,
        int maxConcurrentDecodes,
        Func<DateTimeOffset>? clock = null)
    {
        _downloadScheduler = new PriorityScheduler(maxConcurrentDownloads, clock);
        _localReadScheduler = new PriorityScheduler(maxConcurrentLocalReads, clock);
        _decodeScheduler = new PriorityScheduler(maxConcurrentDecodes, clock);
    }

    internal int ActiveReads => _downloadScheduler.ActiveCount + _localReadScheduler.ActiveCount;

    internal int QueuedReads => _downloadScheduler.QueuedCount + _localReadScheduler.QueuedCount;

    internal int ActiveDecodes => _decodeScheduler.ActiveCount;

    internal int QueuedDecodes => _decodeScheduler.QueuedCount;

    internal Task<T> ScheduleReadAsync<T>(
        ImageSourceKind sourceKind,
        Func<CancellationToken, Task<T>> action,
        Func<ImageRequestPriority> priority,
        CancellationToken cancellationToken)
    {
        var scheduler = sourceKind == ImageSourceKind.Http
            ? _downloadScheduler
            : _localReadScheduler;
        return scheduler.ScheduleAsync(action, priority, cancellationToken);
    }

    internal Task<T> ScheduleDecodeAsync<T>(
        Func<CancellationToken, Task<T>> action,
        Func<ImageRequestPriority> priority,
        CancellationToken cancellationToken,
        Action<T>? releaseCanceledResult = null)
    {
        return _decodeScheduler.ScheduleAsync(
            action,
            priority,
            cancellationToken,
            releaseCanceledResult);
    }

    public void Dispose()
    {
        _downloadScheduler.Dispose();
        _localReadScheduler.Dispose();
        _decodeScheduler.Dispose();
    }

    private sealed class PriorityScheduler : IDisposable
    {
        private readonly object _gate = new();
        private readonly List<IWorkItem> _queue = [];
        private readonly SemaphoreSlim _signal = new(0);
        private readonly CancellationTokenSource _disposeCancellation = new();
        private readonly int _concurrency;
        private readonly Func<DateTimeOffset> _clock;
        private Task[] _workers = [];
        private ExecutionContext? _workerContext;
        private long _sequence;
        private int _activeCount;
        private bool _disposed;

        internal PriorityScheduler(int concurrency, Func<DateTimeOffset>? clock)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(concurrency);
            _concurrency = concurrency;
            _clock = clock ?? GetUtcNow;
            _workerContext = ExecutionContext.Capture();
        }

        internal int ActiveCount => Volatile.Read(ref _activeCount);

        internal int QueuedCount
        {
            get { lock (_gate)
                {
                    return _queue.Count;
                }
            }
        }

        internal Task<T> ScheduleAsync<T>(
            Func<CancellationToken, Task<T>> action,
            Func<ImageRequestPriority> priority,
            CancellationToken cancellationToken,
            Action<T>? releaseCanceledResult = null)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(priority);
            var item = new WorkItem<T>(
                action,
                priority,
                Interlocked.Increment(ref _sequence),
                _clock(),
                cancellationToken,
                releaseCanceledResult);
            var workerStartFailed = false;
            try
            {
                lock (_gate)
                {
                    ObjectDisposedException.ThrowIf(_disposed, this);
                    if (_workers.Length == 0)
                    {
                        try
                        {
                            StartWorkersCore();
                        }
                        catch
                        {
                            // No item is published until the entire pool has started.
                            _disposed = true;
                            _workerContext = null;
                            workerStartFailed = true;
                            throw;
                        }
                    }
                    _queue.Add(item);
                    _signal.Release();
                }
            }
            catch
            {
                if (workerStartFailed)
                {
                    DisposeWorkers();
                }
                throw;
            }
            return item.Task;
        }

        private void StartWorkersCore()
        {
            var workers = new Task[_concurrency];
            // Publish valid task slots before launching: failure cleanup must not allocate
            // another tracking array, particularly when a worker launch runs out of memory.
            Array.Fill(workers, Task.CompletedTask);
            _workers = workers;
            // Workers retain the constructor snapshot independently of the field.
            var workerContext = _workerContext;
            Func<Task> workerEntry = () => StartWorker(workerContext);
            using var suppressedFlow = ExecutionContext.IsFlowSuppressed()
                ? (AsyncFlowControl?)null
                : ExecutionContext.SuppressFlow();
            try
            {
                for (var index = 0; index < workers.Length; index++)
                {
                    workers[index] = Task.Run(workerEntry);
                }
            }
            finally
            {
                _workerContext = null;
            }
        }

        private Task StartWorker(ExecutionContext? workerContext)
        {
            if (workerContext is null)
            {
                return WorkerAsync();
            }
            Task? worker = null;
            // Restore on the worker thread, so AsyncLocal notifications never run
            // synchronously on the enqueue caller while the queue gate is held.
            ExecutionContext.Run(workerContext, _ => worker = WorkerAsync(), null);
            return worker!;
        }

        public void Dispose()
        {
            List<IWorkItem> queued;
            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                _workerContext = null;
                queued = [.. _queue];
                _queue.Clear();
            }
            DisposeWorkers(queued);
        }

        private void DisposeWorkers(List<IWorkItem>? queued = null)
        {
            // Cancel outside the queue gate: active actions can reenter from callbacks.
            // Cancellation also wakes workers waiting on the semaphore.
            _disposeCancellation.Cancel();
            if (queued is not null)
            {
                foreach (var item in queued)
                {
                    item.Cancel();
                }
            }
            _ = CompleteDisposalAsync();
        }

        private async Task CompleteDisposalAsync()
        {
            try
            {
                await Task.WhenAll(_workers).ConfigureAwait(false);
            }
            catch
            {
                // Work-item failures are delivered through their own completion tasks.
            }
            finally
            {
                _disposeCancellation.Dispose();
                _signal.Dispose();
            }
        }

        private async Task WorkerAsync()
        {
            while (true)
            {
                try
                {
                    await _signal.WaitAsync(_disposeCancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                IWorkItem? item;
                lock (_gate)
                {
                    if (_disposed)
                    {
                        return;
                    }
                    item = TakeNextCore();
                }
                if (item is null)
                {
                    continue;
                }

                Interlocked.Increment(ref _activeCount);
                try
                {
                    await item.ExecuteAsync(_disposeCancellation.Token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _activeCount);
                }
            }
        }

        private IWorkItem? TakeNextCore()
        {
            IWorkItem? selected = null;
            var selectedIndex = -1;
            for (var index = 0; index < _queue.Count; index++)
            {
                var candidate = _queue[index];
                if (candidate.IsCanceled)
                {
                    _queue.RemoveAt(index--);
                    candidate.Cancel();
                    continue;
                }
                if (selected is null || Compare(candidate, selected, _clock()) < 0)
                {
                    selected = candidate;
                    selectedIndex = index;
                }
            }
            if (selectedIndex >= 0)
            {
                _queue.RemoveAt(selectedIndex);
            }
            return selected;
        }

        private static int Compare(IWorkItem first, IWorkItem second, DateTimeOffset now)
        {
            var firstPriority = EffectivePriority(first.Priority, first.EnqueuedAt, now);
            var secondPriority = EffectivePriority(second.Priority, second.EnqueuedAt, now);
            var comparison = firstPriority.CompareTo(secondPriority);
            return comparison != 0 ? comparison : first.Sequence.CompareTo(second.Sequence);
        }

        private static int EffectivePriority(
            ImageRequestPriority priority,
            DateTimeOffset enqueuedAt,
            DateTimeOffset now)
        {
            var baseValue = (int)priority;
            if (priority is ImageRequestPriority.Normal or ImageRequestPriority.Low)
            {
                var agedLevels = (int)((now - enqueuedAt).TotalSeconds / 2);
                baseValue = Math.Max((int)ImageRequestPriority.High, baseValue - agedLevels);
            }
            return baseValue;
        }

        private static DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;

        private interface IWorkItem
        {
            ImageRequestPriority Priority { get; }
            DateTimeOffset EnqueuedAt { get; }
            long Sequence { get; }
            bool IsCanceled { get; }
            Task ExecuteAsync(CancellationToken schedulerCancellation);
            void Cancel();
        }

        private sealed class WorkItem<T> : IWorkItem
        {
            private readonly Func<CancellationToken, Task<T>> _action;
            private readonly Func<ImageRequestPriority> _priority;
            private readonly CancellationToken _cancellationToken;
            private readonly Action<T>? _releaseCanceledResult;
            private readonly TaskCompletionSource<T> _completion =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            internal WorkItem(
                Func<CancellationToken, Task<T>> action,
                Func<ImageRequestPriority> priority,
                long sequence,
                DateTimeOffset enqueuedAt,
                CancellationToken cancellationToken,
                Action<T>? releaseCanceledResult)
            {
                _action = action;
                _priority = priority;
                _cancellationToken = cancellationToken;
                _releaseCanceledResult = releaseCanceledResult;
                Sequence = sequence;
                EnqueuedAt = enqueuedAt;
            }

            public ImageRequestPriority Priority => _priority();
            public DateTimeOffset EnqueuedAt { get; }
            public long Sequence { get; }
            public bool IsCanceled => _cancellationToken.IsCancellationRequested;
            internal Task<T> Task => _completion.Task;

            public async Task ExecuteAsync(CancellationToken schedulerCancellation)
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                    schedulerCancellation,
                    _cancellationToken);
                using var registration = linked.Token.Register(() =>
                    _completion.TrySetCanceled(_cancellationToken.IsCancellationRequested
                        ? _cancellationToken
                        : schedulerCancellation));
                try
                {
                    var result = await _action(linked.Token).ConfigureAwait(false);
                    if (linked.IsCancellationRequested || !_completion.TrySetResult(result))
                    {
                        _releaseCanceledResult?.Invoke(result);
                    }
                }
                catch (OperationCanceledException) when (linked.IsCancellationRequested)
                {
                    _completion.TrySetCanceled(_cancellationToken.IsCancellationRequested
                        ? _cancellationToken
                        : schedulerCancellation);
                }
                catch (Exception exception)
                {
                    _completion.TrySetException(exception);
                }
            }

            public void Cancel()
            {
                _completion.TrySetCanceled(_cancellationToken);
            }
        }
    }
}
