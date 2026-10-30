using Avalonia.Logging;
using Avalonia.Threading;

namespace AtomUI;

internal sealed class StartupProcess : IStartup
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _state = (int)StartupState.Preparing;

    public StartupState State => (StartupState)Volatile.Read(ref _state);
    public Task Completion => _completion.Task;
    public event EventHandler<StartupProgressEventArgs>? ProgressChanged;

    internal void MoveTo(StartupState next)
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
                StartupEventSource.Log.StateChanged(next);
                ReportProgress(next.ToString());
                return;
            }
        }
    }

    internal void CompleteReady()
    {
        if (TryEnterTerminal(StartupState.Ready))
        {
            _completion.TrySetResult();
            Finish(StartupState.Ready);
        }
    }

    internal void Fail(Exception failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        if (TryEnterTerminal(StartupState.Failed))
        {
            LogError(failure);
            _completion.TrySetException(failure);
            // 公开任务保留原始异常；没有观察者时也不产生未观察任务异常。失败已写 Error 日志，不是静默的。
            _ = _completion.Task.Exception;
            Finish(StartupState.Failed);
        }
    }

    internal void Cancel()
    {
        if (TryEnterTerminal(StartupState.Canceled))
        {
            _completion.TrySetCanceled();
            Finish(StartupState.Canceled);
        }
    }

    internal void ReportProgress(string detail)
    {
        var handlers = ProgressChanged;
        if (handlers is null)
        {
            return;
        }
        var args = new StartupProgressEventArgs(State, detail);
        if (Dispatcher.UIThread.CheckAccess())
        {
            Dispatch(handlers, args);
        }
        else
        {
            Dispatcher.UIThread.Post(() => Dispatch(handlers, args));
        }
    }

    private bool TryEnterTerminal(StartupState terminal)
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

    private void Finish(StartupState terminal)
    {
        StartupEventSource.Log.StateChanged(terminal);
        ReportProgress(terminal.ToString());
        ProgressChanged = null;
    }

    private static bool IsTerminalState(StartupState state) =>
        state is StartupState.Ready or StartupState.Failed or StartupState.Canceled;

    private void Dispatch(EventHandler<StartupProgressEventArgs> handlers, StartupProgressEventArgs args)
    {
        foreach (EventHandler<StartupProgressEventArgs> handler in handlers.GetInvocationList())
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
            Logger.TryGet(level, AtomUILogArea.System)?.Log(typeof(StartupProcess), message);
        }
        catch
        {
            // 日志宿主失败不能改变启动结果。
        }
    }
}
