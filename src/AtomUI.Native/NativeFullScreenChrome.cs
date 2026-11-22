using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

/// <summary>
/// Geometry of the chrome a platform presents by itself at the top of a full-screen window, in client DIPs with a
/// top-left origin.
/// </summary>
/// <param name="Extent">
/// The client height currently covered by the part of the platform chrome that must not overlap the managed
/// full-screen title bar. It changes continuously while the platform chrome slides in and out.
/// </param>
/// <param name="ControlInset">
/// The leading and trailing widths taken by platform controls laid over the managed full-screen title bar.
/// </param>
/// <param name="PersistentInset">The client height covered by platform chrome that stays presented.</param>
internal readonly record struct FullScreenNativeChromeMetrics(
    double Extent,
    Thickness ControlInset,
    double PersistentInset)
{
    /// <summary>Whether the platform currently presents chrome beyond its persistent part.</summary>
    public bool IsChromePresented => Extent > PersistentInset;
}

/// <summary>
/// How the platform title strip is fused into the managed full-screen title bar.
/// </summary>
/// <param name="IsEnabled">Whether the title strip is made transparent and laid over the managed surface.</param>
/// <param name="SurfaceHeight">The managed surface height the platform buttons are centred in.</param>
/// <param name="ButtonOffsetX">
/// The leading x of the first platform button; <c>NaN</c> keeps the platform position. The platform keeps its own
/// spacing between the buttons.
/// </param>
internal readonly record struct FullScreenTitleStripFusion(
    bool IsEnabled,
    double SurfaceHeight,
    double ButtonOffsetX);

/// <summary>
/// Tracks the chrome a platform presents by itself while a window is full screen. Only platforms that present such
/// chrome provide an instance; it lives for one full-screen session and must be disposed when the session ends.
/// </summary>
internal interface IFullScreenNativeChrome : IDisposable
{
    FullScreenNativeChromeMetrics Metrics { get; }

    void ConfigureTitleStripFusion(FullScreenTitleStripFusion fusion);
}

internal static class FullScreenNativeChromeExtensions
{
    /// <summary>
    /// Starts tracking the platform's own full-screen chrome of <paramref name="window"/>.
    /// </summary>
    /// <param name="metricsChanged">Invoked on the UI thread whenever <see cref="IFullScreenNativeChrome.Metrics"/> changes.</param>
    /// <returns><c>null</c> on platforms that present no chrome of their own in full screen.</returns>
    public static IFullScreenNativeChrome? AttachFullScreenNativeChrome(this Window window, Action metricsChanged)
    {
        if (OperatingSystem.IsMacOS())
        {
            return WindowUtilsMacOS.AttachFullScreenNativeChrome(window, metricsChanged);
        }

        return null;
    }
}
