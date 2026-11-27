using AtomUI.Generated.AtomUIDesktopControlsDataGridThemesPrimeNG;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Provides the extension method that adds the PrimeNG values of the desktop data grid to a PrimeNG language
/// builder.
/// </summary>
public static class PrimeNGDataGridExtensions
{
    /// <summary>
    /// Adds the PrimeNG values of the desktop data grid (AtomUI.Desktop.Controls.DataGrid) and of the desktop controls
    /// it builds on. The control package itself is enabled by the application (<c>UseDesktopDataGrid()</c>).
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddDesktopDataGrid(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
