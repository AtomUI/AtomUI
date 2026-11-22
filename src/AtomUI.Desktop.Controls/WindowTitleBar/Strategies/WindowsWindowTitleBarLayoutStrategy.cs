using Avalonia;

namespace AtomUI.Desktop.Controls;

internal sealed class WindowsWindowTitleBarLayoutStrategy : IWindowTitleBarLayoutStrategy
{
    public static readonly WindowsWindowTitleBarLayoutStrategy Instance = new();

    public WindowTitleBarTitleAlignment AutoAlignment => WindowTitleBarTitleAlignment.Left;

    private WindowsWindowTitleBarLayoutStrategy()
    {
    }

    public Thickness ResolveNativeChromeInsets(
        double width,
        Thickness reportedNativeChromeInsets,
        bool isCsdEnabled)
    {
        return WindowTitleBarNativeChromeInsets.Resolve(width, reportedNativeChromeInsets);
    }
}
