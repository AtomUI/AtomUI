using Avalonia.Threading;

namespace AtomUI;

internal static class DispatcherPump
{
    /// <summary>
    /// Pumps the dispatcher until <paramref name="task" /> completes or <paramref name="timeout" /> elapses.
    /// It serves the stage before the main loop has started, where a DispatcherTimer cannot be used because it
    /// depends on that loop, so the timeout is enforced by a thread-pool timer.
    /// </summary>
    /// <param name="task">
    /// The task to wait for.
    /// </param>
    /// <param name="timeout">
    /// The maximum time to pump, or <see cref="Timeout.InfiniteTimeSpan" /> to pump until the task has completed.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the task has completed; <see langword="false" /> when the timeout ended the
    /// loop first.
    /// </returns>
    internal static bool PumpUntil(Task task, TimeSpan timeout)
    {
        if (task.IsCompleted)
        {
            return true;
        }
        using var cancellation = new CancellationTokenSource();
        if (timeout != Timeout.InfiniteTimeSpan)
        {
            cancellation.CancelAfter(timeout);
        }
        task.ContinueWith(static (_, state) =>
        {
            try
            {
                ((CancellationTokenSource)state!).Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The task completed only after the timeout had ended the loop: the cancellation source is disposed and
                // nothing needs to be woken any more.
            }
        }, cancellation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        Dispatcher.UIThread.MainLoop(cancellation.Token);
        return task.IsCompleted;
    }
}
