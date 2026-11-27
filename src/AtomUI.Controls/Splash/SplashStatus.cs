namespace AtomUI.Controls;

/// <summary>
/// State of the startup that <see cref="Splash.Status" /> reports.
/// </summary>
public enum SplashStatus
{
    /// <summary>
    /// The startup is running. This is the default state: the progress bar is shown and the close button is hidden.
    /// </summary>
    Loading,
    /// <summary>
    /// The startup failed: the default theme paints the message in the error color, hides the progress bar and
    /// shows the close button, so the user can dismiss the splash.
    /// </summary>
    Error
}
