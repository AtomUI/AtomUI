using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.Shadcn;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AtomUI.DesignLanguages.Shadcn.Tests.TestAppBuilder))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AtomUI.DesignLanguages.Shadcn.Tests;

/// <summary>
/// A headless application with the desktop controls and shadcn as the only design language.
/// </summary>
public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<TestApplication>()
                         .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                         .UseAtomUI(static builder => builder.UseDesktopControls()
                                                             .UseDesktopColorPicker()
                                                             .UseDesktopDataGrid()
                                                             .UseShadcn(static language => language.AddDesktopColorPicker()
                                                                                                   .AddDesktopDataGrid()));
    }
}

internal sealed class TestApplication : Application
{
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }
}
