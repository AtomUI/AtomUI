namespace AtomUI.Theme;

/// <summary>
/// The outcome of a theme catalog reload, as reported by <see cref="IThemeManager.ReloadThemesAsync" />.
/// </summary>
public enum ThemeCatalogReloadStatus : byte
{
    /// <summary>
    /// The reloaded catalog is in use.
    /// </summary>
    Committed,
    /// <summary>
    /// The reload found nothing changed; the catalog in use is kept.
    /// </summary>
    NoOp,
    /// <summary>
    /// A theme change or a newer reload got ahead of this one, so nothing was committed.
    /// </summary>
    Superseded,
    /// <summary>
    /// The reload failed before anything was committed; the catalog and theme in use are left untouched.
    /// </summary>
    Failed
}

/// <summary>
/// The outcome of a theme catalog reload, returned by <see cref="IThemeManager.ReloadThemesAsync" /> when the
/// reload has ended.
/// </summary>
public sealed class ThemeCatalogReloadResult
{
    /// <summary>
    /// Creates the outcome of a catalog reload.
    /// </summary>
    public ThemeCatalogReloadResult(
        long generation,
        ThemeCatalogReloadStatus status,
        IReadOnlyList<ThemeInfo> themes,
        ThemeState? state,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        IReadOnlyList<ThemeDiagnostic> publishDiagnostics,
        Exception? exception)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);
        ArgumentNullException.ThrowIfNull(themes);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(publishDiagnostics);

        Generation         = generation;
        Status             = status;
        Themes             = Array.AsReadOnly(themes.ToArray());
        State              = state;
        Diagnostics        = Array.AsReadOnly(diagnostics.ToArray());
        PublishDiagnostics = Array.AsReadOnly(publishDiagnostics.ToArray());
        Exception          = exception;
    }

    /// <summary>
    /// The catalog generation of this reload; every call of <see cref="IThemeManager.ReloadThemesAsync" /> takes the
    /// next one. The <see cref="ThemeCatalogChangedEventArgs" /> raised for a committed reload reports the same
    /// generation.
    /// </summary>
    public long Generation { get; }
    /// <summary>
    /// How the reload ended.
    /// </summary>
    public ThemeCatalogReloadStatus Status { get; }
    /// <summary>
    /// The themes of the catalog in use when the reload ended: the reloaded catalog when the status is
    /// <see cref="ThemeCatalogReloadStatus.Committed" />, otherwise the previous one.
    /// </summary>
    public IReadOnlyList<ThemeInfo> Themes { get; }
    /// <summary>
    /// The theme in effect when the reload ended, or <see langword="null" /> when none is. The current theme is
    /// committed again only when the reloaded catalog changed the compiled theme; a current theme that the reloaded
    /// catalog no longer contains is replaced by its language's default theme.
    /// </summary>
    public ThemeState? State { get; }
    /// <summary>
    /// The diagnostics reported while the reloaded catalog was loaded and compiled. They are empty when a later
    /// request superseded the reload before it started.
    /// </summary>
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    /// <summary>
    /// The diagnostics reported while the current theme was published again after the commit. The list is empty for
    /// every status but <see cref="ThemeCatalogReloadStatus.Committed" />, and for a committed reload that left the
    /// compiled theme unchanged.
    /// </summary>
    public IReadOnlyList<ThemeDiagnostic> PublishDiagnostics { get; }
    /// <summary>
    /// The exception that made the reload fail, or <see langword="null" /> when none was thrown or the reload did not
    /// fail.
    /// </summary>
    public Exception? Exception { get; }
}
