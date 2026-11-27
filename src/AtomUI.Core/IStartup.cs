namespace AtomUI;

/// <summary>
/// The state of the application startup process. It only moves forward, and the first terminal state
/// (<see cref="Ready" />, <see cref="Failed" /> or <see cref="Canceled" />) that is entered is final.
/// </summary>
public enum StartupState
{
    /// <summary>
    /// The initial state: the startup has begun, but the theme and languages are not committed to the
    /// application yet.
    /// </summary>
    Preparing,
    /// <summary>
    /// The theme and the languages are committed to the application. The startup has not begun to present
    /// the main window.
    /// </summary>
    Committed,
    /// <summary>
    /// The main window is being shown and awaited for its first frame. Closing it before that frame cancels
    /// the startup.
    /// </summary>
    Presenting,
    /// <summary>
    /// Terminal state. The startup succeeded: either no main window is presented, or the main window has
    /// rendered its first frame and the splash screen has been closed.
    /// </summary>
    Ready,
    /// <summary>
    /// Terminal state. Startup threw an exception; <see cref="IStartup.Completion" /> is faulted with that
    /// exception.
    /// </summary>
    Failed,
    /// <summary>
    /// Terminal state. The startup was canceled, for example when the user closes the splash screen or the
    /// main window before its first frame; <see cref="IStartup.Completion" /> is canceled.
    /// </summary>
    Canceled
}

/// <summary>
/// Carries one startup progress report to the handlers of <see cref="IStartup.ProgressChanged" />.
/// It reports progress only; the final outcome is read by awaiting <see cref="IStartup.Completion" />.
/// </summary>
public sealed class StartupProgressEventArgs(StartupState state, string detail) : EventArgs
{
    /// <summary>
    /// The state at the time of the report. A report raised on another thread is posted to the UI thread,
    /// so the startup may have advanced further by the time the handler runs.
    /// </summary>
    public StartupState State { get; } = state;
    /// <summary>
    /// A short description of the step. A state change reports the name of the new state.
    /// </summary>
    public string Detail { get; } = detail;
}

/// <summary>
/// The state of the application startup and a task that completes when the startup ends.
/// The task can be awaited by any number of callers.
/// </summary>
public interface IStartup
{
    /// <summary>
    /// The current state, readable from any thread. It is updated before <see cref="ProgressChanged" /> is
    /// raised and before <see cref="Completion" /> completes, so it is final once the task has completed.
    /// </summary>
    StartupState State { get; }

    /// <summary>
    /// Completes when the startup ends: successfully for <see cref="StartupState.Ready" />, faulted with the
    /// original exception for <see cref="StartupState.Failed" />, and canceled for
    /// <see cref="StartupState.Canceled" />.
    /// A caller that only wants to stop waiting can cancel its own wait with <c>WaitAsync(CancellationToken)</c>;
    /// that does not cancel the startup.
    /// </summary>
    Task Completion { get; }

    /// <summary>
    /// Raised on the UI thread after each state change, the terminal states included, with the state already
    /// updated. It reports only changes that happen after subscribing, and an exception thrown by a handler
    /// is logged and does not affect the startup.
    /// </summary>
    event EventHandler<StartupProgressEventArgs>? ProgressChanged;
}
