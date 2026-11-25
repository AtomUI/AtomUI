namespace AtomUI.Theme.Languages;

/// <summary>One combination of the four axes a language derives its values for.</summary>
public readonly record struct DesignVariant(
    DesignStyle Style,
    ThemeAppearance Appearance,
    DesignDensity Density,
    DesignIdiom Idiom)
{
    public bool IsDark => Appearance == ThemeAppearance.Dark;

    public override string ToString() => $"{Style} {Appearance} {Density} {Idiom}";
}
