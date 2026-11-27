namespace AtomUI.Theme;

/// <summary>
/// How a theme transition ended, as reported by <see cref="ThemeTransitionResult.Status" />.
/// </summary>
public enum ThemeTransitionStatus : byte
{
    /// <summary>
    /// The new theme is committed and published. A publish failure after the commit does not change the status:
    /// <see cref="ThemeTransitionResult.PublishDiagnostics" /> then holds the warning that describes the failure.
    /// </summary>
    Committed,
    /// <summary>
    /// The requested theme is already in effect, so nothing was committed.
    /// </summary>
    NoOp,
    /// <summary>
    /// A later request replaced this transition before it was committed.
    /// </summary>
    Superseded,
    /// <summary>
    /// The transition failed before anything was committed.
    /// </summary>
    Failed
}

/// <summary>
/// The outcome of one theme transition: how it ended, which theme is in effect after it and what problems were
/// reported. <see cref="IThemeManager.ApplyThemeAsync" /> completes with it once the transition has ended.
/// </summary>
public sealed record ThemeTransitionResult
{
    /// <summary>
    /// Creates a result that reports one theme transition.
    /// </summary>
    /// <param name="transitionId">
    /// Identifies the transition. It must be greater than zero and is stored in <see cref="TransitionId" />.
    /// </param>
    /// <param name="status">
    /// How the transition ended, stored in <see cref="Status" />.
    /// </param>
    /// <param name="state">
    /// The theme state in effect after the transition, stored in <see cref="State" />.
    /// </param>
    /// <param name="diagnostics">
    /// The diagnostics reported before the commit. The list is copied into <see cref="Diagnostics" />.
    /// </param>
    /// <param name="publishDiagnostics">
    /// The diagnostics reported while publishing. The list is copied into <see cref="PublishDiagnostics" />.
    /// </param>
    /// <param name="exception">
    /// The error that made the transition fail, stored in <see cref="Exception" />, or <see langword="null" />
    /// when the transition did not fail.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="transitionId" /> is zero or negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="diagnostics" /> or <paramref name="publishDiagnostics" /> is <see langword="null" />.
    /// </exception>
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

    /// <summary>
    /// The id of the theme transition that ended with this result. It is greater than zero.
    /// </summary>
    public long TransitionId { get; }
    /// <summary>
    /// How the transition ended. Each of <see cref="State" />, <see cref="Diagnostics" />,
    /// <see cref="PublishDiagnostics" /> and <see cref="Exception" /> states what it holds for each status.
    /// </summary>
    public ThemeTransitionStatus Status { get; }
    /// <summary>
    /// The theme state in effect after the transition: the newly committed theme for
    /// <see cref="ThemeTransitionStatus.Committed" />, and the theme that stayed in effect for every other status.
    /// It is <see langword="null" /> when no theme is in effect.
    /// </summary>
    public ThemeState? State { get; }
    /// <summary>
    /// The diagnostics reported while the requested theme was prepared. For a
    /// <see cref="ThemeTransitionStatus.Failed" /> transition they also include the error entry that describes the
    /// failure. The list is empty when no theme was prepared.
    /// </summary>
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    /// <summary>
    /// The diagnostics reported while the new theme was published, including the warning that describes a publish
    /// failure after the commit. For a failed transition the list holds the diagnostics collected while the
    /// <c>ThemeChangeFailed</c> event was raised. It is empty for <see cref="ThemeTransitionStatus.NoOp" /> and
    /// <see cref="ThemeTransitionStatus.Superseded" />.
    /// </summary>
    public IReadOnlyList<ThemeDiagnostic> PublishDiagnostics { get; }
    /// <summary>
    /// The error that made the transition fail. It is <see langword="null" /> for every status other than
    /// <see cref="ThemeTransitionStatus.Failed" />, and may be <see langword="null" /> for a failed transition.
    /// </summary>
    public Exception? Exception { get; }
}
