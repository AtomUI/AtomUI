using System.Globalization;

namespace AtomUI.Localization;

/// <summary>
/// The language-dependent part of a revision of the localization host: the current language, its formatting
/// culture and text direction, and the number of the revision.
/// A language change or a package load replaces the whole state, so the values a reader captures together always
/// belong together.
/// </summary>
public sealed record LanguageState
{
    /// <summary>
    /// Creates a language state.
    /// </summary>
    /// <param name="currentLanguage">
    /// The language that is current. It must not be the default value.
    /// </param>
    /// <param name="formattingCulture">
    /// The culture used to format values while the language is current. It is stored as a read-only clone.
    /// </param>
    /// <param name="textDirection">
    /// The direction in which the language lays out text.
    /// </param>
    /// <param name="revision">
    /// The number of the revision. It must be zero or positive.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="currentLanguage" /> is the default value.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formattingCulture" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="textDirection" /> is neither <see cref="LanguageTextDirection.LeftToRight" /> nor
    /// <see cref="LanguageTextDirection.RightToLeft" />, or <paramref name="revision" /> is negative.
    /// </exception>
    public LanguageState(
        LanguageTag currentLanguage,
        CultureInfo formattingCulture,
        LanguageTextDirection textDirection,
        long revision)
    {
        if (currentLanguage == default)
        {
            throw new ArgumentException("A language state requires a valid current language.", nameof(currentLanguage));
        }

        ArgumentNullException.ThrowIfNull(formattingCulture);

        if (textDirection is not LanguageTextDirection.LeftToRight and
            not LanguageTextDirection.RightToLeft)
        {
            throw new ArgumentOutOfRangeException(nameof(textDirection), textDirection, "Unknown text direction.");
        }

        if (revision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(revision), revision, "A language revision cannot be negative.");
        }

        CurrentLanguage = currentLanguage;
        FormattingCulture = CultureInfo.ReadOnly((CultureInfo)formattingCulture.Clone());
        TextDirection = textDirection;
        Revision = revision;
    }

    /// <summary>
    /// The language that is current in this revision. The revision reads the translation snapshot requested for
    /// this language.
    /// </summary>
    public LanguageTag CurrentLanguage { get; }

    /// <summary>
    /// The culture that formats dates, numbers and similar values while the language is current.
    /// It is a read-only clone of the formatting culture of the language definition.
    /// </summary>
    public CultureInfo FormattingCulture { get; }

    /// <summary>
    /// The direction in which the current language lays out text. The language resource provider derives the flow
    /// direction resource of the host from it.
    /// </summary>
    public LanguageTextDirection TextDirection { get; }

    /// <summary>
    /// The number of the revision. It starts at 0 and grows by 1 each time the state is replaced, also when a
    /// package is loaded and the language stays the same.
    /// </summary>
    public long Revision { get; }
}
