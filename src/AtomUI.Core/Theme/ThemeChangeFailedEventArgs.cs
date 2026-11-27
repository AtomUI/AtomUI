namespace AtomUI.Theme;

/// <summary>
/// The data of the <see cref="IThemeManager.ThemeChangeFailed" /> event: a theme transition that failed before
/// anything was committed.
/// </summary>
public sealed class ThemeChangeFailedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event data of a failed theme transition.
    /// </summary>
    public ThemeChangeFailedEventArgs(
        ThemeRequest request,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(diagnostics);

        Request     = request;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        Exception   = exception;
    }

    /// <summary>
    /// The theme request whose transition failed, with its configuration and transition reason.
    /// </summary>
    public ThemeRequest Request { get; }
    /// <summary>
    /// The diagnostics that describe why the transition failed.
    /// </summary>
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    /// <summary>
    /// The exception that caused the failure, or <see langword="null" /> when none was thrown.
    /// </summary>
    public Exception? Exception { get; }
}
