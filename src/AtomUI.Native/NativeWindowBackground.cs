using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Native;

/// <summary>
/// Paints the client area of a window with one solid color through the native window procedure.
/// <see cref="NativeWindowBackground.TryAttachWindowsBackgroundHook" /> returns the Windows implementation and
/// <see langword="null" /> on other platforms. AtomUI.Desktop.Controls uses it in
/// <c>WindowsWindowChromeManager</c>.
/// </summary>
/// <remarks>
/// The alpha channel of the color is ignored; <see langword="null" /> or a color with an alpha of 0 removes the
/// color, after which nothing is painted. <see cref="PaintClientArea" /> fills the client area once with the
/// color that is set. Both methods do nothing once the hook is disposed.
/// </remarks>
internal interface INativeWindowBackgroundHook : IDisposable
{
    void SetBackgroundColor(Color? color);

    void PaintClientArea();
}

internal static class NativeWindowBackground
{
    public static INativeWindowBackgroundHook? TryAttachWindowsBackgroundHook(Window window)
    {
        return OperatingSystem.IsWindows()
            ? new Windows.WindowsBackgroundHook(window)
            : null;
    }
}
