namespace AtomUI.Localization;

/// <summary>
/// Reads translated text in code by the value of a language resource enum. Every call reads the language that
/// is current at that moment and returns plain text, so a value that is kept must be read again after a
/// language change. In XAML, language resources are read with
/// <see cref="LanguageResourceExtension{TResourceKind}" />.
/// </summary>
public interface ILocalizer
{
    /// <summary>
    /// Returns the text of the language unit that <paramref name="key" /> stands for, resolved for the current
    /// language and its fallback languages. The text is returned as it is stored; a unit that is a format string
    /// is not formatted here.
    /// </summary>
    /// <typeparam name="TResourceKind">
    /// The language resource enum type that declares <paramref name="key" />.
    /// </typeparam>
    /// <param name="key">
    /// The language resource value to read.
    /// </param>
    /// <exception cref="LanguageCatalogException">
    /// <typeparamref name="TResourceKind" /> is not registered as a language catalog, or <paramref name="key" />
    /// maps to no unit of it.
    /// </exception>
    string Get<TResourceKind>(TResourceKind key)
        where TResourceKind : struct, Enum;

    /// <summary>
    /// Returns the text of the language unit that <paramref name="key" /> stands for, formatted with
    /// <paramref name="arguments" /> in the formatting culture of the current language. A unit that is not a
    /// format string returns its text as it is stored, and the arguments are ignored.
    /// </summary>
    /// <typeparam name="TResourceKind">
    /// The language resource enum type that declares <paramref name="key" />.
    /// </typeparam>
    /// <param name="key">
    /// The language resource value to read.
    /// </param>
    /// <param name="arguments">
    /// The arguments that fill the placeholders of the format string.
    /// </param>
    /// <exception cref="LanguageCatalogException">
    /// The unit cannot be resolved, or formatting the text fails.
    /// </exception>
    string Format<TResourceKind>(TResourceKind key, params object?[] arguments)
        where TResourceKind : struct, Enum;
}
