using System.Diagnostics;
using System.Globalization;

namespace AtomUI.StartupPerformance;

/// <summary>里程碑以 Main 为零点（ms）；Epoch.MainUnixMicroseconds 供采样脚本换算自启动起算的耗时。</summary>
internal static class StartupTimeline
{
    private static readonly double TickToMs = 1000.0 / Stopwatch.Frequency;
    private static readonly object Gate = new();
    private static readonly List<KeyValuePair<string, double>> Values = new();
    private static long _main;
    private static long _mainUnixMicroseconds;

    internal static void MarkMain()
    {
        _main = Stopwatch.GetTimestamp();
        _mainUnixMicroseconds = (DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) / 10;
    }

    internal static long Now() => Stopwatch.GetTimestamp();

    internal static double ToMilliseconds(long elapsedTicks) => elapsedTicks * TickToMs;

    internal static void Mark(string key) => Set(key, ToMilliseconds(Stopwatch.GetTimestamp() - _main));

    internal static void Duration(string key, long start, long end) => Set(key, ToMilliseconds(end - start));

    internal static void Flag(string name) => Set($"Flag.{name}", 1);

    /// <summary>记录调用线程截至此刻的累计 JIT 编译耗时（ms），用于区分 JIT 与实际工作。</summary>
    internal static void ThreadJit(string key) =>
        Set($"Jit.Thread.{key}", System.Runtime.JitInfo.GetCompilationTime(currentThread: true).TotalMilliseconds);

    internal static void Set(string key, double valueMs)
    {
        lock (Gate)
        {
            Values.Add(new KeyValuePair<string, double>(key, valueMs));
        }
    }

    internal static void Report()
    {
        var output = Console.Out;
        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"PERF|Epoch.MainUnixMicroseconds={_mainUnixMicroseconds}"));
        lock (Gate)
        {
            foreach (var (key, value) in Values)
            {
                output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"PERF|{key}={value:F3}"));
            }
        }
        output.Flush();
    }
}
