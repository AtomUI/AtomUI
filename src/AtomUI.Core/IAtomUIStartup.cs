namespace AtomUI;

/// <summary>应用唯一启动过程的当前状态。</summary>
public enum AtomUIStartupState
{
    Preparing,
    Committed,
    Presenting,
    Ready,
    Failed,
    Canceled
}

/// <summary>启动进度；终态结果应通过 <see cref="IAtomUIStartup.Completion"/> 读取。</summary>
public sealed class AtomUIStartupProgressEventArgs(AtomUIStartupState state, string detail) : EventArgs
{
    public AtomUIStartupState State { get; } = state;
    public string Detail { get; } = detail;
}

/// <summary>应用启动状态与可重复等待的框架就绪结果。</summary>
public interface IAtomUIStartup
{
    AtomUIStartupState State { get; }

    /// <summary>
    /// 成功、失败和取消分别以完成、原始异常和取消状态结束。
    /// 调用者可使用 WaitAsync(CancellationToken) 取消自身等待，不取消应用启动。
    /// </summary>
    Task Completion { get; }

    /// <summary>可选的 UI 线程进度通知；订阅不重放已发生的进度。</summary>
    event EventHandler<AtomUIStartupProgressEventArgs>? ProgressChanged;
}
