using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

public static class AntDesignDataGridExtensions
{
    /// <summary>
    /// Adds the Ant Design values of the desktop data grid (AtomUI.Desktop.Controls.DataGrid) and of the desktop controls
    /// it builds on. The control package itself is enabled by the application (<c>UseDesktopDataGrid()</c>).
    /// </summary>
    public static DesignLanguageBuilder<AntDesignLanguage> AddDesktopDataGrid(this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.AddDesktopControls();
        Generated.AtomUIDesktopControlsDataGridThemesAntDesign.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
