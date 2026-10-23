namespace AtomUI.Theme;

using AtomUI.Theme.Schema;

public interface IThemeManager
{
    const string DEFAULT_THEME_ID = "DaybreakBlue";

    IReadOnlyList<ThemeInfo> AvailableThemes { get; }
    IReadOnlyList<ThemeDiagnostic> ThemeCatalogDiagnostics { get; }
    ThemeState? CurrentTheme { get; }
    SemanticPartRegistry SemanticParts { get; }
    // Existing external Token-only implementations remain source-compatible. They must opt in
    // to this protocol explicitly; unsupported presentation operations never report success.
    ThemePresentationResult Presentation =>
        throw new NotSupportedException("This ThemeManager does not implement managed presentation.");

    ThemePresentationResult RetryPresentationCleanup() =>
        throw new NotSupportedException("This ThemeManager does not implement managed presentation cleanup.");

    Task<ThemeTransitionResult> ApplyThemeAsync(
        ThemeRequest request,
        CancellationToken cancellationToken = default);

    Task<ThemeCatalogReloadResult> ReloadThemesAsync(
        CancellationToken cancellationToken = default);

    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;
    event EventHandler<ThemeCatalogChangedEventArgs>? ThemeCatalogChanged;
    event EventHandler<ThemePresentationChangedEventArgs>? ThemePresentationChanged
    {
        add => throw new NotSupportedException("This ThemeManager does not publish managed presentation results.");
        remove => throw new NotSupportedException("This ThemeManager does not publish managed presentation results.");
    }
}
