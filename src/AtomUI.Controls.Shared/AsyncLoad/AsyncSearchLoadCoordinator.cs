namespace AtomUI.Controls.AsyncLoad;

/// <summary>
/// 搜索类场景的异步加载协调器。新请求到来时取消前一次，按 <see cref="DebounceInterval"/> 延迟启动，按 <see cref="Timeout"/> 硬超时。
///
/// Coordinator 自身是有状态的，一个控件持一个实例即可。控件销毁前应调用 <see cref="Cancel"/>。
/// </summary>
public sealed class AsyncSearchLoadCoordinator<TContext, TResult>
    where TResult : class
{
    private readonly object _sync = new();
    private CancellationTokenSource? _cts;
    private long _generation;
    private long _cancelledGeneration = -1;

    public TimeSpan DebounceInterval { get; set; } = TimeSpan.Zero;

    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    public async Task<AsyncLoadOutcome<TResult>> LoadAsync(
        TContext context,
        Func<TContext, CancellationToken, Task<TResult>> loader,
        CancellationToken external = default)
    {
        ArgumentNullException.ThrowIfNull(loader);

        CancellationTokenSource linkedCts;
        long generation;

        lock (_sync)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            linkedCts  = CancellationTokenSource.CreateLinkedTokenSource(external);
            _cts       = linkedCts;
            generation = ++_generation;
        }

        var loaderToken      = linkedCts.Token;
        var debounceInterval = DebounceInterval;
        var timeout          = Timeout;

        try
        {
            if (debounceInterval > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(debounceInterval, loaderToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return ClassifyCancellation(generation, external);
                }
            }

            if (timeout > TimeSpan.Zero && timeout != System.Threading.Timeout.InfiniteTimeSpan)
            {
                linkedCts.CancelAfter(timeout);
            }

            try
            {
                var result = await loader(context, loaderToken).ConfigureAwait(false);
                if (result == null)
                {
                    return AsyncLoadOutcome<TResult>.Fault(new InvalidOperationException("Loader returned null."));
                }
                return AsyncLoadOutcome<TResult>.Successful(result);
            }
            catch (OperationCanceledException)
            {
                if (external.IsCancellationRequested)
                {
                    return AsyncLoadOutcome<TResult>.Cancel();
                }
                if (loaderToken.IsCancellationRequested)
                {
                    return ClassifyCancellation(generation, external);
                }
                return AsyncLoadOutcome<TResult>.Cancel();
            }
            catch (Exception ex)
            {
                return AsyncLoadOutcome<TResult>.Fault(ex);
            }
        }
        finally
        {
            lock (_sync)
            {
                if (_cts == linkedCts)
                {
                    _cts = null;
                }
            }

            linkedCts.Dispose();
        }
    }

    public void Cancel()
    {
        lock (_sync)
        {
            if (_cts is not null)
            {
                _cancelledGeneration = _generation;
                _cts.Cancel();
                _cts.Dispose();
            }
            _cts = null;
        }
    }

    private AsyncLoadOutcome<TResult> ClassifyCancellation(long generation, CancellationToken external)
    {
        if (external.IsCancellationRequested)
        {
            return AsyncLoadOutcome<TResult>.Cancel();
        }

        bool cancelled;
        bool superseded;
        lock (_sync)
        {
            cancelled = _cancelledGeneration == generation;
            superseded = _generation != generation;
        }

        if (cancelled)
        {
            return AsyncLoadOutcome<TResult>.Cancel();
        }

        if (superseded)
        {
            return AsyncLoadOutcome<TResult>.Skip();
        }

        // 只剩 timeout 一种可能
        return AsyncLoadOutcome<TResult>.Timeout();
    }
}
