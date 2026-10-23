namespace AtomUI.Theme;

public enum ThemeCatalogReloadStatus : byte
{
    Committed,
    NoOp,
    Superseded,
    Failed
}

public sealed class ThemeCatalogReloadResult
{
    public ThemeCatalogReloadResult(
        long generation,
        ThemeCatalogReloadStatus status,
        IReadOnlyList<ThemeInfo> availableThemes,
        ThemeState? state,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        IReadOnlyList<ThemeDiagnostic> publishDiagnostics,
        Exception? exception)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);
        ArgumentNullException.ThrowIfNull(availableThemes);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(publishDiagnostics);

        Generation         = generation;
        Status             = status;
        AvailableThemes    = Array.AsReadOnly(availableThemes.ToArray());
        State              = state;
        Diagnostics        = Array.AsReadOnly(diagnostics.ToArray());
        PublishDiagnostics = Array.AsReadOnly(publishDiagnostics.ToArray());
        Exception          = exception;
    }

    public long Generation { get; }
    public ThemeCatalogReloadStatus Status { get; }
    public IReadOnlyList<ThemeInfo> AvailableThemes { get; }
    public ThemeState? State { get; }
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    public IReadOnlyList<ThemeDiagnostic> PublishDiagnostics { get; }
    public Exception? Exception { get; }
    public ThemePresentationResult Presentation { get; init; } = ThemePresentationResult.Unchanged;
}
