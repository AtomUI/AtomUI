using Avalonia;

namespace AtomUI.Desktop.Controls;

internal interface IWindowTitleBarLayoutStrategy
{
    WindowTitleBarTitleAlignment AutoAlignment { get; }

    Thickness ResolveNativeChromeInsets(
        double width,
        Thickness reportedNativeChromeInsets,
        bool isCsdEnabled);
}

internal static class WindowTitleBarLayoutStrategies
{
    private static readonly IWindowTitleBarLayoutStrategy s_unknown = new UnknownWindowTitleBarLayoutStrategy();

    public static IWindowTitleBarLayoutStrategy Get(OsType osType)
    {
        return osType switch
        {
            OsType.macOS => MacOSWindowTitleBarLayoutStrategy.Instance,
            OsType.Windows => WindowsWindowTitleBarLayoutStrategy.Instance,
            OsType.Linux => LinuxWindowTitleBarLayoutStrategy.Instance,
            _ => s_unknown
        };
    }

    private sealed class UnknownWindowTitleBarLayoutStrategy : IWindowTitleBarLayoutStrategy
    {
        public WindowTitleBarTitleAlignment AutoAlignment => WindowTitleBarTitleAlignment.Left;

        public Thickness ResolveNativeChromeInsets(
            double width,
            Thickness reportedNativeChromeInsets,
            bool isCsdEnabled)
        {
            return default;
        }
    }
}

internal static class WindowTitleBarNativeChromeInsets
{
    public static Thickness Resolve(
        double width,
        Thickness reportedNativeChromeInsets)
    {
        var left  = Normalize(reportedNativeChromeInsets.Left);
        var right = Normalize(reportedNativeChromeInsets.Right);
        if (double.IsPositiveInfinity(width))
        {
            return new Thickness(left, 0, right, 0);
        }

        var normalizedWidth = Normalize(width);
        if (normalizedWidth == 0)
        {
            return default;
        }

        return new Thickness(
            Math.Clamp(left, 0, normalizedWidth),
            0,
            Math.Clamp(right, 0, normalizedWidth),
            0);
    }

    private static double Normalize(double value)
    {
        return double.IsFinite(value) && value > 0 ? value : 0;
    }
}
