namespace AtomUI.Localization;

/// <summary>
/// Reads the current language state, lists the supported languages and switches the current language of an
/// application. An application exposes its manager through its <c>GetLanguageManager</c> extension method.
/// </summary>
public interface ILanguageManager
{
    /// <summary>
    /// The language state in effect when it is read: the current language, its formatting culture, its text
    /// direction and the revision number, which increases by one with each committed change. Use
    /// <see cref="LanguageChanged" /> to react to a change.
    /// </summary>
    LanguageState Current { get; }

    /// <summary>
    /// The languages that <see cref="ChangeLanguage" /> accepts, in the order in which the application configured
    /// them. The set is fixed once the localization host is built; loading language packages does not extend it.
    /// </summary>
    IReadOnlyList<LanguageDefinition> SupportedLanguages { get; }

    /// <summary>
    /// Raised on the thread that committed a language change, after the new language state is published and the
    /// language resources are refreshed. It is not raised for a change to the language that is already current.
    /// An exception that a handler throws is logged and does not reach the caller of <see cref="ChangeLanguage" />.
    /// Disposing the manager removes all handlers.
    /// </summary>
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    /// <summary>
    /// Switches the current language. The call must run on the UI thread. When <paramref name="language" /> is
    /// already current, nothing is published and the result reports <see cref="LanguageChangeStatus.NoOp" />;
    /// otherwise the new state is published, bound language resources are refreshed and
    /// <see cref="LanguageChanged" /> is raised before the method returns.
    /// </summary>
    /// <param name="language">
    /// A language of <see cref="SupportedLanguages" />.
    /// </param>
    /// <returns>
    /// The committed change with the previous state and the new state, or the no-op result.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="language" /> is the default <see cref="LanguageTag" />.
    /// </exception>
    /// <exception cref="LanguageNotSupportedException">
    /// <paramref name="language" /> is not one of <see cref="SupportedLanguages" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The manager is disposed, a language package is being loaded, or the call does not run on the UI thread.
    /// </exception>
    LanguageChangeResult ChangeLanguage(LanguageTag language);
}
