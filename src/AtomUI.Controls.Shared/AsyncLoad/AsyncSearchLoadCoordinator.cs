namespace AtomUI.Controls.AsyncLoad;

/// <summary>
/// Coordinates asynchronous loading for search scenarios, where each new request replaces the previous one: a
/// request waits for <see cref="DebounceInterval" /> before it starts the loader, and <see cref="Timeout" />
/// limits how long the loader runs.
/// <para>
/// An instance tracks one sequence of requests, so a control keeps its own instance for its lifetime and calls
/// <see cref="Cancel" /> before the control is destroyed.
/// </para>
/// </summary>
/// <typeparam name="TContext">
/// The input of a request, such as the filter text. It is passed to the loader and may be
/// <see langword="null" />.
/// </typeparam>
/// <typeparam name="TResult">
/// The type of the data that the loader returns.
/// </typeparam>
public sealed class AsyncSearchLoadCoordinator<TContext, TResult>
    where TResult : class
{
    private readonly object _sync = new();
    private CancellationTokenSource? _cts;
    private long _generation;
    private long _cancelledGeneration = -1;

    /// <summary>
    /// Gets or sets how long a request waits before it starts the loader. The default, zero, starts the loader
    /// without waiting.
    /// </summary>
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the maximum duration of the loader of a request. The limit starts when the debounce ends. A
    /// positive value cancels the token of the loader when it elapses; zero, a negative value and the default
    /// infinite timeout disable the limit.
    /// </summary>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Starts a request for <paramref name="context" /> and cancels the request that came before it.
    /// </summary>
    /// <param name="context">
    /// The input of the request, which is passed to <paramref name="loader" />.
    /// </param>
    /// <param name="loader">
    /// The function that loads the data. It runs after the debounce with a token that is canceled when this request
    /// is replaced by a newer one, when <see cref="Cancel" /> is called, when <paramref name="external" /> is
    /// canceled or when <see cref="Timeout" /> elapses.
    /// </param>
    /// <param name="external">
    /// A token that is linked to the token of the loader. A request that stops because of its cancellation reports
    /// <see cref="AsyncLoadStatus.Cancelled" />.
    /// </param>
    /// <returns>
    /// A task that completes with the outcome of the request. A loader that returns <see langword="null" /> or
    /// throws an exception that is not a cancellation produces <see cref="AsyncLoadStatus.Faulted" />.
    /// </returns>
    /// <remarks>
    /// A replaced request reports <see cref="AsyncLoadStatus.Skipped" />, and a canceled or timed out one reports
    /// that status, only when the debounce wait or the loader stops with <see cref="OperationCanceledException" />.
    /// A loader that ignores its token and returns a value makes the request report
    /// <see cref="AsyncLoadStatus.Success" />, although a newer request has replaced it in the meantime.
    /// </remarks>
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

    /// <summary>
    /// Cancels the request that is currently waiting or loading. A request that stops because of this call reports
    /// <see cref="AsyncLoadStatus.Cancelled" />.
    /// </summary>
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

        // External cancellation, Cancel and a newer request are ruled out, so only the timeout remains.
        return AsyncLoadOutcome<TResult>.Timeout();
    }
}
