using System.Diagnostics;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;

internal sealed class ControllableDataGridSource : IDataGridSource
{
    private readonly object _gate = new();
    private readonly List<PendingRequest> _requests = [];
    private int _activeRequestCount;
    private EventHandler? _invalidated;

    public ControllableDataGridSource(DataGridSourceSchema schema)
    {
        Schema = schema;
    }

    public DataGridSourceSchema Schema { get; }

    public bool HonorCancellation { get; set; }

    public int MaximumObservedConcurrency { get; private set; }

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

    public IReadOnlyList<PendingRequest> Requests
    {
        get
        {
            lock (_gate)
            {
                return _requests.ToArray();
            }
        }
    }

    public int InvalidatedAddCount { get; private set; }

    public int InvalidatedRemoveCount { get; private set; }

    public int InvalidatedSubscriberCount
    {
        get
        {
            lock (_gate)
            {
                return _invalidated?.GetInvocationList().Length ?? 0;
            }
        }
    }

    public event EventHandler? Invalidated
    {
        add
        {
            lock (_gate)
            {
                InvalidatedAddCount++;
                _invalidated += value;
            }
        }
        remove
        {
            lock (_gate)
            {
                InvalidatedRemoveCount++;
                _invalidated -= value;
            }
        }
    }

    public ValueTask<DataGridRangeResult> FetchAsync(
        DataGridFetchRequest request,
        CancellationToken cancellationToken)
    {
        PendingRequest pending;
        lock (_gate)
        {
            pending = new PendingRequest(request, cancellationToken, HonorCancellation);
            _requests.Add(pending);
            _activeRequestCount++;
            MaximumObservedConcurrency = Math.Max(MaximumObservedConcurrency, _activeRequestCount);
        }
        return new ValueTask<DataGridRangeResult>(AwaitResultAsync(pending));
    }

    public PendingRequest RequestAt(int index)
    {
        WaitForRequestCount(index + 1);
        lock (_gate)
        {
            return _requests[index];
        }
    }

    public void Complete(int index, DataGridRangeResult result) =>
        RequestAt(index).Complete(result, ignoreCancellation: false);

    public void CompleteIgnoringCancellation(int index, DataGridRangeResult result) =>
        RequestAt(index).Complete(result, ignoreCancellation: true);

    public void Fail(int index, Exception exception) => RequestAt(index).Fail(exception);

    public void RaiseInvalidated()
    {
        EventHandler? invalidated;
        lock (_gate)
        {
            invalidated = _invalidated;
        }
        invalidated?.Invoke(this, EventArgs.Empty);
    }

    public void RaiseInvalidatedFromWorker() =>
        Task.Run(RaiseInvalidated).GetAwaiter().GetResult();

    public EventHandler? CaptureInvalidatedHandler()
    {
        lock (_gate)
        {
            return _invalidated;
        }
    }

    public void WaitForRequestCount(int count)
    {
        var timeout = TimeSpan.FromSeconds(5);
        var start = Stopwatch.GetTimestamp();
        while (true)
        {
            lock (_gate)
            {
                if (_requests.Count >= count)
                {
                    return;
                }
            }
            if (Stopwatch.GetElapsedTime(start) > timeout)
            {
                throw new TimeoutException($"Expected {count} source requests.");
            }
            Thread.Yield();
        }
    }

    private async Task<DataGridRangeResult> AwaitResultAsync(PendingRequest pending)
    {
        try
        {
            return await pending.AwaitAsync().ConfigureAwait(false);
        }
        finally
        {
            lock (_gate)
            {
                _activeRequestCount--;
            }
        }
    }

    internal sealed class PendingRequest
    {
        private readonly TaskCompletionSource<DataGridRangeResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenRegistration _cancellationRegistration;
        private bool _ignoreCancellation;

        public PendingRequest(
            DataGridFetchRequest request,
            CancellationToken cancellationToken,
            bool honorCancellation)
        {
            Request = request;
            _cancellationToken = cancellationToken;
            if (honorCancellation)
            {
                _cancellationRegistration = cancellationToken.Register(
                    static state => ((PendingRequest)state!).Cancel(), this);
            }
        }

        public DataGridFetchRequest Request { get; }

        public bool IsCancellationRequested => _cancellationToken.IsCancellationRequested;

        public Task<DataGridRangeResult> AwaitAsync() => _completion.Task;

        public void Complete(DataGridRangeResult result, bool ignoreCancellation)
        {
            _ignoreCancellation = ignoreCancellation;
            _cancellationRegistration.Dispose();
            if (!_ignoreCancellation)
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
            _completion.TrySetResult(result);
        }

        public void Fail(Exception exception)
        {
            _cancellationRegistration.Dispose();
            _completion.TrySetException(exception);
        }

        private void Cancel()
        {
            if (!_ignoreCancellation)
            {
                _completion.TrySetCanceled(_cancellationToken);
            }
        }
    }
}
