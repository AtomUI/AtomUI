namespace AtomUI;

/// <summary>
/// Diagnostic phases of the application startup. <see cref="StartupEventSource" /> maps each value explicitly
/// to the name used in the event payload.
/// </summary>
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
