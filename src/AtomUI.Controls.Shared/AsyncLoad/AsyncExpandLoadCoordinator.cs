using System.Collections.Concurrent;

namespace AtomUI.Controls.AsyncLoad;

/// <summary>
/// 展开类场景的异步加载协调器。同一 context 引用有请求在飞时复用同一 Task（in-flight 去重），按 <see cref="Timeout"/> 硬超时。
///
/// 不做 debounce，不取消前一次 —— 展开语义下每个节点的加载互相独立。
/// </summary>
public sealed class AsyncExpandLoadCoordinator<TContext, TResult>
    where TContext : notnull
    where TResult : class
{
    private readonly ConcurrentDictionary<
        TContext,
        TaskCompletionSource<AsyncLoadOutcome<TResult>>> _inFlight;
    private readonly ConcurrentDictionary<CancellationTokenSource, byte> _activeCts = new();

    public AsyncExpandLoadCoordinator() : this(null)
    {
    }

    public AsyncExpandLoadCoordinator(IEqualityComparer<TContext>? dedupComparer)
    {
        _inFlight = new ConcurrentDictionary<
            TContext,
            TaskCompletionSource<AsyncLoadOutcome<TResult>>>(
            dedupComparer ?? EqualityComparer<TContext>.Default);
    }

    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    public Task<AsyncLoadOutcome<TResult>> LoadOrJoinAsync(
        TContext context,
        Func<TContext, CancellationToken, Task<TResult>> loader,
        CancellationToken external = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(loader);

        var candidate = new TaskCompletionSource<AsyncLoadOutcome<TResult>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var operation = _inFlight.GetOrAdd(context, candidate);
        if (ReferenceEquals(operation, candidate))
        {
            _ = CompleteAsync(context, loader, external, candidate);
        }

        return operation.Task;
    }

    public void CancelAll()
    {
        foreach (var cts in _activeCts.Keys)
        {
            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private async Task<AsyncLoadOutcome<TResult>> RunAsync(
        TContext context,
        Func<TContext, CancellationToken, Task<TResult>> loader,
        CancellationToken external)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(external);
        _activeCts.TryAdd(linkedCts, 0);
        try
        {
            var timeout = Timeout;
            if (timeout > TimeSpan.Zero && timeout != System.Threading.Timeout.InfiniteTimeSpan)
            {
                linkedCts.CancelAfter(timeout);
            }

            try
            {
                var result = await loader(context, linkedCts.Token).ConfigureAwait(false);
                return AsyncLoadOutcome<TResult>.Successful(result);
            }
            catch (OperationCanceledException)
            {
                if (external.IsCancellationRequested)
                {
                    return AsyncLoadOutcome<TResult>.Cancel();
                }
                if (linkedCts.Token.IsCancellationRequested)
                {
                    return AsyncLoadOutcome<TResult>.Timeout();
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
            _activeCts.TryRemove(linkedCts, out _);
            linkedCts.Dispose();
        }
    }

    private async Task CompleteAsync(
        TContext context,
        Func<TContext, CancellationToken, Task<TResult>> loader,
        CancellationToken external,
        TaskCompletionSource<AsyncLoadOutcome<TResult>> completion)
    {
        try
        {
            completion.TrySetResult(await RunAsync(context, loader, external).ConfigureAwait(false));
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
        }
        finally
        {
            _inFlight.TryRemove(new KeyValuePair<
                TContext,
                TaskCompletionSource<AsyncLoadOutcome<TResult>>>(context, completion));
        }
    }
}
