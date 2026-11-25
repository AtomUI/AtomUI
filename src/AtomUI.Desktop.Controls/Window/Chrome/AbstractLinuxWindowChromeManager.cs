using System.Runtime.Versioning;
using AtomUI.Media;
using AtomUI.Native;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaWindow = Avalonia.Controls.Window;

internal enum LinuxWindowingBackend
{
    Other,
    X11,
    Wayland
}

[SupportedOSPlatform("linux")]
internal abstract class AbstractLinuxWindowChromeManager : IWindowChromeManager
{
    private readonly Window _window;
    private bool _initialShowStatePrepared;
    private bool _frameGeometryUpdateQueued;
    private bool _hasPendingFrameShadowThickness;
    private Thickness _pendingFrameShadowThickness;
    private bool _hasPendingTitleBarHeightHint;
    private double _pendingTitleBarHeightHint;

    protected AbstractLinuxWindowChromeManager(Window window)
    {
        _window = window;
    }

    protected Window Window => _window;

    public virtual bool UsesCustomResizer => !Window.IsCsdEnabled;

    public bool SupportsPinCaptionButton => IsPinCaptionButtonSupported(ResolveBackend(
        AvaloniaLocator.Current.GetService<AtomUIWindowingPlatformOptions>()?.Platform,
        Window.PlatformImpl?.Handle?.HandleDescriptor,
        Window.PlatformImpl?.GetType().Assembly.GetName().Name));

    // Window managers and compositors accept full-screen requests for owned windows; a rejection is reported back
    // through the window state.
    public bool SupportsFullScreen => true;

    // X11 window managers and Wayland compositors present no chrome of their own over a full-screen window.
    public IFullScreenNativeChrome? AttachFullScreenNativeChrome(Action metricsChanged) => null;

    public static AbstractLinuxWindowChromeManager Attach(Window window)
    {
        var configuredPlatform = AvaloniaLocator.Current.GetService<AtomUIWindowingPlatformOptions>()?.Platform;
        var platformImpl       = window.PlatformImpl;
        var backend = ResolveBackend(
            configuredPlatform,
            platformImpl?.Handle?.HandleDescriptor,
            platformImpl?.GetType().Assembly.GetName().Name);
        AbstractLinuxWindowChromeManager manager = backend switch
        {
            LinuxWindowingBackend.X11 => new X11WindowChromeManager(window),
            LinuxWindowingBackend.Wayland => new WaylandWindowChromeManager(window),
            _ => new GenericLinuxWindowChromeManager(window)
        };
        manager.Attach();
        return manager;
    }

    internal static LinuxWindowingBackend ResolveBackend(
        AtomUIWindowingPlatform? configuredPlatform,
        string? handleDescriptor,
        string? platformAssemblyName)
    {
        if (configuredPlatform is AtomUIWindowingPlatform.X11)
        {
            return LinuxWindowingBackend.X11;
        }

        if (configuredPlatform is AtomUIWindowingPlatform.Wayland)
        {
            return LinuxWindowingBackend.Wayland;
        }

        if (string.Equals(handleDescriptor, "XID", StringComparison.Ordinal))
        {
            return LinuxWindowingBackend.X11;
        }

        return string.Equals(platformAssemblyName, "Avalonia.Wayland", StringComparison.Ordinal)
            ? LinuxWindowingBackend.Wayland
            : LinuxWindowingBackend.Other;
    }

    internal static bool IsWayland(Window window)
    {
        if (!OperatingSystem.IsLinux())
        {
            return false;
        }

        var platformImpl = window.PlatformImpl;
        return ResolveBackend(
                   AvaloniaLocator.Current.GetService<AtomUIWindowingPlatformOptions>()?.Platform,
                   platformImpl?.Handle?.HandleDescriptor,
                   platformImpl?.GetType().Assembly.GetName().Name) == LinuxWindowingBackend.Wayland;
    }

    internal static bool IsPinCaptionButtonSupported(LinuxWindowingBackend backend)
    {
        return backend != LinuxWindowingBackend.Wayland;
    }

    private void Attach()
    {
        _window.ScalingChanged += HandleScalingChanged;
        if (_window.PlatformImpl is { } platformImpl)
        {
            platformImpl.DrawnDecorationsRequestChanged += HandleDrawnDecorationsRequestChanged;
        }

        AttachPlatformHooks();
    }

    protected virtual void AttachPlatformHooks()
    {
    }

    public Action? PrepareInitialShowState()
    {
        if (_initialShowStatePrepared || _window.IsVisible || _window.PlatformImpl is null)
        {
            return null;
        }

        _initialShowStatePrepared = true;
        _window.PreparePlatformChromeInitialShowLayout();
        EnsureResizeMinimumSize();
        return PrepareInitialPlatformShowState();
    }

    protected virtual Action? PrepareInitialPlatformShowState()
    {
        return null;
    }

    public void HandleFrameShadowChanged(BoxShadows frameShadow)
    {
        var thickness = frameShadow.Thickness();
        if (!ShouldDeferFrameGeometryUpdate())
        {
            ApplyFrameShadowThickness(thickness);
            return;
        }

        _pendingFrameShadowThickness    = thickness;
        _hasPendingFrameShadowThickness = true;
        RequestFrameGeometryUpdate();
    }

    public void ConfigureTitleBarHeightHint(double height)
    {
        if (!ShouldDeferCsdTitleBarHeightHint())
        {
            ApplyTitleBarHeightHint(height);
            return;
        }

        _pendingTitleBarHeightHint    = height;
        _hasPendingTitleBarHeightHint = true;
        RequestFrameGeometryUpdate();
    }

