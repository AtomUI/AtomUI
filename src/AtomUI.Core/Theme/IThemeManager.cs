namespace AtomUI.Theme;

using AtomUI.Theme.Schema;

public interface IThemeManager
{
    const string DEFAULT_THEME_ID = "DaybreakBlue";

    IReadOnlyList<ThemeInfo> AvailableThemes { get; }
    IReadOnlyList<ThemeDiagnostic> ThemeCatalogDiagnostics { get; }
    ThemeState? CurrentTheme { get; }
    SemanticPartRegistry SemanticParts { get; }

    Task<ThemeTransitionResult> ApplyThemeAsync(
        ThemeRequest request,
        CancellationToken cancellationToken = default);

    Task<ThemeCatalogReloadResult> ReloadThemesAsync(
        CancellationToken cancellationToken = default);

    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;
    event EventHandler<ThemeCatalogChangedEventArgs>? ThemeCatalogChanged;
}
