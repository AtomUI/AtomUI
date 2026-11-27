using AtomUI.Native;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal interface IWindowChromeManager
{
    bool UsesCustomResizer { get; }

    bool SupportsPinCaptionButton { get; }

    /// <summary>
    /// Whether the platform can currently take this window into full screen.
    /// </summary>
    bool SupportsFullScreen { get; }

    Action? PrepareInitialShowState();

    void HandleFrameShadowChanged(BoxShadows frameShadow);

    void ConfigureTitleBarHeightHint(double height);

    void HandlePropertyChanged(AvaloniaProperty property);

    void UpdateFrameGeometry();

    /// <summary>
    /// Starts tracking the chrome the platform presents by itself over a full-screen window.
    /// </summary>
    /// <param name="metricsChanged">
    /// Invoked on the UI thread whenever <see cref="IFullScreenNativeChrome.Metrics" /> changes; it is never
    /// invoked when <see langword="null" /> is returned.
    /// </param>
    /// <returns>
    /// <c>null</c> when the platform presents no chrome of its own in full screen.
    /// </returns>
    IFullScreenNativeChrome? AttachFullScreenNativeChrome(Action metricsChanged);
}

internal static class WindowChromeManager
{
    public static IWindowChromeManager? Attach(Window window)
    {
        if (OperatingSystem.IsLinux())
        {
            return AbstractLinuxWindowChromeManager.Attach(window);
        }

        if (OperatingSystem.IsWindows())
        {
            return WindowsWindowChromeManager.Attach(window);
        }

        if (OperatingSystem.IsMacOS())
        {
            return MacOSWindowChromeManager.Attach(window);
        }

        return null;
    }
}
