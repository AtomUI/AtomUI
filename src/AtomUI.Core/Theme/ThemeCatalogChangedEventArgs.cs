namespace AtomUI.Theme;

/// <summary>
/// The data of the <see cref="IThemeManager.ThemeCatalogChanged" /> event: the catalog and the theme in effect
/// after a reload that was committed.
/// </summary>
public sealed class ThemeCatalogChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event data of a committed catalog reload.
    /// </summary>
    public ThemeCatalogChangedEventArgs(
        long generation,
        IReadOnlyList<ThemeInfo> themes,
        ThemeState? currentTheme)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);
        ArgumentNullException.ThrowIfNull(themes);

        Generation      = generation;
        Themes = Array.AsReadOnly(themes.ToArray());
        CurrentTheme    = currentTheme;
    }

    /// <summary>
    /// The generation of the reload. It is the generation that the result of
    /// <see cref="IThemeManager.ReloadThemesAsync" /> reports for this reload.
    /// </summary>
    public long Generation { get; }
    /// <summary>
    /// The themes of the catalog after the reload.
    /// </summary>
    public IReadOnlyList<ThemeInfo> Themes { get; }
    /// <summary>
    /// The theme the root scope shows after the reload, or <see langword="null" /> when none is committed.
    /// </summary>
    public ThemeState? CurrentTheme { get; }
}
