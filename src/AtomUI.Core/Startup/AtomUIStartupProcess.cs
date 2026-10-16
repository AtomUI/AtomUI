using Avalonia.Logging;
using Avalonia.Threading;

namespace AtomUI;

internal sealed class AtomUIStartupProcess : IAtomUIStartup
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _state = (int)AtomUIStartupState.Preparing;

    public AtomUIStartupState State => (AtomUIStartupState)Volatile.Read(ref _state);
    public Task Completion => _completion.Task;
    public event EventHandler<AtomUIStartupProgressEventArgs>? ProgressChanged;

    internal bool IsTerminal => IsTerminalState(State);

    internal void MoveTo(AtomUIStartupState next)
    {
        while (true)
        {
            var current = State;
            if (IsTerminalState(current) || current >= next)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref _state, (int)next, (int)current) == (int)current)
            {
                AtomUIStartupEventSource.Log.StateChanged(next.ToString());
                ReportProgress(next.ToString());
                return;
            }
        }
    }

    internal void CompleteReady()
    {
        if (TryEnterTerminal(AtomUIStartupState.Ready))
        {
            _completion.TrySetResult();
            Finish(AtomUIStartupState.Ready);
        }
    }

    internal void Fail(Exception failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        if (TryEnterTerminal(AtomUIStartupState.Failed))
        {
            LogError(failure);
            _completion.TrySetException(failure);
            // 公开任务保留原始异常；没有观察者时也不产生未观察任务异常。失败已写 Error 日志，不是静默的。
            _ = _completion.Task.Exception;
            Finish(AtomUIStartupState.Failed);
        }
    }

    internal void Cancel()
    {
        if (TryEnterTerminal(AtomUIStartupState.Canceled))
        {
            _completion.TrySetCanceled();
            Finish(AtomUIStartupState.Canceled);
        }
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

    private bool TryEnterTerminal(AtomUIStartupState terminal)
    {
        while (true)
        {
            var current = State;
            if (IsTerminalState(current))
            {
                return false;
            }
            if (Interlocked.CompareExchange(ref _state, (int)terminal, (int)current) == (int)current)
            {
                return true;
            }
        }
    }

    private void Finish(AtomUIStartupState terminal)
    {
        AtomUIStartupEventSource.Log.StateChanged(terminal.ToString());
        ReportProgress(terminal.ToString());
        ProgressChanged = null;
    }

    private static bool IsTerminalState(AtomUIStartupState state) =>
        state is AtomUIStartupState.Ready or AtomUIStartupState.Failed or AtomUIStartupState.Canceled;

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
                LogWarning(exception);
            }
        }
    }

    internal static void LogError(Exception exception) => Log(LogEventLevel.Error, $"AtomUI startup failed: {exception}");

    internal static void LogWarning(Exception exception) => Log(LogEventLevel.Warning, $"AtomUI startup warning: {exception}");

    internal static void LogWarning(string message) => Log(LogEventLevel.Warning, message);

    private static void Log(LogEventLevel level, string message)
    {
        try
        {
            Logger.TryGet(level, AtomUILogArea.System)?.Log(typeof(AtomUIStartupProcess), message);
        }
        catch
        {
            // 日志宿主失败不能改变启动结果。
        }
    }
}
