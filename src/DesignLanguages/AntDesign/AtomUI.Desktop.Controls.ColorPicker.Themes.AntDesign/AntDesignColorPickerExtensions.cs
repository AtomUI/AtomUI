using AtomUI.Generated.AtomUIDesktopControlsColorPickerThemesAntDesign;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// Provides the entry point that adds the Ant Design values of the desktop color picker to an Ant Design
/// language builder.
/// </summary>
public static class AntDesignColorPickerExtensions
{
    /// <summary>
    /// Adds the Ant Design values of the desktop color picker (AtomUI.Desktop.Controls.ColorPicker) and of the desktop
    /// controls it builds on. The control package itself is enabled by the application
    /// (<c>UseDesktopColorPicker()</c>).
    /// </summary>
    public static DesignLanguageBuilder<AntDesignLanguage> AddDesktopColorPicker(this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
