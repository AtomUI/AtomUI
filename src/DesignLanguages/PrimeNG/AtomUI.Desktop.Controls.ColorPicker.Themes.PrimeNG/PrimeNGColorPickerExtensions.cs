using AtomUI.Generated.AtomUIDesktopControlsColorPickerThemesPrimeNG;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Provides the extension method that adds the PrimeNG values of the desktop color picker to a PrimeNG language
/// builder.
/// </summary>
public static class PrimeNGColorPickerExtensions
{
    /// <summary>
    /// Adds the PrimeNG values of the desktop color picker (AtomUI.Desktop.Controls.ColorPicker) and of the desktop
    /// controls it builds on. The control package itself is enabled by the application
    /// (<c>UseDesktopColorPicker()</c>).
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddDesktopColorPicker(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
