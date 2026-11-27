namespace AtomUI.Controls.AsyncLoad;

/// <summary>
/// The result of one asynchronous load, as returned by the load coordinators. It states how the load ended and
/// carries the loaded value or the error, so that a caller handles every ending without catching exceptions.
/// </summary>
/// <typeparam name="TResult">
/// The type of the loaded value.
/// </typeparam>
public sealed class AsyncLoadOutcome<TResult> where TResult : class
{
    /// <summary>
    /// Gets how the load ended. <see cref="Result" /> is set only for <see cref="AsyncLoadStatus.Success" />, and
    /// <see cref="Error" /> only for <see cref="AsyncLoadStatus.Faulted" />.
    /// </summary>
    public AsyncLoadStatus Status { get; init; }
    /// <summary>
    /// Gets the value that the loader returned, or <see langword="null" /> when the load did not succeed.
    /// </summary>
    public TResult? Result { get; init; }
    /// <summary>
    /// Gets the exception that made the load fail, or <see langword="null" /> when the load did not fail.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// Gets a value indicating whether the load ended with <see cref="AsyncLoadStatus.Success" />.
    /// </summary>
    public bool IsSuccess => Status == AsyncLoadStatus.Success;
    /// <summary>
    /// Gets a value indicating whether the load exceeded its time limit.
    /// </summary>
    public bool IsTimedOut => Status == AsyncLoadStatus.TimedOut;
    /// <summary>
    /// Gets a value indicating whether the load was canceled before it finished.
    /// </summary>
    public bool IsCancelled => Status == AsyncLoadStatus.Cancelled;
    /// <summary>
    /// Gets a value indicating whether the loader failed, in which case <see cref="Error" /> holds the exception.
    /// </summary>
    public bool IsFaulted => Status == AsyncLoadStatus.Faulted;
    /// <summary>
    /// Gets a value indicating whether a newer request replaced the load, so that its result is not used.
    /// </summary>
    public bool IsSkipped => Status == AsyncLoadStatus.Skipped;

    /// <summary>
    /// Creates an outcome that reports a loaded value.
    /// </summary>
    /// <param name="result">
    /// The value that the loader returned.
    /// </param>
    public static AsyncLoadOutcome<TResult> Successful(TResult result)
        => new() { Status = AsyncLoadStatus.Success, Result = result };

    /// <summary>
    /// Creates an outcome for a load that was canceled before it finished.
    /// </summary>
    public static AsyncLoadOutcome<TResult> Cancel()
        => new() { Status = AsyncLoadStatus.Cancelled };

    /// <summary>
    /// Creates an outcome for a load that exceeded its time limit.
    /// </summary>
    public static AsyncLoadOutcome<TResult> Timeout()
        => new() { Status = AsyncLoadStatus.TimedOut };

    /// <summary>
    /// Creates an outcome for a load whose loader failed.
    /// </summary>
    /// <param name="error">
    /// The exception that made the load fail.
    /// </param>
    public static AsyncLoadOutcome<TResult> Fault(Exception error)
        => new() { Status = AsyncLoadStatus.Faulted, Error = error };

    /// <summary>
    /// Creates an outcome for a request that a newer request replaced before it finished.
    /// </summary>
    public static AsyncLoadOutcome<TResult> Skip()
        => new() { Status = AsyncLoadStatus.Skipped };
}
