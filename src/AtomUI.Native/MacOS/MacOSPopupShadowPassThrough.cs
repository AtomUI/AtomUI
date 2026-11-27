using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Native.MacOS;

/// <summary>
/// The input state of a window-based popup on macOS.
/// </summary>
internal enum MacOSPopupInputState
{
    /// <summary>
    /// The window receives mouse events as usual and the cursor is not polled.
    /// </summary>
    Receive,

    /// <summary>
    /// The whole window passes events through while a motion plays and the cursor is not polled. When the motion
    /// ends, <see cref="IPopupShadowHitTest.Invalidate" /> decides the state again.
    /// </summary>
    Suspended,

    /// <summary>
    /// The cursor is on the shadow: the whole window passes events through and the cursor is polled. Receiving
    /// resumes when the cursor returns to the content or leaves the window.
    /// </summary>
    PassThrough
}

/// <summary>
/// Shadow pass-through for window-based popups on macOS.
/// <para>
/// AppKit has no input region that is independent of the visible content, so in a transparent window the shadow
/// pixels with an alpha greater than 0 still receive mouse events. The only switch is the window-wide
/// <c>ignoresMouseEvents</c> flag: when the pointer moves onto the shadow the whole window passes the events
/// through to the window below, in the same process or in another one. The popup then receives no mouse events,
/// so the cursor is read once per frame from <c>mouseLocationOutsideOfEventStream</c>, and receiving resumes
/// when the cursor returns to the content shape or leaves the window.
/// </para>
/// <para>
/// Entering the shadow is detected from the raw pointer moves of the popup window
/// (<see cref="IInputManager.Process" />) and not from routed events: the shadow takes no part in hit testing,
/// so while the pointer is on the shadow Avalonia finds no target element and raises no <c>PointerMoved</c>.
/// </para>
/// <para>
/// Avalonia.Native implements <c>SetHitTestVisible</c> through <c>setIgnoresMouseEvents:</c> as well, so a false
/// <see cref="InputElement.IsHitTestVisible" /> is handled as the suspended state. That flag also decides when
/// receiving resumes, which leaves the setting of Avalonia untouched.
/// </para>
/// </summary>
internal sealed class MacOSPopupShadowPassThrough : IPopupShadowHitTest
{
    // Polls the cursor once per frame, about 60 Hz, the same as the second layer on Windows.
    private static readonly TimeSpan PassThroughPollInterval = TimeSpan.FromMilliseconds(16);

    private const RawInputModifiers MouseButtonsMask =
        RawInputModifiers.LeftMouseButton | RawInputModifiers.RightMouseButton | RawInputModifiers.MiddleMouseButton |
        RawInputModifiers.XButton1MouseButton | RawInputModifiers.XButton2MouseButton;

    private readonly TopLevel _popupRoot;
    private readonly IntPtr _nsWindow;
    private readonly Func<bool> _isInputSuspended;
    private readonly Func<Point, bool> _isInContentShape;
    private readonly IDisposable? _rawInputSubscription;
    private DispatcherTimer? _passThroughTimer;
    // null means that no value was sent yet, so the first state is always sent and the window is never read.
    private bool? _isIgnoringMouseEvents;
    private bool _isDisposed;

    [SupportedOSPlatform("macos")]
    private MacOSPopupShadowPassThrough(
        TopLevel popupRoot,
        IntPtr nsWindow,
        Func<bool> isInputSuspended,
        Func<Point, bool> isInContentShape)
    {
        _popupRoot        = popupRoot;
        _nsWindow         = nsWindow;
        _isInputSuspended = isInputSuspended;
        _isInContentShape = isInContentShape;
        _rawInputSubscription = AvaloniaLocator.Current.GetService<IInputManager>()?.Process.Subscribe(new AnonymousObserver<RawInputEventArgs>(HandleRawInput));
        Invalidate();
    }

    [SupportedOSPlatform("macos")]
    public static MacOSPopupShadowPassThrough? TryCreate(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<Point, bool> isInContentShape)
    {
        ArgumentNullException.ThrowIfNull(popupRoot);
        ArgumentNullException.ThrowIfNull(isInputSuspended);
        ArgumentNullException.ThrowIfNull(isInContentShape);
        if (TryGetNSWindow(popupRoot) is not { } nsWindow)
        {
            return null;
        }

        return new MacOSPopupShadowPassThrough(popupRoot, nsWindow, isInputSuspended, isInContentShape);
    }

    [SupportedOSPlatform("macos")]
    public static void SuspendInputBeforeShow(TopLevel popupRoot)
    {
        if (TryGetNSWindow(popupRoot) is { } nsWindow)
        {
            WindowUtilsInterop.objc_msgSend_void_bool(nsWindow, WindowUtilsInterop.SetIgnoresMouseEventsSelector, true);
        }
    }

