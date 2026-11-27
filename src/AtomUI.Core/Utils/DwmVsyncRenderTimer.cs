using System.Diagnostics;
using System.Runtime.Versioning;
using AtomUI.Animations;
using Avalonia.Rendering;

namespace AtomUI.Utils;

/// <summary>
/// A render loop timer that wakes on the DWM composition cadence: a dedicated thread calls <c>DwmFlush()</c>
/// and emits one tick after each completed DWM composition.
/// </summary>
/// <remarks>
/// Replaces the <c>SleepLoopRenderTimer</c> that Avalonia uses in RedirectionSurface composition mode. A sleep
/// timer does not follow the display refresh and, at the default timer resolution, sleeps one frame too long.
/// The tick must fall after the DWM has finished composing. When the tick is locked to the vertical blank (DXGI
/// <c>WaitForVBlank</c>, the compositor clock), the application presents at nearly the same moment as the DWM
/// starts composing, and some frames reach the screen one refresh period late. The moment <c>DwmFlush()</c>
/// returns inherently avoids that race, and it follows the actual DWM composition rate, so the process timer
/// resolution does not need to be raised.
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
    /// The render loop attaches its handler when it has content to render and sets it to <see langword="null" />
    /// when idle. While it is <see langword="null" />, the timer thread waits on its event and does not call
    /// <c>DwmFlush()</c> again.
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
            // The handler may have been cleared while the composition was awaited, so read the field again.
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
        // When the DWM is not composing, for example in a disconnected session, DwmFlush fails or returns at once.
        // Less than 1 ms since the last tick started means that it did not wait, so the thread sleeps for a fixed
        // interval instead of spinning. A quick return right after rendering took nearly one refresh period is not
        // such a case.
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
