using System.Diagnostics.Tracing;
using System.Runtime;

namespace AtomUI.StartupPerformance;

/// <summary>进程内订阅 AtomUI-Startup 事件，把分段与里程碑写入 StartupTimeline。</summary>
internal sealed class StartupEventListener : EventListener
{
    // 分段起止在同一线程发出，按线程累计的 JIT 耗时与方法数之差即该分段自身的 JIT 成本。
    private readonly Dictionary<string, (long Start, TimeSpan Jit, long Methods)> _phaseStarts = new(StringComparer.Ordinal);
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
                var started = (now, JitInfo.GetCompilationTime(currentThread: true),
                    JitInfo.GetCompiledMethodCount(currentThread: true));
                lock (_phaseStarts)
                {
                    _phaseStarts[(string)eventData.Payload![0]!] = started;
                }
                break;
            case "PhaseStop":
                var phase = (string)eventData.Payload![0]!;
                var jit = JitInfo.GetCompilationTime(currentThread: true);
                var methods = JitInfo.GetCompiledMethodCount(currentThread: true);
                (long Start, TimeSpan Jit, long Methods) start;
                lock (_phaseStarts)
                {
                    if (!_phaseStarts.Remove(phase, out start))
                    {
                        return;
                    }
                }
                StartupTimeline.Duration("Phase." + phase, start.Start, now);
                StartupTimeline.Set("Jit.Phase." + phase, (jit - start.Jit).TotalMilliseconds);
                StartupTimeline.Set("JitMethods.Phase." + phase, methods - start.Methods);
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
