namespace AtomUI.Theme;

public enum ThemeTransitionStatus : byte
{
    Committed,
    NoOp,
    Superseded,
    Failed
}

public sealed record ThemeTransitionResult
{
    public ThemeTransitionResult(
        long transitionId,
        ThemeTransitionStatus status,
        ThemeState? state,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        IReadOnlyList<ThemeDiagnostic> publishDiagnostics,
        Exception? exception)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(transitionId);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(publishDiagnostics);

        TransitionId       = transitionId;
        Status             = status;
        State              = state;
        Diagnostics        = Array.AsReadOnly(diagnostics.ToArray());
        PublishDiagnostics = Array.AsReadOnly(publishDiagnostics.ToArray());
        Exception          = exception;
    }

    public long TransitionId { get; }
    public ThemeTransitionStatus Status { get; }
    public ThemeState? State { get; }
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    public IReadOnlyList<ThemeDiagnostic> PublishDiagnostics { get; }
    public Exception? Exception { get; }
    public ThemePresentationResult Presentation { get; init; } = ThemePresentationResult.Unchanged;
}
