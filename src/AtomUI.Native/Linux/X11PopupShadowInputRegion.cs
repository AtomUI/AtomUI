using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native.Linux;

/// <summary>
/// Shadow pass-through for a windowed Popup on X11: <c>ShapeInput</c> of the SHAPE extension shrinks the input
/// region to the content shape. The visible area stays unchanged, so the shadow is still drawn in the same
/// frame and on the same surface as the content.
/// <para>
/// Unlike the two-layer approach on Windows, the X server selects the receiving window by its input shape, so
/// the pass-through works alike for windows of the same process and of other processes and needs no cursor
/// polling. The input shape is a region submitted ahead of time rather than a per-point query, so it is
/// recomputed when the layout or the motion state changes, and no request is sent again while the content is
/// unchanged.
/// </para>
/// <para>
/// With GPU rendering, Avalonia creates a rendering child window of the same size under the Popup window. When
/// the X server looks up the window under the pointer, a point outside the parent's input shape does not reach
/// its child windows either, so only the top-level XID needs to be set (the same as
/// <c>X11WindowChromeManager</c> does).
/// </para>
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class X11PopupShadowInputRegion : IPopupShadowHitTest
{
    private readonly TopLevel _popupRoot;
    private readonly IntPtr _handle;
    private readonly Func<bool> _isInputSuspended;
    private readonly Func<PopupContentShape?> _getContentShape;
    // null means the default input shape is current (the whole window).
    private PixelRect[]? _appliedRegion;
    private bool _isDisposed;

    private X11PopupShadowInputRegion(
        TopLevel popupRoot,
        IntPtr handle,
        Func<bool> isInputSuspended,
        Func<PopupContentShape?> getContentShape)
    {
        _popupRoot             = popupRoot;
        _handle                = handle;
        _isInputSuspended      = isInputSuspended;
        _getContentShape       = getContentShape;
        _popupRoot.LayoutUpdated += HandleLayoutUpdated;
        Invalidate();
    }

    public static X11PopupShadowInputRegion? TryCreate(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<PopupContentShape?> getContentShape)
    {
        ArgumentNullException.ThrowIfNull(popupRoot);
        ArgumentNullException.ThrowIfNull(isInputSuspended);
        ArgumentNullException.ThrowIfNull(getContentShape);
        if (TryGetX11Handle(popupRoot) is not { } handle)
        {
            return null;
        }

        return new X11PopupShadowInputRegion(popupRoot, handle, isInputSuspended, getContentShape);
    }

    private static IntPtr? TryGetX11Handle(TopLevel popupRoot)
    {
        // On backends other than X11, such as Wayland, the handle descriptor is not an XID.
        if (popupRoot.TryGetPlatformHandle() is not { HandleDescriptor: "XID", Handle: var handle } ||
            handle == IntPtr.Zero ||
            !XcbConnectionHolder.IsShapeSupported)
        {
            return null;
        }

        return handle;
    }

    public static void SuspendInputBeforeShow(TopLevel popupRoot)
    {
        if (TryGetX11Handle(popupRoot) is { } handle)
        {
            WindowUtilsLinux.SetInputRectangles(handle, []);
        }
    }

    public void Invalidate()
    {
        if (_isDisposed || _popupRoot.PlatformImpl is null)
        {
            return;
        }

        // Empty region during a motion: the whole window passes input through. The Popup reports the end of the
        // motion, and the content shape is then restored.
        List<PixelRect>? region = null;
        if (_isInputSuspended())
        {
            region = [];
        }
        else if (_getContentShape() is { } shape)
        {
            region = PopupContentRegion.Rasterize(shape, _popupRoot.RenderScaling);
        }

        if (region is null)
        {
            if (_appliedRegion is not null)
            {
                _appliedRegion = null;
                WindowUtilsLinux.ClearInputShape(_handle);
            }
            return;
        }

        var regionArray = region.ToArray();
        if (_appliedRegion is not null && _appliedRegion.AsSpan().SequenceEqual(regionArray))
        {
            return;
        }

        _appliedRegion = regionArray;
        WindowUtilsLinux.SetInputRectangles(_handle, regionArray);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _popupRoot.LayoutUpdated -= HandleLayoutUpdated;
        // When a Popup closes, Avalonia destroys the native window and clears PlatformImpl before it raises
        // Popup.Closed. Restore the default input shape only while the window is still alive, to avoid sending
        // requests to a destroyed XID.
        if (_appliedRegion is not null && _popupRoot.PlatformImpl is not null)
        {
            WindowUtilsLinux.ClearInputShape(_handle);
        }
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        Invalidate();
    }
}
