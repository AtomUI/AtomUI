using System.Diagnostics.Tracing;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase;
using AtomUIGallery;
using Avalonia;
using Avalonia.Headless;
using ReactiveUI.Avalonia;

namespace AtomUI.ThemeAssetsValidation;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        try
        {
            using var listener = new MaterializationListener();
            AppBuilder.Configure<ValidationApplication>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                .UseReactiveUI(build => build.ConfigureViewLocator(AtomUIGalleryModule.RegisterViews))
                .UseAtomUI(static builder =>
                {
                    builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS]);
                    builder.UseAlibabaSansFont();
                    builder.UseDesktopControls();
                    builder.UseDesktopDataGrid();
                    builder.UseDesktopColorPicker();
                    builder.UseDesktopExtras();
                    builder.UseGalleryBase(AtomUIGalleryModule.Configure);
                    builder.UseGalleryControls();
                    builder.UseSplash(SplashMode.Off);
                })
                .SetupWithoutStarting();

            var application = Application.Current ?? throw new InvalidOperationException("Avalonia application was not created.");
            var count = 0;
            using (var scope = ApplicationScopeRegistry.Get(application) ?? throw new InvalidOperationException("AtomUI did not commit."))
            {
                count = scope.ThemeManager.MaterializeThemeAssets();
                if (count == 0)
                {
                    throw new InvalidOperationException("No real theme assets were selected for validation.");
                }
            }
            foreach (var pair in listener.AssetCounts.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                Console.WriteLine($"[PACKAGE] {pair.Key}: {pair.Value} theme assets");
            }
            Console.WriteLine($"[SUMMARY] Materialized {count} real theme assets and released the application scope.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("[FAIL] Theme asset materialization: " + error);
            return 1;
        }
    }

    private sealed class MaterializationListener : EventListener
    {
        internal Dictionary<string, int> AssetCounts { get; } = new(StringComparer.Ordinal);

        protected override void OnEventSourceCreated(EventSource source)
        {
            if (source.Name == "AtomUI-Startup")
            {
                EnableEvents(source, EventLevel.Verbose);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs args)
        {
            if (args.EventName == "ThemeAssetMaterialized" && args.Payload?[0] is string packageId)
            {
                lock (AssetCounts)
                {
                    AssetCounts.TryGetValue(packageId, out var count);
                    AssetCounts[packageId] = count + 1;
                }
            }
        }
    }
}

public sealed partial class ValidationApplication : Application
{
    public ValidationApplication()
    {
        Name = "AtomUI theme asset validation";
    }
}
