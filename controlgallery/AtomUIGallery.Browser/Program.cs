using System.Runtime.Versioning;
using AtomUI;
using Avalonia;
using Avalonia.Browser;
using ReactiveUI.Avalonia;

[assembly: SupportedOSPlatform("browser")]

namespace AtomUIGallery.Browser;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        await BuildAvaloniaApp()
            .LogToTrace()
            .StartBrowserAppAsync("out");
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<BrowserGalleryApplication>()
                                .UseReactiveUI(build =>
                                    build.ConfigureViewLocator(locator => AtomUIGalleryModule.RegisterViews(locator)))
                                .UseAtomUI(BrowserGalleryApplication.Configure);
#if DEBUG
        builder = builder.WithDeveloperTools();
#endif
        return builder;
    }
}