    /// <summary>
    /// Decides the input state of a popup window from its suspension flag, the cursor position and its drag state.
    /// </summary>
    /// <param name="isInputSuspended">
    /// Input must stay off for the whole window, for example while a motion plays or while
    /// <see cref="InputElement.IsHitTestVisible" /> is false. The result is then
    /// <see cref="MacOSPopupInputState.Suspended" /> whatever the other arguments are.
    /// </param>
    /// <param name="isCursorInWindow">
    /// The cursor is inside the bounds of the popup window.
    /// </param>
    /// <param name="isCursorInContent">
    /// The cursor is inside the content shape of the popup.
    /// </param>
    /// <param name="isDragging">
    /// This window owns a button drag, that is a button was pressed inside the content. During a drag AppKit keeps
    /// sending events to the window that received the press, so passing the whole window through midway would lose
    /// the drag and the button-up events.
    /// </param>
    /// <returns>
    /// <see cref="MacOSPopupInputState.Suspended" /> while input is suspended,
    /// <see cref="MacOSPopupInputState.PassThrough" /> when the cursor is inside the window but outside the content
    /// and no drag is in progress, and <see cref="MacOSPopupInputState.Receive" /> otherwise.
    /// </returns>
    internal static MacOSPopupInputState ResolveState(bool isInputSuspended, bool isCursorInWindow, bool isCursorInContent,
                                                      bool isDragging)
    {
        if (isInputSuspended)
        {
            return MacOSPopupInputState.Suspended;
        }

        return !isCursorInWindow || isCursorInContent || isDragging
            ? MacOSPopupInputState.Receive
            : MacOSPopupInputState.PassThrough;
    }

    public void Invalidate()
    {
        if (!OperatingSystem.IsMacOS() || _isDisposed || _popupRoot.PlatformImpl is null)
        {
            // Polling cannot recover once the native window is destroyed, so it is stopped right away.
            StopPassThroughPolling();
            return;
        }

        // mouseLocationOutsideOfEventStream still returns the live cursor while the window ignores mouse
        // events. Window-based coordinates have their origin at the bottom left and DesktopScaling is 1, so
        // flipping Y gives client-area DIPs.
        var location = WindowUtilsInterop.objc_msgSend_point(
            _nsWindow, WindowUtilsInterop.MouseLocationOutsideOfEventStreamSelector);
        var point = new Point(location.X, _popupRoot.ClientSize.Height - location.Y);
        var isInWindow = new Rect(_popupRoot.ClientSize).Contains(point);
        // A button pressed while passing through belongs to the window below, not to this window.
        Apply(ResolveState(IsInputSuspended(), isInWindow, isInWindow && _isInContentShape(point), false));
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        StopPassThroughPolling();
        _rawInputSubscription?.Dispose();
        // When the popup closes, Avalonia may already have destroyed the native window and set PlatformImpl
        // to null. No message is sent to the NSWindow then.
        if (OperatingSystem.IsMacOS() && _popupRoot.PlatformImpl is not null)
        {
            SetIgnoresMouseEvents(!_popupRoot.IsHitTestVisible);
        }
    }

    private bool IsInputSuspended() => _isInputSuspended() || !_popupRoot.IsHitTestVisible;

    private void HandleRawInput(RawInputEventArgs e)
    {
        if (!OperatingSystem.IsMacOS() ||
            _isDisposed ||
            e is not RawPointerEventArgs { Type: RawPointerEventType.Move } pointerEvent ||
            _popupRoot.PlatformImpl is null ||
            pointerEvent.Root != _popupRoot.GetPresentationSource() as IInputRoot)
        {
            return;
        }

        // This window is receiving input here. During a button drag AppKit also sends the moves outside
        // the window to it, so Position decides whether the pointer is inside the window.
        var point      = pointerEvent.Position;
        var isInWindow = new Rect(_popupRoot.ClientSize).Contains(point);
        var isDragging = (pointerEvent.InputModifiers & MouseButtonsMask) != RawInputModifiers.None;
        Apply(ResolveState(IsInputSuspended(), isInWindow, isInWindow && _isInContentShape(point), isDragging));
    }

    [SupportedOSPlatform("macos")]
    private void Apply(MacOSPopupInputState state)
    {
        SetIgnoresMouseEvents(state != MacOSPopupInputState.Receive);
        if (state == MacOSPopupInputState.PassThrough)
        {
            StartPassThroughPolling();
        }
        else
        {
            StopPassThroughPolling();
        }
    }

    [SupportedOSPlatform("macos")]
    private void SetIgnoresMouseEvents(bool isIgnoring)
    {
        if (_isIgnoringMouseEvents == isIgnoring)
        {
            return;
        }

        _isIgnoringMouseEvents = isIgnoring;
        WindowUtilsInterop.objc_msgSend_void_bool(_nsWindow, WindowUtilsInterop.SetIgnoresMouseEventsSelector, isIgnoring);
    }

    private void StartPassThroughPolling()
    {
        if (_passThroughTimer is not null)
        {
            return;
        }

        _passThroughTimer = new DispatcherTimer(PassThroughPollInterval, DispatcherPriority.Input, HandlePassThroughPoll);
        _passThroughTimer.Start();
    }

    private void HandlePassThroughPoll(object? sender, EventArgs e)
    {
        Invalidate();
    }

    private void StopPassThroughPolling()
    {
        if (_passThroughTimer is null)
        {
            return;
        }

        _passThroughTimer.Stop();
        _passThroughTimer.Tick -= HandlePassThroughPoll;
        _passThroughTimer = null;
    }

    [SupportedOSPlatform("macos")]
    private static IntPtr? TryGetNSWindow(TopLevel popupRoot)
    {
        if (popupRoot.TryGetPlatformHandle() is not { Handle: var handle } || handle == IntPtr.Zero)
        {
            return null;
        }

        return handle;
    }
}
