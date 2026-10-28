using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Animations;

internal sealed class MotionPlayback
{
    private static readonly ConditionalWeakTable<Control, TargetState> States = new();
    private static long _nextRequest;

    private readonly TargetState _state;
    private readonly MotionPlan _plan;
    private readonly MotionValues _baseline;
    private readonly CancellationToken _externalToken;
    private CancellationTokenSource? _cancellation;
    private readonly long _requestId;
    private MotionPresentation? _presentation;
    private MotionProgress? _progress;
    private MotionOutcome? _stopReason;
    private Exception? _failure;
    private Phase _phase;

    internal bool CanWrite => ReferenceEquals(_state.Active, this) && _stopReason is null && _phase != Phase.Finished;

    private MotionPlayback(TargetState state, MotionPlan plan, MotionValues initial, CancellationToken externalToken, long requestId)
    {
        _state = state;
        _plan = plan;
        _baseline = MotionValues.Baseline(initial.Opacity);
        _externalToken = externalToken;
        _requestId = requestId;
        var first = plan.FillsBefore || plan.Delay == TimeSpan.Zero || plan.Duration == TimeSpan.Zero
            ? plan.Sample(TimeSpan.Zero, _baseline) : _baseline;
        _presentation = new MotionPresentation(state.Target, plan, first);
    }

