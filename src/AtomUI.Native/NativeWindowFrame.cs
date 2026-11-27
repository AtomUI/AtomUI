using Avalonia.Controls;

namespace AtomUI.Native;

internal static class NativeWindowFrame
{
    /// <summary>
    /// Returns a hook only on Windows 10: it corrects the DWM border of the active state being covered for an
    /// extended-client-area window under a RedirectionSurface.
    /// Returns <c>null</c> on Windows 11 and on other platforms.
    /// </summary>
    public static IDisposable? TryAttachLegacyFrameMarginHook(Window window)
    {
        return OperatingSystem.IsWindows()
            ? Windows.WindowsLegacyFrameMarginHook.TryCreate(window)
            : null;
    }
}
