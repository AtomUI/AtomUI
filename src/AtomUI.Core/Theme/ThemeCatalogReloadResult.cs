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

    public long Generation { get; }
    public ThemeCatalogReloadStatus Status { get; }
    public IReadOnlyList<ThemeInfo> Themes { get; }
    public ThemeState? State { get; }
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    public IReadOnlyList<ThemeDiagnostic> PublishDiagnostics { get; }
    public Exception? Exception { get; }
}
