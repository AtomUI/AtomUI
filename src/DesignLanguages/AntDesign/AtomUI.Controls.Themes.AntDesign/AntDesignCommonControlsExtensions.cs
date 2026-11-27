using AtomUI.Generated.AtomUIControlsThemesAntDesign;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.AntDesign;

/// <summary>
/// Provides the entry point that adds the Ant Design values of the shared controls (<c>AtomUI.Controls</c>) to
/// an Ant Design language builder.
/// </summary>
public static class AntDesignCommonControlsExtensions
{
    /// <summary>
    /// Registers the Ant Design values of the shared controls (<c>AtomUI.Controls</c>). The idiom value packages call
    /// it (<c>AddDesktopControls</c>); the shared controls are enabled by the idiom's control package.
    /// </summary>
    public static DesignLanguageBuilder<AntDesignLanguage> AddCommonControls(this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
