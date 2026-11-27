using System.Runtime.Versioning;
using AtomUI.Native.Windows;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

[SupportedOSPlatform("windows")]
internal static class WindowUtilsWindows
{
    private const int Windows10DarkFrameMinimumBuild = 17763;
    private const int Windows10DarkFrame20H1AttributeBuild = 18985;

    public static void SetWindowIgnoreMouseEventsWindows(IntPtr handle, bool flag)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }

        var styles = WindowUtilsInterop.GetWindowLongPtr(handle, WindowUtilsInterop.GWL_EXSTYLE);
        if (flag)
        {
            styles |= (WindowUtilsInterop.WS_EX_TRANSPARENT | WindowUtilsInterop.WS_EX_LAYERED);
        }
        else
        {
            styles &= ~(WindowUtilsInterop.WS_EX_TRANSPARENT | WindowUtilsInterop.WS_EX_LAYERED);
        }

        WindowUtilsInterop.SetWindowLongPtr(handle, WindowUtilsInterop.GWL_EXSTYLE, styles);
    }
    
    public static bool IsWindowIgnoreMouseEventsWindows(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        var styles = WindowUtilsInterop.GetWindowLongPtr(handle, WindowUtilsInterop.GWL_EXSTYLE);
        return (styles & WindowUtilsInterop.WS_EX_TRANSPARENT) != 0;
    }

    /// <summary>
    /// Gets the default title bar height of the Windows system, from Avalonia's WindowDecorationMargin.
    /// </summary>
    /// <returns>
    /// <c>null</c> when not on Windows, when the window has no handle, or when the top margin is not positive.
    /// </returns>
    public static double? GetSystemTitleBarHeightWindows(Window window)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        // Use the WindowDecorationMargin property that Avalonia provides.
        // Its Top value is the title bar height.
        var margin = window.WindowDecorationMargin;
        return margin.Top > 0 ? margin.Top : null;
    }

    /// <summary>
    /// Gets the visible native frame border in device-independent units.
    /// Windows 11 exposes the actual DWM border thickness; older systems use
    /// the DPI-aware system border metrics as a compatible fallback.
    /// </summary>
    public static Thickness GetVisibleFrameBorderThicknessWindows(
        IntPtr handle,
        double renderScaling)
    {
        if (!OperatingSystem.IsWindows() || handle == IntPtr.Zero)
        {
            return default;
        }

        var scaling = double.IsFinite(renderScaling) && renderScaling > 0
            ? renderScaling
            : 1.0;

        if (WindowUtilsInterop.DwmGetWindowAttribute(
                handle,
                WindowUtilsInterop.DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
                out var visibleBorderPixels,
                sizeof(uint)) == WindowUtilsInterop.S_OK)
        {
            var visibleBorder = visibleBorderPixels / scaling;
            return new Thickness(visibleBorder);
        }

        var dpi = GetWindowDpi(handle);
        var horizontalPixels = GetSystemMetricForDpi(WindowUtilsInterop.SM_CXBORDER, dpi);
        var verticalPixels = GetSystemMetricForDpi(WindowUtilsInterop.SM_CYBORDER, dpi);
        return new Thickness(
            horizontalPixels / scaling,
            verticalPixels / scaling,
            horizontalPixels / scaling,
            verticalPixels / scaling);
    }

    public static void SetWindowFrameDarkModeWindows(IntPtr handle, bool isDarkMode)
    {
        if (handle == IntPtr.Zero ||
            !OperatingSystem.IsWindowsVersionAtLeast(10, 0, Windows10DarkFrameMinimumBuild) ||
            OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        var value = isDarkMode ? 1 : 0;
        if (!TrySetWindowFrameDarkMode(handle, SelectDarkModeAttribute(), value) &&
            !TrySetWindowFrameDarkMode(handle, SelectFallbackDarkModeAttribute(), value))
        {
            return;
        }

        ReapplyActiveNonClientFrame(handle);
    }

    private static int SelectDarkModeAttribute()
    {
        return Environment.OSVersion.Version.Build >= Windows10DarkFrame20H1AttributeBuild
            ? WindowUtilsInterop.DWMWA_USE_IMMERSIVE_DARK_MODE
            : WindowUtilsInterop.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
    }

    private static int SelectFallbackDarkModeAttribute()
    {
        return Environment.OSVersion.Version.Build >= Windows10DarkFrame20H1AttributeBuild
            ? WindowUtilsInterop.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1
            : WindowUtilsInterop.DWMWA_USE_IMMERSIVE_DARK_MODE;
    }

    private static bool TrySetWindowFrameDarkMode(IntPtr handle, int attribute, int value)
    {
        return WindowUtilsInterop.DwmSetWindowAttribute(
            handle,
            attribute,
            ref value,
            sizeof(int)) == WindowUtilsInterop.S_OK;
    }

    private static uint GetWindowDpi(IntPtr handle)
    {
        try
        {
            var dpi = WindowUtilsInterop.GetDpiForWindow(handle);
            return dpi > 0 ? dpi : 96;
        }
        catch (EntryPointNotFoundException)
        {
            return 96;
        }
    }

    private static double GetSystemMetricForDpi(int metric, uint dpi)
    {
        try
        {
            var value = WindowUtilsInterop.GetSystemMetricsForDpi(metric, dpi);
            if (value >= 0)
            {
                return value;
            }
        }
        catch (EntryPointNotFoundException)
        {
            // Windows 10 1607 and earlier do not export GetSystemMetricsForDpi.
        }

        return Math.Max(0, WindowUtilsInterop.GetSystemMetrics(metric));
    }

    private static void ReapplyActiveNonClientFrame(IntPtr handle)
    {
        // Avalonia suppresses default non-client activation handling for CSD windows.
        // Let Windows repaint the active non-client frame once so the Win10 DWM
        // dark-frame attribute is reflected immediately instead of after focus changes.
        WindowUtilsInterop.DefWindowProc(
            handle,
            WindowUtilsInterop.WM_NCACTIVATE,
            IntPtr.Zero,
            IntPtr.Zero);
        WindowUtilsInterop.DefWindowProc(
            handle,
            WindowUtilsInterop.WM_NCACTIVATE,
            new IntPtr(1),
            IntPtr.Zero);
    }
}
