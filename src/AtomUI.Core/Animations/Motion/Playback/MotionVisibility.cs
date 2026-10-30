using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Logging;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Animations;

internal sealed class MotionVisibility
{
    internal Task<MotionResult> Completion => _completion?.Task ?? Stable;

    private static readonly Task<MotionResult> Stable =
        Task.FromResult(new MotionResult(MotionOutcome.Completed, 0, false));

    private readonly Control _target;
    private Preparation? _preparation;
    private TaskCompletionSource<MotionResult>? _completion;
    private long _version;
    private bool _initialized;
    private bool _hasAttached;
    private bool _desired;

    internal MotionVisibility(Control target)
    {
        _target = target;
        _hasAttached = target.IsAttachedToVisualTree();
        _desired = Motion.GetIsVisible(target);
        target.AttachedToVisualTree += Attached;
        target.DetachedFromVisualTree += Detached;
    }

    internal void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        SnapCurrent();
    }

    internal void SnapCurrent()
    {
        if (_hasAttached && !_target.IsAttachedToVisualTree())
        {
            return;
        }
        _initialized = true;
        var (version, completion) = BeginRequest();
        var visible = Motion.GetIsVisible(_target);
        _desired = visible;
        var preparation = _preparation;
        _preparation = null;
        try
        {
            SnapAndRelease(version, visible, preparation);
            CompleteStable(version, visible, completion);
        }
        catch (Exception exception)
        {
            Fail(version, visible, completion, exception);
        }
    }

    internal void Request()
    {
        // TemplateBinding can revert to its default while the template is being
        // detached. That is not a new business visibility request.
        if (_hasAttached && !_target.IsAttachedToVisualTree())
        {
            return;
        }
        _initialized = true;
        var (version, completion) = BeginRequest();
        var visible = Motion.GetIsVisible(_target);
        _desired = visible;
        var previousPreparation = _preparation;
        _preparation = null;
        try
        {
            if (!_target.IsAttachedToVisualTree() || (!visible && !_target.IsVisible))
            {
                var previous = previousPreparation;
                previousPreparation = null;
                SnapAndRelease(version, visible, previous);
                CompleteStable(version, visible, completion);
                return;
            }
            var effect = visible ? Motion.GetEnter(_target) : Motion.GetLeave(_target);
            if (effect is null)
            {
                var previous = previousPreparation;
                previousPreparation = null;
                SnapAndRelease(version, visible, previous);
                CompleteStable(version, visible, completion);
                return;
            }
            var revealing = visible && !_target.IsVisible;
            if (revealing || (!_target.IsArrangeValid && _target is IMotionLayout))
            {
                var preparation = new Preparation();
                _preparation = preparation;
                var opacity = revealing ? 0 : _target.Opacity;
                preparation.HoldOpacity(_target, opacity);
                if (!IsCurrent(version, visible))
                {
                    preparation.Dispose();
                    return;
                }
                if (_target is IMotionLayout layout)
                {
                    var plan = effect.Prepare(_target, MotionValues.Baseline(opacity), null);
                    if (!IsCurrent(version, visible))
                    {
                        preparation.Dispose();
                        return;
                    }
                    if (plan.Layouts.Length > 0 && plan.Layouts[0] is { Start.Ticks: 0 } segment &&
                        (!revealing || segment.Expanded))
                    {
                        // Keep zero/current occupancy through readiness and replacement.
                        // Playback takes its own lease before this preparation is released.
                        var lease = layout.BeginLayoutMotion(segment.Direction, segment.Expanded);
                        preparation.OwnLayout(lease);
                        if (!IsCurrent(version, visible))
                        {
                            preparation.Dispose();
                            return;
                        }
                        lease.SetProgress(0);
                    }
                }
                if (!IsCurrent(version, visible))
                {
                    preparation.Dispose();
                    return;
                }
                if (revealing)
                {
                    _target.SetCurrentValue(Visual.IsVisibleProperty, true);
                    if (!IsCurrent(version, visible))
                    {
                        return;
                    }
                }
            }
            if (_target.IsArrangeValid)
            {
                _ = PlayAsync(version, visible, effect, completion);
            }
            else
            {
                // One readiness boundary, not a frame driver or an unbounded layout wait.
                Dispatcher.UIThread.Post(() =>
                {
                    if (IsCurrent(version, visible))
                    {
                        _ = PlayAsync(version, visible, effect, completion);
                    }
                }, DispatcherPriority.Loaded);
            }
        }
        catch (Exception exception)
        {
            Fail(version, visible, completion, exception);
        }
        finally
        {
            try
            {
                previousPreparation?.Dispose();
            }
            catch (Exception exception)
            {
                Fail(version, visible, completion, exception);
            }
        }
    }

    private (long Version, TaskCompletionSource<MotionResult> Completion) BeginRequest()
    {
        var version = ++_version;
        var previous = _completion;
        var completion = new TaskCompletionSource<MotionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _completion = completion;
        previous?.TrySetResult(new MotionResult(MotionOutcome.Superseded, 0, false));
        return (version, completion);
    }

    private async Task PlayAsync(long version, bool visible, AbstractMotion effect, TaskCompletionSource<MotionResult> completion)
    {
        if (!IsCurrent(version, visible))
        {
            completion.TrySetResult(new MotionResult(MotionOutcome.Superseded, 0, false));
            return;
        }
        var preparation = _preparation;
        _preparation = null;
        try
        {
            Task<MotionResult> playback;
            try
            {
                playback = effect.PlayAsync(_target);
            }
            finally
            {
                preparation?.Dispose();
            }
            var result = await playback;
            if (!IsCurrent(version, visible))
            {
                completion.TrySetResult(new MotionResult(MotionOutcome.Superseded, result.RequestId, result.WasAnimated));
                return;
            }
            if (result.Outcome == MotionOutcome.NotReady)
            {
                Snap(version, visible);
                CompleteStable(version, visible, completion);
                return;
            }
            if (result.IsCompleted && !Motion.IsCurrent(_target, result))
            {
                result = result with { Outcome = MotionOutcome.Superseded };
            }
            if (result.IsCompleted && !visible)
            {
                _target.SetCurrentValue(Visual.IsVisibleProperty, false);
                if (IsCurrent(version, visible))
                {
                    Motion.Reset(_target);
                }
            }
            completion.TrySetResult(IsCurrent(version, visible)
                ? result : result with { Outcome = MotionOutcome.Superseded });
        }
        catch (Exception exception)
        {
            Fail(version, visible, completion, exception);
        }
    }

    private void Snap(long version, bool visible)
    {
        if (!IsCurrent(version, visible))
        {
            return;
        }
        if (!visible)
        {
            _target.SetCurrentValue(Visual.IsVisibleProperty, false);
        }
        if (!IsCurrent(version, visible))
        {
            return;
        }
        MotionPlayback.CancelAndReset(_target);
        if (IsCurrent(version, visible))
        {
            _target.SetCurrentValue(Visual.IsVisibleProperty, visible);
        }
    }

    private void SnapAndRelease(long version, bool visible, Preparation? preparation)
    {
        Exception? failure = null;
        try
        {
            Snap(version, visible);
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        try
        {
            preparation?.Dispose();
        }
        catch (Exception exception)
        {
            failure = failure is null ? exception : new AggregateException(failure, exception);
        }
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private void CompleteStable(long version, bool visible, TaskCompletionSource<MotionResult> completion)
    {
        completion.TrySetResult(new MotionResult(IsCurrent(version, visible)
            ? MotionOutcome.Completed : MotionOutcome.Superseded, 0, false));
    }

    private void Fail(long version, bool visible, TaskCompletionSource<MotionResult> completion, Exception exception)
    {
        Logger.TryGet(LogEventLevel.Error, AtomUILogArea.Control)?.Log(
            _target, "Motion visibility failed: {Exception}", exception);
        if (IsCurrent(version, visible))
        {
            try
            {
                var preparation = _preparation;
                _preparation = null;
                preparation?.Dispose();
            }
            catch (Exception cleanup)
            {
                exception = new AggregateException(exception, cleanup);
            }
            try
            {
                Snap(version, visible);
            }
            catch (Exception cleanup)
            {
                exception = new AggregateException(exception, cleanup);
            }
        }
        completion.TrySetException(exception);
        // The attached path has no mandatory awaiter; faults are logged/observed here,
        // and an explicit GetVisibilityTask caller can still observe the same failure.
        _ = completion.Task.Exception;
    }

    private bool IsCurrent(long version, bool visible) =>
        version == _version && _desired == visible && (!_hasAttached || _target.IsAttachedToVisualTree());

    private void Attached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        _hasAttached = true;
        SnapCurrent();
    }

    private void Detached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        var version = ++_version;
        var visible = _desired;
        _completion?.TrySetResult(new MotionResult(MotionOutcome.Detached, 0, false));
        var preparation = _preparation;
        _preparation = null;
        try
        {
            MotionPlayback.CancelAndReset(_target, MotionOutcome.Detached);
        }
        finally
        {
            try
            {
                preparation?.Dispose();
            }
            finally
            {
                if (version == _version && !_target.IsAttachedToVisualTree())
                {
                    _target.SetCurrentValue(Visual.IsVisibleProperty, visible);
                }
            }
        }
    }

    private sealed class Preparation : IDisposable
    {
        private IDisposable? _opacity;
        private Subject<double>? _opacitySource;
        private IMotionLayoutLease? _layout;
        private bool _disposed;

        internal void HoldOpacity(Control target, double opacity)
        {
            var source = new Subject<double>();
            _opacitySource = source;
            // Runtime presentation snapshots cannot be declared in ControlTheme.
            // Own the binding before publishing a value that can synchronously throw/reenter.
            var binding = target.Bind(Visual.OpacityProperty, source, BindingPriority.Animation);
            if (_disposed)
            {
                binding.Dispose();
            }
            else
            {
                _opacity = binding;
                source.OnNext(opacity);
            }
        }

        internal void OwnLayout(IMotionLayoutLease layout)
        {
            if (_disposed)
            {
                layout.Dispose();
            }
            else
            {
                _layout = layout;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            var layout = _layout;
            var opacity = _opacity;
            var source = _opacitySource;
            _layout = null;
            _opacity = null;
            _opacitySource = null;
            try
            {
                Exception? opacityFailure = null;
                // A reentrant request can capture the old lease's pending geometry.
                // Its replacement is installed before the old lease is released.
                try
                {
                    opacity?.Dispose();
                }
                catch (Exception exception)
                {
                    opacityFailure = exception;
                }
                try
                {
                    layout?.Dispose();
                }
                catch (Exception exception)
                {
                    if (opacityFailure is not null)
                    {
                        throw new AggregateException(opacityFailure, exception);
                    }
                    throw;
                }
                if (opacityFailure is not null)
                {
                    ExceptionDispatchInfo.Capture(opacityFailure).Throw();
                }
            }
            finally
            {
                source?.Dispose();
            }
        }
    }
}
