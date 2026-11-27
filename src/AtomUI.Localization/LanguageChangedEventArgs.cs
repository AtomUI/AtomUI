namespace AtomUI.Localization;

/// <summary>
/// Carries the result of a language change to the handlers of
/// <see cref="ILanguageManager.LanguageChanged" />. Only a committed change creates these arguments.
/// </summary>
public sealed class LanguageChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the arguments for a language change that has been committed.
    /// </summary>
    /// <param name="result">
    /// The result of the committed change.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="result" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="result" /> reports a status other than <see cref="LanguageChangeStatus.Committed" />.
    /// </exception>
    public LanguageChangedEventArgs(LanguageChangeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Status != LanguageChangeStatus.Committed)
        {
            throw new ArgumentException(
                "LanguageChangedEventArgs requires a committed language change result.",
                nameof(result));
        }

        Result = result;
    }

    /// <summary>
    /// The committed change, with the state before the change in <see cref="LanguageChangeResult.OldState" />
    /// and the state now in effect in <see cref="LanguageChangeResult.NewState" />.
    /// </summary>
    public LanguageChangeResult Result { get; }
}
