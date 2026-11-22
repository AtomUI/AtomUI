using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.Workspace.Views;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.GalleryPerformance;

internal sealed record GalleryAuditSample(
    double WorkMs,
    double TriggerMs,
    double MaxPumpMs,
    int PumpCount,
    long AllocatedBytes,
    int Gen0Collections,
    int VisualCount);

internal sealed record GalleryAuditShape(
    int VisualCount,
    int LogicalCount,
    int TemplatedControlCount,
    int MaxDepth,
    IReadOnlyList<KeyValuePair<string, int>> TopTypes);

/// <summary>
/// Navigates every registered Gallery ShowCase in one process and records only the navigation trigger and layout pump
/// work. Visual tree statistics used for readiness are collected outside the timed and allocation-counted regions.
/// </summary>
internal static class GalleryAuditProbe
{
    public static int Run(string[] args)
    {
        var iterations = 15;
        var warmup     = 2;
        var timeout    = TimeSpan.FromSeconds(30);
        var label      = "audit";
        var jsonPath   = default(string);
        var markdown   = default(string);
        var selection  = default(string);
        var reverse    = false;
        var full       = false;
        var loose      = false;
        var snapshotDirectory = default(string);
        var leakVisits        = 0;
        var designLanguage    = default(string);
        var appearance        = default(string);
        var density           = default(string);
        var freezeLoadingIcons = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case SnapshotFreeze.Argument:
                    freezeLoadingIcons = true;
                    break;
                case "--design-language" when i + 1 < args.Length:
                    designLanguage = args[++i];
                    break;
                case "--appearance" when i + 1 < args.Length:
                    appearance = args[++i];
                    break;
                case "--density" when i + 1 < args.Length:
                    density = args[++i];
                    break;
                case "--iterations" when i + 1 < args.Length:
                    iterations = Math.Max(1, int.Parse(args[++i], CultureInfo.InvariantCulture));
                    break;
                case "--warmup" when i + 1 < args.Length:
                    warmup = Math.Max(0, int.Parse(args[++i], CultureInfo.InvariantCulture));
                    break;
                case "--timeout-ms" when i + 1 < args.Length:
                    timeout = TimeSpan.FromMilliseconds(int.Parse(args[++i], CultureInfo.InvariantCulture));
                    break;
                case "--label" when i + 1 < args.Length:
                    label = args[++i];
                    break;
                case "--json" when i + 1 < args.Length:
                    jsonPath = args[++i];
                    break;
                case "--markdown" when i + 1 < args.Length:
                    markdown = args[++i];
                    break;
                case "--audit-showcases" when i + 1 < args.Length:
                    selection = args[++i];
                    break;
                case "--audit-reverse":
                    reverse = true;
                    break;
                case "--audit-full":
                    full = true;
                    break;
                case "--audit-loose-ready":
                    loose = true;
                    break;
                case "--snapshot-dir" when i + 1 < args.Length:
                    snapshotDirectory = args[++i];
                    break;
                case "--leak-check" when i + 1 < args.Length:
                    leakVisits = Math.Max(1, int.Parse(args[++i], CultureInfo.InvariantCulture));
                    break;
            }
        }

        var keys = string.IsNullOrWhiteSpace(selection) || selection == "all"
            ? Program.ShowCases.Keys.ToList()
            : selection.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (reverse)
        {
            keys.Reverse();
        }

        // The three variant arguments come together or not at all.
        DesignLanguageVariant? variant = null;
        if (designLanguage is not null || appearance is not null || density is not null)
        {
            if (designLanguage is null || appearance is null || density is null ||
                !DesignLanguageVariants.TryParse(designLanguage, appearance, density, out var parsed))
            {
                Console.Error.WriteLine(
                    "--design-language, --appearance and --density must be given together and name one of the " +
                    "variants listed by --list-variants.");
                return 2;
            }
            variant = parsed;
        }

        Program.SetupAvalonia(out var lifetime, renderPixels: !string.IsNullOrWhiteSpace(snapshotDirectory));
        if (lifetime.MainWindow is not WorkspaceWindow window)
        {
            Console.Error.WriteLine("Gallery workspace window was not created.");
            return 1;
        }

        // Full mode mounts every ShowCaseItem instead of only the viewport batch the Gallery normally materializes.
        GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = full;
        window.ShowInTaskbar = false;
        window.Width         = Program.WindowSize.Width;
        window.Height        = Program.WindowSize.Height;
        window.Show();
        Program.WaitForRoute(window, Program.Overview, timeout);

        // Before any page is visited, so snapshots, leak checks and timings all run under the variant.
        if (variant is { } selected && Program.ApplyVariant(window, selected, timeout) != 0)
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            return 1;
        }

        if (!string.IsNullOrWhiteSpace(snapshotDirectory))
        {
            var snapshotResult = GalleryGuardProbe.RunSnapshots(window, keys, timeout, snapshotDirectory, freezeLoadingIcons);
            window.Close();
            Dispatcher.UIThread.RunJobs();
            return snapshotResult;
        }

        if (leakVisits > 0)
        {
            var leakResult = GalleryGuardProbe.RunLeakCheck(window, keys, timeout, leakVisits, markdown);
            window.Close();
            Dispatcher.UIThread.RunJobs();
            return leakResult;
        }

        var rows = new List<Dictionary<string, object?>>();
        var order = 0;
        foreach (var key in keys)
        {
            order++;
            var row = new Dictionary<string, object?>
            {
                ["key"]   = key,
                ["order"] = order,
                ["label"] = label
            };
            rows.Add(row);
            if (!Program.ShowCases.TryGetValue(key, out var showCase))
            {
                row["error"] = "unknown showcase";
                continue;
            }

            try
            {
                var first = Measure(window, showCase, timeout, loose, out var shape);
                Program.NavigateToOverview(window, timeout);
                for (var i = 0; i < warmup; i++)
                {
                    _ = Measure(window, showCase, timeout, loose, out _);
                    Program.NavigateToOverview(window, timeout);
                }

                var samples = new List<GalleryAuditSample>(iterations);
                for (var i = 0; i < iterations; i++)
                {
                    samples.Add(Measure(window, showCase, timeout, loose, out shape));
                    Program.NavigateToOverview(window, timeout);
                }

                var work = samples.Select(sample => sample.WorkMs).Order().ToArray();
                row["showcase"]        = showCase.Label;
                row["visuals"]         = shape.VisualCount;
                row["logical"]         = shape.LogicalCount;
                row["templated"]       = shape.TemplatedControlCount;
                row["maxDepth"]        = shape.MaxDepth;
                row["firstMs"]         = Round(first.WorkMs);
                row["firstAllocKb"]    = first.AllocatedBytes / 1024;
                row["repMinMs"]        = Round(work[0]);
                row["repMedianMs"]     = Round(Percentile(work, 0.5));
                row["repMeanMs"]       = Round(work.Average());
                row["repP95Ms"]        = Round(Percentile(work, 0.95));
                row["repTriggerMs"]    = Round(Median(samples.Select(sample => sample.TriggerMs)));
                row["repMaxPumpMs"]    = Round(Median(samples.Select(sample => sample.MaxPumpMs)));
                row["repPumps"]        = Median(samples.Select(sample => (double)sample.PumpCount));
                row["repAllocKb"]      = (long)Median(samples.Select(sample => sample.AllocatedBytes / 1024d));
                row["repGen0"]         = Median(samples.Select(sample => (double)sample.Gen0Collections));
                row["topTypes"]        = shape.TopTypes.ToDictionary(pair => pair.Key, pair => pair.Value);
                Console.WriteLine(
                    $"[{order}/{keys.Count}] {key}: visuals={shape.VisualCount} first={first.WorkMs:0.0}ms " +
                    $"repMin={work[0]:0.0}ms repMedian={Percentile(work, 0.5):0.0}ms alloc={row["repAllocKb"]}KB");
            }
            catch (Exception ex)
            {
                row["error"] = ex.GetType().Name + ": " + ex.Message;
                Console.WriteLine($"[{order}/{keys.Count}] {key}: FAILED {ex.GetType().Name}: {ex.Message}");
                try
                {
                    Program.NavigateToOverview(window, timeout);
                }
                catch (Exception recoverException)
                {
                    Console.WriteLine($"Recovery navigation failed: {recoverException.Message}");
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(jsonPath))
        {
            var fullPath = Path.GetFullPath(jsonPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote audit json: {fullPath}");
        }

        if (!string.IsNullOrWhiteSpace(markdown))
        {
            var fullPath = Path.GetFullPath(markdown);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, RenderMarkdown(rows, label, iterations, warmup), new UTF8Encoding(false));
            Console.WriteLine($"Wrote audit markdown: {fullPath}");
        }

        window.Close();
        Dispatcher.UIThread.RunJobs();
        return 0;
    }

    private static GalleryAuditSample Measure(WorkspaceWindow window,
                                              ShowCaseSpec showCase,
                                              TimeSpan timeout,
                                              bool looseReady,
                                              out GalleryAuditShape shape)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var gen0Before = GC.CollectionCount(0);
        var workTicks  = 0L;
        var allocated  = 0L;
        var maxPump    = 0L;
        var pumpCount  = 0;

        var allocBefore = GC.GetAllocatedBytesForCurrentThread();
        var start       = Stopwatch.GetTimestamp();
        Program.TriggerNavigation(window, showCase);
        var triggerTicks = Stopwatch.GetTimestamp() - start;
        allocated += GC.GetAllocatedBytesForCurrentThread() - allocBefore;
        workTicks += triggerTicks;

        var deadline      = Stopwatch.StartNew();
        var stablePasses  = 0;
        var previousStats = default(RouteStats);
        Control? route    = null;
        while (deadline.Elapsed < timeout)
        {
            allocBefore = GC.GetAllocatedBytesForCurrentThread();
            start       = Stopwatch.GetTimestamp();
            Program.PumpLayout(window);
            var pumpTicks = Stopwatch.GetTimestamp() - start;
            allocated += GC.GetAllocatedBytesForCurrentThread() - allocBefore;
            workTicks += pumpTicks;
            maxPump   =  Math.Max(maxPump, pumpTicks);
            pumpCount++;

            route = window.GetSelfAndVisualDescendants()
                          .OfType<Control>()
                          .FirstOrDefault(control => control.GetType().FullName == showCase.RouteTypeName);
            if (route is null || !route.IsVisible || route.Bounds.Width <= 0 || route.Bounds.Height <= 0)
            {
                continue;
            }

            var currentStats = RouteStats.Collect(route);
            // Loose readiness accepts any stable non-empty route; it exists for showcases whose dedicated shape
            // predicate no longer matches the current Gallery page.
            if ((looseReady ? currentStats.VisualCount > 0 : currentStats.IsDisplayReady(showCase)) &&
                previousStats is not null &&
                currentStats.HasSameShape(previousStats))
            {
                stablePasses++;
                if (stablePasses >= 2)
                {
                    shape = CollectShape(route);
                    return new GalleryAuditSample(
                        ToMs(workTicks),
                        ToMs(triggerTicks),
                        ToMs(maxPump),
                        pumpCount,
                        allocated,
                        GC.CollectionCount(0) - gen0Before,
                        shape.VisualCount);
                }
            }
            else
            {
                stablePasses  = 0;
                previousStats = currentStats;
            }
        }

        throw new TimeoutException(route is null
            ? $"route {showCase.RouteTypeName} was not found"
            : $"route {showCase.RouteTypeName} did not stabilize");
    }

    private static GalleryAuditShape CollectShape(Control route)
    {
        var counts    = new Dictionary<string, int>(StringComparer.Ordinal);
        var visuals   = 0;
        var templated = 0;
        var maxDepth  = 0;
        var stack     = new Stack<(Avalonia.Visual Visual, int Depth)>();
        stack.Push((route, 0));
        while (stack.Count > 0)
        {
            var (visual, depth) = stack.Pop();
            visuals++;
            maxDepth = Math.Max(maxDepth, depth);
            if (visual is TemplatedControl)
            {
                templated++;
            }

            var name = visual.GetType().Name;
            counts[name] = counts.GetValueOrDefault(name) + 1;
            foreach (var child in visual.GetVisualChildren())
            {
                stack.Push((child, depth + 1));
            }
        }

        var logical = 0;
        var logicalStack = new Stack<Avalonia.LogicalTree.ILogical>();
        logicalStack.Push(route);
        while (logicalStack.Count > 0)
        {
            var current = logicalStack.Pop();
            logical++;
            foreach (var child in current.LogicalChildren)
            {
                logicalStack.Push(child);
            }
        }

        var top = counts.OrderByDescending(pair => pair.Value).Take(25).ToList();
        return new GalleryAuditShape(visuals, logical, templated, maxDepth, top);
    }

    private static string RenderMarkdown(IReadOnlyList<Dictionary<string, object?>> rows,
                                         string label,
                                         int iterations,
                                         int warmup)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Gallery ShowCase audit - {label}");
        builder.AppendLine();
        builder.AppendLine($"- Timestamp: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine($"- Headless, {Program.WindowSize.Width:0}x{Program.WindowSize.Height:0} window, one process for all showcases");
        builder.AppendLine($"- Warmup: {warmup}, measured iterations: {iterations}");
        builder.AppendLine("- Work ms: navigation trigger plus layout pumps until the route shape is stable; readiness statistics are excluded");
        builder.AppendLine("- First ms: first visit in this process, so it depends on showcase order");
        builder.AppendLine(GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled
            ? "- Materialization: full, deferred ShowCaseItem loading disabled"
            : "- Materialization: Gallery default, deferred ShowCaseItem loading fills the viewport only");
        builder.AppendLine();
        builder.AppendLine("| # | ShowCase | Visuals | Templated | First ms | Rep min ms | Rep median ms | Rep P95 ms | us/visual (min) | Alloc KB | KB/visual |");
        builder.AppendLine("| ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var row in rows)
        {
            if (row.TryGetValue("error", out var error))
            {
                builder.AppendLine($"| {row["order"]} | {row["key"]} | failed: {error} | | | | | | | | |");
                continue;
            }

            var visuals = Convert.ToDouble(row["visuals"], CultureInfo.InvariantCulture);
            var min     = Convert.ToDouble(row["repMinMs"], CultureInfo.InvariantCulture);
            var alloc   = Convert.ToDouble(row["repAllocKb"], CultureInfo.InvariantCulture);
            builder.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"| {row["order"]} | {row["key"]} | {visuals:0} | {row["templated"]} | {row["firstMs"]} | {row["repMinMs"]} | {row["repMedianMs"]} | {row["repP95Ms"]} | {min * 1000 / Math.Max(1, visuals):0.0} | {alloc:0} | {alloc / Math.Max(1, visuals):0.0} |"));
        }

        return builder.ToString();
    }

    private static double ToMs(long ticks)
    {
        return ticks * 1000d / Stopwatch.Frequency;
    }

    private static double Round(double value)
    {
        return Math.Round(value, 2);
    }

    private static double Median(IEnumerable<double> values)
    {
        return Percentile(values.Order().ToArray(), 0.5);
    }

    private static double Percentile(IReadOnlyList<double> ordered, double percentile)
    {
        if (ordered.Count == 0)
        {
            return 0;
        }

        var position = (ordered.Count - 1) * percentile;
        var lower    = (int)Math.Floor(position);
        var upper    = (int)Math.Ceiling(position);
        return ordered[lower] + (ordered[upper] - ordered[lower]) * (position - lower);
    }
}
