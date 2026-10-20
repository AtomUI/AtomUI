using System.Globalization;

namespace AtomUIGallery.Localization;

public static class GalleryLanguageDefaults
{
    public static LanguageTag Resolve(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        var languageTag = culture.IetfLanguageTag;
        if (IsTraditionalChinese(languageTag))
        {
            return LanguageTags.ZhTW;
        }

        if (IsSimplifiedChinese(languageTag))
        {
            return LanguageTags.ZhCN;
        }

        if (IsBrazilianPortuguese(languageTag))
        {
            return LanguageTags.PtBR;
        }

        return LanguageTags.EnUS;
    }

    private static bool IsBrazilianPortuguese(string languageTag)
    {
        return languageTag.Equals("pt-BR", StringComparison.OrdinalIgnoreCase) ||
               languageTag.Equals("pt", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTraditionalChinese(string languageTag)
    {
        return languageTag.Equals("zh-TW", StringComparison.OrdinalIgnoreCase) ||
               languageTag.Equals("zh-HK", StringComparison.OrdinalIgnoreCase) ||
               languageTag.Equals("zh-MO", StringComparison.OrdinalIgnoreCase) ||
               languageTag.StartsWith("zh-Hant-", StringComparison.OrdinalIgnoreCase) ||
               languageTag.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSimplifiedChinese(string languageTag)
    {
        return languageTag.Equals("zh-CN", StringComparison.OrdinalIgnoreCase) ||
               languageTag.Equals("zh-SG", StringComparison.OrdinalIgnoreCase) ||
               languageTag.StartsWith("zh-Hans-", StringComparison.OrdinalIgnoreCase) ||
               languageTag.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase) ||
               languageTag.Equals("zh", StringComparison.OrdinalIgnoreCase);
    }
}
