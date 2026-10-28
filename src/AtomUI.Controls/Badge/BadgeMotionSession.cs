using System.Reactive.Disposables;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Logging;
using Avalonia.Threading;

namespace AtomUI.Controls.Commons;

/// <summary>Coordinates badge readiness and physical removal; Core owns finite playback.</summary>
internal sealed class BadgeMotionSession(Control owner)
{
    private Control? _target;
    private IDisposable? _preparation;
    private Action? _hidden;
    private long _version;
    private long _scheduledVersion;
    private bool _hasGoal;
    private bool _visible;
    private bool _pending;
    private bool _playing;
    private bool _enabled;
    private bool _motionRequested;
    private TimeSpan _duration;

    internal void SetTarget(Control? target)
    {
        var version = ++_version;
        var previous = _target;
        _target = target;
        _playing = false;
        ReleasePreparation();
        if (previous is not null && !ReferenceEquals(previous, target))
        {
            MotionPlayback.CancelAndReset(previous);
        }
        if (version != _version || !_hasGoal)
        {
            return;
        }
        _pending = true;
        if (_visible && _enabled)
        {
            Prepare(version);
        }
        if (!_enabled)
        {
            Complete(version);
        }
        else
        {
            TryStart();
        }
    }

    internal void Show(bool enabled, TimeSpan duration, Action? ready)
    {
        var wasPlaying = _playing;
        var version = ++_version;
        _hasGoal = true;
        _visible = true;
        _hidden = null;
        _pending = enabled;
        _playing = false;
        _enabled = enabled;
        _motionRequested = enabled;
        _duration = duration;
        if (!enabled)
        {
            Complete(version);
        }
        else if (!wasPlaying)
        {
            Prepare(version);
        }
        if (version != _version)
        {
            return;
        }
        ready?.Invoke();
        if (version == _version)
        {
            TryStart();
        }
    }

    internal void Hide(bool enabled, TimeSpan duration, Action hidden)
    {
        var version = ++_version;
        _hasGoal = true;
        _visible = false;
        _hidden = hidden;
        _enabled = enabled;
        _motionRequested = enabled;
        _duration = duration;
        _pending = true;
        _playing = false;
        if (!enabled || _target is null || !owner.IsLoaded)
        {
            Complete(version);
        }
        else
        {
            TryStart();
        }
    }

    internal void UpdateConfiguration(bool enabled, TimeSpan duration)
    {
        _enabled = enabled && _motionRequested;
        _duration = duration;
        if (!_enabled && _hasGoal && (_pending || _playing))
        {
            Complete(++_version);
        }
    }

    internal void TryStart()
    {
        if (!_pending || _target is null || !owner.IsLoaded || _scheduledVersion == _version)
        {
            return;
        }
        var version = _version;
        var target = _target;
        var visible = _visible;
        _scheduledVersion = version;
        Dispatcher.UIThread.Post(() => _ = RunAsync(version, target, visible), DispatcherPriority.Loaded);
    }

    internal void Detach()
    {
        ++_version;
        _hasGoal = false;
        _pending = false;
        _playing = false;
        _hidden = null;
        ReleasePreparation();
        if (_target is { } target)
        {
            MotionPlayback.CancelAndReset(target);
        }
    }

    private void Prepare(long version)
    {
        if (_target is not { } target || version != _version)
        {
            return;
        }
        ReleasePreparation();
        var leases = new CompositeDisposable();
        try
        {
            leases.Add(target.SetValue(Visual.OpacityProperty, 0, BindingPriority.Animation) ?? Disposable.Empty);
            if (version != _version)
            {
                leases.Dispose();
                return;
            }
            leases.Add(target.SetValue(Visual.RenderTransformProperty, BadgeMotionTransforms.ScaleNearZero,
                BindingPriority.Animation) ?? Disposable.Empty);
            if (version != _version)
            {
                leases.Dispose();
                return;
            }
            leases.Add(target.SetValue(Visual.RenderTransformOriginProperty, BadgeMotionTransforms.Origin,
                BindingPriority.Animation) ?? Disposable.Empty);
            if (version != _version)
            {
                leases.Dispose();
                return;
            }
            _preparation = leases;
            target.SetCurrentValue(Visual.IsVisibleProperty, true);
        }
        catch
        {
            leases.Dispose();
            throw;
        }
    }

    private async Task RunAsync(long version, Control target, bool visible)
    {
        if (!IsCurrent(version, target, visible))
        {
            return;
        }
        _pending = false;
        _playing = true;
        var preparation = _preparation;
        _preparation = null;
        try
        {
            target.UpdateLayout();
            Task<MotionResult> playback;
            try
            {
                AbstractMotion motion = visible ? new BadgeInMotion(_duration) : new BadgeOutMotion(_duration);
                playback = motion.PlayAsync(target);
            }
            finally
            {
                preparation?.Dispose();
            }
            var result = await playback;
            if (!IsCurrent(version, target, visible) ||
                (result.Outcome != MotionOutcome.NotReady && (!result.IsCompleted || !Motion.IsCurrent(target, result))))
            {
                return;
            }
            Complete(version);
        }
        catch (Exception exception)
        {
            Logger.TryGet(LogEventLevel.Error, AtomUILogArea.Control)?.Log(owner, "Badge motion failed: {Exception}", exception);
            if (IsCurrent(version, target, visible))
            {
                Complete(version);
            }
        }
    }

    private bool IsCurrent(long version, Control target, bool visible) =>
        version == _version && _hasGoal && _visible == visible && ReferenceEquals(target, _target);

    private void Complete(long version)
    {
        if (version != _version)
        {
            return;
        }
        _pending = false;
        _playing = false;
        var target = _target;
        var visible = _visible;
        ReleasePreparation();
        if (version != _version)
        {
            return;
        }
        if (target is not null)
        {
            MotionPlayback.CancelAndReset(target);
            if (!IsCurrent(version, target, visible))
            {
                return;
            }
            target.SetCurrentValue(Visual.IsVisibleProperty, visible);
        }
        if (version == _version && !visible)
        {
            var hidden = _hidden;
            _hidden = null;
            hidden?.Invoke();
        }
    }

    private void ReleasePreparation()
    {
        var preparation = _preparation;
        _preparation = null;
        preparation?.Dispose();
    }
}