    internal static Task<MotionResult> PlayAsync(AbstractMotion motion, Control target, TimeSpan? duration,
        CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromResult(new MotionResult(MotionOutcome.Cancelled, 0, false));
        }
        if (!target.IsAttachedToVisualTree() || !target.IsArrangeValid)
        {
            return Task.FromResult(new MotionResult(MotionOutcome.NotReady, 0, false));
        }
        var requestId = Interlocked.Increment(ref _nextRequest);
        var visualRoot = TopLevel.GetTopLevel(target);
        States.TryGetValue(target, out var state);
        var previousPresentation = state?.Active?._presentation ?? state?.Terminal;
        var initial = previousPresentation is { HasTransform: true }
            ? previousPresentation.Values with { Opacity = target.Opacity }
            : MotionValues.Baseline(target.Opacity);
        var plan = motion.Prepare(target, initial, duration);
        States.TryGetValue(target, out state);
        if (state?.RequestId > requestId)
        {
            return Task.FromResult(new MotionResult(MotionOutcome.Superseded, requestId, false));
        }
        if (!target.IsAttachedToVisualTree() || !target.IsArrangeValid || !ReferenceEquals(visualRoot, TopLevel.GetTopLevel(target)))
        {
            return Task.FromResult(new MotionResult(MotionOutcome.NotReady, 0, false));
        }
        if (plan.HasTransform && target.GetBaseValue(Visual.RenderTransformProperty).HasValue)
        {
            throw new NotSupportedException("This native motion target has an independent RenderTransform source. " +
                "Provide an explicit template transform boundary for the motion.");
        }
        if (plan.Layouts.Length > 0 && target is not IMotionLayout)
        {
            throw new NotSupportedException("This motion requires a target implementing IMotionLayout.");
        }
        if (state?.Active is not null && previousPresentation is not null)
        {
            var retargeted = 0;
            for (var index = 0; index < plan.Tracks.Length; index++)
            {
                var track = plan.Tracks[index];
                var flag = 1 << (int)track.Kind;
                if (track.Kind != MotionKind.LayoutProgress && (retargeted & flag) == 0 && previousPresentation.HasChannel(track.Kind))
                {
                    plan.Tracks[index] = track.Retarget(initial.Get(track.Kind));
                }
                retargeted |= flag;
            }
            foreach (var timeline in plan.Properties)
            {
                if (previousPresentation.HasProperty(timeline.Property))
                {
                    timeline.Retarget();
                }
            }
        }
        state ??= States.GetValue(target, static control => new TargetState(control));
        var run = new MotionPlayback(state, plan, initial, token, requestId);
        if (state.RequestId > requestId)
        {
            run._presentation?.Dispose();
            run._presentation = null;
            run._cancellation?.Dispose();
            return Task.FromResult(new MotionResult(MotionOutcome.Superseded, requestId, false));
        }
        var previous = state.Active;
        var terminal = state.Terminal;
        state.Active = run;
        state.Terminal = null;
        state.RequestId = run._requestId;
        return run.ExecuteAsync(previous, terminal);
    }

    internal static bool IsCurrent(Control target, MotionResult result)
    {
        return result.Outcome == MotionOutcome.Completed && result.RequestId != 0 &&
            target.IsAttachedToVisualTree() && States.TryGetValue(target, out var state) && state.RequestId == result.RequestId;
    }

    internal static void Reset(Control target)
    {
        if (!States.TryGetValue(target, out var state))
        {
            return;
        }
        if (state.Active is not null)
        {
            throw new InvalidOperationException("Cancel and await an active motion before resetting its presentation.");
        }
        state.Release();
    }

    internal static void CancelAndReset(Control target, MotionOutcome reason = MotionOutcome.Cancelled)
    {
        if (!States.TryGetValue(target, out var state))
        {
            return;
        }
        var requestId = state.RequestId;
        var active = state.Active;
        var terminal = state.Terminal;
        var presentation = active?._presentation;
        state.Active = null;
        state.Terminal = null;
        if (active is not null)
        {
            active._presentation = null;
        }
        List<Exception>? failures = null;
        Try(() => active?.Cancel(reason));
        Try(() => presentation?.Dispose());
        Try(() => terminal?.Dispose());
        if (state.RequestId == requestId && state.Active is null)
        {
            Try(state.Release);
        }
        if (failures is not null)
        {
            throw new AggregateException("Motion reset cleanup failed.", failures);
        }

        void Try(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                (failures ??= []).Add(exception);
            }
        }
    }

    private async Task<MotionResult> ExecuteAsync(MotionPlayback? previous, MotionPresentation? terminal)
    {
        var shouldAnimate = _plan.Duration > TimeSpan.Zero &&
            (_plan.Tracks.Any(track => track.Frames.Any(frame => frame.Value != track.Frames[0].Value)) ||
                _plan.Properties.Any(timeline => timeline.HasChanges));
        var animated = false;
        Exception? executionFailure = null;
        CancellationTokenRegistration registration = default;
        try
        {
            try
            {
                if (_plan.FillsBefore || _plan.Delay == TimeSpan.Zero || !shouldAnimate)
                {
                    _presentation!.Attach(this);
                }
            }
            finally
            {
                previous?.Cancel(MotionOutcome.Superseded);
                terminal?.Dispose();
            }
            if (!CanWrite)
            {
                return Result(_stopReason ?? MotionOutcome.Superseded, false);
            }
            registration = _externalToken.Register(static value =>
            {
                var run = (MotionPlayback)value!;
                if (Dispatcher.UIThread.CheckAccess())
                {
                    run.Cancel(MotionOutcome.Cancelled);
                }
                else
                {
                    Dispatcher.UIThread.Post(() => run.Cancel(MotionOutcome.Cancelled));
                }
            }, this);
            _phase = Phase.Playing;
            if (_plan.FillsBefore || _plan.Delay == TimeSpan.Zero)
            {
                Apply(TimeSpan.Zero);
            }
            if (shouldAnimate && CanWrite)
            {
                animated = true;
                var cancellation = new CancellationTokenSource();
                _cancellation = cancellation;
                _progress = new MotionProgress { Tick = Tick };
                var animation = new Animation
                {
                    Duration = _plan.Duration + _plan.Delay,
                    Easing = new LinearEasing(),
                    FillMode = FillMode.Forward,
                    Children =
                    {
                        new KeyFrame { Cue = new Cue(0), Setters = { new Setter(MotionProgress.ValueProperty, 0d) } },
                        new KeyFrame { Cue = new Cue(1), Setters = { new Setter(MotionProgress.ValueProperty, 1d) } }
                    }
                };
                await animation.RunAsync(_progress, cancellation.Token);
            }
            if (_failure is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(_failure).Throw();
            }
            if (!CanWrite)
            {
                return Result(_stopReason ?? MotionOutcome.Superseded, animated);
            }
            _phase = Phase.Settling;
            Apply(_plan.Duration);
            if (!CanWrite)
            {
                return Result(_stopReason ?? MotionOutcome.Superseded, animated);
            }
            if (_plan.FillsAfter)
            {
                _presentation!.Seal(this);
                if (CanWrite)
                {
                    _state.Terminal = _presentation;
                    _presentation = null;
                }
            }
            return Result(CanWrite ? MotionOutcome.Completed : _stopReason ?? MotionOutcome.Superseded, animated);
        }
        catch (Exception exception)
        {
            executionFailure = exception;
            throw;
        }
        finally
        {
            _phase = Phase.Finished;
            if (_progress is not null)
            {
                _progress.Tick = null;
                _progress = null;
            }
            registration.Dispose();
            _cancellation?.Dispose();
            _cancellation = null;
            try
            {
                _presentation?.Dispose();
            }
            catch (Exception cleanup) when (executionFailure is not null)
            {
                throw new AggregateException("Motion playback and cleanup failed.", executionFailure, cleanup);
            }
            finally
            {
                _presentation = null;
                if (ReferenceEquals(_state.Active, this))
                {
                    _state.Active = null;
                }
            }
        }
    }

    private void Tick(double progress)
    {
        if (!CanWrite || _phase != Phase.Playing || _externalToken.IsCancellationRequested)
        {
            return;
        }
        var elapsed = TimeSpan.FromTicks((long)((_plan.Duration + _plan.Delay).Ticks * progress)) - _plan.Delay;
        if (elapsed < TimeSpan.Zero)
        {
            return;
        }
        try
        {
            Apply(elapsed);
        }
        catch (Exception exception)
        {
            _failure = exception;
            Cancel(MotionOutcome.Cancelled);
        }
    }

    private void Apply(TimeSpan elapsed)
    {
        _presentation?.Apply(_plan.Sample(elapsed, _baseline), elapsed, this);
    }

    private void Cancel(MotionOutcome outcome)
    {
        if (_phase == Phase.Finished || _stopReason is not null)
        {
            return;
        }
        _stopReason = outcome;
        _phase = Phase.Settling;
        _cancellation?.Cancel();
    }

    private MotionResult Result(MotionOutcome outcome, bool animated) => new(outcome, _requestId, animated);

    private enum Phase
    {
        Preparing,
        Playing,
        Settling,
        Finished
    }

    private sealed class TargetState
    {
        internal Control Target { get; }
        internal MotionPlayback? Active { get; set; }
        internal MotionPresentation? Terminal { get; set; }
        internal long RequestId { get; set; }

        internal TargetState(Control target)
        {
            Target = target;
            target.DetachedFromVisualTree += Detached;
        }

        internal void Release()
        {
            Target.DetachedFromVisualTree -= Detached;
            if (States.TryGetValue(Target, out var current) && ReferenceEquals(current, this))
            {
                States.Remove(Target);
            }
            var terminal = Terminal;
            Terminal = null;
            terminal?.Dispose();
        }

        private void Detached(object? sender, VisualTreeAttachmentEventArgs args)
        {
            try
            {
                Active?.Cancel(MotionOutcome.Detached);
            }
            finally
            {
                Release();
            }
        }
    }
}
