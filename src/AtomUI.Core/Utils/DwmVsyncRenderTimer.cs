using System.Diagnostics;
using System.Runtime.Versioning;
using AtomUI.Animations;
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
internal sealed class DwmVsyncRenderTimer : IRenderTimer, IMotionPresentationClock
{
    private static readonly TimeSpan FallbackInterval = TimeSpan.FromMilliseconds(16);
    private static readonly long MinimumTickSpacing = Stopwatch.Frequency / 1000;

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly AutoResetEvent _attached = new(false);
    private readonly object _timingLock = new();
    private volatile Action<TimeSpan>? _tick;
    private long _verticalBlank;
    private long _refreshInterval;
    private long _framesStarted;
    private int _framesInFlight;
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

    public event Action? FrameStarted;

    public long RefreshInterval => Volatile.Read(ref _refreshInterval);
    public long TransitionSamplingInterval => 0;
    public long FramesStarted => Volatile.Read(ref _framesStarted);
    public int FramesInFlight => Volatile.Read(ref _framesInFlight);

    public bool RequiresInitialCompositorPulse => true;

    public long? PredictPresentation(long now)
    {
        lock (_timingLock)
        {
            if (_refreshInterval <= 0)
            {
                return null;
            }

            return now < _verticalBlank
                ? _verticalBlank
                : _verticalBlank + ((now - _verticalBlank) / _refreshInterval + 1) * _refreshInterval;
        }
    }

    private void Run()
    {
        while (true)
        {
            if (_tick is null)
            {
                _attached.WaitOne();
                continue;
            }
            var synchronized = WaitForComposition();
            // 等待期间可能已被置空，重新读取。
            var tick = _tick;
            if (tick is null)
            {
                continue;
            }
            _lastTickStart = Stopwatch.GetTimestamp();
            UpdatePresentationTiming(synchronized);
            Interlocked.Increment(ref _framesStarted);
            Volatile.Write(ref _framesInFlight, 1);
            try
            {
                FrameStarted?.Invoke();
                tick(_clock.Elapsed);
            }
            finally
            {
                Volatile.Write(ref _framesInFlight, 0);
            }
        }
    }

    private bool WaitForComposition()
    {
        var result = DwmCompositionInterop.Flush();
        // DWM 没有在合成时（例如会话断开）DwmFlush 会失败或立即返回。距上个节拍开始不足 1 ms 说明它没有等待，
        // 此时按固定间隔睡眠，避免空转。渲染耗时接近一个刷新周期后紧接着的快速返回不属于这种情况。
        if (result < 0 || Stopwatch.GetTimestamp() - _lastTickStart < MinimumTickSpacing)
        {
            Thread.Sleep(FallbackInterval);
            return false;
        }
        return true;
    }

    private void UpdatePresentationTiming(bool synchronized)
    {
        long refreshPeriod = 0;
        long verticalBlank = 0;
        var reliable = synchronized &&
                       DwmCompositionInterop.TryGetSingleMonitorTiming(out refreshPeriod, out verticalBlank) &&
                       verticalBlank > 0 &&
                       refreshPeriod >= MinimumTickSpacing &&
                       refreshPeriod <= Stopwatch.Frequency / 10 &&
                       Math.Abs(verticalBlank - _lastTickStart) <= 2 * refreshPeriod;

        lock (_timingLock)
        {
            _verticalBlank = verticalBlank;
            Volatile.Write(ref _refreshInterval, reliable ? refreshPeriod : 0);
        }

        if (reliable)
        {
            if (!ReferenceEquals(MotionFrameTiming.Clock, this))
            {
                MotionFrameTiming.Clock = this;
            }
        }
        else if (ReferenceEquals(MotionFrameTiming.Clock, this))
        {
            MotionFrameTiming.Clock = null;
        }
    }
}
