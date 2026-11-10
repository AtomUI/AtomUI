using AtomUI.Theme;

namespace AtomUI.Themes.Material;

public static class MaterialThemeManagerBuilderExtensions
{
    public static IThemeManagerBuilder UseMaterialThemes(this IThemeManagerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Generated.AtomUIThemesMaterial.GeneratedThemePackageRegistration.Register(builder);
        return builder;
    }
}
