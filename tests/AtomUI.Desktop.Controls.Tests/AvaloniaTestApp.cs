using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using AtomUI.Controls;
using AtomUI.Localization;

[assembly: AvaloniaTestApplication(typeof(AtomUI.Desktop.Controls.Tests.TestAppBuilder))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AtomUI.Desktop.Controls.Tests;

internal static class AvaloniaTestApp
{
    private static int _initialized;

    public static void EnsureInitialized()
    {
        if (Application.Current is not null)
        {
            Volatile.Write(ref _initialized, 1);
            return;
        }

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
                         // Keep Skia redraw damage separate: merging text updates into one clip
                         // can change antialiased pixels of otherwise unchanged progress paths.
                         .With(new global::Avalonia.Rendering.Composition.CompositionOptions
                         {
                             UseRegionDirtyRectClipping = true
                         })
                         .UseSkia()
                         .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                         .UseAtomUI(builder => builder.UseDesktopControls()
                                                      .UseImageLoading(options =>
                                                      {
                                                          options.MaxConcurrentDownloads = 1;
                                                          options.MaxConcurrentLocalReads = 1;
                                                          options.MaxConcurrentDecodes = 1;
                                                      })
                                                      .UseDesktopColorPicker()
                                                      .UseDesktopExtras()
                                                      .UseLanguages(
                                                          LanguageTags.EnUS,
                                                          [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW]));
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
