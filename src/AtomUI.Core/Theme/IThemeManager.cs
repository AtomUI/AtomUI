namespace AtomUI.Theme;

using AtomUI.Theme.Schema;

public interface IThemeManager
{
    /// <summary>The enabled design languages and what each offers.</summary>
    IReadOnlyList<DesignLanguageInfo> Languages { get; }

    /// <summary>The theme catalog: every named theme and the implicit default entry of every enabled language.</summary>
    IReadOnlyList<ThemeInfo> Themes { get; }

    IReadOnlyList<ThemeDiagnostic> ThemeCatalogDiagnostics { get; }
    ThemeState? CurrentTheme { get; }

    /// <summary>The selection last applied through <see cref="ApplyAsync"/>, or <see langword="null"/>.</summary>
    ThemeSelection? CurrentSelection { get; }

    SemanticPartRegistry SemanticParts { get; }

    /// <summary>Resolves and applies an end-user selection (spec 7.8).</summary>
    /// <exception cref="ThemeSelectionException">The language, theme or an axis value is not available.</exception>
    Task<ThemeTransitionResult> ApplyAsync(
        ThemeSelection selection,
        CancellationToken cancellationToken = default);

    Task<ThemeTransitionResult> ApplyThemeAsync(
        ThemeRequest request,
        CancellationToken cancellationToken = default);

    Task<ThemeCatalogReloadResult> ReloadThemesAsync(
        CancellationToken cancellationToken = default);

    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;
    event EventHandler<ThemeCatalogChangedEventArgs>? ThemeCatalogChanged;
}
