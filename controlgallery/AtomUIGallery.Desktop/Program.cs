using Avalonia;
using Avalonia.Media;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.Desktop;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .With(new FontManagerOptions
                {
                    FontFallbacks = [new FontFallback
                    {
                        FontFamily = new FontFamily("Microsoft YaHei")
                    }]
                })
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

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<GalleryApplication>()
                         .UseReactiveUI()
                         .UsePlatformDetect()
                         .With(new Win32PlatformOptions())
                         .LogToTrace();
        
    }
}