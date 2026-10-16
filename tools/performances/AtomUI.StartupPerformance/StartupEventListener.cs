using System.Diagnostics.Tracing;

namespace AtomUI.StartupPerformance;

/// <summary>进程内订阅 AtomUI-Startup 事件，把分段与里程碑写入 StartupTimeline。</summary>
internal sealed class StartupEventListener : EventListener
{
    private readonly Dictionary<string, long> _phaseStarts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _materializations = new(StringComparer.Ordinal);

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "AtomUI-Startup")
        {
            EnableEvents(eventSource, EventLevel.Verbose);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        var now = StartupTimeline.Now();
        switch (eventData.EventName)
        {
            case "PhaseStart":
                lock (_phaseStarts)
                {
                    _phaseStarts[(string)eventData.Payload![0]!] = now;
                }
                break;
            case "PhaseStop":
                var phase = (string)eventData.Payload![0]!;
                long start;
                lock (_phaseStarts)
                {
                    if (!_phaseStarts.Remove(phase, out start))
                    {
                        return;
                    }
                }
                StartupTimeline.Duration("Phase." + phase, start, now);
                break;
            case "SplashFirstFrame":
                StartupTimeline.Mark("Mark.SplashFirstFrame");
                break;
            case "MainWindowFirstFrame":
                StartupTimeline.Mark("Mark.MainWindowFirstFrameEvent");
                break;
            case "StateChanged":
                StartupTimeline.Mark("Mark.State." + (string)eventData.Payload![0]!);
                break;
            case "PackageTiming":
                StartupTimeline.Set("Package." + (string)eventData.Payload![0]! + "." + (string)eventData.Payload[1]!,
                    (double)eventData.Payload[2]!);
                break;
            case "FragmentMaterialized":
            case "ThemeAssetMaterialized":
                var category = eventData.EventName == "FragmentMaterialized" ? "Fragments" : "ThemeAssets";
                var key = "Count." + category + "." + (string)eventData.Payload![0]!;
                lock (_materializations)
                {
                    _materializations.TryGetValue(key, out var count);
                    _materializations[key] = ++count;
                    StartupTimeline.Set(key, count);
                }
                break;
        }
    }
}
