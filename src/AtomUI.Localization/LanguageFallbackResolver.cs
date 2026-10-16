using System.Globalization;
using AtomUI.Build.Tasks.LocalizationBuild;

namespace AtomUI.Localization;

internal static class LanguageFallbackResolver
{
    internal static IReadOnlyList<LanguageTag> Resolve(
        LanguageTag requestedLanguage,
        CultureInfo formattingCulture)
    {
        if (requestedLanguage == default)
        {
            throw new ArgumentException("A valid requested language is required.", nameof(requestedLanguage));
        }
        ArgumentNullException.ThrowIfNull(formattingCulture);
        return Array.AsReadOnly(LanguageFallbackChain.Resolve(requestedLanguage.Value)
            .Select(LanguageTag.Parse).ToArray());
    }

    internal static bool HasEnglishPrimaryLanguage(LanguageTag language)
    {
        if (language == default)
        {
            throw new ArgumentException("A valid language is required.", nameof(language));
        }
        return LanguageFallbackChain.IsEnglish(language.Value);
    }
}
