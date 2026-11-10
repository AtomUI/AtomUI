using AtomUI.Theme;

namespace AtomUI.Themes.Shadcn;

public static class ShadcnThemeManagerBuilderExtensions
{
    public static IThemeManagerBuilder UseShadcnThemes(this IThemeManagerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIThemesShadcn.GeneratedThemePackageRegistration.Register(builder);
        return builder;
    }
}
