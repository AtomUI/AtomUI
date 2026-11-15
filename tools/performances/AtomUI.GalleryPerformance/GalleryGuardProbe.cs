using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.GalleryPerformance;

/// <summary>
/// Regression guards for performance work: full-height page snapshots for before/after pixel comparison, and
/// page collectability after navigating away. Neither guard measures time.
/// </summary>
internal static class GalleryGuardProbe
{
    // Bounds the capture surface; taller pages are clipped at this height in both builds alike.
    private const double MaxSnapshotHeight = 12000;

    public static int RunSnapshots(WorkspaceWindow window,
                                   IReadOnlyList<string> keys,
                                   TimeSpan timeout,
                                   string directory)
    {
        Directory.CreateDirectory(directory);
        var failures = 0;
        foreach (var key in keys)
        {
            if (!Program.ShowCases.TryGetValue(key, out var showCase))
            {
                Console.WriteLine($"{key}: unknown showcase");
                failures++;
                continue;
            }

            try
            {
                Program.TriggerNavigation(window, showCase);
                var route = WaitForStableRoute(window, showCase, timeout);
                var height = ResolveFullHeight(window, route);
                LayoutAt(window, height);
                LayoutAt(window, height);
                Settle(window);

                using var frame = window.CaptureRenderedFrame()
                                  ?? throw new InvalidOperationException("No rendered frame was captured.");
                var path = Path.Combine(directory, key + ".png");
                frame.Save(path, PngBitmapEncoderOptions.Default);
                Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                    $"{key}: {Program.WindowSize.Width:0}x{height:0} -> {path}"));
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine($"{key}: FAILED {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                LayoutAt(window, Program.WindowSize.Height);
                Program.NavigateToOverview(window, timeout);
            }
        }

        return failures == 0 ? 0 : 2;
    }

    public static int RunLeakCheck(WorkspaceWindow window,
                                   IReadOnlyList<string> keys,
                                   TimeSpan timeout,
                                   int visits,
                                   string? reportPath)
    {
        var report = new StringBuilder();
        report.AppendLine("| ShowCase | Visits | Alive after GC |");
        report.AppendLine("| --- | ---: | ---: |");
        var leaking = 0;
        foreach (var key in keys)
        {
            if (!Program.ShowCases.TryGetValue(key, out var showCase))
            {
                continue;
            }

            try
            {
                var references = VisitAndRelease(window, showCase, timeout, visits);
                var alive      = CountAliveAfterCollection(window, references);
                if (alive > 0)
                {
                    leaking++;
                }

                var line = $"| {key} | {visits} | {alive} |";
                report.AppendLine(line);
                Console.WriteLine($"{key}: visits={visits} alive={alive}");
            }
            catch (Exception ex)
            {
                leaking++;
                report.AppendLine($"| {key} | {visits} | failed: {ex.GetType().Name} |");
                Console.WriteLine($"{key}: FAILED {ex.GetType().Name}: {ex.Message}");
            }
        }

        if (!string.IsNullOrWhiteSpace(reportPath))
        {
            File.WriteAllText(Path.GetFullPath(reportPath), report.ToString());
        }

        Console.WriteLine($"Pages with surviving instances: {leaking}/{keys.Count}");
        return 0;
    }

    // Kept out of line so no page reference survives in this frame's locals when the caller collects.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static List<WeakReference> VisitAndRelease(WorkspaceWindow window,
                                                       ShowCaseSpec showCase,
                                                       TimeSpan timeout,
                                                       int visits)
    {
        var references = new List<WeakReference>(visits);
        for (var i = 0; i < visits; i++)
        {
            Program.TriggerNavigation(window, showCase);
            references.Add(new WeakReference(WaitForStableRoute(window, showCase, timeout)));
            Program.NavigateToOverview(window, timeout);
        }

        return references;
    }

    private static int CountAliveAfterCollection(WorkspaceWindow window, IReadOnlyList<WeakReference> references)
    {
        // Several rounds let dispatcher-posted cleanup and finalizers run before references are judged.
        for (var round = 0; round < 4; round++)
        {
            Program.PumpLayout(window);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(2);
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return references.Count(reference => reference.IsAlive);
    }

    private static Control WaitForStableRoute(WorkspaceWindow window, ShowCaseSpec showCase, TimeSpan timeout)
    {
        var stopwatch     = Stopwatch.StartNew();
        var stablePasses  = 0;
        var previousStats = default(RouteStats);
        while (stopwatch.Elapsed < timeout)
        {
            Program.PumpLayout(window);
            var route = window.GetSelfAndVisualDescendants()
                              .OfType<Control>()
                              .FirstOrDefault(control => control.GetType().FullName == showCase.RouteTypeName);
            if (route is null || !route.IsVisible || route.Bounds.Width <= 0 || route.Bounds.Height <= 0)
            {
                continue;
            }

            var stats = RouteStats.Collect(route);
            if (stats.VisualCount > 0 && previousStats is not null && stats.HasSameShape(previousStats))
            {
                if (++stablePasses >= 2)
                {
                    return route;
                }
            }
            else
            {
                stablePasses  = 0;
                previousStats = stats;
            }
        }

        throw new TimeoutException($"route {showCase.RouteTypeName} did not stabilize");
    }

    private static double ResolveFullHeight(WorkspaceWindow window, Control route)
    {
        // The page scrolls inside its own ScrollViewer; grow the window by the hidden part of the tallest one.
        var hidden = route.GetSelfAndVisualDescendants()
                          .OfType<ScrollViewer>()
                          .Select(viewer => viewer.Extent.Height - viewer.Viewport.Height)
                          .DefaultIfEmpty(0)
                          .Max();
        return Math.Min(MaxSnapshotHeight, Program.WindowSize.Height + Math.Max(0, Math.Ceiling(hidden)));
    }

    private static void LayoutAt(WorkspaceWindow window, double height)
    {
        var size = new Size(Program.WindowSize.Width, height);
        window.Height = height;
        Dispatcher.UIThread.RunJobs();
        window.Measure(size);
        window.Arrange(new Rect(size));
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static void Settle(WorkspaceWindow window)
    {
        for (var i = 0; i < 3; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(2);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
