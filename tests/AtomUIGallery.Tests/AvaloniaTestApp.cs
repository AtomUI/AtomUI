using Avalonia;
using Avalonia.Headless;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase;
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
                             builder.UseGalleryBase(global::AtomUIGallery.AtomUIGalleryModule.Configure);
                             builder.UseGalleryControls();
                             builder.UseLanguages(
                                 LanguageTags.EnUS,
                                 [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW, LanguageTags.PtBR]);
                         });
    }
}

// 本工程引用生成器：应用类声明为 partial，由生成器补上 RegisterServices 挂接。
internal sealed partial class TestApplication : Application;
