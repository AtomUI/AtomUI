using AtomUI.Generated.AtomUIGalleryTests;
using Avalonia;
using Avalonia.Headless;
using AtomUI.Desktop.Controls;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Localization;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(AtomUIGallery.Tests.TestAppBuilder))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AtomUIGallery.Tests;

internal static class AvaloniaTestApp
{
    private static int _initialized;

    public static void EnsureInitialized()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
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
                         .UseHeadless(new AvaloniaHeadlessPlatformOptions())
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
                             builder.UseLanguages(
                                 LanguageTags.EnUS,
                                 [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW, LanguageTags.PtBR]);
                         });
    }
}

// This project references the generator: the app class is declared partial and the generator adds the
// RegisterServices hook.
internal sealed partial class TestApplication : Application;
