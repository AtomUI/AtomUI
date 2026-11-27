using System.Collections.Concurrent;

namespace AtomUI.Controls.AsyncLoad;

/// <summary>
/// Coordinates asynchronous loading for expanding items such as tree nodes. A request for a context that is
/// already loading joins the load that is in flight instead of starting another one, and every load has its own
/// time limit from <see cref="Timeout" />.
/// <para>
/// The coordinator neither debounces nor cancels an earlier request, because the items load independently of
/// each other.
/// </para>
/// </summary>
/// <typeparam name="TContext">
/// The item whose data is loaded. Requests whose contexts are equal under the comparer of the constructor share
/// one load.
/// </typeparam>
/// <typeparam name="TResult">
/// The type of the loaded data.
/// </typeparam>
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

    /// <summary>
    /// Initializes the coordinator with the comparer that decides whether two contexts belong to the same item.
    /// </summary>
    /// <param name="dedupComparer">
    /// The comparer for contexts, or <see langword="null" /> to use the default equality comparer of
    /// <typeparamref name="TContext" />.
    /// </param>
    public AsyncExpandLoadCoordinator(IEqualityComparer<TContext>? dedupComparer)
    {
        _inFlight = new ConcurrentDictionary<
            TContext,
            TaskCompletionSource<AsyncLoadOutcome<TResult>>>(
            dedupComparer ?? EqualityComparer<TContext>.Default);
    }

    /// <summary>
    /// Gets or sets the maximum duration of one load. A positive value cancels the token of the loader when it
    /// elapses; zero, a negative value and the default infinite timeout disable the limit.
    /// </summary>
    /// <remarks>
    /// The value is read when a load starts. Changing it does not affect loads that are already running.
    /// </remarks>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Loads the data of <paramref name="context" />, or joins the load that is already running for it.
    /// </summary>
    /// <param name="context">
    /// The item to load. Its equality with other contexts is decided by the comparer of the constructor.
    /// </param>
    /// <param name="loader">
    /// The function that loads the data. Only a request that starts a new load calls it, with a token that
    /// <see cref="CancelAll" /> and <see cref="Timeout" /> cancel.
    /// </param>
    /// <param name="external">
    /// A token that is linked to the token of the loader. Only the request that starts the load links its token;
    /// the token of a request that joins the load is ignored.
    /// </param>
    /// <returns>
    /// A task that completes with the outcome of the load. Requests that share a load receive the same outcome.
    /// An exception of the loader that is not a cancellation is reported as <see cref="AsyncLoadStatus.Faulted" />
    /// and does not fault the task.
    /// </returns>
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

    /// <summary>
    /// Cancels the token of the loader of every load that is still running.
    /// </summary>
    /// <remarks>
    /// A load whose loader stops with <see cref="OperationCanceledException" /> after this call reports
    /// <see cref="AsyncLoadStatus.TimedOut" />. It reports <see cref="AsyncLoadStatus.Cancelled" /> only when the
    /// external token of the request that started it is canceled as well.
    /// </remarks>
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
