using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Branding;
using Avalonia.Media;

namespace AtomUI;

public static class AtomUIBuilderThemeExtensions
{
    public static IAtomUIBuilder AddThemeStyleAsset(this IAtomUIBuilder builder, ThemeStyleAssetRegistration registration)
    {
        GetTheme(builder).AddThemeStyleAsset(registration);
        return builder;
    }

    public static IAtomUIBuilder AddThemeDefinitionResolver(
        this IAtomUIBuilder builder,
        IThemeDefinitionResolver resolver)
    {
        GetTheme(builder).AddThemeDefinitionResolver(resolver);
        return builder;
    }

    public static IAtomUIBuilder AddControlPackage(
        this IAtomUIBuilder builder,
        ControlPackageRegistration package)
    {
        GetTheme(builder).AddControlPackage(package);
        return builder;
    }

    public static IAtomUIBuilder AddInitializer(
        this IAtomUIBuilder builder,
        Action<IThemeManager> initializer)
    {
        GetTheme(builder).AddInitializer(initializer);
        return builder;
    }

    public static IAtomUIBuilder WithInitialTheme(
        this IAtomUIBuilder builder,
        string themeId,
        ThemeConfig? config = null)
    {
        GetTheme(builder).WithInitialTheme(themeId, config);
        return builder;
    }

    public static IAtomUIBuilder WithFollowSystemThemes(
        this IAtomUIBuilder builder,
        ThemeRequest light,
        ThemeRequest dark)
    {
        GetTheme(builder).WithFollowSystemThemes(light, dark);
        return builder;
    }

    public static IAtomUIBuilder WithApplicationId(
        this IAtomUIBuilder builder,
        string applicationId)
    {
        GetTheme(builder).WithApplicationId(applicationId);
        return builder;
    }

    public static IAtomUIBuilder UseUserThemeDirectory(this IAtomUIBuilder builder)
    {
        GetTheme(builder).UseUserThemeDirectory();
        return builder;
    }

    public static IAtomUIBuilder UseUserThemeDirectory(
        this IAtomUIBuilder builder,
        string directory)
    {
        GetTheme(builder).UseUserThemeDirectory(directory);
        return builder;
    }

    public static IAtomUIBuilder WithDefaultFontFamily(
        this IAtomUIBuilder builder,
        FontFamily fontFamily)
    {
        GetTheme(builder).WithDefaultFontFamily(fontFamily);
        return builder;
    }

    public static IAtomUIBuilder WithDefaultFontFamily(
        this IAtomUIBuilder builder,
        string fontFamily)
    {
        GetTheme(builder).WithDefaultFontFamily(fontFamily);
        return builder;
    }

    private static IThemeManagerBuilder GetTheme(IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Theme;
    }
}
