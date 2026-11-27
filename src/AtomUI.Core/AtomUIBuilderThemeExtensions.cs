using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI;

/// <summary>
/// Theme configuration of an <see cref="IAtomUIBuilder" />. Every method forwards to
/// <see cref="IAtomUIBuilder.Theme" /> and returns the builder for chaining.
/// </summary>
/// <remarks>
/// Except for <see cref="AddControlPackage" />, the methods change the initial application configuration and
/// are rejected inside <see cref="IAtomUI.LoadPackages" />.
/// </remarks>
public static class AtomUIBuilderThemeExtensions
{
    /// <summary>
    /// Adds a resolver that supplies theme definitions to the theme catalog.
    /// </summary>
    /// <remarks>
    /// The theme manager reads the resolver when it builds the catalog at startup and when the catalog reloads.
    /// <see cref="IThemeDefinitionResolver.Id" /> must be non-blank and unique among the registered resolvers and
    /// compiled theme packages; a duplicate throws <see cref="ThemeResourceRegisterException" />.
    /// </remarks>
    public static IAtomUIBuilder AddThemeDefinitionResolver(
        this IAtomUIBuilder builder,
        IThemeDefinitionResolver resolver)
    {
        GetTheme(builder).AddThemeDefinitionResolver(resolver);
        return builder;
    }

    /// <summary>
    /// Registers a control package with the theme manager: its control tokens, theme assets, semantic part
    /// descriptors and control themes.
    /// </summary>
    /// <exception cref="ThemeResourceRegisterException">
    /// The package id, the id of its control themes provider, or one of its control token or semantic part
    /// descriptor identities is already registered. The builder then rejects every further registration.
    /// </exception>
    public static IAtomUIBuilder AddControlPackage(
        this IAtomUIBuilder builder,
        ControlPackageRegistration package)
    {
        GetTheme(builder).AddControlPackage(package);
        return builder;
    }

    /// <summary>
    /// Adds a callback that receives the theme manager during the startup commit.
    /// </summary>
    /// <remarks>
    /// Initializers run in registration order on the thread that calls <see cref="ApplicationStartup.Attach" />,
    /// after the application scope and the initial theme are initialized and before the control packages are
    /// committed. An exception that an initializer throws rolls back the startup commit.
    /// </remarks>
    public static IAtomUIBuilder AddInitializer(
        this IAtomUIBuilder builder,
        Action<IThemeManager> initializer)
    {
        GetTheme(builder).AddInitializer(initializer);
        return builder;
    }

    /// <summary>
    /// Selects the theme that the application starts with, by its id in the theme catalog.
    /// </summary>
    /// <remarks>
    /// <paramref name="config" /> is the application layer of the theme configuration: it is applied on top of the
    /// theme's defaults and stays in effect for every later theme request. An id that the catalog does not
    /// contain makes startup fail with a <see cref="ThemeLoadException" />. A later call, or
    /// <see cref="WithFollowSystemThemes" />, replaces the selection.
    /// </remarks>
    public static IAtomUIBuilder WithInitialTheme(
        this IAtomUIBuilder builder,
        string themeId,
        ThemeConfig? config = null)
    {
        GetTheme(builder).WithInitialTheme(themeId, config);
        return builder;
    }

    /// <summary>
    /// Sets the selection that the application starts with: a design language with, as chosen, a named theme,
    /// an appearance (or following the system), a density and seeds.
    /// </summary>
    /// <remarks>
    /// The selection is resolved against the theme catalog when the theme manager is initialized. A selection
    /// that follows the system starts with the theme of the current system appearance and switches when the
    /// system appearance changes. A later call, or <see cref="WithFollowSystemThemes" />, replaces the selection.
    /// </remarks>
    public static IAtomUIBuilder WithInitialTheme(this IAtomUIBuilder builder, ThemeSelection selection)
    {
        GetTheme(builder).WithInitialTheme(selection);
        return builder;
    }

