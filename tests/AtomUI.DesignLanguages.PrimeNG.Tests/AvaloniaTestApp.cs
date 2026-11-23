using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.PrimeNG;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AtomUI.DesignLanguages.PrimeNG.Tests.TestAppBuilder))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AtomUI.DesignLanguages.PrimeNG.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions())
        .UseAtomUI(static builder =>
        {
            builder.UseDesktopControls();
            builder.UseDesktopColorPicker();
            builder.UseDesktopDataGrid();
            builder.UsePrimeNG(static language => language.AddDesktopColorPicker().AddDesktopDataGrid());
            builder.WithInitialTheme(ThemeSelection.Of<PrimeNGLanguage>());
        });
}

internal sealed class TestApplication : Application
{
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }
}
