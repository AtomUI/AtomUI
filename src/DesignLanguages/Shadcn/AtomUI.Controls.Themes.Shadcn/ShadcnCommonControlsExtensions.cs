using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.Shadcn;

public static class ShadcnCommonControlsExtensions
{
    /// <summary>
    /// Registers the shadcn values of the shared controls (<c>AtomUI.Controls</c>). The idiom value packages call
    /// it (<c>AddDesktopControls</c>); the shared controls are enabled by the idiom's control package.
    /// </summary>
    public static DesignLanguageBuilder<ShadcnLanguage> AddCommonControls(this DesignLanguageBuilder<ShadcnLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        Generated.AtomUIControlsThemesShadcn.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
