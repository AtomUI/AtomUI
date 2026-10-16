using System.Globalization;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Fonts.AlibabaPuHuiTi;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase;
using AtomUIGallery.Localization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;

namespace AtomUIGallery.Browser;

public partial class BrowserGalleryApplication : Application
{
    /// <summary>宿主在 AppBuilder 链上登记 Browser Gallery 的 AtomUI 配置。</summary>
    public static void Configure(IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseLanguages(
            GalleryLanguageDefaults.Resolve(CultureInfo.CurrentUICulture),
            [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW, LanguageTags.PtBR]);
        builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
        builder.UseAlibabaSansFont();
        builder.UseAlibabaPuHuiTiFont();
        builder.WithDefaultFontFamily(FontFamily.Parse(
            $"fonts:AlibabaSans#Alibaba Sans, {AlibabaPuHuiTiFontConstants.FontFamily}, $Default"));
        builder.UseDesktopControls();
        builder.UseDesktopExtras();
        builder.UseDesktopColorPicker();
        builder.UseDesktopDataGrid();
        builder.UseGalleryBase(AtomUIGalleryModule.Configure);
        builder.UseGalleryControls();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            singleViewLifetime.MainView = new BrowserGalleryView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
