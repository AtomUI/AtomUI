namespace AtomUI;

/// <summary>
/// Context passed to the factory registered with <see cref="StartupBuilderExtensions.UseMainWindow" />.
/// </summary>
public sealed class MainWindowContext
{
    internal MainWindowContext(IProgress<StartupProgress> progress, CancellationToken cancellationToken)
    {
        Progress = progress;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Reports startup progress. The splash shows a report when the UI thread is idle.
    /// </summary>
    public IProgress<StartupProgress> Progress { get; }

    /// <summary>
    /// Becomes canceled when the user closes the splash before the handoff to the main window.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}
