using AtomUI.Generated.AtomUIDesktopControlsThemesShadcn;
using AtomUI.Controls.Themes.Shadcn;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// Provides <see cref="AddDesktopControls" />, the entry point that adds the shadcn values of the desktop controls.
/// </summary>
public static class ShadcnDesktopControlsExtensions
{
    /// <summary>
    /// Adds the shadcn values of the desktop controls and of the shared controls they build on. The control package
    /// itself is enabled by the application (<c>UseDesktopControls()</c>).
    /// </summary>
    public static DesignLanguageBuilder<ShadcnLanguage> AddDesktopControls(this DesignLanguageBuilder<ShadcnLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddCommonControls();
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        PackageMetadata.Definition.Ensure(language.Root);
        return language;
    }
}
