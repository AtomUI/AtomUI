using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Conditions that keep an open full-screen title bar open.
/// </summary>
[Flags]
internal enum FullScreenTitleBarHolds
{
    None = 0,

    /// <summary>The pointer is inside the keep zone of the surface.</summary>
    Pointer = 1 << 0,

    /// <summary>An element inside the surface holds pointer capture.</summary>
    Capture = 1 << 1,

    /// <summary>Keyboard focus is inside the surface.</summary>
    Focus = 1 << 2,

    /// <summary>A popup opened from the surface is open.</summary>
    Popup = 1 << 3,

    /// <summary>The platform currently presents its own top chrome above the surface.</summary>
    NativeChrome = 1 << 4,

    /// <summary>The application explicitly requested the surface to be open.</summary>
    Explicit = 1 << 5
}

/// <summary>
/// The reveal state machine of the <see cref="FullScreenTitleBarMode.AutoHide"/> full-screen title bar.
/// It owns no visuals and calls no platform API: the window feeds it pointer, focus, hold and suppression inputs and
/// applies the open state it reports. Delays run on an injectable scheduler so the machine can be driven by a test clock.
/// </summary>
internal sealed class FullScreenTitleBarRevealController : IDisposable
{
    private enum RevealState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    private readonly Func<TimeSpan, Action, IDisposable> _schedule;
    private readonly Action<bool> _openChanged;
    private RevealState _state;
    private FullScreenTitleBarHolds _holds;
    private IDisposable? _pendingDelay;
    private bool _isRunning;
    private bool _isSuppressed;

    public FullScreenTitleBarRevealController(Action<bool> openChanged)
        : this(openChanged, ScheduleOnDispatcher)
    {
    }

    internal FullScreenTitleBarRevealController(Action<bool> openChanged, Func<TimeSpan, Action, IDisposable> schedule)
    {
        _openChanged = openChanged;
        _schedule    = schedule;
    }

    public TimeSpan EnterDelay { get; set; }

    public TimeSpan LeaveDelay { get; set; }

    public bool IsRunning => _isRunning;

    public bool IsOpen => _state is RevealState.Open or RevealState.Closing;

    public FullScreenTitleBarHolds Holds => _holds;

    public void Start()
    {
        _isRunning = true;
    }

    public void Stop()
    {
        _isRunning    = false;
        _holds        = FullScreenTitleBarHolds.None;
        _isSuppressed = false;
        TransitionTo(RevealState.Closed);
    }

    /// <summary>
    /// Reports whether the mouse pointer is inside the trigger band at the top of the window.
    /// </summary>
    public void SetPointerInTriggerBand(bool isInTriggerBand)
    {
        if (!_isRunning)
        {
            return;
        }

        if (isInTriggerBand)
        {
            if (_state != RevealState.Closed || _isSuppressed)
            {
                return;
            }

            if (EnterDelay <= TimeSpan.Zero)
            {
                Open();
                return;
            }

            TransitionTo(RevealState.Opening);
            _pendingDelay = _schedule(EnterDelay, CompleteOpening);
        }
        else if (_state == RevealState.Opening)
        {
            TransitionTo(RevealState.Closed);
        }
    }

    public void SetHold(FullScreenTitleBarHolds hold, bool isActive)
    {
        var wasActive = (_holds & hold) != 0;
        _holds = isActive ? _holds | hold : _holds & ~hold;
        if (!_isRunning)
        {
            return;
        }

        // The platform has already decided to reveal its own chrome, so the surface follows it without a delay.
        if (hold == FullScreenTitleBarHolds.NativeChrome &&
            isActive &&
            !wasActive &&
            !_isSuppressed &&
            _state is RevealState.Closed or RevealState.Opening)
        {
            Open();
            return;
        }

        EvaluateHolds();
    }

    /// <summary>
    /// Opens the surface on an explicit request and keeps it open until <see cref="RequestClose"/>.
    /// </summary>
    /// <returns><c>false</c> when the request is rejected because the machine is stopped or suppressed.</returns>
    public bool RequestOpen()
    {
        if (!_isRunning || _isSuppressed)
        {
            return false;
        }

        _holds |= FullScreenTitleBarHolds.Explicit;
        if (_state is RevealState.Closed or RevealState.Opening)
        {
            Open();
        }
        else
        {
            EvaluateHolds();
        }

        return true;
    }

    public void RequestClose()
    {
        CloseImmediately();
    }

    /// <summary>
    /// Closes the surface without the leave delay. A visible platform chrome keeps the surface open because the surface
    /// is its background.
    /// </summary>
    public void CloseImmediately()
    {
        _holds &= ~FullScreenTitleBarHolds.Explicit;
        if (!_isRunning)
        {
            return;
        }

        if ((_holds & FullScreenTitleBarHolds.NativeChrome) != 0)
        {
            EvaluateHolds();
            return;
        }

        TransitionTo(RevealState.Closed);
    }

    public void SetSuppressed(bool isSuppressed)
    {
        if (_isSuppressed == isSuppressed)
        {
            return;
        }

        _isSuppressed = isSuppressed;
        if (!isSuppressed)
        {
            return;
        }

        _holds &= ~FullScreenTitleBarHolds.Explicit;
        if (_isRunning)
        {
            TransitionTo(RevealState.Closed);
        }
    }

    public void Dispose()
    {
        _pendingDelay?.Dispose();
        _pendingDelay = null;
        _isRunning    = false;
        _state        = RevealState.Closed;
    }

    private void CompleteOpening()
    {
        _pendingDelay = null;
        if (_state == RevealState.Opening && !_isSuppressed)
        {
            Open();
        }
    }

    private void CompleteClosing()
    {
        _pendingDelay = null;
        if (_state == RevealState.Closing)
        {
            TransitionTo(RevealState.Closed);
        }
    }

    private void Open()
    {
        TransitionTo(RevealState.Open);
        EvaluateHolds();
    }

    private void EvaluateHolds()
    {
        if (_state == RevealState.Open && _holds == FullScreenTitleBarHolds.None)
        {
            if (LeaveDelay <= TimeSpan.Zero)
            {
                TransitionTo(RevealState.Closed);
                return;
            }

            TransitionTo(RevealState.Closing);
            _pendingDelay = _schedule(LeaveDelay, CompleteClosing);
        }
        else if (_state == RevealState.Closing && _holds != FullScreenTitleBarHolds.None)
        {
            TransitionTo(RevealState.Open);
        }
    }

    private void TransitionTo(RevealState state)
    {
        _pendingDelay?.Dispose();
        _pendingDelay = null;
        var wasOpen = IsOpen;
        _state = state;
        var isOpen = IsOpen;
        if (wasOpen != isOpen)
        {
            _openChanged(isOpen);
        }
    }

    private static IDisposable ScheduleOnDispatcher(TimeSpan delay, Action action)
    {
        return DispatcherTimer.RunOnce(action, delay, DispatcherPriority.Input);
    }
}
