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
/// <param name="IsChromePresented">
/// Whether the platform currently shows chrome of its own inside the client area. It is reported by the platform
/// rather than derived from <paramref name="Extent"/>: platform chrome can be presented without covering any client
/// height that the managed title bar has to stay clear of.
/// </param>
internal readonly record struct FullScreenNativeChromeMetrics(
    double Extent,
    Thickness ControlInset,
    double PersistentInset,
    bool IsChromePresented);

/// <summary>
/// What AppKit currently reports about the title strip of a full-screen window, in client DIPs.
/// </summary>
/// <param name="TopOffset">The strip's top edge measured down from the top of the client area; negative above it.</param>
/// <param name="Height">The height of the strip.</param>
/// <param name="Alpha">The alpha of the system window that hosts the strip.</param>
/// <param name="IsFused">Whether the strip is transparent and laid over the managed surface.</param>
/// <param name="ButtonsTrailingEdge">
/// The distance from the leading edge of the client area to the trailing edge reserved for the platform buttons.
/// </param>
internal readonly record struct TitleStripState(
    double TopOffset,
    double Height,
    double Alpha,
    bool IsFused,
    double ButtonsTrailingEdge);

/// <summary>
/// Derives the full-screen chrome metrics from the observed title strip. It calls no platform API, so the rules are
/// testable on every platform.
/// </summary>
internal static class MacOSFullScreenChromeGeometry
{
    /// <param name="persistentInset">The client height covered by platform chrome that stays presented.</param>
    /// <param name="strip">The observed title strip; <c>null</c> when the window has none.</param>
    public static FullScreenNativeChromeMetrics Compute(double persistentInset, TitleStripState? strip)
    {
        if (strip is not { } state)
        {
            return new FullScreenNativeChromeMetrics(0, default, persistentInset, false);
        }

        // The strip is presented while it is shown and reaches into the client area. AppKit hides it either by
        // sliding it above the window or, where the window does not reach the top of the screen (a display with a
        // camera housing), by fading it out in place; position alone cannot tell the second case.
        var bottomOffset = state.TopOffset + state.Height;
        var isPresented  = state.Alpha > 0 && bottomOffset > 0;
        var extent       = 0.0;
        if (state.IsFused)
        {
            // The strip lies over the managed surface, so only the menu bar above the strip counts.
            extent = Math.Max(0, state.TopOffset);
        }
        else if (isPresented)
        {
            extent = bottomOffset;
        }

        var controlInset = state.IsFused && isPresented
            ? new Thickness(state.ButtonsTrailingEdge, 0, 0, 0)
            : default;
        return new FullScreenNativeChromeMetrics(extent, controlInset, persistentInset, isPresented);
    }
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