    public void HandlePropertyChanged(AvaloniaProperty property)
    {
        if (property == AvaloniaWindow.WindowStateProperty ||
            property == AvaloniaWindow.CanResizeProperty ||
            property == AvaloniaWindow.WindowDecorationMarginProperty ||
            property == Layoutable.MinWidthProperty ||
            property == Layoutable.MinHeightProperty ||
            property == Layoutable.MaxWidthProperty ||
            property == Layoutable.MaxHeightProperty ||
            property == Window.FrameShadowThicknessProperty ||
            property == Window.TitleBarHeightProperty ||
            property == Window.IsCsdEnabledProperty)
        {
            RequestFrameGeometryUpdate();
        }
    }

    public void UpdateFrameGeometry()
    {
        EnsureResizeMinimumSize();
        UpdatePlatformFrameGeometry();
        UpdateVisibleFrameBorderThickness();
    }

    protected virtual void UpdatePlatformFrameGeometry()
    {
    }

    private void HandleScalingChanged(object? sender, EventArgs e)
    {
        RequestFrameGeometryUpdate();
    }

    protected void RequestFrameGeometryUpdate()
    {
        if (!ShouldDeferFrameGeometryUpdate())
        {
            ApplyPendingFrameGeometryInputs();
            UpdateFrameGeometry();
            return;
        }

        if (_frameGeometryUpdateQueued)
        {
            return;
        }

        _frameGeometryUpdateQueued = true;
        _window.Dispatcher.Post(ApplyPendingFrameGeometryUpdate, Avalonia.Threading.DispatcherPriority.Render);
    }

    private void HandleDrawnDecorationsRequestChanged()
    {
        _window.RefreshPlatformCsdStatus();
        RequestFrameGeometryUpdate();
    }

    private bool ShouldDeferFrameGeometryUpdate()
    {
        return _window.IsVisible && _window.PlatformImpl is not null;
    }

    private bool ShouldDeferCsdTitleBarHeightHint()
    {
        return _window.IsCsdEnabled && ShouldDeferFrameGeometryUpdate();
    }

    private void ApplyPendingFrameGeometryUpdate()
    {
        ApplyPendingFrameGeometryInputs();
        UpdateFrameGeometry();
        _frameGeometryUpdateQueued = false;
    }

    private void ApplyPendingFrameGeometryInputs()
    {
        if (_hasPendingFrameShadowThickness)
        {
            _hasPendingFrameShadowThickness = false;
            ApplyFrameShadowThickness(_pendingFrameShadowThickness);
        }

        if (_hasPendingTitleBarHeightHint)
        {
            _hasPendingTitleBarHeightHint = false;
            ApplyTitleBarHeightHint(_pendingTitleBarHeightHint);
        }
    }

    private void ApplyFrameShadowThickness(Thickness thickness)
    {
        if (!AreThicknessClose(_window.FrameShadowThickness, thickness))
        {
            _window.FrameShadowThickness = thickness;
        }
    }

    private void ApplyTitleBarHeightHint(double height)
    {
        if (!MathUtils.AreClose(_window.ExtendClientAreaTitleBarHeightHint, height))
        {
            _window.SetCurrentValue(AvaloniaWindow.ExtendClientAreaTitleBarHeightHintProperty, height);
        }
    }

    private void UpdateVisibleFrameBorderThickness()
    {
        var thickness = Window.IsCsdEnabled
            ? Window.GetDrawnDecorationsFrameThickness()
            : default;
        if (!AreThicknessClose(Window.VisibleFrameBorderThickness, thickness))
        {
            Window.VisibleFrameBorderThickness = thickness;
        }
    }

    private void EnsureResizeMinimumSize()
    {
        var titleBarHeight = Math.Max(_window.TitleBar?.DesiredSize.Height ?? 0, _window.TitleBarHeight);
        var minimumSize = CalculateResizeMinimumSize(
            _window.FrameShadowThickness,
            _window.CornerRadius,
            titleBarHeight);

        if (_window.MinWidth <= 0 && minimumSize.Width > 0)
        {
            _window.SetCurrentValue(Layoutable.MinWidthProperty, minimumSize.Width);
        }

        if (_window.MinHeight <= 0 && minimumSize.Height > 0)
        {
            _window.SetCurrentValue(Layoutable.MinHeightProperty, minimumSize.Height);
        }
    }

    internal static Size CalculateResizeMinimumSize(
        Thickness shadow,
        CornerRadius cornerRadius,
        double titleBarHeight)
    {
        var maxCorner = Math.Max(
            Math.Max(cornerRadius.TopLeft, cornerRadius.TopRight),
            Math.Max(cornerRadius.BottomLeft, cornerRadius.BottomRight));

        const double frameBorder = 1;

        var horizontalChrome = shadow.Left + shadow.Right + frameBorder * 2;
        var minResizeWidth = horizontalChrome + maxCorner * 2;

        var verticalChrome = shadow.Top + shadow.Bottom + frameBorder * 2;
        var normalizedTitleBarHeight = double.IsFinite(titleBarHeight) && titleBarHeight > 0
            ? titleBarHeight
            : 0;
        var minResizeHeight = Math.Max(
            verticalChrome + normalizedTitleBarHeight + maxCorner * 2,
            verticalChrome + normalizedTitleBarHeight * 3);

        return new Size(minResizeWidth, minResizeHeight);
    }

    private static bool AreThicknessClose(Thickness left, Thickness right)
    {
        return MathUtils.AreClose(left.Left, right.Left) &&
               MathUtils.AreClose(left.Top, right.Top) &&
               MathUtils.AreClose(left.Right, right.Right) &&
               MathUtils.AreClose(left.Bottom, right.Bottom);
    }
}
