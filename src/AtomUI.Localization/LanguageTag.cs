using System.Globalization;

namespace AtomUI.Localization;

/// <summary>
/// A validated BCP 47 language tag in canonical form. Two tags are equal when their canonical text is equal.
/// The default value is not a valid tag: methods reject it through <c>tag == default</c>, and reading
/// <see cref="Value" /> on it throws.
/// </summary>
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

    /// <summary>
    /// The canonical text of the tag.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This instance is the default value.
    /// </exception>
    public string Value => _value ?? throw new InvalidOperationException(
        "The default LanguageTag value is invalid.");

    /// <summary>
    /// Parses a BCP 47 language tag and returns it in canonical form.
    /// </summary>
    /// <param name="value">
    /// The text of the language tag.
    /// </param>
    /// <returns>
    /// The tag in canonical form, with normalized subtag casing.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value" /> is not a valid BCP 47 language tag.
    /// </exception>
    public static LanguageTag Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!Bcp47LanguageTagParser.TryParse(value, out var canonicalValue))
        {
            throw new FormatException($"'{value}' is not a valid BCP 47 language tag.");
        }

        return new LanguageTag(canonicalValue);
    }

    /// <summary>
    /// Tries to parse a BCP 47 language tag.
    /// </summary>
    /// <param name="value">
    /// The text of the language tag, or <see langword="null" />.
    /// </param>
    /// <param name="language">
    /// The tag in canonical form when parsing succeeds; otherwise the default value.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the tag is valid; otherwise <see langword="false" />.
    /// </returns>
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

    /// <summary>
    /// Creates a language tag from the IETF language tag of a culture.
    /// </summary>
    /// <param name="culture">
    /// The culture whose IETF language tag names the language.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="culture" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="FormatException">
    /// The IETF language tag of <paramref name="culture" /> is not a valid BCP 47 language tag.
    /// </exception>
    public static LanguageTag FromCultureInfo(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return Parse(culture.IetfLanguageTag);
    }

    /// <summary>
    /// Returns the canonical text of the tag, or an empty string for the default value.
    /// </summary>
    public override string ToString()
    {
        return _value ?? string.Empty;
    }
}
