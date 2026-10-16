using Avalonia.Threading;

namespace AtomUI;

internal static class DispatcherPump
{
    /// <summary>
    /// 主循环开始前驱动 Dispatcher，直到 <paramref name="task"/> 完成或超时。超时由线程池计时器
    /// （CancelAfter）取消：DispatcherTimer 依赖主循环，此阶段不可用。返回任务是否已完成。
    /// </summary>
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
                // 超时退出后任务才完成：取消源已释放，无需再唤醒。
            }
        }, cancellation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        Dispatcher.UIThread.MainLoop(cancellation.Token);
        return task.IsCompleted;
    }
}
