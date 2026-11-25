using AtomUI.Reflection;
using Avalonia.Logging;
using Avalonia.Threading;

namespace AtomUI.Animations;

/// <summary>
/// Lands every animation started inside a settle window on its final value in the frame it started.
/// </summary>
/// <remarks>
/// Avalonia transitions, keyframe animations and AtomUI motion frame drivers all observe the render-loop clock,
/// and take their local time from the pulses that clock delivers. An observer that subscribed after the window
/// opened is sent the current frame time and then a time past any finite duration, which completes it there and
/// then. Settling runs right after the work that opened the window, again at the start of every frame the window
/// spans (before layout), and once more after that frame's layout through the clock's new-subscription pulse,
/// so a transition started by layout lands before the frame is committed. Animations that were running when the
/// window opened keep their own time. The window ends once the next rendered frame has been laid out and
/// committed; input handled before that frame is inside the window too.
/// </remarks>
internal sealed class MotionSettle
{
    // Longer than any finite transition or animation run; ClockBase consumers accumulate deltas, so an infinite
    // animation only sees one far sample before its time continues from the real clock.
    private static readonly TimeSpan SettleJump = TimeSpan.FromDays(1);
    private const int MaxCascade = 16;

    private int _depth;
    private IObservable<TimeSpan>? _clock;
    private HashSet<IObserver<TimeSpan>>? _known;
    private FrameProbe? _frameProbe;
    private List<IDisposable>? _pendingReleases;

    internal bool IsSettling => _depth > 0;

    /// <summary>Opens (or nests into) the settle window; the scope ends it.</summary>
    internal IDisposable Begin()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_depth++ == 0)
        {
            Open();
        }
        return new Scope(this);
    }

    /// <summary>Completes the animations started since the window opened.</summary>
    internal void SettleStarted()
    {
        if (_clock is { } clock)
        {
            Settle(GlobalClockReflection.GetCurrentTimestamp(clock));
        }
    }

    /// <summary>
    /// Disposes <paramref name="scope"/> once the next rendered frame has been laid out and committed. A job
    /// posted directly would run before that frame, since the render loop is driven by a timer rather than
    /// queued, so the release is posted from the frame's clock pulse at Loaded priority, right after the render
    /// job that pulse belongs to.
    /// </summary>
    internal void EndAfterNextFrame(IDisposable scope)
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_frameProbe is null)
        {
            scope.Dispose();
            return;
        }
        (_pendingReleases ??= []).Add(scope);
    }

    private void Open()
    {
        if (GlobalClockReflection.TryGetGlobalClock() is not { } clock)
        {
            return;
        }
        List<IObserver<TimeSpan>> observers;
        try
        {
            observers = GlobalClockReflection.GetObservers(clock);
        }
        catch (Exception exception) when (exception is MissingMemberException or TypeLoadException or InvalidCastException or ArgumentException)
        {
            // Settling is an optimization: without the render-loop clock internals animations simply run.
            Logger.TryGet(LogEventLevel.Warning, LogArea.Animations)?.Log(this,
                "Motion settle is unavailable: {Reason}", exception.Message);
            return;
        }
        _clock = clock;
        _known = new HashSet<IObserver<TimeSpan>>(observers, ReferenceEqualityComparer.Instance);
        // Subscribing also schedules a frame when nothing else invalidated the scene.
        _frameProbe = new FrameProbe(this, isFrameStart: true);
        _known.Add(_frameProbe);
        _frameProbe.Attach(clock.Subscribe(_frameProbe));
    }

    private void Close()
    {
        _frameProbe?.Dispose();
        _frameProbe = null;
        _known = null;
        _clock = null;
    }

    private void Settle(TimeSpan now)
    {
        if (_clock is not { } clock || _known is not { } known)
        {
            return;
        }
        // Completing an animation changes properties, which can start another transition, so settle until no new
        // observer appears.
        for (var pass = 0; pass < MaxCascade; pass++)
        {
            var settled = false;
            foreach (var observer in GlobalClockReflection.GetObservers(clock).ToArray())
            {
                if (!known.Add(observer))
                {
                    continue;
                }
                settled = true;
                // Local time starts at the first pulse; the second is a monotonic jump past the end, which the
                // observer treats like any late frame.
                observer.OnNext(now);
                observer.OnNext(now + SettleJump);
            }
            if (!settled)
            {
                return;
            }
        }
    }

    private void OnFrameStart(TimeSpan now)
    {
        Settle(now);
        if (_clock is { } clock && _known is { } known)
        {
            // Subscribed during the pulse, the probe is a new subscription of this frame: the render loop pulses
            // those again after layout and before commit.
            var afterLayout = new FrameProbe(this, isFrameStart: false);
            known.Add(afterLayout);
            afterLayout.Attach(clock.Subscribe(afterLayout));
        }
        if (_pendingReleases is { Count: > 0 } releases)
        {
            _pendingReleases = null;
            foreach (var release in releases)
            {
                Dispatcher.UIThread.Post(release.Dispose, DispatcherPriority.Loaded);
            }
        }
    }

    private sealed class Scope(MotionSettle owner) : IDisposable
    {
        private MotionSettle? _owner = owner;

        public void Dispose()
        {
            Dispatcher.UIThread.VerifyAccess();
            if (_owner is not { } owner)
            {
                return;
            }
            _owner = null;
            if (--owner._depth == 0)
            {
                owner.Close();
            }
        }
    }

    private sealed class FrameProbe(MotionSettle owner, bool isFrameStart) : IObserver<TimeSpan>, IDisposable
    {
        private IDisposable? _subscription;
        private bool _disposed;

        internal void Attach(IDisposable subscription)
        {
            if (_disposed)
            {
                subscription.Dispose();
                return;
            }
            _subscription = subscription;
        }

        public void OnNext(TimeSpan value)
        {
            if (_disposed)
            {
                return;
            }
            if (isFrameStart)
            {
                owner.OnFrameStart(value);
            }
            else
            {
                Dispose();
                owner.Settle(value);
            }
        }

        public void OnError(Exception error) => Dispose();

        public void OnCompleted() => Dispose();

        public void Dispose()
        {
            _disposed = true;
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
