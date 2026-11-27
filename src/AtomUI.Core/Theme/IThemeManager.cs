namespace AtomUI.Theme;

using AtomUI.Theme.Schema;

/// <summary>
/// The running theme manager: the theme catalog to choose from, the theme in effect, and the operations that
/// change it.
/// </summary>
/// <remarks>
/// An application obtains it with the <c>GetThemeManager</c> extension method of <c>Application</c>. The events
/// are raised synchronously on the UI thread, and an exception thrown by an event handler is caught and reported
/// in the publish diagnostics of the result.
/// </remarks>
public interface IThemeManager
{
    /// <summary>
    /// The enabled design languages and what each offers.
    /// </summary>
    IReadOnlyList<DesignLanguageInfo> Languages { get; }

    /// <summary>
    /// The theme catalog: every named theme and the implicit default entry of every enabled language.
    /// </summary>
    IReadOnlyList<ThemeInfo> Themes { get; }

    /// <summary>
    /// The diagnostics of the most recent theme catalog load: the startup load, or the last reload that was not
    /// superseded.
    /// </summary>
    IReadOnlyList<ThemeDiagnostic> ThemeCatalogDiagnostics { get; }
    /// <summary>
    /// The theme the root scope currently shows, or <see langword="null" /> before the first theme is committed and
    /// after the manager is disposed.
    /// </summary>
    ThemeState? CurrentTheme { get; }

    /// <summary>
    /// The selection last applied through <see cref="ApplyAsync" />, or <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// At startup it is the configured initial selection. <see cref="ApplyThemeAsync" /> leaves it unchanged.
    /// </remarks>
    ThemeSelection? CurrentSelection { get; }

    /// <summary>
    /// The semantic part descriptors of the registered controls, searchable by control type and by control identity.
    /// </summary>
    SemanticPartRegistry SemanticParts { get; }

    /// <summary>
    /// Resolves an end-user selection against the theme catalog and applies the theme it denotes.
    /// </summary>
    /// <param name="selection">
    /// The selection to apply: the language, an optional named theme, the style, appearance and density, and the seeds
    /// the user picked.
    /// </param>
    /// <param name="cancellationToken">
    /// Only ends the wait of the caller. The transition itself keeps running and may still commit.
    /// </param>
    /// <returns>
    /// The outcome of the transition, with the statuses described on <see cref="ApplyThemeAsync" />.
    /// </returns>
    /// <exception cref="ThemeSelectionException">
    /// The language, theme or an axis value is not available.
    /// </exception>
    /// <remarks>
    /// A selection that follows the system appearance stores a request for light and one for dark, applies the one
    /// that matches the current system appearance, and makes every later system appearance change switch between the
    /// two.
    /// <see cref="CurrentSelection" /> is replaced only when the status is
    /// <see cref="ThemeTransitionStatus.Committed" /> or <see cref="ThemeTransitionStatus.NoOp" />.
    /// </remarks>
    Task<ThemeTransitionResult> ApplyAsync(
        ThemeSelection selection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a theme request and completes when the transition has ended.
    /// </summary>
    /// <param name="request">
    /// The theme to apply, with an optional configuration and the reason of the change.
    /// </param>
    /// <param name="cancellationToken">
    /// Only ends the wait of the caller. The transition itself keeps running and may still commit.
    /// </param>
    /// <returns>
    /// The outcome of the transition:
    /// <see cref="ThemeTransitionStatus.Committed" /> when the new theme is committed and published,
    /// <see cref="ThemeTransitionStatus.NoOp" /> when that theme is already in effect,
    /// <see cref="ThemeTransitionStatus.Superseded" /> when a later request replaced this one before it was committed,
    /// and <see cref="ThemeTransitionStatus.Failed" /> when it failed before anything was committed.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="request" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The theme id of <paramref name="request" /> is blank.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The call comes from a thread other than the UI thread, control packages are being loaded, or the manager is
    /// disposed.
    /// </exception>
    /// <remarks>
    /// With no transition in progress, a request equal to the last committed one returns
    /// <see cref="ThemeTransitionStatus.NoOp" /> without preparing a theme. A request that repeats the queued or the
    /// running one joins that transition.
    /// </remarks>
    Task<ThemeTransitionResult> ApplyThemeAsync(
        ThemeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reloads the theme catalog from the theme definition resolvers that support reload, and completes when the
    /// reload has ended.
    /// </summary>
    /// <param name="cancellationToken">
    /// Only ends the wait of the caller. The reload itself keeps running and may still commit.
    /// </param>
    /// <returns>
    /// The outcome of the reload:
    /// <see cref="ThemeCatalogReloadStatus.Committed" /> when the reloaded catalog is in use,
    /// <see cref="ThemeCatalogReloadStatus.NoOp" /> when nothing changed,
    /// <see cref="ThemeCatalogReloadStatus.Superseded" /> when a theme change or a newer reload got ahead of it, and
    /// <see cref="ThemeCatalogReloadStatus.Failed" /> when the catalog could not be loaded, which leaves the catalog
    /// in use untouched.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The theme manager is not initialized, the call comes from a thread other than the UI thread, or control
    /// packages are being loaded.
    /// </exception>
    /// <remarks>
    /// When the compiled theme changed, the current theme is committed and published again before the task completes.
    /// A current theme that no longer exists in the reloaded catalog is replaced by the default theme of its language.
    /// </remarks>
    Task<ThemeCatalogReloadResult> ReloadThemesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Raised after a theme transition is committed and published, so <see cref="CurrentTheme" /> already reports the
    /// new theme.
    /// </summary>
    /// <remarks>
    /// It is not raised for a transition that ends as <see cref="ThemeTransitionStatus.NoOp" />,
    /// <see cref="ThemeTransitionStatus.Superseded" /> or <see cref="ThemeTransitionStatus.Failed" />.
    /// </remarks>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    /// <summary>
    /// Raised when a theme transition fails before anything is committed, so the theme in effect stays as it was.
    /// </summary>
    /// <remarks>
    /// It is not raised for a transition that ends as <see cref="ThemeTransitionStatus.Superseded" />, nor for a
    /// failure while publishing after the commit, which is reported as a diagnostic of a committed transition.
    /// </remarks>
    event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;
    /// <summary>
    /// Raised after a theme catalog reload is committed and published, before the task of
    /// <see cref="ReloadThemesAsync" /> completes.
    /// </summary>
    /// <remarks>
    /// It is not raised for a reload that ends as <see cref="ThemeCatalogReloadStatus.NoOp" />,
    /// <see cref="ThemeCatalogReloadStatus.Superseded" /> or <see cref="ThemeCatalogReloadStatus.Failed" />.
    /// </remarks>
    event EventHandler<ThemeCatalogChangedEventArgs>? ThemeCatalogChanged;
}
