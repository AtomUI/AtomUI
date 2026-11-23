using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

public static class PrimeNGDataGridExtensions
{
    /// <summary>
    /// Adds the Ant Design values of the desktop data grid (AtomUI.Desktop.Controls.DataGrid) and of the desktop controls
    /// it builds on. The control package itself is enabled by the application (<c>UseDesktopDataGrid()</c>).
    /// </summary>
    public static DesignLanguageBuilder<PrimeNGLanguage> AddDesktopDataGrid(this DesignLanguageBuilder<PrimeNGLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        Generated.AtomUIDesktopControlsDataGridThemesPrimeNG.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
