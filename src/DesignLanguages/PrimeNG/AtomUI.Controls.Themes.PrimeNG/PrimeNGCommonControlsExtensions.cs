using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.PrimeNG;

public static class PrimeNGCommonControlsExtensions
{
    /// <summary>
    /// Registers the Ant Design values of the shared controls (<c>AtomUI.Controls</c>). The idiom value packages call
    /// it (<c>AddDesktopControls</c>); the shared controls are enabled by the idiom's control package.
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddCommonControls(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        Generated.AtomUIControlsThemesPrimeNG.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
