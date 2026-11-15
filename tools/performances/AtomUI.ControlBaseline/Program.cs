using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.ControlBaseline;

/// <summary>
/// Measures the fixed per-instance cost of every default-constructible control type under either the AtomUI theme or
/// the stock Avalonia Fluent theme, using the same harness so the two processes can be compared directly.
/// </summary>
internal static class Program
{
    private static readonly Size MeasureSize = new(1280, 4096);
    private static readonly Rect ArrangeRect = new(0, 0, 1280, 4096);
    private static List<Control>? s_retainForSelfTest;

    [STAThread]
    public static int Main(string[] args)
    {
        var mode       = "atomui";
        var count      = 100;
        var iterations = 9;
        var warmup     = 2;
        var jsonPath   = default(string);
        var filter     = default(string);
        var leakCheck  = false;
        var stateSnapshotDirectory = default(string);
        var holdSignalFile         = default(string);
        var frameReport            = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--mode" when i + 1 < args.Length:
                    mode = args[++i].ToLowerInvariant();
                    break;
                case "--count" when i + 1 < args.Length:
                    count = Math.Max(1, int.Parse(args[++i], CultureInfo.InvariantCulture));
                    break;
                case "--iterations" when i + 1 < args.Length:
                    iterations = Math.Max(1, int.Parse(args[++i], CultureInfo.InvariantCulture));
                    break;
                case "--warmup" when i + 1 < args.Length:
                    warmup = Math.Max(0, int.Parse(args[++i], CultureInfo.InvariantCulture));
                    break;
                case "--json" when i + 1 < args.Length:
                    jsonPath = args[++i];
                    break;
                case "--filter" when i + 1 < args.Length:
                    filter = args[++i];
                    break;
                case "--leak":
                    leakCheck = true;
                    break;
                case "--state-snapshots" when i + 1 < args.Length:
                    stateSnapshotDirectory = args[++i];
                    break;
                case "--frames":
                    frameReport = true;
                    break;
                case "--hold" when i + 1 < args.Length:
                    // Shows --count instances of the filtered types and keeps them alive until the file exists,
                    // so a heap snapshot (dotnet-gcdump) can attribute retained objects per instance.
                    holdSignalFile = args[++i];
                    break;
                case "--leak-selftest":
                    // Retains every instance on purpose; the check must then report all of them as alive.
                    leakCheck        = true;
                    s_retainForSelfTest = new List<Control>();
                    break;
            }
        }

        IReadOnlyList<Assembly> assemblies;
        // Generated icon assemblies hold hundreds of equivalent types; two representatives are enough.
        IReadOnlyList<Type> sampleTypes = [];
        if (mode == "fluent")
        {
            AppBuilder.Configure<FluentBaselineApplication>()
                      .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                      .SetupWithLifetime(CreateLifetime());
            assemblies = [typeof(Avalonia.Controls.Button).Assembly];
        }
        else
        {
            var builder = AppBuilder.Configure<AtomUIBaselineApplication>();
            if (stateSnapshotDirectory is not null)
            {
                // Pixel capture needs a real renderer; timing modes keep the headless drawing stub.
                builder = builder.UseSkia();
            }

            builder.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = stateSnapshotDirectory is null })
                      .UseAtomUI(atom =>
                      {
                          atom.Theme.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
                          atom.UseDesktopControls();
                          atom.UseDesktopColorPicker();
                          atom.UseDesktopDataGrid();
                      })
                      .SetupWithLifetime(CreateLifetime());
            assemblies =
            [
                typeof(AtomUI.Controls.Icon).Assembly,
                typeof(AtomUI.Controls.IconPresenter).Assembly,
                typeof(AtomUI.Desktop.Controls.Button).Assembly,
                typeof(AtomUI.Desktop.Controls.ColorPicker).Assembly,
                typeof(AtomUI.Desktop.Controls.DataGrid).Assembly
            ];
            sampleTypes =
            [
                typeof(AtomUI.Icons.AntDesign.LoadingOutlined),
                typeof(AtomUI.Icons.AntDesign.SearchOutlined)
            ];
        }

        Dispatcher.UIThread.RunJobs();

        var types = assemblies.Distinct()
                              .SelectMany(assembly => assembly.GetExportedTypes())
                              .Concat(sampleTypes)
                              .Where(IsMeasurable)
                              .Where(type => filter is null ||
                                             filter.Split(',').Contains(type.Name, StringComparer.OrdinalIgnoreCase))
                              .OrderBy(type => type.Name, StringComparer.Ordinal)
                              .ToList();

        if (frameReport)
        {
            return FrameReport.Run(types, Create, MeasureSize);
        }

        if (holdSignalFile is not null)
        {
            return Hold(types, count, holdSignalFile);
        }

        if (stateSnapshotDirectory is not null)
        {
            return StateSnapshots.Run(types, stateSnapshotDirectory);
        }

        if (leakCheck)
        {
            return RunLeakCheck(types, count, mode, jsonPath);
        }

        // The empty-window floor is subtracted so cheap controls are not dominated by host window cost.
        for (var i = 0; i < 3; i++)
        {
            _ = Measure(null, 0);
        }

        var floorSamples = Enumerable.Range(0, 15).Select(_ => Measure(null, 0)).ToList();
        var floorAttach  = Median(floorSamples.Select(sample => sample.AttachMs));
        var floorAlloc   = Median(floorSamples.Select(sample => (double)sample.AttachBytes));
        Console.WriteLine($"mode={mode} count={count} floor attach={floorAttach:0.000}ms alloc={floorAlloc / 1024:0}KB types={types.Count}");

        var rows = new List<Dictionary<string, object?>>();
        var index = 0;
        foreach (var type in types)
        {
            index++;
            var row = new Dictionary<string, object?>
            {
                ["mode"] = mode,
                ["type"] = type.Name,
                ["base"] = DescribeBase(type)
            };
            rows.Add(row);
            Console.Write($"[{index}/{types.Count}] {type.Name} ");
            Console.Out.Flush();
            try
            {
                _ = Measure(type, 1);
                for (var i = 0; i < warmup; i++)
                {
                    _ = Measure(type, count);
                }

                var samples = new List<Sample>(iterations);
                for (var i = 0; i < iterations; i++)
                {
                    samples.Add(Measure(type, count));
                }

                var createUs = samples.Select(sample => sample.CreateMs * 1000 / count).Order().ToArray();
                var attachUs = samples.Select(sample => Math.Max(0, sample.AttachMs - floorAttach) * 1000 / count)
                                      .Order()
                                      .ToArray();
                var totalUs = samples.Select(sample =>
                                         (sample.CreateMs + Math.Max(0, sample.AttachMs - floorAttach)) * 1000 / count)
                                     .Order()
                                     .ToArray();
                var createKb = Median(samples.Select(sample => sample.CreateBytes / 1024d / count));
                var attachKb = Median(samples.Select(sample =>
                    Math.Max(0, sample.AttachBytes - floorAlloc) / 1024d / count));
                row["visuals"]        = Math.Round(samples[0].Visuals / (double)count, 2);
                row["createUsMin"]    = Math.Round(createUs[0], 2);
                row["createUsMedian"] = Math.Round(Median(createUs), 2);
                row["attachUsMin"]    = Math.Round(attachUs[0], 2);
                row["attachUsMedian"] = Math.Round(Median(attachUs), 2);
                row["totalUsMin"]     = Math.Round(totalUs[0], 2);
                row["totalUsMedian"]  = Math.Round(Median(totalUs), 2);
                row["createKb"]       = Math.Round(createKb, 2);
                row["attachKb"]       = Math.Round(attachKb, 2);
                row["teardownUs"]     = Math.Round(Median(samples.Select(sample => sample.TeardownMs * 1000 / count)), 2);
                // Collections that landed inside the timed create + attach region; compare timings only when equal.
                row["gcPerSample"]    = Math.Round(samples.Average(sample => sample.Collections), 2);
                Console.WriteLine(
                    $"visuals={row["visuals"]} total={totalUs[0]:0.0}us(min) {Median(totalUs):0.0}us(median) alloc={createKb + attachKb:0.0}KB gc/sample={row["gcPerSample"]}");
            }
            catch (Exception ex)
            {
                var inner = ex is TargetInvocationException { InnerException: { } innerException } ? innerException : ex;
                row["error"] = inner.GetType().Name + ": " + inner.Message;
                Console.WriteLine("FAILED " + row["error"]);
            }
        }

        if (!string.IsNullOrWhiteSpace(jsonPath))
        {
            var fullPath = Path.GetFullPath(jsonPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote {fullPath}");
        }

        return 0;
    }

    /// <summary>
    /// Shows <paramref name="count"/> instances of each type, closes the window and reports how many instances are
    /// still reachable after collection. A non-zero count means something outlives the closed window.
    /// </summary>
    private static int RunLeakCheck(IReadOnlyList<Type> types, int count, string mode, string? jsonPath)
    {
        var rows        = new List<Dictionary<string, object?>>();
        var leakingTypes = 0;
        foreach (var type in types)
        {
            var row = new Dictionary<string, object?> { ["mode"] = mode, ["type"] = type.Name };
            rows.Add(row);
            try
            {
                var references = ShowAndClose(type, count);
                for (var round = 0; round < 4; round++)
                {
                    AvaloniaHeadlessPlatform.ForceRenderTimerTick(2);
                    Dispatcher.UIThread.RunJobs();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                var alive = references.Count(reference => reference.IsAlive);
                row["alive"] = alive;
                if (alive > 0)
                {
                    leakingTypes++;
                    Console.WriteLine($"{type.Name}: {alive}/{count} alive after close");
                }
            }
            catch (Exception ex)
            {
                var inner = ex is TargetInvocationException { InnerException: { } innerException } ? innerException : ex;
                row["error"] = inner.GetType().Name + ": " + inner.Message;
            }
        }

        Console.WriteLine($"mode={mode} types={types.Count} leaking types={leakingTypes}");
        if (!string.IsNullOrWhiteSpace(jsonPath))
        {
            File.WriteAllText(Path.GetFullPath(jsonPath), JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
        }

        return 0;
    }

    // Kept out of line so no instance survives in the caller's locals when it collects.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static List<WeakReference> ShowAndClose(Type type, int count)
    {
        var references = new List<WeakReference>(count);
        var panel      = new WrapPanel();
        for (var i = 0; i < count; i++)
        {
            var control = Create(type);
            references.Add(new WeakReference(control));
            s_retainForSelfTest?.Add(control);
            panel.Children.Add(control);
        }

        var window = new Avalonia.Controls.Window
        {
            Width         = MeasureSize.Width,
            Height        = 900,
            Content       = panel,
            ShowInTaskbar = false
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(2);
        Dispatcher.UIThread.RunJobs();
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return references;
    }

    private static int Hold(IReadOnlyList<Type> types, int count, string signalFile)
    {
        var panel = new WrapPanel();
        foreach (var type in types)
        {
            for (var i = 0; i < count; i++)
            {
                panel.Children.Add(Create(type));
            }
        }

        var window = new Avalonia.Controls.Window
        {
            Width         = MeasureSize.Width,
            Height        = 900,
            Content       = panel,
            ShowInTaskbar = false
        };
        window.Show();
        for (var i = 0; i < 4; i++)
        {
            Dispatcher.UIThread.RunJobs();
            window.Measure(MeasureSize);
            window.Arrange(ArrangeRect);
            window.UpdateLayout();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(2);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Console.WriteLine($"READY pid={Environment.ProcessId} instances={panel.Children.Count}");
        Console.Out.Flush();
        while (!File.Exists(signalFile))
        {
            Thread.Sleep(100);
        }

        GC.KeepAlive(window);
        return 0;
    }

    private static ClassicDesktopStyleApplicationLifetime CreateLifetime()
    {
        return new ClassicDesktopStyleApplicationLifetime
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };
    }

    private static bool IsMeasurable(Type type)
    {
        return typeof(Control).IsAssignableFrom(type) &&
               !type.IsAbstract &&
               !type.IsGenericTypeDefinition &&
               !typeof(TopLevel).IsAssignableFrom(type) &&
               !typeof(Avalonia.Controls.Primitives.Popup).IsAssignableFrom(type) &&
               type.GetConstructor(Type.EmptyTypes) is not null;
    }

    private static string DescribeBase(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.Assembly != type.Assembly)
            {
                return current.Name;
            }
        }

        return string.Empty;
    }

    private static Control Create(Type type)
    {
        var control = (Control)Activator.CreateInstance(type)!;
        switch (control)
        {
            case HeaderedContentControl headered:
                headered.Header  ??= "Header";
                headered.Content ??= "Text";
                break;
            case ContentControl content:
                content.Content ??= "Text";
                break;
            case Avalonia.Controls.TextBox textBox:
                textBox.Text = "Text";
                break;
            case Avalonia.Controls.TextBlock textBlock:
                textBlock.Text = "Text";
                break;
        }

        return control;
    }

    private static Sample Measure(Type? type, int count)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var gcStart    = GC.CollectionCount(0);
        var allocStart = GC.GetAllocatedBytesForCurrentThread();
        var start      = Stopwatch.GetTimestamp();
        var controls   = new Control[count];
        for (var i = 0; i < count; i++)
        {
            controls[i] = Create(type!);
        }

        var createTicks = Stopwatch.GetTimestamp() - start;
        var createBytes = GC.GetAllocatedBytesForCurrentThread() - allocStart;

        allocStart = GC.GetAllocatedBytesForCurrentThread();
        start      = Stopwatch.GetTimestamp();
        var panel = new WrapPanel();
        panel.Children.AddRange(controls);
        var window = new Avalonia.Controls.Window
        {
            Width         = MeasureSize.Width,
            Height        = 900,
            Content       = panel,
            ShowInTaskbar = false
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.Measure(MeasureSize);
        window.Arrange(ArrangeRect);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        var attachTicks = Stopwatch.GetTimestamp() - start;
        var attachBytes = GC.GetAllocatedBytesForCurrentThread() - allocStart;
        var collections = GC.CollectionCount(0) - gcStart;

        var visuals = 0;
        foreach (var control in controls)
        {
            visuals += control.GetSelfAndVisualDescendants().Count();
        }

        start = Stopwatch.GetTimestamp();
        window.Close();
        Dispatcher.UIThread.RunJobs();
        var teardownTicks = Stopwatch.GetTimestamp() - start;

        return new Sample(ToMs(createTicks), ToMs(attachTicks), ToMs(teardownTicks), createBytes, attachBytes, visuals, collections);
    }

    private static double ToMs(long ticks)
    {
        return ticks * 1000d / Stopwatch.Frequency;
    }

    private static double Median(IEnumerable<double> values)
    {
        var ordered = values.Order().ToArray();
        if (ordered.Length == 0)
        {
            return 0;
        }

        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 1 ? ordered[middle] : (ordered[middle - 1] + ordered[middle]) / 2;
    }

    private sealed record Sample(
        double CreateMs,
        double AttachMs,
        double TeardownMs,
        long CreateBytes,
        long AttachBytes,
        int Visuals,
        int Collections);
}
