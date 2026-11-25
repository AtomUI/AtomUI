using System.Diagnostics;
using AtomUI.Reflection;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace AtomUI.Animations;

/// <summary>
/// Samples one playback on every Avalonia render pulse, without Animation, keyframes or a property per frame.
/// </summary>
/// <remarks>
/// When the platform reports presentation timing, each pulse is sampled at the time its frame is expected to be
/// shown instead of the pulse time. The target advances by one refresh only once the renderer has taken (or is
/// free to take) the content committed for the previous target; until then pulses write nothing. This keeps one
/// distinct sample per presented frame even when pulses run faster or slower than the display.
/// </remarks>
internal sealed class MotionFrameDriver : IObserver<TimeSpan>
{
    private readonly Control _target;
    private readonly TimeSpan _total;
    private readonly Action<TimeSpan> _sample;
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IDisposable? _subscription;
    private IMotionPresentationClock? _presentationClock;
    private Action? _requestFramePulse;
    private DispatcherTimer? _framePulseTimer;
    private int _framePulseQueued;
    private TimeSpan? _start;
    private TimeSpan? _lastSample;
    private long _lastTarget;
    private long _startedAtCommit;
    private bool _advanced;
    private TimeSpan _startSlip;

    internal MotionFrameDriver(Control target, TimeSpan total, Action<TimeSpan> sample)
    {
        _target = target;
        _total = total;
        _sample = sample;
    }

    internal Task Completion => _completion.Task;

    internal void Start()
    {
        // Like an Avalonia animation, local time starts at the first pulse delivered to this subscription.
        _subscription = GlobalClockReflection.GetGlobalClock().Subscribe(this);
        if (MotionFrameTiming.Clock is { RequiresInitialCompositorPulse: true } clock)
        {
            SetPresentationClock(clock);
            RequestCompositorPulse();
        }
    }

    internal void Stop()
    {
        SetPresentationClock(null);
        var subscription = _subscription;
        _subscription = null;
        subscription?.Dispose();
        _completion.TrySetResult();
    }

    public void OnNext(TimeSpan time)
    {
        if (_completion.Task.IsCompleted)
        {
            return;
        }
        var clock = MotionFrameTiming.Clock;
        if (!ReferenceEquals(_presentationClock, clock))
        {
            SetPresentationClock(clock);
        }
        var framesStarted = clock?.FramesStarted ?? 0;
        var sampleTime = ToPresentationTime(time, clock, framesStarted, out var target);
        // The Avalonia pulse and Stopwatch are read at different times. Reusing a prediction must reuse
        // its sampled value even when the difference between those reads changes slightly.
        if (_lastSample is { } last && (sampleTime < last || (target > 0 && target == _lastTarget)))
        {
            sampleTime = last;
        }
        _start ??= sampleTime;
        var local = sampleTime - _start.Value - _startSlip;
        var finished = local >= _total;
        // A prepared frame can hold the worker across several refreshes. Keep the pending sample
        // until a draw can use it, but let completion settle even if drawing stops.
        var waitingForDraw = target > 0 && _lastSample is not null &&
                             framesStarted == _startedAtCommit && clock is { FramesInFlight: > 0 };
        if (finished || (!waitingForDraw && _lastSample != sampleTime))
        {
            if (target > _lastTarget && clock is not null)
            {
                _lastTarget = target;
                _startedAtCommit = framesStarted;
            }
            try
            {
                _sample(finished ? _total : local);
                _lastSample = sampleTime;
            }
            catch (Exception error)
            {
                _completion.TrySetException(error);
                Stop();
                return;
            }
        }
        if (finished)
        {
            Stop();
        }
        if (_completion.Task.IsCompleted)
        {
            return;
        }
        // A presentation clock wakes the next pulse when the renderer takes a frame. Requesting an empty
        // batch here would wake the UI again as soon as that batch is processed, even while the display waits.
        if (clock is null)
        {
            RequestCompositorPulse();
        }
        else if (clock.RefreshInterval is > 0 and var refresh && _framePulseTimer is not { IsEnabled: true })
        {
            // A motion may start or wait through its delay without changing pixels. In that case no draw
            // will wake it, so arm one fallback pulse at the measured refresh rate.
            if (_framePulseTimer is null)
            {
                _framePulseTimer = new DispatcherTimer(DispatcherPriority.Render);
                _framePulseTimer.Tick += FramePulseTimedOut;
            }
            _framePulseTimer.Interval = Stopwatch.GetElapsedTime(0, refresh);
            _framePulseTimer.Start();
        }
    }

    private void SetPresentationClock(IMotionPresentationClock? clock)
    {
        _framePulseTimer?.Stop();
        if (_presentationClock is not null)
        {
            _presentationClock.FrameStarted -= FrameStarted;
        }
        _presentationClock = clock;
        if (clock is not null)
        {
            _requestFramePulse ??= RequestFramePulse;
            clock.FrameStarted += FrameStarted;
        }
    }

    private void FrameStarted()
    {
        if (Interlocked.Exchange(ref _framePulseQueued, 1) == 0)
        {
            Dispatcher.UIThread.Post(_requestFramePulse!, DispatcherPriority.Render);
        }
    }

    private void RequestFramePulse()
    {
        Interlocked.Exchange(ref _framePulseQueued, 0);
        if (_presentationClock is not null)
        {
            _framePulseTimer?.Stop();
            RequestAnimationPulse();
        }
    }

    private void FramePulseTimedOut(object? sender, EventArgs args)
    {
        var timer = (DispatcherTimer)sender!;
        timer.Stop();
        RequestAnimationPulse();
    }

    private void RequestAnimationPulse() =>
        TopLevel.GetTopLevel(_target)?.RequestAnimationFrame(static _ => { });

    private void RequestCompositorPulse() =>
        ElementComposition.GetElementVisual(_target)?.Compositor.RequestCompositionBatchCommitAsync();

    private TimeSpan ToPresentationTime(TimeSpan time, IMotionPresentationClock? clock, long framesStarted, out long target)
    {
        target = 0;
        if (clock is null)
        {
            return time;
        }
        var now = Stopwatch.GetTimestamp();
        if (clock.PredictPresentation(now) is not { } earliest || clock.RefreshInterval <= 0)
        {
            return time;
        }
        target = earliest;
        if (_lastTarget > 0)
        {
            // Only a frame started after the last commit is known to carry it. Pulses can outnumber frames, so an
            // idle renderer is not evidence that the previous target was consumed.
            var taken = framesStarted != _startedAtCommit;
            target = Math.Max(target, taken ? _lastTarget + clock.RefreshInterval : _lastTarget);
            if (taken && !_advanced)
            {
                // An idle renderer shows the first frame at once and starts the next before the UI can follow,
                // so frames beyond the first repeat the start state. Slip the start by those frames instead of
                // jumping ahead on the next sample; later stale frames are not absorbed, so completion cannot drift.
                _advanced = true;
                var stale = framesStarted - _startedAtCommit - 1;
                if (stale > 0)
                {
                    _startSlip = Stopwatch.GetElapsedTime(0, Math.Min(stale, 2) * clock.RefreshInterval);
                }
            }
        }
        return target > now ? time + Stopwatch.GetElapsedTime(now, target) : time;
    }

    public void OnError(Exception error) => Stop();

    public void OnCompleted() => Stop();
}
