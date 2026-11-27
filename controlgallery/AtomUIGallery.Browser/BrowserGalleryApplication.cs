using AtomUI.DesignLanguages.AntDesign;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.DesignLanguages.Shadcn;
using System.Globalization;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Fonts.AlibabaPuHuiTi;
using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase;
using AtomUIGallery.Localization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;

namespace AtomUIGallery.Browser;

public partial class BrowserGalleryApplication : Application
{
    /// <summary>
    /// The host registers the Browser Gallery AtomUI configuration on the AppBuilder chain.
    /// </summary>
    public static void Configure(IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseLanguages(
            GalleryLanguageDefaults.Resolve(CultureInfo.CurrentUICulture),
            [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW, LanguageTags.PtBR]);
        builder.UseAlibabaSansFont();
        builder.UseAlibabaPuHuiTiFont();
        builder.WithDefaultFontFamily(FontFamily.Parse(
            $"fonts:AlibabaSans#Alibaba Sans, {AlibabaPuHuiTiFontConstants.FontFamily}, $Default"));
        builder.UseDesktopControls();
        builder.UseDesktopExtras();
        builder.UseDesktopColorPicker();
        builder.UseDesktopDataGrid();
        builder.UseGalleryControls();
        builder.UseAntDesign(static ant => ant.WithDefaultTheme(AntDesignThemes.DaybreakBlue));
        builder.UsePrimeNG();
        builder.UseShadcn();
        builder.WithInitialDesignLanguage<AntDesignLanguage>();
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
