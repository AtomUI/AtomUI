using AtomUI.Theme;

namespace AtomUI.Themes.PrimeNG;

public static class PrimeNGThemeManagerBuilderExtensions
{
    public static IThemeManagerBuilder UsePrimeNGThemes(this IThemeManagerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIThemesPrimeNG.GeneratedThemePackageRegistration.Register(builder);
        return builder;
    }
}
