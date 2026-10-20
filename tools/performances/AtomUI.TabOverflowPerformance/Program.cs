using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaButton = Avalonia.Controls.Button;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using TabStrip = AtomUI.Desktop.Controls.TabStrip;
using TabStripItem = AtomUI.Desktop.Controls.TabStripItem;

namespace AtomUI.TabOverflowPerformance;

internal static class Program
{
    private static readonly Size MeasureSize = new(1280, 4096);
    private static readonly Rect ArrangeRect = new(0, 0, 1280, 4096);

    [STAThread]
    private static int Main(string[] args)
    {
        var options = Options.Parse(args);
        AppBuilder.Configure<PerformanceApplication>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .UseAtomUI(builder => builder.UseDesktopControls())
                  .SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime());

        if (options.CreationOnly)
        {
            return RunCreationBenchmarks(options);
        }

        var results = new[]
        {
            Measure("TabControl.Line", CreateTabControl(card: false), options),
            Measure("TabControl.Card", CreateTabControl(card: true), options),
            Measure("TabStrip.Line", CreateTabStrip(card: false), options),
            Measure("TabStrip.Card", CreateTabStrip(card: true), options)
        };
        var output = RenderTable(results);
        Console.WriteLine(output);

        if (options.MarkdownPath is { Length: > 0 } markdownPath)
        {
            var fullPath = Path.GetFullPath(markdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, RenderMarkdown(results, options),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"Wrote markdown result: {fullPath}");
        }

