using Avalonia.Logging;
using Avalonia.Threading;

namespace AtomUI;

internal sealed class AtomUIStartup : IAtomUIStartup
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _state = (int)AtomUIStartupState.Configuring;

    public AtomUIStartupState State => (AtomUIStartupState)Volatile.Read(ref _state);
    public Task Completion => _completion.Task;
    public event EventHandler<AtomUIStartupProgressEventArgs>? ProgressChanged;

    internal bool IsTerminal => State is AtomUIStartupState.Ready or AtomUIStartupState.Failed or AtomUIStartupState.Canceled;

    internal bool TryTransition(AtomUIStartupState expected, AtomUIStartupState next)
    {
        if (Interlocked.CompareExchange(ref _state, (int)next, (int)expected) != (int)expected)
        {
            return false;
        }
        ReportProgress(next.ToString());
        return true;
    }

    internal void Complete(Exception? failure, CancellationToken cancellation)
    {
        var state = State;
        if (IsTerminal)
        {
            return;
        }
        var terminal = failure is not null ? AtomUIStartupState.Failed :
            cancellation.IsCancellationRequested ? AtomUIStartupState.Canceled : AtomUIStartupState.Ready;
        if (Interlocked.CompareExchange(ref _state, (int)terminal, (int)state) != (int)state)
        {
            return;
        }
        if (failure is not null)
        {
            _completion.TrySetException(failure);
            // 公开任务仍保留原始异常；没有观察者的应用也不产生未观察任务异常。
            _ = _completion.Task.Exception;
        }
        else if (cancellation.IsCancellationRequested)
        {
            _completion.TrySetCanceled(cancellation);
        }
        else
        {
            _completion.TrySetResult();
        }
        ReportProgress(terminal.ToString());
        ProgressChanged = null;
    }

    internal void ReportProgress(string detail)
    {
        var handlers = ProgressChanged;
        if (handlers is null)
        {
            return;
        }
        var args = new AtomUIStartupProgressEventArgs(State, detail);
        if (Dispatcher.UIThread.CheckAccess())
        {
            Dispatch(handlers, args);
        }
        else
        {
            Dispatcher.UIThread.Post(() => Dispatch(handlers, args));
        }
    }

    private void Dispatch(EventHandler<AtomUIStartupProgressEventArgs> handlers, AtomUIStartupProgressEventArgs args)
    {
        foreach (EventHandler<AtomUIStartupProgressEventArgs> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(this, args);
            }
            catch (Exception exception)
            {
                LogFailure(exception);
            }
        }
    }

    internal static void LogFailure(Exception exception)
    {
        try
        {
            Logger.TryGet(LogEventLevel.Warning, AtomUILogArea.System)?.Log(
                typeof(AtomUIStartup), $"Startup observer or cleanup failed: {exception}");
        }
        catch
        {
            // 日志宿主失败不能改变启动结果，也不能中断后续订阅者或清理。
        }
    }
}
