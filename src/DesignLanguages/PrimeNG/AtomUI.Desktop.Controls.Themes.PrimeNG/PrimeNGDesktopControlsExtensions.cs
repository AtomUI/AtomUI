using AtomUI.Generated.AtomUIDesktopControlsThemesPrimeNG;
using AtomUI.Controls.Themes.PrimeNG;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Provides the extension method that adds the PrimeNG values of the desktop controls to a PrimeNG language builder.
/// </summary>
public static class PrimeNGDesktopControlsExtensions
{
    /// <summary>
    /// Adds the PrimeNG values of the desktop controls and of the shared controls they build on. The control package
    /// itself is enabled by the application (<c>UseDesktopControls()</c>).
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddDesktopControls(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddCommonControls();
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
