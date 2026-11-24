using AtomUI.Generated.AtomUIDesktopControlsThemesAntDesign;
using AtomUI.Controls.Themes.AntDesign;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

public static class AntDesignDesktopControlsExtensions
{
    /// <summary>
    /// Adds the Ant Design values of the desktop controls and of the shared controls they build on. The control package
    /// itself is enabled by the application (<c>UseDesktopControls()</c>).
    /// </summary>
    public static DesignLanguageBuilder<AntDesignLanguage> AddDesktopControls(this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddCommonControls();
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
