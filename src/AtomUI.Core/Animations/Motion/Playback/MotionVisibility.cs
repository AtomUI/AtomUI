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
    private IDisposable? _preparation;
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
            Snap(version, visible);
            CompleteStable(version, visible, completion);
        }
        catch (Exception exception)
        {
            Fail(version, visible, completion, exception);
        }
        finally
        {
            preparation?.Dispose();
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
                Snap(version, visible);
                CompleteStable(version, visible, completion);
                return;
            }
            var effect = visible ? Motion.GetEnter(_target) : Motion.GetLeave(_target);
            if (effect is null)
            {
                Snap(version, visible);
                CompleteStable(version, visible, completion);
                return;
            }
            if (visible && !_target.IsVisible)
            {
                var preparation = _target.SetValue(Visual.OpacityProperty, 0, BindingPriority.Animation);
                if (!IsCurrent(version, visible))
                {
                    preparation?.Dispose();
                    return;
                }
                _preparation = preparation;
                _target.SetCurrentValue(Visual.IsVisibleProperty, true);
                if (!IsCurrent(version, visible))
                {
                    return;
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
            previousPreparation?.Dispose();
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

    private async Task PlayAsync(long version, bool visible, Motion effect, TaskCompletionSource<MotionResult> completion)
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
}
