using AtomUI.Theme;
using AtomUI.Theme.Language;
using AtomUIGallery.Controls;
using AtomUIGallery.ShowCases.ShowCaseControls;

namespace AtomUIGallery;

public static class ThemeManagerBuilderExtensions
{
    public static IThemeManagerBuilder UseGalleryControls(this IThemeManagerBuilder themeManagerBuilder)
    {
        themeManagerBuilder.AddControlThemesProvider(new GalleryControlThemesProvider());
        themeManagerBuilder.AddControlThemesProvider(new ShowCaseControlsThemesProvider());
        var languageProviders = LanguageProviderPool.GetLanguageProviders();
        foreach (var languageProvider in languageProviders)
        {
            themeManagerBuilder.AddLanguageProviders(languageProvider);
        }
        return themeManagerBuilder;
    }
}