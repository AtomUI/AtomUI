using AtomUI.Generated.AtomUIDesktopControlsDataGridThemesShadcn;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// Provides <see cref="AddDesktopDataGrid" />, the entry point that adds the shadcn values of the desktop
/// data grid.
/// </summary>
public static class ShadcnDataGridExtensions
{
    /// <summary>
    /// Adds the shadcn values of the desktop data grid (AtomUI.Desktop.Controls.DataGrid) and of the desktop controls
    /// it builds on. The control package itself is enabled by the application (<c>UseDesktopDataGrid()</c>).
    /// </summary>
    public static DesignLanguageBuilder<ShadcnLanguage> AddDesktopDataGrid(this DesignLanguageBuilder<ShadcnLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
