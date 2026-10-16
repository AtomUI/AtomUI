using AtomUI;
using Avalonia;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.Desktop;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp(args)
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LogException(ex);
            throw;
        }
    }

    private static void LogException(Exception ex)
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        var logDirectory = Path.Combine(homeDirectory, Path.Combine("AtomUIGallery", "AppCrashLogs"));
        Directory.CreateDirectory(logDirectory);

        var logFileName = $"CrashLog_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        var logFilePath = Path.Combine(logDirectory, logFileName);

        File.WriteAllText(logFilePath,
            $"CrashTime: {DateTime.Now}\r\n" +
            $"Exception Type: {ex.GetType().Name}\r\n" +
            $"Exception Message: {ex.Message}\r\n" +
            $"Stack Info: \r\n{ex.StackTrace}");
    }

    public static AppBuilder BuildAvaloniaApp(string[]? args = null)
    {
        var windowingPlatform = ResolveWindowingPlatform(args);
        var builder = AppBuilder.Configure<GalleryApplication>()
                                .UseReactiveUI(build =>
                                    build.ConfigureViewLocator(locator => AtomUIGalleryModule.RegisterViews(locator)))
                                .UseAtomUIPlatformDetect(windowingPlatform)
                                .WithAtomUIDefaultOptions()
                                .UseAtomUI(GalleryApplication.Configure);

#if DEBUG
        builder = builder.WithDeveloperTools();
#endif
        return builder.LogToTrace();
    }

    private static AtomUIWindowingPlatform ResolveWindowingPlatform(string[]? args)
    {
        if (args is null)
        {
            return AtomUIWindowingPlatform.Auto;
        }

        const string optionName = "--windowing-platform";
        for (var i = 0; i < args.Length; i++)
        {
            string? value = null;
            if (args[i].StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                value = args[i][(optionName.Length + 1)..];
            }
            else if (string.Equals(args[i], optionName, StringComparison.OrdinalIgnoreCase))
            {
                if (++i >= args.Length)
                {
                    throw new ArgumentException($"{optionName} requires wayland, x11, or auto.", nameof(args));
                }

                value = args[i];
            }

            if (value is not null)
            {
                return value.Trim().ToLowerInvariant() switch
                {
                    "auto" => AtomUIWindowingPlatform.Auto,
                    "wayland" => AtomUIWindowingPlatform.Wayland,
                    "x11" => AtomUIWindowingPlatform.X11,
                    _ => throw new ArgumentException(
                        $"{optionName} must be wayland, x11, or auto, but was '{value}'.",
                        nameof(args))
                };
            }
        }

        return AtomUIWindowingPlatform.Auto;
    }
}
