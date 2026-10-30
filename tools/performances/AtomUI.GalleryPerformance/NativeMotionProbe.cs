using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using AtomUI;
using AtomUIGallery;
using AtomUIGallery.Desktop;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI.Avalonia;

namespace AtomUI.GalleryPerformance;

internal static class NativeMotionProbe
{
    internal static int Run(string[] args)
    {
        NativeMotionOptions options;
        try
        {
            options = NativeMotionOptions.Parse(args);
        }
        catch (ArgumentException error)
        {
            Console.Error.WriteLine(error.Message);
            PrintUsage();
            return 2;
        }
        if (args.Contains("--help", StringComparer.Ordinal))
        {
            PrintUsage();
            return 0;
        }

        return AppBuilder.Configure<GalleryApplication>()
            .UseReactiveUI(build => build.ConfigureViewLocator(AtomUIGalleryModule.RegisterViews))
            .UseAtomUIPlatformDetect()
            .WithAtomUIDefaultOptions()
            .UseAtomUI(GalleryApplication.Configure)
            .AfterSetup(builder => Dispatcher.UIThread.Post(() => _ = RunAsync(options)))
            .StartWithClassicDesktopLifetime([], ShutdownMode.OnExplicitShutdown);
    }

    private static async Task RunAsync(NativeMotionOptions options)
    {
        var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        try
        {
            await WaitUntilAsync(() => lifetime.MainWindow is WorkspaceWindow { IsLoaded: true, IsVisible: true });
            var window = (WorkspaceWindow)lifetime.MainWindow!;
            window.Width = 1300;
            window.Height = 900;
            window.Activate();
            await Task.Delay(1500);
            var results = new List<NativeMotionSample>();
            var scenes = new List<object>();
            foreach (var name in options.Scenario == "all" ? NativeMotionOptions.Scenarios : [options.Scenario])
            {
                using var scene = await NativeMotionScenarios.CreateAsync(name, window);
                using var observation = new NativeMotionObservation(scene.Root, scene.Target, scene.PropertyName);
                Console.WriteLine($"NATIVE_MOTION_READY {name} target={scene.Target.GetType().FullName} root={scene.Root.GetType().FullName}");
                scenes.Add(new
                {
                    Scenario = name, Target = scene.Target.GetType().FullName, Root = scene.Root.GetType().FullName,
                    scene.PropertyName, scene.AnimationContract, Visuals = window.GetVisualDescendants().Count(),
                    Bounds = new { scene.Target.Bounds.X, scene.Target.Bounds.Y, scene.Target.Bounds.Width, scene.Target.Bounds.Height }
                });
                var sampleMs = Math.Max(options.SampleMilliseconds, scene.MinimumSampleMilliseconds);
                for (var iteration = -options.Warmup; iteration < options.Iterations; iteration++)
                {
                    foreach (var direction in scene.Directions)
                    {
                        var sample = observation.Begin(name, direction, iteration);
                        sample.AnimationContract = scene.AnimationContract;
                        scene.Change(direction);
                        scene.ObserveAfterChange?.Invoke(observation);
                        await Task.Delay(sampleMs);
                        await observation.EndAsync(sample);
                        scene.Validate(direction);
                        if (scene.RequiresMotion && !sample.HasIntermediateValues())
                        {
                            throw new InvalidOperationException($"{name}/{direction}: no observed intermediate animation values.");
                        }
                        if (name == "beam-offscreen" && sample.Values.Count > 1)
                        {
                            throw new InvalidOperationException("Offscreen Beam continued producing animation values.");
                        }
                        if (sample.MissedBatches != 0)
                        {
                            throw new InvalidOperationException($"{name}/{direction}: missed {sample.MissedBatches} compositor batches.");
                        }
                        sample.Final = scene.FinalValue;
                        sample.TargetType = observation.TargetType;
                        sample.SampleMilliseconds = sampleMs;
                        if (iteration >= 0)
                        {
                            results.Add(sample);
                        }
                    }
                }
                Console.WriteLine($"NATIVE_MOTION_SCENARIO {name} complete");
            }

            var report = new
            {
                Schema = 2,
                options.Label,
                options.Scenario,
                options.Warmup,
                options.Iterations,
                DeclaredRefreshRateHz = options.RefreshRate,
                Platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
                Executable = Environment.ProcessPath,
                Pid = Environment.ProcessId,
                Configuration = BuildConfiguration,
                GalleryAssembly = typeof(GalleryApplication).Assembly.Location,
                AvaloniaVersion = typeof(Application).Assembly.GetName().Version?.ToString(),
                ControlsAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(AtomUI.Desktop.Controls.NavMenu).Assembly.Location))),
                CoreAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(AtomUI.Animations.Motion).Assembly.Location))),
                SharedControlsAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(AtomUI.Controls.Primitives.ContentViewport).Assembly.Location))),
                SourceRevision = Git("rev-parse", "HEAD"),
                ProductDiffSha256 = HashProductChanges(),
                Host = window.PlatformImpl?.GetType().FullName,
                ClientSize = new { window.ClientSize.Width, window.ClientSize.Height },
                window.RenderScaling,
                ThemeVariant = window.ActualThemeVariant.ToString(),
                Input = "Public control API; does not measure native pointer latency",
                Presentation = "Not measured: batch Rendered is not display present; SceneInvalidated is not counted",
                Instrumentation = "Read-only existing batch/task and layout timing hooks; CPU/allocation include observer overhead. Compare identical instrumentation and A/A noise.",
                Scenes = scenes,
                Samples = results
            };
            Directory.CreateDirectory(Path.GetDirectoryName(options.Output)!);
            await File.WriteAllTextAsync(options.Output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"NATIVE_MOTION_COMPLETE output={options.Output} samples={results.Count}");
            lifetime.Shutdown(0);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            lifetime.Shutdown(1);
        }
    }

    internal static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = Stopwatch.StartNew();
        while (!condition())
        {
            if (deadline.Elapsed > TimeSpan.FromSeconds(30))
            {
                throw new TimeoutException("Native Gallery scenario did not become ready.");
            }
            await Task.Delay(50);
        }
    }

    private static string HashProductChanges()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(System.Text.Encoding.UTF8.GetBytes(Git("diff", "HEAD", "--", "src")));
        var root = Git("rev-parse", "--show-toplevel");
        var untracked = Git("ls-files", "--others", "--exclude-standard", "-z", "--", "src")
            .Split('\0', StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(untracked, StringComparer.Ordinal);
        foreach (var path in untracked)
        {
            hash.AppendData(System.Text.Encoding.UTF8.GetBytes(path));
            hash.AppendData([0]);
            hash.AppendData(File.ReadAllBytes(Path.Combine(root, path)));
            hash.AppendData([0]);
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static string Git(params string[] args)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")) &&
               !File.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        var start = new ProcessStartInfo("git")
        {
            RedirectStandardOutput = true, RedirectStandardError = true,
            WorkingDirectory = directory?.FullName ?? Environment.CurrentDirectory
        };
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(error);
        }
        return output.Trim();
    }

    private static void PrintUsage() => Console.WriteLine(
        "--native-motion --scenario " + string.Join('|', NativeMotionOptions.Scenarios) +
        "|all [--iterations 30] [--warmup 5] [--sample-ms 1000] [--refresh-rate 60] [--label name] [--output /tmp/result.json]");

