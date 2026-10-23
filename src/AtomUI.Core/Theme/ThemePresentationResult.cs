namespace AtomUI.Theme;

public enum ThemePresentationStatus : byte
{
    Unchanged,
    Applied,
    Restored,
    Degraded
}

public sealed record ThemePresentationResult
{
    public ThemePresentationResult(ThemePresentationStatus status,
        IReadOnlyList<ThemeDiagnostic> diagnostics, Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        Status = status;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        Exception = exception;
    }

    public int Version => 1;
    public ThemePresentationStatus Status { get; }
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    public Exception? Exception { get; }

    internal static ThemePresentationResult Unchanged { get; } = new(ThemePresentationStatus.Unchanged, []);
}

public sealed class ThemePresentationChangedEventArgs : EventArgs
{
    public ThemePresentationChangedEventArgs(ThemePresentationResult result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    public ThemePresentationResult Result { get; }
}
