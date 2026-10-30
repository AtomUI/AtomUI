using System.Runtime.Versioning;
using AtomUI.Media;
using AtomUI.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

using AvaloniaWindow = Avalonia.Controls.Window;

[SupportedOSPlatform("windows")]
internal sealed class WindowsWindowChromeManager : IWindowChromeManager
{
    private readonly Window _window;
    private INativeWindowBackgroundHook? _backgroundHook;
    private IDisposable? _legacyFrameMarginHook;
    private bool _initialShowStatePrepared;
    private bool _frameGeometryUpdateQueued;

    private WindowsWindowChromeManager(Window window)
    {
        _window = window;
        _window.ScalingChanged += HandleScalingChanged;
        _window.Closed += HandleWindowClosed;
    }

    internal static WindowsWindowChromeManager Attach(Window window)
    {
        return new WindowsWindowChromeManager(window);
    }

    public bool UsesCustomResizer => false;

    public bool SupportsPinCaptionButton => true;

    public Action? PrepareInitialShowState()
    {
        if (_initialShowStatePrepared || _window.IsVisible)
        {
            return null;
        }

        _initialShowStatePrepared = true;
        // Let the initial show path create the HWND before first paint so native
        // background fill and DWM dark-frame setup can target a real platform handle.
        _window.PreparePlatformChromeInitialShowHandle();
        EnsureBackgroundHook();
        _legacyFrameMarginHook ??= NativeWindowFrame.TryAttachLegacyFrameMarginHook(_window);
        UpdateNativeBackgroundHook();
        _backgroundHook?.PaintClientArea();
        return null;
    }

    public void HandleFrameShadowChanged(BoxShadows frameShadow)
    {
        SetFrameShadowThickness(frameShadow.Thickness());
        RequestFrameGeometryUpdate();
    }

    public void ConfigureTitleBarHeightHint(double height)
    {
        _window.SetCurrentValue(AvaloniaWindow.ExtendClientAreaTitleBarHeightHintProperty, height);
    }

    public void HandlePropertyChanged(AvaloniaProperty property)
    {
        if (property == AvaloniaWindow.WindowStateProperty ||
            property == AvaloniaWindow.ExtendClientAreaToDecorationsHintProperty ||
            property == AvaloniaWindow.WindowDecorationMarginProperty ||
            property == AvaloniaWindow.IsExtendedIntoWindowDecorationsProperty ||
            property == Window.IsCsdEnabledProperty)
        {
            RequestFrameGeometryUpdate();
        }

        if (property == TemplatedControl.BackgroundProperty ||
            property == TopLevel.TransparencyBackgroundFallbackProperty ||
            property == TopLevel.RequestedThemeVariantProperty ||
            property == Window.IsCsdEnabledProperty)
        {
            UpdateNativeBackgroundHook();
        }
    }

    public void UpdateFrameGeometry()
    {
        var thickness = _window.OsType == OsType.Windows && ShouldUseVisibleFrameBorder(
                _window.IsCsdEnabled,
                _window.IsExtendedIntoWindowDecorations,
                _window.WindowState)
            ? ResolveVisibleFrameBorderThickness()
            : default;
        _window.VisibleFrameBorderThickness = thickness;
    }

    internal static bool ShouldUseVisibleFrameBorder(
        bool isCsdEnabled,
        bool isExtendedIntoWindowDecorations,
        WindowState windowState)
    {
        return isCsdEnabled &&
               isExtendedIntoWindowDecorations &&
               windowState == WindowState.Normal;
    }

    private Thickness ResolveVisibleFrameBorderThickness()
    {
        var handle = _window.TryGetPlatformHandle();
        if (handle is null || handle.Handle == IntPtr.Zero)
        {
            return default;
        }

        var native = WindowUtilsWindows.GetVisibleFrameBorderThicknessWindows(
            handle.Handle,
            _window.RenderScaling);
        var drawn = _window.GetDrawnDecorationsFrameThickness();
        return new Thickness(
            native.Left + drawn.Left,
            native.Top + drawn.Top,
            native.Right + drawn.Right,
            native.Bottom + drawn.Bottom);
    }

    private void HandleScalingChanged(object? sender, EventArgs e)
    {
        RequestFrameGeometryUpdate();
    }

    private void HandleWindowClosed(object? sender, EventArgs e)
    {
        _backgroundHook?.Dispose();
        _backgroundHook = null;
        _legacyFrameMarginHook?.Dispose();
        _legacyFrameMarginHook = null;
        _window.ScalingChanged -= HandleScalingChanged;
        _window.Closed -= HandleWindowClosed;
    }

    private void UpdateNativeBackgroundHook()
    {
        if (_backgroundHook is null)
        {
            return;
        }

        _backgroundHook.SetBackgroundColor(ResolveNativeBackgroundColor());
    }

    private void EnsureBackgroundHook()
    {
        _backgroundHook ??= NativeWindowBackground.TryAttachWindowsBackgroundHook(_window);
    }

    private Color? ResolveNativeBackgroundColor()
    {
        return TryGetBrushColor(_window.Background) ??
               TryGetBrushColor(_window.TransparencyBackgroundFallback);
    }

    private void RequestFrameGeometryUpdate()
    {
        if (!_window.IsVisible || _window.PlatformImpl is null)
        {
            UpdateFrameGeometry();
            return;
        }

        if (_frameGeometryUpdateQueued)
        {
            return;
        }

        _frameGeometryUpdateQueued = true;
        _window.Dispatcher.Post(
            ApplyQueuedFrameGeometryUpdate,
            DispatcherPriority.Render);
    }

    private void ApplyQueuedFrameGeometryUpdate()
    {
        _frameGeometryUpdateQueued = false;
        UpdateFrameGeometry();
    }

    private void SetFrameShadowThickness(Thickness thickness)
    {
        if (!AreThicknessClose(_window.FrameShadowThickness, thickness))
        {
            _window.FrameShadowThickness = thickness;
        }
    }

    private static bool AreThicknessClose(Thickness left, Thickness right)
    {
        const double epsilon = 0.0001;
        return Math.Abs(left.Left - right.Left) <= epsilon &&
               Math.Abs(left.Top - right.Top) <= epsilon &&
               Math.Abs(left.Right - right.Right) <= epsilon &&
               Math.Abs(left.Bottom - right.Bottom) <= epsilon;
    }

    private static Color? TryGetBrushColor(IBrush? brush)
    {
        return brush is ISolidColorBrush solidBrush
            ? solidBrush.Color
            : null;
    }
}
