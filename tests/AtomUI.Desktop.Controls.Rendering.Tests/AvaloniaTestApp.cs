using AtomUI.Localization;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.DesignLanguages.Shadcn;
using AtomUIGallery;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AtomUI.Desktop.Controls.Rendering.Tests.RenderingTestApp))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public sealed partial class RenderingTestApp : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<RenderingTestApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
        .UseAtomUI(builder =>
        {
            builder.UseDesktopControls();
            builder.UseDesktopExtras();
            builder.UseDesktopColorPicker();
            builder.UseDesktopDataGrid();
            builder.UseGalleryControls();
            builder.UseAntDesign(static ant => ant.WithDefaultTheme(AntDesignThemes.DaybreakBlue));
            builder.UsePrimeNG();
            builder.UseShadcn();
            builder.WithInitialDesignLanguage<AntDesignLanguage>();
            builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS]);
        });

    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }
}
