namespace AtomUI.Theme;

/// <summary>
/// The data of a <c>ThemeChanged</c> event, raised once a theme transition is committed and published: the request
/// that was applied, the state it produced and the diagnostics collected during the publish.
/// </summary>
public sealed class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the data of one committed theme transition.
    /// </summary>
    /// <param name="request">
    /// The request whose transition was committed.
    /// </param>
    /// <param name="state">
    /// The theme state that the request produced.
    /// </param>
    /// <param name="publishDiagnostics">
    /// The diagnostics collected while the theme was published, such as a warning for each theme event handler that
    /// threw. The list is copied.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="request" />, <paramref name="state" /> or <paramref name="publishDiagnostics" /> is
    /// <see langword="null" />.
    /// </exception>
    public ThemeChangedEventArgs(
        ThemeRequest request,
        ThemeState state,
        IReadOnlyList<ThemeDiagnostic> publishDiagnostics)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(publishDiagnostics);

        Request            = request;
        State              = state;
        PublishDiagnostics = Array.AsReadOnly(publishDiagnostics.ToArray());
    }

    internal ThemeChangedEventArgs(
        ThemeRequest request,
        ThemeState state,
        List<ThemeDiagnostic> publishDiagnostics)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(publishDiagnostics);

        Request            = request;
        State              = state;
        PublishDiagnostics = publishDiagnostics.AsReadOnly();
    }

    /// <summary>
    /// The request whose transition was committed.
    /// </summary>
    public ThemeRequest Request { get; }
    /// <summary>
    /// The theme state that the committed request produced.
    /// </summary>
    public ThemeState State { get; }
    /// <summary>
    /// The diagnostics collected while the committed theme was published, such as a warning for each theme event
    /// handler that threw.
    /// </summary>
    /// <remarks>
    /// In the arguments of <see cref="IThemeManager.ThemeChanged" /> the collection is a live view: a diagnostic
    /// added while the handlers run, such as the warning for a handler that threw, appears in it afterwards.
    /// </remarks>
    public IReadOnlyList<ThemeDiagnostic> PublishDiagnostics { get; }
}
