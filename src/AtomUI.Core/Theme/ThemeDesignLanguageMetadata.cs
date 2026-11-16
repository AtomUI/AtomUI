namespace AtomUI.Theme;

/// <summary>
/// The spacing density a theme definition is authored for.
/// </summary>
public enum ThemeDensity : byte
{
    /// <summary>The regular density.</summary>
    Normal,

    /// <summary>The compact density.</summary>
    Compact
}

/// <summary>
/// Describes whether a theme definition lets the user replace its brand color.
/// </summary>
public enum ThemeBrandColorSupport : byte
{
    /// <summary>The brand color is fixed by the definition and cannot be replaced.</summary>
    Fixed,

    /// <summary>The definition derives its brand tokens from a replaceable seed color.</summary>
    Seed
}

/// <summary>
/// Describes whether a theme definition lets the user replace its neutral color family.
/// </summary>
public enum ThemeNeutralColorSupport : byte
{
    /// <summary>The neutral family is fixed by the definition and cannot be replaced.</summary>
    Fixed,

    /// <summary>The definition derives its neutral tokens from a replaceable seed color.</summary>
    Seed
}
