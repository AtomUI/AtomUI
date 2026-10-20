using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace AtomUI.StartupPerformance;

/// <summary>按需独立进程验收：配置间接创建 AvaloniaObject 时，启动顺序与稳态定时器必须正常。</summary>
internal static class StartupConfigurationAcceptance
{
    private static bool s_platformReady;
    private static int s_configureCount;
    private static bool s_timerPassed;

    internal static int Run()
    {
        try
        {
            Require(Dispatcher.FromThread(Thread.CurrentThread) is null,
                "This acceptance must run in a fresh process before dispatcher creation.");
            var builder = AppBuilder.Configure(() =>
                                    {
                                        Require(s_configureCount == 1,
                                            "Configuration must run once before Application construction.");
                                        return new StartupConfigurationAcceptanceApplication();
                                    })
                                    .UseAtomUIPlatformDetect()
                                    .WithAtomUIDefaultOptions()
                                    .AfterPlatformServicesSetup(_ => s_platformReady = true)
                                    .UseAtomUI(Configure);
            Require(s_configureCount == 0 && Dispatcher.FromThread(Thread.CurrentThread) is null,
                "UseAtomUI must only register configuration and must not create a Dispatcher.");
            builder.StartWithClassicDesktopLifetime([], ShutdownMode.OnExplicitShutdown);
            StartupTimeline.Report();
            Require(s_timerPassed, "Steady-state 16 ms DispatcherTimer sampling failed; inspect Timer.* metrics.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"FAIL|{exception.Message}");
            return 1;
        }
    }

    private static void Configure(IAtomUIBuilder builder)
    {
        Require(s_platformReady, "Application configuration ran before platform services were ready.");
        Dispatcher.UIThread.VerifyAccess();
        s_configureCount++;
        builder.WithApplicationId("AtomUI.StartupConfigurationAcceptance");
        ConfigureModule();
    }

    private static void ConfigureModule()
    {
        // Reproduce an ordinary module reading metadata from an AvaloniaObject-derived repository.
        GC.KeepAlive(new CategoryRepository().Categories);
    }

    internal static void VerifyInitialized(Application application)
    {
        Require(s_configureCount == 1, "Application configuration ran more than once.");
        Require(application.GetStartup().State == StartupState.Committed,
            "AtomUI must commit before Application.Initialize.");
    }

    internal static void SampleTimers(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        var dispatcher = Dispatcher.UIThread;
        var clock = Stopwatch.StartNew();
        var samples = new List<double>();
        var previous = 0d;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += (_, _) =>
        {
            var now = clock.Elapsed.TotalMilliseconds;
            if (previous > 0)
            {
                samples.Add(now - previous);
            }
            previous = now;
        };
        // Start measuring only when the native main loop is running, not during startup commit.
        dispatcher.Post(timer.Start, DispatcherPriority.Send);
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            dispatcher.Post(() =>
            {
                timer.Stop();
                samples.Sort();
                var median = samples.Count == 0 ? 0 : samples[samples.Count / 2];
                StartupTimeline.Set("Timer.Samples", samples.Count);
                StartupTimeline.Set("Timer.MedianMs", median);
                StartupTimeline.Set("Timer.MaxMs", samples.Count == 0 ? 0 : samples[^1]);
                s_timerPassed = samples.Count >= 20 && median is > 0 and <= 64;
                lifetime.Shutdown();
            }, DispatcherPriority.Send);
        });
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class CategoryRepository : AvaloniaObject
    {
        internal IReadOnlyList<string> Categories { get; } = ["Av"];
    }
}

internal sealed partial class StartupConfigurationAcceptanceApplication : Application
{
    public override void Initialize() => StartupConfigurationAcceptance.VerifyInitialized(this);

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
        StartupConfigurationAcceptance.SampleTimers((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!);
    }
}
