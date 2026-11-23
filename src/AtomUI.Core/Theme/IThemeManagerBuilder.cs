using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Definitions.Compiled;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

public interface IThemeManagerBuilder
{
    void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver);
    void AddCompiledThemePackage(CompiledThemePackage package);

    /// <summary>
    /// Enables a design language. The generated <c>GeneratedDesignLanguageRegistration.Register</c> of a language package
    /// calls this; two languages with the same identity are rejected when the theme schema registry is built.
    /// </summary>
    IThemeManagerBuilder AddDesignLanguage(DesignLanguageDescriptor language);

    /// <summary>
    /// Registers the control derivations of a value package. The generated
    /// <c>GeneratedControlDerivationRegistration.Register</c> calls this; two derivations for the same language token type
    /// and control are rejected when the theme schema registry is built.
    /// </summary>
    IThemeManagerBuilder AddControlDerivations(IEnumerable<ControlDerivationDescriptor> derivations);
    void AddControlPackage(ControlPackageRegistration package);
    void AddInitializer(Action<IThemeManager> initializer);
    void WithInitialTheme(string themeId, ThemeConfig? config = null);

    /// <summary>The application's initial selection (spec 7.9); resolved when the theme manager starts.</summary>
    void WithInitialTheme(ThemeSelection selection);
    void WithInitialDesignLanguage<TLanguage>() where TLanguage : DesignLanguage;
    void SetDefaultTheme<TLanguage>(string themeId) where TLanguage : DesignLanguage;
    void SelectValuePackage<TLanguage>(ControlValuePackage package) where TLanguage : DesignLanguage;
    void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark);
    void WithApplicationId(string applicationId);
    void UseUserThemeDirectory();
    void UseUserThemeDirectory(string directory);
    void WithDefaultFontFamily(FontFamily fontFamily);
    void WithDefaultFontFamily(string fontFamily);
}
