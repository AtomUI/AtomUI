using System.Diagnostics;
using System.Globalization;
using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Localization;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace AtomUI.StartupPerformance;

/// <summary>
/// 空窗口启动阶段树测量 harness。边界全部来自公共可观测事件（builder 回调、Opened、
/// 根哨兵首次 Render、Dispatcher idle），不依赖任何源码内探针。
/// 输出为机器可读 <c>PERF|key=value</c> 行（ms，invariant），供采样脚本聚合中位数。
/// </summary>
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        Timeline.ProcessEntry = Stopwatch.GetTimestamp();
        AppBuilder.Configure<PerfApplication>()
                  .UseAtomUIPlatformDetect()
                  .WithAtomUIDefaultOptions()
                  .StartWithClassicDesktopLifetime(args);
        Timeline.Report();
        return 0;
    }
}

internal static class Timeline
{
    private static readonly double TickToMs = 1000.0 / Stopwatch.Frequency;

    internal static long ProcessEntry;
    internal static long InitializeEntered;
    internal static long ConfigureStart;
    internal static long ConfigureEnd;
    internal static double ConfigureUseLanguagesMs;
    internal static double ConfigureUseAlibabaSansFontMs;
    internal static double ConfigureUseDesktopControlsMs;
    internal static long UseAtomUIStart;
    internal static long UseAtomUIEnd;
    internal static long FrameworkCompleted;
    internal static long WindowConstructed;
    internal static long WindowOpened;
    internal static long FirstRender;
    internal static long DispatcherIdle;

    internal static double Mark(long timestamp)
    {
        return (timestamp - ProcessEntry) * TickToMs;
    }

    internal static double Duration(long start, long end)
    {
        return (end - start) * TickToMs;
    }

    internal static double Since(long startTimestamp)
    {
        return (Stopwatch.GetTimestamp() - startTimestamp) * TickToMs;
    }

    internal static void Report()
    {
        var configureTotal = Duration(ConfigureStart, ConfigureEnd);
        var useAtomUITotal = Duration(UseAtomUIStart, UseAtomUIEnd);
        WriteLine("MainToInitialize", Mark(InitializeEntered));
        WriteLine("Configure.UseLanguages", ConfigureUseLanguagesMs);
        WriteLine("Configure.UseAlibabaSansFont", ConfigureUseAlibabaSansFontMs);
        WriteLine("Configure.UseDesktopControls", ConfigureUseDesktopControlsMs);
        WriteLine("Configure.Total", configureTotal);
        WriteLine("UseAtomUI.SyncTotal", useAtomUITotal);
        WriteLine("UseAtomUI.PostConfigure", useAtomUITotal - configureTotal);
        WriteLine("Mark.FrameworkCompleted", Mark(FrameworkCompleted));
        WriteLine("Mark.WindowConstructed", Mark(WindowConstructed));
        WriteLine("Mark.WindowOpened", Mark(WindowOpened));
        WriteLine("Mark.FirstRender", Mark(FirstRender));
        WriteLine("Mark.DispatcherIdle", Mark(DispatcherIdle));
        WriteLine("TotalToFirstRender", Mark(FirstRender));
    }

    private static void WriteLine(string key, double valueMs)
    {
        Console.Out.WriteLine(
            string.Create(CultureInfo.InvariantCulture, $"PERF|{key}={valueMs:F3}"));
    }
}

internal sealed class PerfApplication : Application
{
    public override void Initialize()
    {
        Timeline.InitializeEntered = Stopwatch.GetTimestamp();
        Timeline.UseAtomUIStart = Stopwatch.GetTimestamp();
        this.UseAtomUI(builder =>
        {
            Timeline.ConfigureStart = Stopwatch.GetTimestamp();
            builder.WithApplicationId("AtomUI.StartupPerformance");
            builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);

            var segmentStart = Stopwatch.GetTimestamp();
            builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS]);
            Timeline.ConfigureUseLanguagesMs = Timeline.Since(segmentStart);

            segmentStart = Stopwatch.GetTimestamp();
            builder.UseAlibabaSansFont();
            Timeline.ConfigureUseAlibabaSansFontMs = Timeline.Since(segmentStart);

            segmentStart = Stopwatch.GetTimestamp();
            builder.UseDesktopControls();
            Timeline.ConfigureUseDesktopControlsMs = Timeline.Since(segmentStart);

            Timeline.ConfigureEnd = Stopwatch.GetTimestamp();
        });
        Timeline.UseAtomUIEnd = Stopwatch.GetTimestamp();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Timeline.FrameworkCompleted = Stopwatch.GetTimestamp();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new Avalonia.Controls.Window
            {
                Width = 800,
                Height = 600,
                Content = new RenderSentinel()
            };
            window.Opened += (_, _) =>
            {
                Timeline.WindowOpened = Stopwatch.GetTimestamp();
            };
            desktop.MainWindow = window;
        }
        Timeline.WindowConstructed = Stopwatch.GetTimestamp();
        base.OnFrameworkInitializationCompleted();
    }
}

internal sealed class RenderSentinel : Control
{
    private bool _reported;

    public override void Render(Avalonia.Media.DrawingContext context)
    {
        base.Render(context);
        if (_reported)
        {
            return;
        }
        _reported = true;
        Timeline.FirstRender = Stopwatch.GetTimestamp();
        Dispatcher.UIThread.Post(() =>
        {
            Timeline.DispatcherIdle = Stopwatch.GetTimestamp();
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }, DispatcherPriority.ApplicationIdle);
    }
}
