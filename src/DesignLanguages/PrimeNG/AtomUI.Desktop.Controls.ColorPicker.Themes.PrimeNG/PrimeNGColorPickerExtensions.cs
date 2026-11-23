using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

public static class PrimeNGColorPickerExtensions
{
    /// <summary>
    /// Adds the Ant Design values of the desktop colour picker (AtomUI.Desktop.Controls.ColorPicker) and of the desktop
    /// controls it builds on. The control package itself is enabled by the application (<c>UseDesktopColorPicker()</c>).
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddDesktopColorPicker(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        Generated.AtomUIDesktopControlsColorPickerThemesPrimeNG.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
