using System.Runtime;
using AtomUI.Desktop.Controls;
using Avalonia;

namespace AtomUI.StartupPerformance;

/// <summary>
/// 启动测量 harness。场景：plain（不初始化 AtomUI）、atom-plain（AtomUI 初始化＋Avalonia Window）、
/// atom-window（AtomUI 初始化＋AtomUI Window）、atom-splash（atom-window＋启动 Splash）。各场景使用相同的平台选项，
/// 输出 PERF|key=value 供 run_samples.py 聚合；AtomUI 启动分段由进程内的 AtomUI-Startup 事件监听器写入。
/// </summary>
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        StartupTimeline.MarkMain();
        using var listener = new StartupEventListener();
        if (!HarnessOptions.TryParse(args, out var options, out var error))
        {
            Console.Error.WriteLine(error);
            Console.Error.WriteLine("usage: AtomUI.StartupPerformance [--scenario plain|atom-plain|atom-window|atom-splash] [--multicorejit <dir>] [--platform-defaults-off]");
            return 2;
        }
        HarnessOptions.Current = options;
        if (options.MultiCoreJitDirectory is { } profileDirectory)
        {
            Directory.CreateDirectory(profileDirectory);
            ProfileOptimization.SetProfileRoot(profileDirectory);
            ProfileOptimization.StartProfile("atomui-startup.jitprofile");
            StartupTimeline.Flag("MultiCoreJit");
        }
        var builder = options.Scenario == HarnessScenario.Plain
            ? AppBuilder.Configure<PlainHarnessApplication>()
            : AppBuilder.Configure<AtomUIHarnessApplication>();
        builder = builder.UseAtomUIPlatformDetect();
        if (!options.PlatformDefaultsOff)
        {
            builder = builder.WithAtomUIDefaultOptions();
        }
        if (options.Scenario != HarnessScenario.Plain)
        {
            builder = builder.UseAtomUI(AtomUIHarnessApplication.Configure);
        }
        // Capturing the dispatcher before platform setup gives its timers a different
        // clock origin from the native dispatcher and changes the behavior being measured.
        builder = builder.AfterPlatformServicesSetup(static _ => UiGapTracker.Start());
        builder.StartWithClassicDesktopLifetime(args);
        StartupTimeline.Report();
        return 0;
    }
}
