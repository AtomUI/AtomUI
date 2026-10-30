namespace AtomUI;

/// <summary>启动诊断阶段；事件负载名称由 StartupEventSource 显式映射。</summary>
internal enum StartupPhase
{
    Configure,
    Prepare,
    PrepareWait,
    Splash,
    Commit,
    CommitLocalization,
    CommitTheme,
    CommitOwnedServices,
    CommitInitializers
}
