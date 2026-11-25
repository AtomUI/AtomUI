using AtomUI.DesignLanguages.AntDesign;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.DesignLanguages.Shadcn;
using System.Globalization;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUIGallery.Localization;
using AtomUIGallery.Workspace.Views;
using Window = Avalonia.Controls.Window;

namespace AtomUIGallery.Desktop;

public partial class GalleryApplication : BaseGalleryApplication
{
    public GalleryApplication()
    {
        Name = "AtomUI Desktop Gallery";
    }

    /// <summary>宿主在 AppBuilder 链上登记 Gallery 的 AtomUI 配置。</summary>
    public static void Configure(IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithApplicationId("AtomUIGallery");
        builder.UseUserThemeDirectory();
        builder.UseLanguages(
            GalleryLanguageDefaults.Resolve(CultureInfo.CurrentUICulture),
            [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW, LanguageTags.PtBR]);
        builder.UseAlibabaSansFont();
        builder.UseDesktopControls();
        builder.UseDesktopExtras();
        builder.UseDesktopColorPicker();
        builder.UseDesktopDataGrid();
        builder.UseGalleryControls();
        builder.UseAntDesign(static ant => ant.WithDefaultTheme(AntDesignThemes.DaybreakBlue));
        builder.UsePrimeNG();
        builder.UseShadcn();
        builder.WithInitialDesignLanguage<AntDesignLanguage>();
        // Auto：动态代码编译（JIT/ReadyToRun）时显示，NativeAOT 下不显示。图片在显示 Splash 时才加载。
        builder.UseSplash(SplashMode.Auto, splash => splash
            .WithLogo(new Uri("avares://AtomUIGallery.Desktop/Assets/Images/AppLogo.iconset/icon_128x128@2x.png"))
            .WithTitle("AtomUI Gallery"));
        builder.UseMainWindow(static _ => Task.FromResult<Window>(new WorkspaceWindow()));
    }
}
