using System.Runtime.Versioning;
using Avalonia;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native;

[SupportedOSPlatform("linux")]
internal static class WaylandWindowUtils
{
    public static bool SetInputRectangle(
        object surface,
        object compositor,
        int x,
        int y,
        int width,
        int height)
    {
        if (!OperatingSystem.IsLinux() ||
            surface is not WlSurface wlSurface ||
            compositor is not WlCompositor wlCompositor ||
            width <= 0 ||
            height <= 0)
        {
            return false;
        }

        try
        {
            var region = wlCompositor.CreateRegion();
            try
            {
                region.Add(x, y, width, height);
                wlSurface.SetInputRegion(region);
            }
            finally
            {
                try
                {
                    region.Destroy();
                }
                finally
                {
                    region.Dispose();
                }
            }

            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Sets the input region of the surface to the union of several rectangles, in surface logical coordinates.
    /// An empty list means an empty input region (the whole surface is transparent to input); <c>null</c> restores
    /// the unlimited input region that is the protocol default.
    /// The input region is double-buffered state; the caller is responsible for the following
    /// <c>wl_surface.commit</c>.
    /// Must be called only on the Wayland worker thread.
    /// </summary>
    /// <returns>
    /// <c>true</c> when the input region was set. <c>false</c> when the surface or the compositor has been
    /// disposed.
    /// </returns>
    public static bool SetInputRectangles(
        WlSurface surface,
        WlCompositor compositor,
        IReadOnlyList<PixelRect>? rectangles)
    {
        try
        {
            if (rectangles is null)
            {
                surface.SetInputRegion(null!);
                return true;
            }

            var region = compositor.CreateRegion();
            try
            {
                foreach (var rect in rectangles)
                {
                    region.Add(rect.X, rect.Y, rect.Width, rect.Height);
                }
                surface.SetInputRegion(region);
            }
            finally
            {
                try
                {
                    region.Destroy();
                }
                finally
                {
                    region.Dispose();
                }
            }

            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }
}
