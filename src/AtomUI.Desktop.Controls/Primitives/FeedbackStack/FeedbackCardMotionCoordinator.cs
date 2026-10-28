using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Logging;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>Owns card entry and close intent; Core Motion owns playback and replacement.</summary>
internal sealed class FeedbackCardMotionCoordinator(Action completeClose) : IDisposable
{
    private Action? _completeClose = completeClose;
    private Control? _target;
    private IDisposable? _preparation;
    private long _version;
    private Phase _phase;
    private NotificationPosition _position;
    private TimeSpan _duration;
    private bool _isMotionEnabled;
    private bool _isClosing;
    private bool _isClosed;
    private bool _entryCompleted;
    private bool _closeCompleted;
    private bool _isDisposed;

    internal void ApplyTarget(Control? target, bool isClosing, bool isClosed, bool isMotionEnabled,
        NotificationPosition position, TimeSpan duration)
    {
        if (_isDisposed)
        {
            return;
        }
        UpdateState(isClosing, isClosed, isMotionEnabled, position, duration);
        var version = ++_version;
        var previous = _target;
        _target = target;
        _phase = Phase.Idle;
        ReleasePreparation();
        if (previous is not null && !ReferenceEquals(previous, target))
        {
            MotionPlayback.CancelAndReset(previous);
        }
        if (version != _version)
        {
            return;
        }
        if (_isClosed)
        {
            Converge(false);
        }
        else if (_isClosing)
        {
            Schedule(false);
        }
        else if (target is null)
        {
            return;
        }
        else if (!_isMotionEnabled || _entryCompleted)
        {
            _entryCompleted = true;
            Converge(true);
        }
        else
        {
            var preparation = target.SetValue(Visual.OpacityProperty, 0, BindingPriority.Animation);
            if (version != _version || _isDisposed || !ReferenceEquals(target, _target))
            {
                preparation?.Dispose();
                return;
            }
            _preparation = preparation;
            Schedule(true);
        }
    }

    internal void StartClose(bool isClosed, bool isMotionEnabled, NotificationPosition position, TimeSpan duration)
    {
        if (_isDisposed || isClosed || _closeCompleted)
        {
            return;
        }
        UpdateState(true, isClosed, isMotionEnabled, position, duration);
        if (!_isMotionEnabled)
        {
            ++_version;
            CompleteExit();
        }
        else if (_phase != Phase.Exit)
        {
            Schedule(false);
        }
    }

    internal void UpdateConfiguration(bool isClosing, bool isClosed, bool isMotionEnabled,
        NotificationPosition position, TimeSpan duration)
    {
        if (_isDisposed)
        {
            return;
        }
        UpdateState(isClosing, isClosed, isMotionEnabled, position, duration);
        if (_isClosed)
        {
            ++_version;
            _phase = Phase.Idle;
            Converge(false);
        }
        else if (!_isMotionEnabled)
        {
            ++_version;
            _phase = Phase.Idle;
            if (_isClosing)
            {
                CompleteExit();
            }
            else if (_target is not null)
            {
                _entryCompleted = true;
                Converge(true);
            }
        }
    }

    internal void DetachTarget(bool isClosing, bool isClosed)
    {
        if (_isDisposed)
        {
            return;
        }
        var version = ++_version;
        _phase = Phase.Idle;
        _isClosing = isClosing;
        _isClosed = isClosed;
        if (_isClosing && !_isClosed)
        {
            CompleteExit();
        }
        else
        {
            Converge(!_isClosed);
        }
        if (version == _version)
        {
            _target = null;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        ++_version;
        _isDisposed = true;
        _phase = Phase.Idle;
        Converge(!_isClosing && !_isClosed);
        _target = null;
        _completeClose = null;
    }

    private void UpdateState(bool isClosing, bool isClosed, bool isMotionEnabled,
        NotificationPosition position, TimeSpan duration)
    {
        _isClosing = isClosing;
        _isClosed = isClosed;
        _isMotionEnabled = isMotionEnabled;
        _position = position;
        _duration = duration;
    }

    private void Schedule(bool entering)
    {
        if (_isDisposed)
        {
            return;
        }
        var version = ++_version;
        var target = _target;
        _phase = entering ? Phase.Entry : Phase.Exit;
        Dispatcher.UIThread.Post(() => _ = RunAsync(version, target, entering), DispatcherPriority.Loaded);
    }

    private async Task RunAsync(long version, Control? target, bool entering)
    {
        if (!IsCurrent(version, target, entering))
        {
            return;
        }
        if (target is null || !_isMotionEnabled)
        {
            Complete(entering);
            return;
        }
        if (entering && !target.IsAttachedToVisualTree())
        {
            _phase = Phase.Idle;
            return;
        }
        var preparation = _preparation;
        _preparation = null;
        try
        {
            target.UpdateLayout();
            Task<MotionResult> playback;
            try
            {
                playback = new FeedbackCardMotion(entering, _position, _duration).PlayAsync(target);
            }
            finally
            {
                preparation?.Dispose();
            }
            var result = await playback;
            if (!IsCurrent(version, target, entering) ||
                (result.Outcome != MotionOutcome.NotReady && (!result.IsCompleted || !Motion.IsCurrent(target, result))))
            {
                return;
            }
            Complete(entering);
        }
        catch (Exception exception)
        {
            Logger.TryGet(LogEventLevel.Error, AtomUILogArea.Control)?.Log(
                target, "Feedback card motion failed: {Exception}", exception);
            if (IsCurrent(version, target, entering))
            {
                Complete(entering);
            }
        }
    }

    private bool IsCurrent(long version, Control? target, bool entering) =>
        !_isDisposed && version == _version && ReferenceEquals(target, _target) && !_isClosed &&
        _isClosing != entering && _phase == (entering ? Phase.Entry : Phase.Exit);

    private void Complete(bool entering)
    {
        _phase = Phase.Idle;
        if (entering)
        {
            _entryCompleted = true;
            Converge(true);
        }
        else
        {
            CompleteExit();
        }
    }

    private void CompleteExit()
    {
        if (_isDisposed || _isClosed || _closeCompleted)
        {
            return;
        }
        _closeCompleted = true;
        _phase = Phase.Idle;
        var complete = _completeClose;
        _completeClose = null;
        var version = _version;
        Converge(false);
        if (!_isDisposed && version == _version)
        {
            complete?.Invoke();
        }
    }

    private void ReleasePreparation()
    {
        var preparation = _preparation;
        _preparation = null;
        preparation?.Dispose();
    }

    private void Converge(bool visible)
    {
        var version = _version;
        var target = _target;
        ReleasePreparation();
        if (target is null || version != _version)
        {
            return;
        }
        MotionPlayback.CancelAndReset(target);
        if (version == _version && ReferenceEquals(target, _target))
        {
            target.SetCurrentValue(Visual.IsVisibleProperty, visible);
        }
    }

    private enum Phase
    {
        Idle,
        Entry,
        Exit
    }
}
