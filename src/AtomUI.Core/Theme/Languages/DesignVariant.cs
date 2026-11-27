namespace AtomUI.Theme.Languages;

/// <summary>
/// One combination of the four axes a language derives its values for.
/// </summary>
public readonly record struct DesignVariant(
    DesignStyle Style,
    ThemeAppearance Appearance,
    DesignDensity Density,
    DesignIdiom Idiom)
{
    /// <summary>
    /// Whether the appearance is <see cref="ThemeAppearance.Dark" />.
    /// </summary>
    public bool IsDark => Appearance == ThemeAppearance.Dark;

    /// <summary>
    /// Returns the identities of the style, appearance, density and idiom, separated by spaces.
    /// </summary>
    public override string ToString() => $"{Style} {Appearance} {Density} {Idiom}";
}
