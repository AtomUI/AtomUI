namespace AtomUI.Build.Tasks.LocalizationBuild;

internal static class Bcp47LanguageTagAliases
{
    private static readonly GrandfatheredTag[] GrandfatheredTags =
    [
        new("art-lojban", "jbo"),
        new("cel-gaulish", "cel-gaulish"),
        new("en-GB-oed", "en-GB-oxendict"),
        new("i-ami", "ami"),
        new("i-bnn", "bnn"),
        new("i-default", "i-default"),
        new("i-enochian", "i-enochian"),
        new("i-hak", "hak"),
        new("i-klingon", "tlh"),
        new("i-lux", "lb"),
        new("i-mingo", "i-mingo"),
        new("i-navajo", "nv"),
        new("i-pwn", "pwn"),
        new("i-tao", "tao"),
        new("i-tay", "tay"),
        new("i-tsu", "tsu"),
        new("no-bok", "nb"),
        new("no-nyn", "nn"),
        new("sgn-BE-FR", "sfb"),
        new("sgn-BE-NL", "vgt"),
        new("sgn-CH-DE", "sgg"),
        new("zh-guoyu", "cmn"),
        new("zh-hakka", "hak"),
        new("zh-min", "zh-min"),
        new("zh-min-nan", "nan"),
        new("zh-xiang", "hsn")
    ];

    private static readonly SubtagAlias[] LanguageAliases =
    [
        new("in", "id"),
        new("iw", "he"),
        new("ji", "yi")
    ];

    public static bool TryGetGrandfatheredPreferredValue(
        ReadOnlySpan<char> value,
        out string preferredValue)
    {
        foreach (var tag in GrandfatheredTags)
        {
            if (value.Equals(tag.Source, StringComparison.OrdinalIgnoreCase))
            {
                preferredValue = tag.PreferredValue;
                return true;
            }
        }

        preferredValue = string.Empty;
        return false;
    }

    public static bool TryGetLanguagePreferredValue(
        ReadOnlySpan<char> value,
        out string preferredValue)
    {
        foreach (var alias in LanguageAliases)
        {
            if (value.Equals(alias.Source, StringComparison.OrdinalIgnoreCase))
            {
                preferredValue = alias.PreferredValue;
                return true;
            }
        }

        preferredValue = string.Empty;
        return false;
    }

    private readonly struct GrandfatheredTag
    {
        internal GrandfatheredTag(string source, string preferredValue)
        {
            Source = source;
            PreferredValue = preferredValue;
        }

        internal string Source { get; }

        internal string PreferredValue { get; }
    }

    private readonly struct SubtagAlias
    {
        internal SubtagAlias(string source, string preferredValue)
        {
            Source = source;
            PreferredValue = preferredValue;
        }

        internal string Source { get; }

        internal string PreferredValue { get; }
    }
}
