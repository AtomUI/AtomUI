using AtomUI.Generated.AtomUIControlsThemesPrimeNG;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.PrimeNG;

/// <summary>
/// Provides the extension method that adds the PrimeNG values of the shared controls to a PrimeNG language builder.
/// </summary>
public static class PrimeNGCommonControlsExtensions
{
    /// <summary>
    /// Registers the PrimeNG values of the shared controls (<c>AtomUI.Controls</c>). The idiom value packages call
    /// it (<c>AddDesktopControls</c>); the shared controls are enabled by the idiom's control package.
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddCommonControls(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