#if DEBUG
    private const string BuildConfiguration = "Debug";
#else
    private const string BuildConfiguration = "Release";
#endif
}

internal sealed record NativeMotionOptions(string Scenario, int Iterations, int Warmup, int SampleMilliseconds,
    double? RefreshRate, string Label, string Output)
{
    internal static readonly string[] Scenarios = ["sidebar", "submenu", "progress", "statistic", "beam", "popup", "beam-offscreen"];

    internal static NativeMotionOptions Parse(string[] args)
    {
        var scenario = "sidebar";
        var iterations = 30;
        var warmup = 5;
        var sample = 1000;
        double? refresh = null;
        var label = "current";
        var output = Path.Combine(Path.GetTempPath(), $"atomui-native-motion-{Guid.NewGuid():N}.json");
        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i];
            if (key is "--native-motion" or "--help")
            {
                continue;
            }
            if (++i == args.Length)
            {
                throw new ArgumentException($"Missing native motion value for {key}.");
            }
            var value = args[i];
            switch (key)
            {
                case "--scenario": scenario = value; break;
                case "--iterations": iterations = int.TryParse(value, out var n) && n > 0 ? n : throw new ArgumentException("iterations must be positive"); break;
                case "--warmup": warmup = int.TryParse(value, out var w) && w >= 0 ? w : throw new ArgumentException("warmup must be nonnegative"); break;
                case "--sample-ms": sample = int.TryParse(value, out var ms) && ms is >= 100 and <= 60000 ? ms : throw new ArgumentException("sample-ms must be 100..60000"); break;
                case "--refresh-rate": refresh = double.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var hz) && double.IsFinite(hz) && hz > 0 ? hz : throw new ArgumentException("refresh-rate must be finite and positive"); break;
                case "--label": label = value; break;
                case "--output": output = Path.GetFullPath(value); break;
                default: throw new ArgumentException($"Unknown native motion option '{key}'.");
            }
        }
        if (scenario != "all" && !Scenarios.Contains(scenario, StringComparer.Ordinal))
        {
            throw new ArgumentException($"Unknown native motion scenario '{scenario}'.");
        }
        return new NativeMotionOptions(scenario, iterations, warmup, sample, refresh, label, output);
    }
}
