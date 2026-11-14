using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Definitions.Compiled;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

public interface IThemeManagerBuilder
{
    void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver);
    void AddCompiledThemePackage(CompiledThemePackage package);

    /// <summary>
    /// Registers theme algorithms declared by a package so that theme definitions and configurations can reference them.
    /// Generated <c>GeneratedThemeAlgorithmRegistration.Register</c> calls this; duplicate identities are rejected when the
    /// theme schema registry is built.
    /// </summary>
    IThemeManagerBuilder AddThemeAlgorithms(IEnumerable<ThemeAlgorithmDescriptor> algorithms);

    /// <summary>
    /// Registers control token stages declared by a package with <c>[ControlTokenAlgorithm]</c>.
    /// Generated <c>GeneratedThemeAlgorithmRegistration.Register</c> calls this; two stages for the same algorithm and
    /// control are rejected when the theme schema registry is built. A stage whose algorithm or control is not
    /// registered is kept but never called.
    /// </summary>
    IThemeManagerBuilder AddControlTokenAlgorithms(IEnumerable<ControlTokenAlgorithmDescriptor> algorithms);
    void AddControlPackage(ControlPackageRegistration package);
    void AddInitializer(Action<IThemeManager> initializer);
    void WithInitialTheme(string themeId, ThemeConfig? config = null);
    void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark);
    void WithApplicationId(string applicationId);
    void UseUserThemeDirectory();
    void UseUserThemeDirectory(string directory);
    void WithDefaultFontFamily(FontFamily fontFamily);
    void WithDefaultFontFamily(string fontFamily);
}
