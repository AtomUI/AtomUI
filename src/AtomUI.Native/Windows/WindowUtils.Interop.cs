using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AtomUI.Native.Windows;

[SupportedOSPlatform("windows")]
internal static class WindowUtilsInterop
{
    // 常量定义
    public const int GWL_EXSTYLE = -20;
    public const long WS_EX_TRANSPARENT = 0x20L;
    public const long WS_EX_LAYERED = 0x80000L;
    public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    public const int DWMWA_VISIBLE_FRAME_BORDER_THICKNESS = 37;
    public const int SM_CXBORDER = 5;
    public const int SM_CYBORDER = 6;
    public const int WM_ERASEBKGND = 0x0014;
    public const int WM_ENTERSIZEMOVE = 0x0231;
    public const int WM_EXITSIZEMOVE = 0x0232;
    public const int WM_SHOWWINDOW = 0x0018;
    public const int WM_NCACTIVATE = 0x0086;
    public const int S_OK = 0;

    // 使用正确的 Windows 类型
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    public static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    public static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(IntPtr hWnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmGetWindowAttribute(
        IntPtr hWnd,
        int dwAttribute,
        out uint pvAttribute,
        int cbAttribute);

    [DllImport("user32.dll")]
    public static extern uint GetDpiForWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetricsForDpi(int nIndex, uint dpi);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll", EntryPoint = "FillRect")]
    public static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hbr);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateSolidBrush(int colorRef);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
    public static extern IntPtr DefWindowProc(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }
}
