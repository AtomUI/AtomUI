using Avalonia;
using Avalonia.Headless;
using AtomUI.Localization;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(AtomUI.Desktop.Controls.Tests.DataGrid.TestAppBuilder))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AtomUI.Desktop.Controls.Tests.DataGrid;

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
                             builder.UseDesktopDataGrid();
                             builder.UseLanguages(
                                 LanguageTags.EnUS,
                                 [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW]);
                         });
    }
}

// 本工程引用生成器：应用类声明为 partial，由生成器补上 RegisterServices 挂接。
internal sealed partial class TestApplication : Application;