        return 0;
    }

    private static int RunCreationBenchmarks(Options options)
    {
        var results = new[]
        {
            MeasureCreation("TabControl.Line", () => CreateTabControl(card: false), options.Count),
            MeasureCreation("TabControl.Card", () => CreateTabControl(card: true), options.Count),
            MeasureCreation("TabStrip.Line", () => CreateTabStrip(card: false), options.Count),
            MeasureCreation("TabStrip.Card", () => CreateTabStrip(card: true), options.Count)
        };
        var output = RenderCreationTable(results);
        Console.WriteLine(output);

        if (options.MarkdownPath is { Length: > 0 } markdownPath)
        {
            var fullPath = Path.GetFullPath(markdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, RenderCreationMarkdown(results, options),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"Wrote markdown result: {fullPath}");
        }

        return 0;
    }

    private static CreationResult MeasureCreation(string name, Func<Control> factory, int count)
    {
        using (var warmup = new RealizedControl(factory()))
        {
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        var panel = new StackPanel { Spacing = 8 };
        for (var i = 0; i < count; i++)
        {
            panel.Children.Add(factory());
        }
        using var realized = new RealizedControl(panel);
        stopwatch.Stop();

        return new CreationResult(
            name,
            count,
            stopwatch.Elapsed,
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            panel.GetVisualDescendants().Count());
    }

    private static Result Measure(string name, Control control, Options options)
    {
        using var realized = new RealizedControl(control);
        var scrollViewer = FindNamed<Control>(control, "PART_CardTabStripScrollViewer") ??
                           FindNamed<Control>(control, "PART_TabsContainer") ??
                           throw new InvalidOperationException($"{name} did not realize its tab scroll viewer.");
        var indicator = FindNamed<IconButton>(scrollViewer, "PART_ScrollMenuIndicator") ??
                        throw new InvalidOperationException($"{name} did not realize PART_ScrollMenuIndicator.");
        if (!indicator.IsVisible)
        {
            throw new InvalidOperationException($"{name} did not overflow at the benchmark viewport.");
        }

        var closeOverflow = CreateCloseOverflowAction(scrollViewer);
        OpenOverflow(indicator);
        realized.RefreshLayout();
        realized.AssertPopupOpen();
        closeOverflow();
        realized.RefreshLayout();
        for (var i = 0; i < options.WarmupCount; i++)
        {
            OpenOverflow(indicator);
            realized.RefreshLayout();
            closeOverflow();
            realized.RefreshLayout();
        }

        var visualsAfterWarmup = realized.VisualCount;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var gen0Before = GC.CollectionCount(0);
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < options.Count; i++)
        {
            OpenOverflow(indicator);
            realized.RefreshLayout();
            closeOverflow();
            realized.RefreshLayout();
        }
        stopwatch.Stop();

        return new Result(
            name,
            options.Count,
            stopwatch.Elapsed,
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            GC.CollectionCount(0) - gen0Before,
            visualsAfterWarmup,
            realized.VisualCount);
    }

    private static void OpenOverflow(IconButton indicator)
    {
        indicator.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, indicator));
    }

    private static Action CreateCloseOverflowAction(Control scrollViewer)
    {
        foreach (var methodName in new[] { "CloseOverflowPopup", "CloseForLifecycle" })
        {
            for (var type = scrollViewer.GetType(); type is not null; type = type.BaseType)
            {
                var method = type.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method is not null)
                {
                    return method.CreateDelegate<Action>(scrollViewer);
                }
            }
        }

        throw new InvalidOperationException(
            $"{scrollViewer.GetType().FullName} does not expose an ordinary close path or its legacy fallback.");
    }

    private static T? FindNamed<T>(Control root, string name)
        where T : Control
    {
        if (root is T typedRoot && root.Name == name)
        {
            return typedRoot;
        }

        return root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => control.Name == name);
    }

    private static BaseTabControl CreateTabControl(bool card)
    {
        BaseTabControl control = card ? new CardTabControl() : new AtomUI.Desktop.Controls.TabControl();
        control.Width = 260;
        for (var i = 0; i < 24; i++)
        {
            control.Items.Add(new AtomTabItem
            {
                Header = $"Long Tab Header {i + 1}",
                Content = $"Content {i + 1}",
                IsClosable = i % 3 == 0
            });
        }
        return control;
    }

    private static BaseTabStrip CreateTabStrip(bool card)
    {
        BaseTabStrip control = card ? new CardTabStrip() : new TabStrip();
        control.Width = 260;
        for (var i = 0; i < 24; i++)
        {
            control.Items.Add(new TabStripItem
            {
                Content = $"Long Tab Header {i + 1}",
                IsClosable = i % 3 == 0
            });
        }
        return control;
    }

    private static string RenderTable(IReadOnlyList<Result> results)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Scenario          Cycles  Total ms  us/cycle  KB total  bytes/cycle  Gen0  Visual warm/end");
        builder.AppendLine("-------------------------------------------------------------------------------------------");
        foreach (var result in results)
        {
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"{result.Name,-18}{result.Cycles,7}{result.Elapsed.TotalMilliseconds,10:0.00}{result.MicrosecondsPerCycle,10:0.00}{result.AllocatedBytes / 1024.0,10:0.0}{result.BytesPerCycle,13:0.0}{result.Gen0Collections,6}{result.VisualsAfterWarmup,8}/{result.VisualsAfterMeasurement}");
        }
        return builder.ToString();
    }

    private static string RenderMarkdown(IReadOnlyList<Result> results, Options options)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Tab Overflow Interaction Benchmark");
        builder.AppendLine();
        builder.AppendLine($"- Date: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine($"- Configuration: {typeof(Program).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration} / net10.0");
        builder.AppendLine("- Runner: `tools/performances/AtomUI.TabOverflowPerformance`");
        builder.AppendLine($"- Warmup: {options.WarmupCount} complete open/close cycles per control");
        builder.AppendLine("- Operation: realized overflowing control -> click more indicator -> ordinary close (legacy lifecycle fallback when no ordinary close method exists), with layout and a compositor frame at both boundaries");
        builder.AppendLine("- Host: real headless overlay popup; visible menu content is required. Open/close layout work is included and host errors fail the run.");
        builder.AppendLine();
        builder.AppendLine("| Scenario | Cycles | Total ms | us/cycle | KB total | bytes/cycle | Gen0 | Visual warm/end |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var result in results)
        {
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"| {result.Name} | {result.Cycles} | {result.Elapsed.TotalMilliseconds:0.00} | {result.MicrosecondsPerCycle:0.00} | {result.AllocatedBytes / 1024.0:0.0} | {result.BytesPerCycle:0.0} | {result.Gen0Collections} | {result.VisualsAfterWarmup}/{result.VisualsAfterMeasurement} |");
        }
        return builder.ToString();
    }

    private static string RenderCreationTable(IReadOnlyList<CreationResult> results)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Scenario          Count  Total ms  ms/item  KB/item  Visuals");
        builder.AppendLine("----------------------------------------------------------------");
        foreach (var result in results)
        {
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"{result.Name,-18}{result.Count,6}{result.Elapsed.TotalMilliseconds,10:0.00}{result.MillisecondsPerItem,9:0.000}{result.KilobytesPerItem,9:0.0}{result.VisualCount,9}");
        }
        return builder.ToString();
    }

    private static string RenderCreationMarkdown(IReadOnlyList<CreationResult> results, Options options)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Tab Never-open Creation/Layout Benchmark");
        builder.AppendLine();
        builder.AppendLine($"- Date: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine($"- Configuration: {typeof(Program).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration} / net10.0");
        builder.AppendLine("- Runner: `tools/performances/AtomUI.TabOverflowPerformance --creation-only`");
        builder.AppendLine($"- Controls per scenario: {options.Count}");
        builder.AppendLine("- Operation: construct overflowing tab owners -> attach -> measure/arrange/update layout without opening the overflow popup");
        builder.AppendLine();
        builder.AppendLine("| Scenario | Count | Total ms | ms/item | KB/item | Visuals |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: |");
        foreach (var result in results)
        {
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"| {result.Name} | {result.Count} | {result.Elapsed.TotalMilliseconds:0.00} | {result.MillisecondsPerItem:0.000} | {result.KilobytesPerItem:0.0} | {result.VisualCount} |");
        }
        return builder.ToString();
    }

    private sealed record Options(int Count, int WarmupCount, string? MarkdownPath, bool CreationOnly)
    {
        public static Options Parse(IReadOnlyList<string> args)
        {
            var count = 500;
            var warmup = 25;
            string? markdown = null;
            var creationOnly = false;
            for (var i = 0; i < args.Count; i++)
            {
                if (args[i] == "--count" && i + 1 < args.Count && int.TryParse(args[++i], out var parsedCount))
                {
                    count = Math.Max(1, parsedCount);
                }
                else if (args[i] == "--warmup" && i + 1 < args.Count && int.TryParse(args[++i], out var parsedWarmup))
                {
                    warmup = Math.Max(0, parsedWarmup);
                }
                else if (args[i] == "--markdown" && i + 1 < args.Count)
                {
                    markdown = args[++i];
                }
                else if (args[i] == "--creation-only")
                {
                    creationOnly = true;
                }
            }
            return new Options(count, warmup, markdown, creationOnly);
        }
    }

    private sealed record Result(
        string Name,
        int Cycles,
        TimeSpan Elapsed,
        long AllocatedBytes,
        int Gen0Collections,
        int VisualsAfterWarmup,
        int VisualsAfterMeasurement)
    {
        public double MicrosecondsPerCycle => Elapsed.TotalMilliseconds * 1000 / Cycles;
        public double BytesPerCycle => AllocatedBytes / (double)Cycles;
    }

    private sealed record CreationResult(
        string Name,
        int Count,
        TimeSpan Elapsed,
        long AllocatedBytes,
        int VisualCount)
    {
        public double MillisecondsPerItem => Elapsed.TotalMilliseconds / Count;
        public double KilobytesPerItem => AllocatedBytes / 1024.0 / Count;
    }

    private sealed class RealizedControl : IDisposable
    {
        public RealizedControl(Control control)
        {
            var host = new ScopeAwareOverlayLayerPanel();
            host.Children.Add(control);
            var layers = new VisualLayerManager { Child = host };
            var overlayProperty = typeof(VisualLayerManager).GetProperty(
                "EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic) ??
                throw new InvalidOperationException("The headless benchmark requires a popup overlay layer.");
            overlayProperty.SetValue(layers, true);
            Window = new Avalonia.Controls.Window
            {
                Width = MeasureSize.Width,
                Height = 900,
                Content = layers,
                ShowInTaskbar = false
            };
            Window.Show();
            RefreshLayout();
        }

        private Avalonia.Controls.Window Window { get; }

        public int VisualCount => Window.GetVisualDescendants().Count();

        public void AssertPopupOpen()
        {
            if (!Window.GetVisualDescendants().OfType<OverlayPopupHost>().Any(host =>
                    host.IsVisible && host.Bounds.Width > 0 && host.Bounds.Height > 0 &&
                    host.GetVisualDescendants().OfType<Control>().Any(item =>
                        item.GetType().Name.EndsWith("OverflowMenuItem", StringComparison.Ordinal) &&
                        item.IsVisible && item.Bounds.Width > 0 && item.Bounds.Height > 0)))
            {
                throw new InvalidOperationException("The overflow popup did not realize visible menu content.");
            }
        }

        public void Dispose()
        {
            Window.Close();
            CompleteFrames();
        }

        public void RefreshLayout()
        {
            Dispatcher.UIThread.RunJobs();
            Window.Measure(MeasureSize);
            Window.Arrange(ArrangeRect);
            Window.UpdateLayout();
            // Complete the same frame boundary for both implementations. Layout
            // alone can leave detached visuals queued for compositor serialization.
            CompleteFrames();
        }

        private static void CompleteFrames()
        {
            var dispatcher = Dispatcher.UIThread;
            for (var frame = 0; frame < 10; frame++)
            {
                dispatcher.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                if (!dispatcher.HasJobsWithPriority(DispatcherPriority.SystemIdle))
                {
                    return;
                }
            }
            throw new InvalidOperationException("The headless benchmark did not settle within ten frames.");
        }
    }
}

public sealed class PerformanceApplication : Application
{
    // 本工程不引用 AtomUI 生成器，按公开契约手写挂接。
    public override void RegisterServices()
    {
        base.RegisterServices();
        ApplicationStartup.Attach(this);
    }
}
