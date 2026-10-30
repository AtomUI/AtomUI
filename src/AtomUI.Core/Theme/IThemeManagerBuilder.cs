using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Definitions.Compiled;
using Avalonia.Media;

namespace AtomUI.Theme;

public interface IThemeManagerBuilder
{
    void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver);
    void AddCompiledThemePackage(CompiledThemePackage package);
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
