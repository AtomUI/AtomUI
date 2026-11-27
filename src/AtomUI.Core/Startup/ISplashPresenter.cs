using Avalonia.Media;

namespace AtomUI;

/// <summary>
/// The splash content supplied by the application; <c>UseSplash</c> collects it. The images are factories
/// because the configuration callback runs before the platform is initialized, when no bitmap can be created
/// yet. The presenter invokes each factory on the UI thread when it shows the splash.
/// </summary>
internal sealed record SplashContent(Func<IImage>? Logo, Func<IImage>? BackgroundImage, string? Title, string? Message,
    Func<Avalonia.Controls.ResourceDictionary>? CreateControlTheme);

/// <summary>
/// The splash registration collected by <c>UseSplash</c>: when it is shown, how its presenter is created and
/// what it displays.
/// </summary>
internal sealed record SplashRegistration(SplashMode Mode, Func<ISplashPresenter> CreatePresenter,
    SplashContent Content);

/// <summary>
/// The seam through which AtomUI.Core drives the splash implemented in AtomUI.Controls. All members are
/// invoked on the UI thread.
/// </summary>
internal interface ISplashPresenter
{
    /// <summary>
    /// Creates and shows the splash. Returns <see langword="false" /> when no splash is shown for this startup;
    /// the reason has been written to the log.
    /// </summary>
    bool Show(IReadOnlyDictionary<object, object?> globalResources, SplashContent content);

    /// <summary>
    /// Completes once the first composition batch of the splash has been rendered.
    /// </summary>
    Task FirstFrameRendered { get; }

    /// <summary>
    /// Forwards a progress report given to <see cref="MainWindowContext.Progress" /> to the splash.
    /// </summary>
    void ReportProgress(StartupProgress progress);

    /// <summary>
    /// Switches the splash to its error state. The user can then close the splash, which completes
    /// <see cref="ErrorAcknowledged" />.
    /// </summary>
    void ShowError(Exception exception);

    /// <summary>
    /// Completes when the user closes the splash after <see cref="ShowError" /> switched it to the error state.
    /// </summary>
    Task ErrorAcknowledged { get; }

    /// <summary>
    /// Hides the splash once the main window has rendered its first frame. The window itself is closed afterwards
    /// by the startup orchestration, at a safe point.
    /// </summary>
    Task HideAsync();

    /// <summary>
    /// Closes the splash window immediately. Used to release the window when startup is canceled or showing the
    /// splash fails.
    /// </summary>
    void Close();

    /// <summary>
    /// Raised when the user closes the splash before the handoff to the main window. Closing the splash to
    /// acknowledge an error does not raise it, and neither does the handoff hide.
    /// </summary>
    event EventHandler? ClosedByUser;
}
