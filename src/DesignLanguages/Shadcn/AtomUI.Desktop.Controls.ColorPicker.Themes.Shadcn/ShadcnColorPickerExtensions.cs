using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

public static class ShadcnColorPickerExtensions
{
    /// <summary>
    /// Adds the shadcn values of the desktop colour picker (AtomUI.Desktop.Controls.ColorPicker) and of the desktop
    /// controls it builds on. The control package itself is enabled by the application (<c>UseDesktopColorPicker()</c>).
    /// </summary>
    public static DesignLanguageBuilder<ShadcnLanguage> AddDesktopColorPicker(this DesignLanguageBuilder<ShadcnLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        Generated.AtomUIDesktopControlsColorPickerThemesShadcn.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
