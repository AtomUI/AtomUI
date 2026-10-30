using System.Diagnostics;
using System.Globalization;
using System.Text;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.GalleryPerformance;

/// <summary>
/// Persisted headless probe for the manager/card/template/layout paths that Gallery navigation does not exercise.
/// Every timed item is an aggregate of 24 shows, 20 collapse/expand cycles, or 24 normal closes.
/// </summary>
internal static class FeedbackStackPerformanceProbe
{
    private const int ItemCount = 24;
    private const int ToggleCount = 20;

    internal static string Run(int iterations, int warmup, string label)
    {
        for (var i = 0; i < warmup; i++)
        {
            _ = MeasureMessage();
            _ = MeasureNotification();
        }

        var messageSamples = new FeedbackProbeSample[iterations];
        var notificationSamples = new FeedbackProbeSample[iterations];
        for (var i = 0; i < iterations; i++)
        {
            messageSamples[i] = MeasureMessage();
            notificationSamples[i] = MeasureNotification();
        }

        var builder = new StringBuilder();
        builder.AppendLine($"# Feedback stack performance - {label}");
        builder.AppendLine();
        builder.AppendLine($"- Timestamp: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine("- Runtime: Debug, Avalonia headless, motion disabled");
        builder.AppendLine($"- Samples: {iterations} measured after {warmup} warmups");
        builder.AppendLine($"- Work per sample: show/destroy {ItemCount} cards; toggle stack {ToggleCount} collapse/expand cycles");
        builder.AppendLine("- Permanent cards are used so the probe measures manager/template/layout work without wall-clock timer noise");
        builder.AppendLine();
        builder.AppendLine("| Control | Operation | Mean ms | Median ms | P95 ms | Mean allocated KB | Median allocated KB | P95 allocated KB |");
        builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |");
        AppendRows(builder, "Message", messageSamples);
        AppendRows(builder, "Notification", notificationSamples);
        return builder.ToString();
    }

    private static FeedbackProbeSample MeasureMessage()
    {
        using var manager = new WindowMessageManager(null)
        {
            IsMotionEnabled = false,
            IsStackEnabled = true,
            MaxItems = 0
        };
        var window = CreateWindow(manager);
        try
        {
            var show = Measure(() =>
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    manager.Show(new Message($"Message {i}", expiration: TimeSpan.Zero));
                }
                Pump(window);
            });

            var visibleCount = manager.GetVisualDescendants()
                                      .OfType<MessageCard>()
                                      .Count(card => card.IsHitTestVisible);
            if (visibleCount != 1)
            {
                throw new InvalidOperationException($"Message collapsed projection expected 1 visible card, actual {visibleCount}.");
            }

            var toggle = Measure(() =>
            {
                for (var i = 0; i < ToggleCount; i++)
                {
                    manager.IsStackEnabled = false;
                    Pump(window);
                    manager.IsStackEnabled = true;
                    Pump(window);
                }
            });
            var destroy = Measure(() =>
            {
                manager.DestroyAll();
                Pump(window);
            });
            EnsureDestroyed<MessageCard>(manager);
            return new FeedbackProbeSample(show, toggle, destroy);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static FeedbackProbeSample MeasureNotification()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false,
            IsStackEnabled = true,
            MaxItems = 0
        };
        var window = CreateWindow(manager);
        try
        {
            var show = Measure(() =>
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    manager.Show(new Notification(
                        $"Notification {i}",
                        "Content",
                        expiration: TimeSpan.Zero));
                }
                Pump(window);
            });

            var visibleCount = manager.GetVisualDescendants()
                                      .OfType<NotificationCard>()
                                      .Count(card => card.IsHitTestVisible);
            if (visibleCount != 3)
            {
                throw new InvalidOperationException($"Notification collapsed projection expected 3 visible cards, actual {visibleCount}.");
            }

            var toggle = Measure(() =>
            {
                for (var i = 0; i < ToggleCount; i++)
                {
                    manager.IsStackEnabled = false;
                    Pump(window);
                    manager.IsStackEnabled = true;
                    Pump(window);
                }
            });
            var destroy = Measure(() =>
            {
                manager.DestroyAll();
                Pump(window);
            });
            EnsureDestroyed<NotificationCard>(manager);
            return new FeedbackProbeSample(show, toggle, destroy);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 900,
            Height = 900,
            ShowInTaskbar = false,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        Pump(window);
        return window;
    }

    private static void Pump(AvaloniaWindow window)
    {
        for (var i = 0; i < 2; i++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static ProbeMeasurement Measure(Action action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return new ProbeMeasurement(
            stopwatch.Elapsed.TotalMilliseconds,
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore);
    }

    private static void EnsureDestroyed<TCard>(Control manager) where TCard : Control
    {
        var remainingCards = manager.GetVisualDescendants().OfType<TCard>().ToArray();
        if (remainingCards.Length > 0)
        {
            var motionEnabled = remainingCards.Count(card =>
                card is IMotionAwareControl motionAware && motionAware.IsMotionEnabled);
            var closing = remainingCards.Count(card => card switch
            {
                MessageCard messageCard => messageCard.IsClosing,
                NotificationCard notificationCard => notificationCard.IsClosing,
                _ => false
            });
            var closed = remainingCards.Count(card => card switch
            {
                MessageCard messageCard => messageCard.IsClosed,
                NotificationCard notificationCard => notificationCard.IsClosed,
                _ => false
            });
            throw new InvalidOperationException(
                $"{remainingCards.Length} {typeof(TCard).Name} visuals remained after DestroyAll; " +
                $"attached={remainingCards.Count(card => card.IsAttachedToVisualTree())}, " +
                $"motionEnabled={motionEnabled}, closing={closing}, closed={closed}.");
        }
    }

    private static void AppendRows(StringBuilder builder, string control, FeedbackProbeSample[] samples)
    {
        AppendRow(builder, control, "Show 24", samples.Select(sample => sample.Show).ToArray());
        AppendRow(builder, control, "Toggle 20x", samples.Select(sample => sample.Toggle).ToArray());
        AppendRow(builder, control, "Destroy 24", samples.Select(sample => sample.Destroy).ToArray());
    }

    private static void AppendRow(StringBuilder builder,
                                  string control,
                                  string operation,
                                  ProbeMeasurement[] measurements)
    {
        var times = measurements.Select(value => value.ElapsedMilliseconds).Order().ToArray();
        var allocations = measurements.Select(value => (double)value.AllocatedBytes / 1024).Order().ToArray();
        builder.AppendLine(
            $"| {control} | {operation} | {Format(times.Average())} | {Format(Percentile(times, 0.5))} | {Format(Percentile(times, 0.95))} | {Format(allocations.Average())} | {Format(Percentile(allocations, 0.5))} | {Format(Percentile(allocations, 0.95))} |");
    }

    private static double Percentile(double[] sortedValues, double percentile)
    {
        var index = (int)Math.Ceiling(percentile * sortedValues.Length) - 1;
        return sortedValues[Math.Clamp(index, 0, sortedValues.Length - 1)];
    }

    private static string Format(double value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private readonly record struct FeedbackProbeSample(
        ProbeMeasurement Show,
        ProbeMeasurement Toggle,
        ProbeMeasurement Destroy);

    private readonly record struct ProbeMeasurement(double ElapsedMilliseconds, long AllocatedBytes);
}
