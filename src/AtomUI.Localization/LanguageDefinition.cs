using System.Globalization;

namespace AtomUI.Localization;

/// <summary>
/// Describes one supported language: its tag, the culture that formats values while the language is current, the
/// name of the language as it is written in that language, and its text direction.
/// Register an instance with <see cref="ILocalizationBuilder.AddLanguageDefinition(LanguageDefinition)" />; it is
/// then used in place of the generated standard metadata of the same tag.
/// </summary>
public sealed record LanguageDefinition
{
    /// <summary>
    /// Creates a language definition.
    /// </summary>
    /// <param name="tag">
    /// The language the definition describes. It must not be the default value.
    /// </param>
    /// <param name="formattingCulture">
    /// The culture used to format values while the language is current. It is stored as a read-only clone.
    /// </param>
    /// <param name="nativeName">
    /// The name of the language as it is written in that language. It must not be empty or white space.
    /// </param>
    /// <param name="textDirection">
    /// The direction in which the language lays out text.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formattingCulture" /> or <paramref name="nativeName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="tag" /> is the default value, or <paramref name="nativeName" /> is empty or white space.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="textDirection" /> is neither <see cref="LanguageTextDirection.LeftToRight" /> nor
    /// <see cref="LanguageTextDirection.RightToLeft" />.
    /// </exception>
    public LanguageDefinition(
        LanguageTag tag,
        CultureInfo formattingCulture,
        string nativeName,
        LanguageTextDirection textDirection)
    {
        if (tag == default)
        {
            throw new ArgumentException("A language definition requires a valid language tag.", nameof(tag));
        }

        ArgumentNullException.ThrowIfNull(formattingCulture);
        ArgumentNullException.ThrowIfNull(nativeName);

        if (string.IsNullOrWhiteSpace(nativeName))
        {
            throw new ArgumentException("A native language name cannot be empty or whitespace.", nameof(nativeName));
        }

        if (textDirection is not LanguageTextDirection.LeftToRight and
            not LanguageTextDirection.RightToLeft)
        {
            throw new ArgumentOutOfRangeException(nameof(textDirection), textDirection, "Unknown text direction.");
        }

        Tag = tag;
        FormattingCulture = CultureInfo.ReadOnly((CultureInfo)formattingCulture.Clone());
        NativeName = nativeName;
        TextDirection = textDirection;
    }

    /// <summary>
    /// The language the definition describes. Definitions are registered and looked up by this tag, and a tag can
    /// have only one definition.
    /// </summary>
    public LanguageTag Tag { get; }

    /// <summary>
    /// The culture that formats dates, numbers and similar values while the language is current.
    /// It is a read-only clone of the culture passed to the constructor, so later changes to that culture do not
    /// affect the definition.
    /// </summary>
    public CultureInfo FormattingCulture { get; }

    /// <summary>
    /// The name of the language as it is written in that language.
    /// </summary>
    public string NativeName { get; }

    /// <summary>
    /// The direction in which the language lays out text. The language resource provider derives the flow direction
    /// resource of the current language from it.
    /// </summary>
    public LanguageTextDirection TextDirection { get; }
}
