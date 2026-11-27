namespace AtomUI.Localization;

/// <summary>
/// Whether a language change request switched the current language, or found the requested language already
/// in effect.
/// </summary>
public enum LanguageChangeStatus : byte
{
    /// <summary>
    /// The language was switched, and <see cref="LanguageChangeResult.NewState" /> is the state now in effect.
    /// </summary>
    Committed = 0,
    /// <summary>
    /// The requested language was already in effect, so the current language is unchanged and no change is
    /// published.
    /// </summary>
    NoOp = 1
}

/// <summary>
/// The outcome of a language change request: whether the current language was switched, together with the
/// language state before and after the request.
/// </summary>
public sealed record LanguageChangeResult
{
    private LanguageChangeResult(
        LanguageChangeStatus status,
        LanguageState oldState,
        LanguageState newState)
    {
        Status = status;
        OldState = oldState;
        NewState = newState;
    }

    /// <summary>
    /// Whether the request switched the current language or left it unchanged.
    /// </summary>
    public LanguageChangeStatus Status { get; }

    /// <summary>
    /// The language state in effect before the request. For <see cref="LanguageChangeStatus.NoOp" /> it is the same
    /// state as <see cref="NewState" />.
    /// </summary>
    public LanguageState OldState { get; }

    /// <summary>
    /// The language state in effect after the request. For <see cref="LanguageChangeStatus.NoOp" /> it is the same
    /// state as <see cref="OldState" />.
    /// </summary>
    public LanguageState NewState { get; }

    /// <summary>
    /// Creates a result that reports <see cref="LanguageChangeStatus.Committed" />.
    /// </summary>
    /// <param name="oldState">
    /// The language state in effect before the change.
    /// </param>
    /// <param name="newState">
    /// The language state in effect after the change.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either argument is <see langword="null" />.
    /// </exception>
    public static LanguageChangeResult Committed(LanguageState oldState, LanguageState newState)
    {
        ArgumentNullException.ThrowIfNull(oldState);
        ArgumentNullException.ThrowIfNull(newState);
        return new LanguageChangeResult(LanguageChangeStatus.Committed, oldState, newState);
    }

    /// <summary>
    /// Creates a result that reports <see cref="LanguageChangeStatus.NoOp" />.
    /// </summary>
    /// <param name="state">
    /// The language state in effect before and after the request.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="state" /> is <see langword="null" />.
    /// </exception>
    public static LanguageChangeResult NoOp(LanguageState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return new LanguageChangeResult(LanguageChangeStatus.NoOp, state, state);
    }
}
