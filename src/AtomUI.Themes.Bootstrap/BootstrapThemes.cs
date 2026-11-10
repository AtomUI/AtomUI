using AtomUI.Theme;

namespace AtomUI.Themes.Bootstrap;

public static class BootstrapThemeManagerBuilderExtensions
{
    public static IThemeManagerBuilder UseBootstrapThemes(this IThemeManagerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIThemesBootstrap.GeneratedThemePackageRegistration.Register(builder);
        return builder;
    }
}
