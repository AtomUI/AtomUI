namespace AtomUI;

/// <summary><see cref="AtomUIStartupBuilderExtensions.UseMainWindow"/> 工厂的上下文。</summary>
public sealed class AtomUIMainWindowContext
{
    internal AtomUIMainWindowContext(IProgress<AtomUIStartupProgress> progress, CancellationToken cancellationToken)
    {
        Progress = progress;
        CancellationToken = cancellationToken;
    }

    /// <summary>汇报进度；Splash 在 UI 线程空闲时显示。</summary>
    public IProgress<AtomUIStartupProgress> Progress { get; }

    /// <summary>用户在交接前关闭 Splash 时取消。</summary>
    public CancellationToken CancellationToken { get; }
}
