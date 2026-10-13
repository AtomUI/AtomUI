using System.Globalization;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase;
using AtomUIGallery.Localization;
using Avalonia.Controls.ApplicationLifetimes;

namespace AtomUIGallery.Desktop;

public partial class GalleryApplication : BaseGalleryApplication
{
    public GalleryApplication()
    {
        Name = "AtomUI Desktop Gallery";
    }

    public override void Initialize()
    {
        base.Initialize();
        this.UseAtomUI(builder =>
        {
            builder.WithApplicationId("AtomUIGallery");
            builder.UseUserThemeDirectory();
            builder.UseLanguages(
                GalleryLanguageDefaults.Resolve(CultureInfo.CurrentUICulture),
                [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW, LanguageTags.PtBR]);
            builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
            builder.UseAlibabaSansFont();
            // Stage 1：Desktop 包按 AtomUICriticalControls 关键集分区注册（chrome 闭包先行，
            // 余量 fragments 由 Stage 2 注册窗口收集——生成器分区发射，见 Desktop.Controls.csproj）。
            builder.UseDesktopControls();
            // Stage 1：GalleryBase 含 GalleryWindowTitleBar（窗口 chrome），必须先行。
            builder.UseGalleryBase(AtomUIGalleryModule.Configure);
            // Stage 2（分级启动参照模式）：非 chrome 包与 Gallery 语言模块后置到主窗体出现后。
            builder.Deferred(() =>
            {
                builder.UseDesktopExtras();
                builder.UseDesktopColorPicker();
                builder.UseDesktopDataGrid();
                builder.UseGalleryControls();
            });
        });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = CreateWorkspaceWindow();
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
