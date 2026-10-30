using System.Diagnostics;
using AtomUI.Reflection;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;

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
    }

    internal void Stop()
    {
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
        var sampleTime = ToPresentationTime(time, out var target);
        if (_lastSample is { } last && sampleTime < last)
        {
            sampleTime = last;
        }
        _start ??= sampleTime;
        var local = sampleTime - _start.Value - _startSlip;
        var finished = local >= _total;
        if (finished || _lastSample != sampleTime)
        {
            if (target > _lastTarget && MotionFrameTiming.Clock is { } clock)
            {
                _lastTarget = target;
                _startedAtCommit = clock.FramesStarted;
            }
            _sample(finished ? _total : local);
        }
        _lastSample = sampleTime;
        if (finished)
        {
            Stop();
            return;
        }
        // A pulse that changes nothing (delay, unchanged values) requests no commit, and MediaContext then
        // reschedules through a 16ms DispatcherTimer that is not aligned with the display refresh.
        // An empty commit keeps the next pulse on the compositor's own pacing.
        ElementComposition.GetElementVisual(_target)?.Compositor.RequestCompositionBatchCommitAsync();
    }

    private TimeSpan ToPresentationTime(TimeSpan time, out long target)
    {
        target = 0;
        if (MotionFrameTiming.Clock is not { } clock)
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
            var taken = clock.FramesStarted != _startedAtCommit;
            target = Math.Max(target, taken ? _lastTarget + clock.RefreshInterval : _lastTarget);
            if (taken && !_advanced)
            {
                // An idle renderer shows the first frame at once and starts the next before the UI can follow,
                // so frames beyond the first repeat the start state. Slip the start by those frames instead of
                // jumping ahead on the next sample; later stale frames are not absorbed, so completion cannot drift.
                _advanced = true;
                var stale = clock.FramesStarted - _startedAtCommit - 1;
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
