using AtomUI.Localization;

namespace AtomUI;

/// <summary>
/// Extension methods that configure the localization of an <see cref="IAtomUIBuilder" />.
/// </summary>
public static class AtomUIBuilderLocalizationExtensions
{
    /// <summary>
    /// Sets the languages the application supports and the language it starts with. Call it while the builder
    /// is being configured; a later call replaces the earlier languages.
    /// </summary>
    /// <param name="builder">
    /// The builder whose localization is configured.
    /// </param>
    /// <param name="defaultLanguage">
    /// The language that is active at startup. It must be a valid tag and one of
    /// <paramref name="supportedLanguages" />.
    /// </param>
    /// <param name="supportedLanguages">
    /// The languages the application can switch to. The sequence is read once and must contain at least one
    /// valid tag.
    /// </param>
    /// <returns>
    /// <paramref name="builder" />, for chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The localization builder is already frozen or is loading existing packages.
    /// </exception>
    /// <remarks>
    /// The build reads the arguments of this call from source to verify that every supported language has its
    /// translations. The call must be inside the <c>UseAtomUI</c> setup lambda or in a method that takes an
    /// <see cref="IAtomUIBuilder" />. Each language must be a property of <c>LanguageTags</c> or a
    /// <c>LanguageTag.Parse</c> call with a string literal, inside an array or collection expression.
    /// </remarks>
    public static IAtomUIBuilder UseLanguages(
        this IAtomUIBuilder builder,
        LanguageTag defaultLanguage,
        IEnumerable<LanguageTag> supportedLanguages)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Localization.ConfigureLanguages(defaultLanguage, supportedLanguages);
        return builder;
    }
}
