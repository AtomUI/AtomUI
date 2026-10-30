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
/// macOS 窗口型 Popup 的输入状态。
/// </summary>
internal enum MacOSPopupInputState
{
    /// <summary>窗口正常接收鼠标事件，不轮询光标。</summary>
    Receive,

    /// <summary>动效期间整窗穿透，不轮询；动效结束由 <see cref="IPopupShadowHitTest.Invalidate"/> 重新判定。</summary>
    Suspended,

    /// <summary>光标位于阴影上：整窗穿透并轮询光标，回到内容或离开窗口时恢复。</summary>
    PassThrough
}

/// <summary>
/// macOS 窗口型 Popup 的阴影穿透。
/// <para>
/// AppKit 没有独立于可见内容的输入区域，透明窗口中 alpha 大于 0 的阴影像素仍会接收鼠标，只能整窗切换
/// <c>ignoresMouseEvents</c>：指针移到阴影上时整窗穿透，事件交给下方窗口（同进程与跨进程一致）；
/// 穿透后 Popup 收不到鼠标事件，改为按帧读取 <c>mouseLocationOutsideOfEventStream</c>，
/// 光标回到内容形状或离开窗口时恢复接收。
/// </para>
/// <para>
/// 进入阴影的判定监听 Popup 窗口的原始指针移动（<see cref="IInputManager.Process"/>），不用路由事件：
/// 阴影不参与命中，指针位于阴影上时 Avalonia 找不到目标元素，不会引发 <c>PointerMoved</c>。
/// </para>
/// <para>
/// Avalonia.Native 的 <c>SetHitTestVisible</c> 同样通过 <c>setIgnoresMouseEvents:</c> 实现整窗不可命中，
/// 因此 <see cref="TopLevel.IsHitTestVisible"/> 为 false 时按暂停处理，恢复时也以它为准，不覆盖 Avalonia 的设置。
/// </para>
/// </summary>
internal sealed class MacOSPopupShadowPassThrough : IPopupShadowHitTest
{
    // 按帧轮询光标，约 60Hz，与 Windows 第二层一致
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
    // null 表示尚未下发：首次状态必定下发，不依赖读取窗口当前值
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

    /// <param name="isDragging">
    /// 本窗口持有按键拖拽（在内容内按下）。拖拽期间 AppKit 持续把事件发给按下时的窗口，中途整窗穿透会丢失拖拽与抬起事件。
    /// </param>
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
            // 原生窗口已销毁时轮询无从恢复，直接停止
            StopPassThroughPolling();
            return;
        }

        // 窗口忽略鼠标事件时仍返回实时光标；窗口基准坐标原点在左下，DesktopScaling 为 1，翻转 Y 即为客户区 DIP
        var location = WindowUtilsInterop.objc_msgSend_point(
            _nsWindow, WindowUtilsInterop.MouseLocationOutsideOfEventStreamSelector);
        var point = new Point(location.X, _popupRoot.ClientSize.Height - location.Y);
        var isInWindow = new Rect(_popupRoot.ClientSize).Contains(point);
        // 穿透期间按下的按键属于下方窗口，不影响本窗口状态
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
        // Popup 关闭时 Avalonia 可能已销毁原生窗口并置空 PlatformImpl，此时不再向 NSWindow 发消息
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

        // 本窗口正在接收输入；按键拖拽时 AppKit 也把窗口外的移动发给本窗口，因此按 Position 判定是否在窗口内
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
