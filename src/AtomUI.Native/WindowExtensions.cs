using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

internal static class WindowExtensions
{
    public static void SetWindowIgnoreMouseEvents(this WindowBase window, bool flag)
    {
        var handle = window.PlatformImpl?.Handle?.Handle;
        Debug.Assert(handle is not null);
        if (OperatingSystem.IsWindows())
        {
            WindowUtilsWindows.SetWindowIgnoreMouseEventsWindows(handle.Value, flag);
        }
        else if (OperatingSystem.IsMacOS())
        {
            WindowUtilsMacOS.SetWindowIgnoreMouseEventsMacOS(handle.Value, flag);
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform: {RuntimeInformation.OSDescription}");
        }
    }

    public static bool IsWindowIgnoreMouseEvents(this WindowBase window)
    {
        var handle = window.PlatformImpl?.Handle?.Handle;
        Debug.Assert(handle is not null);
        if (OperatingSystem.IsWindows())
        {
            return WindowUtilsWindows.IsWindowIgnoreMouseEventsWindows(handle.Value);
        }
        if (OperatingSystem.IsMacOS())
        {
            return WindowUtilsMacOS.IsWindowIgnoreMouseEventsMacOS(handle.Value);
        }
        throw new PlatformNotSupportedException($"Unsupported platform: {RuntimeInformation.OSDescription}");
    }

    /// <summary>
    /// Gets the default title bar height of the system.
    /// When <c>ExtendClientAreaTitleBarHeightHint</c> is -1, this method can be used to get the actual title bar
    /// height.
    /// </summary>
    /// <returns>
    /// The title bar height, or <c>null</c> when it cannot be obtained.
    /// </returns>
    public static double? GetSystemTitleBarHeight(this Window window)
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowUtilsWindows.GetSystemTitleBarHeightWindows(window);
        }
        if (OperatingSystem.IsMacOS())
        {
            return WindowUtilsMacOS.GetSystemTitleBarHeightMacOS(window);
        }
        if (OperatingSystem.IsLinux())
        {
            return WindowUtilsLinux.GetSystemTitleBarHeightLinux(window);
        }
        return null;
    }

    [SupportedOSPlatform("windows")]
    public static void SetWindowsCsdFrameDarkMode(this WindowBase window, bool isDarkMode)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var handle = window.TryGetPlatformHandle();
        if (handle is null || handle.Handle == IntPtr.Zero)
        {
            return;
        }

        WindowUtilsWindows.SetWindowFrameDarkModeWindows(handle.Handle, isDarkMode);
    }

    [SupportedOSPlatform("macos")]
    public static void SetMacOsResizeIndicatorVisible(this Window window, bool isVisible)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        WindowUtilsMacOS.SetResizeIndicatorVisible(window, isVisible);
    }
    
    /// <summary>
    /// Controls the input region of a window (the X11 SHAPE input region).
    /// Sets the input region of the window to the given rectangle, in device pixels; mouse events outside the
    /// rectangle pass through to the windows below.
    /// Mainly used to make clicks on the shadow area of <c>WindowDrawnDecorations</c> pass through.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public static void SetWindowInputRectangle(this Window window, int x, int y, int width, int height)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var handle = window.PlatformImpl?.Handle?.Handle;
        Debug.Assert(handle is not null);
        WindowUtilsLinux.SetInputRectangle(handle.Value, x, y, width, height);
    }

    /// <summary>
    /// Resets the input region of the window to the whole client area, given in device pixels.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public static void ResetWindowInputRegion(this Window window, int width, int height)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var handle = window.PlatformImpl?.Handle?.Handle;
        Debug.Assert(handle is not null);
        WindowUtilsLinux.ResetInputRegion(handle.Value, width, height);
    }

    [SupportedOSPlatform("linux")]
    public static void ConfigureLinuxInitialWindowGeometry(
        this Window window,
        PixelPoint position,
        Size clientSize,
        double scaling)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var handle = window.PlatformImpl?.Handle?.Handle;
        Debug.Assert(handle is not null);

        scaling = Math.Max(1, scaling);
        var width    = ToPixelLength(clientSize.Width, scaling);
        var height   = ToPixelLength(clientSize.Height, scaling);
        var minWidth = ToPixelLength(window.MinWidth, scaling);
        var minHeight = ToPixelLength(window.MinHeight, scaling);
        var maxWidth = ToOptionalPixelLength(window.MaxWidth, scaling);
        var maxHeight = ToOptionalPixelLength(window.MaxHeight, scaling);

        WindowUtilsLinux.ConfigureInitialWindowGeometry(
            handle.Value,
            position.X,
            position.Y,
            width,
            height,
            minWidth,
            minHeight,
            maxWidth,
            maxHeight);
    }

    [SupportedOSPlatform("linux")]
    public static void SetLinuxX11CsdFrameExtents(this Window window, Thickness frameExtents)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var handle = window.TryGetPlatformHandle();
        if (handle is null || handle.HandleDescriptor != "XID")
        {
            return;
        }

        var scaling = window.RenderScaling <= 0 ? 1.0 : window.RenderScaling;
        WindowUtilsLinux.SetX11CsdFrameExtents(
            handle.Handle,
            ToPixelMargin(frameExtents.Left, scaling),
            ToPixelMargin(frameExtents.Top, scaling),
            ToPixelMargin(frameExtents.Right, scaling),
            ToPixelMargin(frameExtents.Bottom, scaling));
    }

    private static int ToPixelLength(double value, double scaling)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            return 1;
        }
        return Math.Max(1, (int)(value * scaling));
    }

    private static int? ToOptionalPixelLength(double value, double scaling)
    {
        if (!double.IsFinite(value) || value <= 0 || value > 100_000)
        {
            return null;
        }
        return ToPixelLength(value, scaling);
    }

    private static int ToPixelMargin(double value, double scaling)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            return 0;
        }
        return Math.Max(0, (int)Math.Ceiling(value * scaling));
    }
}
