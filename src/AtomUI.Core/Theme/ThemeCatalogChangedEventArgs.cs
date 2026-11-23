namespace AtomUI.Theme;

public sealed class ThemeCatalogChangedEventArgs : EventArgs
{
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

    public long Generation { get; }
    public IReadOnlyList<ThemeInfo> Themes { get; }
    public ThemeState? CurrentTheme { get; }
}
