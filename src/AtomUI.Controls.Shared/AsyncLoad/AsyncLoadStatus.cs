namespace AtomUI.Controls.AsyncLoad;

/// <summary>
/// How an asynchronous load ended, as reported by <see cref="AsyncLoadOutcome{TResult}" />.
/// </summary>
public enum AsyncLoadStatus
{
    /// <summary>
    /// The loader returned a result.
    /// </summary>
    Success,
    /// <summary>
    /// The load was canceled for a reason other than the time limit, such as the external token of the request.
    /// </summary>
    Cancelled,
    /// <summary>
    /// The request exceeded the time limit of the coordinator.
    /// </summary>
    TimedOut,
    /// <summary>
    /// The loader failed with an exception that is not a cancellation.
    /// </summary>
    Faulted,
    /// <summary>
    /// A newer request replaced this one before it finished, so its result is not used.
    /// </summary>
    Skipped
}
