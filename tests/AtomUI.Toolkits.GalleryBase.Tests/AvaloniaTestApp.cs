using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using ReactiveUI.Avalonia;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(AtomUI.Toolkits.GalleryBase.Tests.TestAppBuilder))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AtomUI.Toolkits.GalleryBase.Tests;

internal static class AvaloniaTestApp
{
    private static int s_initialized;

    public static void EnsureInitialized()
    {
        if (Application.Current is not null)
        {
            return;
        }

        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        TestAppBuilder.BuildAvaloniaApp().SetupWithoutStarting();
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<TestApplication>()
                         .UseReactiveUI(_ => { })
                         .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                         .UseAtomUI(builder =>
                         {
                             builder.UseDesktopControls();
                             builder.UseGalleryBase();
                             builder.UseLanguages(
                                 LanguageTags.EnUS,
                                 [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW]);
                         });
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
