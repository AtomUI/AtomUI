using AtomUI.Controls;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Definitions.Compiled;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

/// <summary>
/// Collects the theme configuration of an application until its theme manager is built: design languages, control
/// packages, theme definitions and the theme to start with.
/// </summary>
/// <remarks>
/// Every method throws an <see cref="InvalidOperationException" /> once the theme manager is built, or after a
/// registration has failed. All methods but <see cref="AddDesignLanguage" />,
/// <see cref="AddControlTokenRules" /> and <see cref="AddControlPackage" /> also throw it inside
/// <see cref="IAtomUI.LoadPackages" />.
/// </remarks>
public interface IThemeManagerBuilder
{
    /// <summary>
    /// Adds a resolver that supplies theme definitions to the theme catalog.
    /// </summary>
    /// <param name="resolver">
    /// The resolver to add; a blank <see cref="IThemeDefinitionResolver.Id" /> is rejected.
    /// </param>
    /// <exception cref="ThemeResourceRegisterException">
    /// A resolver or compiled theme package with the same id is already registered.
    /// </exception>
    /// <remarks>
    /// The theme manager reads the resolver when it builds the catalog at startup and, if the resolver supports
    /// reload, whenever the catalog is reloaded.
    /// </remarks>
    void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver);
    /// <summary>
    /// Registers a compiled theme package, a generated set of theme definitions, with the theme catalog.
    /// </summary>
    /// <param name="package">
    /// The compiled theme package to add.
    /// </param>
    /// <exception cref="ThemeResourceRegisterException">
    /// A resolver or compiled theme package with the same id is already registered.
    /// </exception>
    /// <remarks>
    /// The generated <c>Register</c> method of the package calls this.
    /// </remarks>
    void AddCompiledThemePackage(CompiledThemePackage package);

    /// <summary>
    /// Enables a design language. The generated <c>DesignLanguageRegistration.Register</c> of a language package
    /// calls this; two languages with the same identity are rejected when the theme schema registry is built.
    /// </summary>
    IThemeManagerBuilder AddDesignLanguage(DesignLanguageDescriptor language);

    /// <summary>
    /// Registers the control rules of a value package. The generated
    /// <c>GeneratedControlTokenRuleRegistration.Register</c> calls this; two rules for the same language token type
    /// and control are rejected when the theme schema registry is built.
    /// </summary>
    IThemeManagerBuilder AddControlTokenRules(IEnumerable<ControlTokenRuleDescriptor> rules);

    /// <summary>
    /// Queues generated rule metadata for package preparation rather than configuration.
    /// </summary>
    void AddDeferredControlTokenRules(Action<IThemeManagerBuilder> register) => register(this);

    /// <summary>
    /// Registers a generated implementation for a control and a design language.
    /// </summary>
    void AddControlBehavior(ControlBehaviorDescriptor behavior)
    {
        throw new NotSupportedException("This builder does not support language behavior contributions.");
    }

    /// <summary>
    /// Registers a control package: its control and semantic part descriptors, its control themes provider and its
    /// theme resources.
    /// </summary>
    /// <param name="package">
    /// The control package to register.
    /// </param>
    /// <exception cref="ThemeResourceRegisterException">
    /// The package id, the id of its control themes provider, or the identity of one of its control or semantic
    /// control descriptors is already registered. The builder then rejects every further registration.
    /// </exception>
    void AddControlPackage(ControlPackageRegistration package);
    /// <summary>
    /// Adds a callback that receives the theme manager during the startup commit.
    /// </summary>
    /// <param name="initializer">
    /// The callback to run.
    /// </param>
    /// <remarks>
    /// Initializers run in registration order, after the application scope is initialized and before the control
    /// packages are committed. An exception that an initializer throws aborts the startup commit.
    /// </remarks>
    void AddInitializer(Action<IThemeManager> initializer);
    /// <summary>
    /// Selects the theme that the application starts with, by its id in the theme catalog.
    /// </summary>
    /// <param name="themeId">
    /// The id of a named theme of the catalog.
    /// </param>
    /// <param name="config">
    /// The theme configuration that is applied on top of the theme's defaults.
    /// </param>
    /// <remarks>
    /// Startup fails with a <see cref="ThemeLoadException" /> when the catalog does not contain the id. A later
    /// call to this method or to <see cref="WithFollowSystemThemes" /> replaces the selection. The method cannot
    /// be combined with <c>WithInitialDesignLanguage</c>: whichever is called second throws an
    /// <see cref="InvalidOperationException" />.
    /// </remarks>
    void WithInitialTheme(string themeId, ThemeConfig? config = null);

    /// <summary>
    /// Sets the selection that the application starts with. It is resolved against the theme catalog when the theme
    /// manager starts.
    /// </summary>
    /// <param name="selection">
    /// A design language with, as chosen, a named theme, an appearance or following the system, a density and seeds.
    /// </param>
    /// <remarks>
    /// A selection that follows the system starts with the theme of the current system appearance and switches to
    /// the other theme whenever the system appearance changes. A later call to this method or to
    /// <see cref="WithFollowSystemThemes" /> replaces the selection. The method cannot be combined with
    /// <c>WithInitialDesignLanguage</c>: whichever is called second throws an
    /// <see cref="InvalidOperationException" />.
    /// </remarks>
    void WithInitialTheme(ThemeSelection selection);
    /// <summary>
    /// Starts with the given design language and its default theme.
    /// </summary>
    /// <typeparam name="TLanguage">
    /// An enabled design language.
    /// </typeparam>
    /// <remarks>
    /// The language must be enabled with <see cref="AddDesignLanguage" />; otherwise building the theme manager
    /// fails. Its default theme is the one set with <c>SetDefaultTheme</c>, or else the language's own default. The
    /// method cannot be combined with <c>WithInitialTheme</c> or <see cref="WithFollowSystemThemes" />: whichever is
    /// called second throws an <see cref="InvalidOperationException" />.
    /// </remarks>
    void WithInitialDesignLanguage<TLanguage>() where TLanguage : DesignLanguage;
    /// <summary>
    /// Sets the default theme of a design language, which is used when a selection names the language without a
    /// theme.
    /// </summary>
    /// <typeparam name="TLanguage">
    /// An enabled design language.
    /// </typeparam>
    /// <param name="themeId">
    /// The id of a named theme of the catalog.
    /// </param>
    /// <remarks>
    /// The theme is used only while the catalog contains it for that language; otherwise the language's own default
    /// is used. Setting a different theme for the same language again throws an
    /// <see cref="InvalidOperationException" />.
    /// </remarks>
    void SetDefaultTheme<TLanguage>(string themeId) where TLanguage : DesignLanguage;
    /// <summary>
    /// Selects the value package that provides the control rules of a control assembly for a design language.
    /// </summary>
    /// <typeparam name="TLanguage">
    /// The design language that the value package applies to.
    /// </typeparam>
    /// <param name="package">
    /// The value package to select.
    /// </param>
    /// <remarks>
    /// Only one value package can be selected for a control assembly and language. Selecting another one for the
    /// same pair throws an <see cref="InvalidOperationException" />.
    /// </remarks>
    void SelectValuePackage<TLanguage>(ControlValuePackage package) where TLanguage : DesignLanguage;
    /// <summary>
    /// Starts with the theme that matches the system appearance and switches to <paramref name="dark" /> or
    /// <paramref name="light" /> whenever the system appearance changes.
    /// </summary>
    /// <param name="light">
    /// The theme request that applies while the system appearance is light.
    /// </param>
    /// <param name="dark">
    /// The theme request that applies while the system appearance is dark.
    /// </param>
    /// <remarks>
    /// The transition reason of both requests becomes <c>ThemeTransitionReason.FollowSystem</c>. The call replaces
    /// an earlier <c>WithInitialTheme</c> or <see cref="WithFollowSystemThemes" /> call, and cannot be combined
    /// with <c>WithInitialDesignLanguage</c>: whichever is called second throws an
    /// <see cref="InvalidOperationException" />.
    /// </remarks>
    void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark);
    /// <summary>
    /// Sets the application id. Theme definition resolvers receive it, and it names the application's user theme
    /// directory.
    /// </summary>
    /// <param name="applicationId">
    /// ASCII letters, digits, <c>.</c>, <c>_</c> and <c>-</c>; <c>.</c> and <c>..</c> alone are rejected.
    /// </param>
    /// <remarks>
    /// When it is not set, the id is the name of the assembly that declares the application type.
    /// </remarks>
    void WithApplicationId(string applicationId);
    /// <summary>
    /// Loads theme definitions from the <c>*.theme.xml</c> files in the application's user theme directory.
    /// </summary>
    /// <remarks>
    /// The directory is <c>Themes</c> under <c>&lt;application data&gt;/&lt;application id&gt;</c>; it is created
    /// when it does not exist. The directory itself must not be a symbolic link. The files must be its direct
    /// children and not symbolic links, and at most 128 files are read. A theme definition resolver with the id
    /// <c>AtomUI.UserThemeDirectory</c> makes building the theme manager fail.
    /// </remarks>
    void UseUserThemeDirectory();
    /// <summary>
    /// Loads theme definitions from the <c>*.theme.xml</c> files in the given directory instead of the application's
    /// user theme directory.
    /// </summary>
    /// <param name="directory">
    /// The directory to read; a relative path is resolved against the current directory.
    /// </param>
    /// <remarks>
    /// The same file rules and limits apply as for <see cref="UseUserThemeDirectory()" />.
    /// </remarks>
    void UseUserThemeDirectory(string directory);
    /// <summary>
    /// Sets the default font family. It supplies the global <c>FontFamily</c> Token whenever a theme is compiled from
    /// a configuration that does not set that Token.
    /// </summary>
    /// <param name="fontFamily">
    /// The font family to use as the default.
    /// </param>
    void WithDefaultFontFamily(FontFamily fontFamily);
    /// <summary>
    /// Sets the default font family from a font family string that
    /// <see cref="FontFamily.Parse(string)" /> accepts.
    /// </summary>
    /// <param name="fontFamily">
    /// The font family string to parse.
    /// </param>
    void WithDefaultFontFamily(string fontFamily);
}
