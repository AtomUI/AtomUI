using AtomUI.Controls.Themes.PrimeNG;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

public static class PrimeNGDesktopControlsExtensions
{
    /// <summary>
    /// Adds the Ant Design values of the desktop controls and of the shared controls they build on. The control package
    /// itself is enabled by the application (<c>UseDesktopControls()</c>).
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddDesktopControls(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddCommonControls();
        Generated.AtomUIDesktopControlsThemesPrimeNG.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
