using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Rendering;

namespace AtomUI.Utils;

/// <summary>
/// 按 DWM 合成节拍唤醒的渲染循环计时器：在专用线程上调用 <c>DwmFlush()</c>，每次 DWM 完成一次合成后出一个节拍。
/// </summary>
/// <remarks>
/// 用于替换 Avalonia 在 RedirectionSurface 合成模式下使用的 SleepLoopRenderTimer。睡眠计时器不跟随显示器刷新，
/// 默认计时精度下还会多睡一帧。节拍必须落在 DWM 合成完成之后：锁在垂直同步时刻（DXGI WaitForVBlank、合成器时钟）时，
/// 应用的呈现与 DWM 开始合成几乎同时发生，部分帧会晚一个刷新周期上屏。<c>DwmFlush()</c> 返回的时刻天然避开这一竞争，
/// 并且跟随 DWM 的实际合成频率，不需要提高进程计时精度。实测依据见 docs/architecture/systems/windowing/windows-live-resize.md。
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class DwmVsyncRenderTimer : IRenderTimer
{
    private static readonly TimeSpan FallbackInterval = TimeSpan.FromMilliseconds(16);
    private static readonly long MinimumTickSpacing = Stopwatch.Frequency / 1000;

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly AutoResetEvent _attached = new(false);
    private volatile Action<TimeSpan>? _tick;
    private int _threadStarted;
    private long _lastTickStart;

    /// <summary>
    /// 渲染循环有内容要渲染时挂上处理器，空闲时置空。置空后线程停在事件上等待，不再调用 <c>DwmFlush()</c>。
    /// </summary>
    public Action<TimeSpan>? Tick
    {
        get => _tick;
        set
        {
            _tick = value;
            if (value is null)
            {
                return;
            }
            if (Interlocked.Exchange(ref _threadStarted, 1) == 0)
            {
                new Thread(Run) { IsBackground = true, Name = "AtomUI DWM vsync render timer" }.Start();
            }
            else
            {
                _attached.Set();
            }
        }
    }

    public bool RunsInBackground => true;

    private void Run()
    {
        while (true)
        {
            if (_tick is null)
            {
                _attached.WaitOne();
                continue;
            }
            WaitForComposition();
            // 等待期间可能已被置空，重新读取。
            var tick = _tick;
            if (tick is null)
            {
                continue;
            }
            _lastTickStart = Stopwatch.GetTimestamp();
            tick(_clock.Elapsed);
        }
    }

    private void WaitForComposition()
    {
        var result = DwmFlush();
        // DWM 没有在合成时（例如会话断开）DwmFlush 会失败或立即返回。距上个节拍开始不足 1 ms 说明它没有等待，
        // 此时按固定间隔睡眠，避免空转。渲染耗时接近一个刷新周期后紧接着的快速返回不属于这种情况。
        if (result < 0 || Stopwatch.GetTimestamp() - _lastTickStart < MinimumTickSpacing)
        {
            Thread.Sleep(FallbackInterval);
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmFlush();
}
