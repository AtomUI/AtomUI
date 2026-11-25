using AtomUI.Media;
using AtomUI.Native;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaWindow = Avalonia.Controls.Window;

internal sealed class MacOSWindowChromeManager : IWindowChromeManager
{
    private readonly Window _window;
    private bool _initialShowStatePrepared;

    private MacOSWindowChromeManager(Window window)
    {
        _window = window;
    }

    internal static MacOSWindowChromeManager Attach(Window window)
    {
        return new MacOSWindowChromeManager(window);
    }

    public bool UsesCustomResizer => false;

    public bool SupportsPinCaptionButton => true;

    // AppKit marks an owned window as a full-screen auxiliary window and never takes it into native full screen.
    public bool SupportsFullScreen => _window.Owner is null;

    // AppKit slides the menu bar and the window's title strip in over a full-screen window.
    public IFullScreenNativeChrome? AttachFullScreenNativeChrome(Action metricsChanged)
    {
        return _window.AttachFullScreenNativeChrome(metricsChanged);
    }

    public Action? PrepareInitialShowState()
    {
        if (_initialShowStatePrepared || _window.IsVisible)
        {
            return null;
        }

        _initialShowStatePrepared = true;
        _window.PreparePlatformChromeInitialShowLayout();
        UpdateFrameGeometry();
        return null;
    }

    public void HandleFrameShadowChanged(BoxShadows frameShadow)
    {
        _window.FrameShadowThickness = frameShadow.Thickness();
    }

    public void ConfigureTitleBarHeightHint(double height)
    {
        _window.SetCurrentValue(AvaloniaWindow.ExtendClientAreaTitleBarHeightHintProperty, height);
    }

    public void HandlePropertyChanged(AvaloniaProperty property)
    {
        if (property == AvaloniaWindow.WindowStateProperty ||
            property == AvaloniaWindow.CanResizeProperty ||
            property == AvaloniaWindow.WindowDecorationMarginProperty ||
            property == Window.FrameShadowThicknessProperty ||
            property == Window.IsCsdEnabledProperty)
        {
            UpdateFrameGeometry();
        }
    }

    public void UpdateFrameGeometry()
    {
        if (_window.IsCsdEnabled)
        {
            if (OperatingSystem.IsMacOS())
            {
                _window.SetMacOsResizeIndicatorVisible(true);
            }
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            _window.SetMacOsResizeIndicatorVisible(false);
        }
    }
}
