using Avalonia.Controls;

namespace AtomUI.Native;

internal static class NativeWindowStyles
{
    public static IDisposable? TryAttachWindowsBorderlessStyleHook(Window window, Action<string> reportFailure)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(reportFailure);
        return OperatingSystem.IsWindows()
            ? Windows.WindowsBorderlessStyleHook.TryCreate(window, reportFailure)
            : null;
    }
}
