using AtomUI.Desktop.Controls;
using AtomUI.Themes.PrimeNG;
using AtomUI.Themes.Shadcn;
using AtomUI.Themes.Bootstrap;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AtomUI.Themes.Tests.TestAppBuilder))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AtomUI.Themes.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions())
        .UseAtomUI(builder =>
        {
            builder.UseDesktopControls();
            builder.Theme.UsePrimeNGThemes().UseShadcnThemes().UseBootstrapThemes();
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
