using AtomUI.Theme;

namespace AtomUI.Themes.PrimeNG;

public static class PrimeNGThemeManagerBuilderExtensions
{
    /// <summary>
    /// Registers the PrimeNG preset algorithms (<c>PrimeNG:Aura</c>, <c>PrimeNG:Aura.Dark</c>) and the PrimeNG preset
    /// theme profiles.
    /// </summary>
    public static IThemeManagerBuilder UsePrimeNGThemes(this IThemeManagerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIThemesPrimeNG.GeneratedThemeAlgorithmRegistration.Register(builder);
        Generated.AtomUIThemesPrimeNG.GeneratedThemePackageRegistration.Register(builder);
        return builder;
    }
}
