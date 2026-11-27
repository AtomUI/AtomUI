using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

/// <summary>
/// Input pass-through over the shadow of a windowed Popup, for the duration of one open session.
/// </summary>
internal interface IPopupShadowHitTest : IDisposable
{
    /// <summary>
    /// The input suspension state or the content shape may have changed.
    /// Platforms that query per point (Windows) ignore it. Platforms that submit the input region ahead of time
    /// (X11, Wayland) recompute and submit the region. macOS uses it to decide again, from the current cursor
    /// position, whether the whole window passes input through.
    /// </summary>
    void Invalidate();
}

/// <summary>
/// The side of a windowed Popup that carries the arrow.
/// </summary>
internal enum PopupArrowSide
{
    None,
    Left,
    Top,
    Right,
    Bottom
}

/// <summary>
/// Platform-independent description of the content shape of a windowed Popup (client-area DIPs, without the
/// shadow): a rounded-rectangle body plus one arrow triangle whose base touches the body and whose tip points
/// outward.
/// </summary>
internal readonly record struct PopupContentShape(
    Rect Body,
    CornerRadius CornerRadius,
    Rect ArrowBounds,
    PopupArrowSide ArrowSide);

/// <summary>
/// Input pass-through for the shadow area of a windowed Popup. The shadow and the content are drawn on the same
/// window surface, so only the input region is shrunk to the content shape.
/// </summary>
internal static class NativePopupShadowHitTest
{
    /// <summary>
    /// Makes the whole window transparent to input before the native window is mapped (shown); the following
    /// <see cref="TryAttach" /> takes over.
    /// <para>
    /// X11: when a window with the default (whole-window) input shape is mapped under the pointer, the X server
    /// immediately sends Leave to the window below, and the control below loses hover (a Hover flyout therefore
    /// closes and opens again). An empty input shape set before the mapping avoids this boundary event.
    /// </para>
    /// <para>
    /// macOS: likewise sets <c>ignoresMouseEvents</c> on the whole window before it is shown, and
    /// <see cref="TryAttach" /> restores it according to the cursor position.
    /// </para>
    /// <para>
    /// Wayland: the first submission made by <see cref="TryAttach" /> takes effect before the first frame (the
    /// mapping), so nothing is done here.
    /// </para>
    /// <para>
    /// Windows: queries <c>WM_NCHITTEST</c> per point, so nothing needs to be done and nothing is done.
    /// </para>
    /// </summary>
    /// <param name="popupRoot">
    /// The not yet mapped TopLevel of the windowed Popup.
    /// </param>
    public static void SuspendInputBeforeShow(TopLevel popupRoot)
    {
        if (OperatingSystem.IsLinux())
        {
            Linux.X11PopupShadowInputRegion.SuspendInputBeforeShow(popupRoot);
        }
        else if (OperatingSystem.IsMacOS())
        {
            MacOS.MacOSPopupShadowPassThrough.SuspendInputBeforeShow(popupRoot);
        }
    }

    /// <param name="popupRoot">
    /// The TopLevel of the windowed Popup (a native window of its own).
    /// </param>
    /// <param name="isInputSuspended">
    /// Returns <c>true</c> while the whole window passes input through. This is the case during the opening and
    /// closing animations.
    /// </param>
    /// <param name="isInContentShape">
    /// Whether the point given in client-area DIPs lies inside the content shape (rounded rectangle plus arrow).
    /// Used by the platforms that hit-test per point (Windows, macOS).
    /// </param>
    /// <param name="getContentShape">
    /// Supplies the content shape geometry, used by the platforms that submit the input region ahead of time. A
    /// <c>null</c> result means the shape cannot be computed, in which case the input region is not restricted.
    /// </param>
    /// <returns>
    /// <c>null</c> when the current platform is not supported.
    /// </returns>
    public static IPopupShadowHitTest? TryAttach(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<Point, bool> isInContentShape,
        Func<PopupContentShape?> getContentShape)
    {
        if (OperatingSystem.IsWindows())
        {
            return Windows.WindowsPopupShadowHitTestHook.TryCreate(popupRoot, isInputSuspended, isInContentShape);
        }

        if (OperatingSystem.IsLinux())
        {
            return (IPopupShadowHitTest?)Linux.X11PopupShadowInputRegion.TryCreate(popupRoot, isInputSuspended, getContentShape) ??
                   Linux.WaylandPopupShadowInputRegion.TryCreate(popupRoot, isInputSuspended, getContentShape);
        }

        if (OperatingSystem.IsMacOS())
        {
            return MacOS.MacOSPopupShadowPassThrough.TryCreate(popupRoot, isInputSuspended, isInContentShape);
        }

        return null;
    }
}
