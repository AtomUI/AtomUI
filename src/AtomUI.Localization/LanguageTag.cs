using System.Globalization;

namespace AtomUI.Localization;

public readonly record struct LanguageTag
{
    private readonly string? _value;

    private LanguageTag(string canonicalValue)
    {
        _value = canonicalValue;
    }

    // Only generated LanguageTags data validated by LanguageDataFileParser may bypass parsing.
    internal static LanguageTag FromCanonical(string canonicalValue)
    {
        return new LanguageTag(canonicalValue);
    }

    public string Value => _value ?? throw new InvalidOperationException(
        "The default LanguageTag value is invalid.");

    public static LanguageTag Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!Bcp47LanguageTagParser.TryParse(value, out var canonicalValue))
        {
            throw new FormatException($"'{value}' is not a valid BCP 47 language tag.");
        }

        return new LanguageTag(canonicalValue);
    }

    public static bool TryParse(string? value, out LanguageTag language)
    {
        if (value is not null && Bcp47LanguageTagParser.TryParse(value, out var canonicalValue))
        {
            language = new LanguageTag(canonicalValue);
            return true;
        }

        language = default;
        return false;
    }

    public static LanguageTag FromCultureInfo(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return Parse(culture.IetfLanguageTag);
    }

    public override string ToString()
    {
        return _value ?? string.Empty;
    }
}
