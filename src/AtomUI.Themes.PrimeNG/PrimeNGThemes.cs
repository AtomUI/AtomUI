using AtomUI.Theme;

namespace AtomUI.Themes.PrimeNG;

public static class PrimeNGThemeManagerBuilderExtensions
{
    /// <summary>
    /// Registers the PrimeNG preset algorithms (<c>PrimeNG:Aura</c>, <c>PrimeNG:Aura.Dark</c>), their control token
    /// stages and the PrimeNG preset theme profiles. The generated <c>GeneratedThemeAlgorithmRegistration</c> adds
    /// both the algorithms and the control stages.
    /// </summary>
    public static IThemeManagerBuilder UsePrimeNGThemes(this IThemeManagerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIThemesPrimeNG.GeneratedThemeAlgorithmRegistration.Register(builder);
        Generated.AtomUIThemesPrimeNG.GeneratedThemePackageRegistration.Register(builder);
        return builder;
    }
}
