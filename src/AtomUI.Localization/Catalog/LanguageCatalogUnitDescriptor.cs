namespace AtomUI.Localization;

/// <summary>
/// Describes one unit of a language catalog: the key of a translatable text and whether its text is a format
/// string. A catalog lists its units in a fixed order, and translation bundles and snapshots store their
/// values at the index of the unit in that order.
/// </summary>
public sealed record LanguageCatalogUnitDescriptor
{
    /// <summary>
    /// Creates the descriptor of one language unit.
    /// </summary>
    /// <param name="key">
    /// The key of the unit, unique inside its catalog.
    /// </param>
    /// <param name="isFormatted">
    /// <see langword="true" /> when the text of the unit is a composite format string.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="key" /> is <see langword="null" /> or white space.
    /// </exception>
    public LanguageCatalogUnitDescriptor(string key, bool isFormatted = false)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A language unit Key cannot be empty or whitespace.", nameof(key));
        }

        Key = key;
        IsFormatted = isFormatted;
    }

    /// <summary>
    /// The key of the unit. It is unique inside its catalog, and compiled translation bundles map their values to
    /// units by it, compared ordinally.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// <see langword="true" /> when the text of the unit is a composite format string. The localizer then formats
    /// it with arguments; a snapshot parses the text when it resolves the unit.
    /// </summary>
    public bool IsFormatted { get; }
}