    /// <summary>
    /// Starts with the given enabled design language and its default theme.
    /// </summary>
    /// <remarks>
    /// The default theme is the one configured with <c>SetDefaultTheme</c>, or else the language's own default.
    /// This method cannot be combined with <c>WithInitialTheme</c> or <c>WithFollowSystemThemes</c>: whichever
    /// is called second throws an <see cref="InvalidOperationException" />.
    /// </remarks>
    public static IAtomUIBuilder WithInitialDesignLanguage<TLanguage>(this IAtomUIBuilder builder)
        where TLanguage : DesignLanguage
    {
        GetTheme(builder).WithInitialDesignLanguage<TLanguage>();
        return builder;
    }

    /// <summary>
    /// Starts with the theme that matches the system appearance and switches to <paramref name="dark" /> or
    /// <paramref name="light" /> whenever the system appearance changes.
    /// </summary>
    /// <remarks>
    /// Each request names a theme and its configuration; its transition reason becomes
    /// <see cref="ThemeTransitionReason.FollowSystem" />. The call replaces an earlier <c>WithInitialTheme</c>
    /// or <c>WithFollowSystemThemes</c> call.
    /// </remarks>
    public static IAtomUIBuilder WithFollowSystemThemes(
        this IAtomUIBuilder builder,
        ThemeRequest light,
        ThemeRequest dark)
    {
        GetTheme(builder).WithFollowSystemThemes(light, dark);
        return builder;
    }

    /// <summary>
    /// Sets the application id. It is passed to theme definition resolvers and names the application's user theme
    /// directory.
    /// </summary>
    /// <remarks>
    /// The id defaults to the name of the assembly that declares the application type, or to the name of the
    /// AtomUI.Core assembly when that type is unknown. It may contain only ASCII letters, digits, <c>.</c>,
    /// <c>_</c> and <c>-</c>, and must not be <c>.</c> or <c>..</c>.
    /// </remarks>
    public static IAtomUIBuilder WithApplicationId(
        this IAtomUIBuilder builder,
        string applicationId)
    {
        GetTheme(builder).WithApplicationId(applicationId);
        return builder;
    }

    /// <summary>
    /// Loads theme definitions from the <c>*.theme.xml</c> files in the application's user theme directory.
    /// </summary>
    /// <remarks>
    /// The directory is <c>Themes</c> under <c>&lt;application data&gt;/&lt;application id&gt;</c>; it is created
    /// when it does not exist. The files must be its direct children and must not be symbolic links; at most
    /// 128 files and 32 MiB in total are read. It cannot be combined with a resolver whose <c>Id</c> is
    /// <c>AtomUI.UserThemeDirectory</c>.
    /// </remarks>
    public static IAtomUIBuilder UseUserThemeDirectory(this IAtomUIBuilder builder)
    {
        GetTheme(builder).UseUserThemeDirectory();
        return builder;
    }

    /// <summary>
    /// Loads theme definitions from the <c>*.theme.xml</c> files in the given directory instead of the
    /// application's user theme directory.
    /// </summary>
    /// <remarks>
    /// The same file rules and limits apply as for <see cref="UseUserThemeDirectory(IAtomUIBuilder)" />.
    /// </remarks>
    public static IAtomUIBuilder UseUserThemeDirectory(
        this IAtomUIBuilder builder,
        string directory)
    {
        GetTheme(builder).UseUserThemeDirectory(directory);
        return builder;
    }

    /// <summary>
    /// Sets the default font family, which supplies the global <c>FontFamily</c> Token when the configuration of a
    /// theme request does not set it. It is read each time a theme is compiled.
    /// </summary>
    public static IAtomUIBuilder WithDefaultFontFamily(
        this IAtomUIBuilder builder,
        FontFamily fontFamily)
    {
        GetTheme(builder).WithDefaultFontFamily(fontFamily);
        return builder;
    }

    /// <summary>
    /// Sets the default font family from a font family string that <see cref="FontFamily.Parse(string)" /> accepts.
    /// </summary>
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
