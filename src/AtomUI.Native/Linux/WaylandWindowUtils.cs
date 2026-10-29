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
    /// 将 surface 的输入区域设置为多个矩形的并集（surface 逻辑坐标）。
    /// 空列表表示空输入区域（整个 surface 对输入透明）；<c>null</c> 恢复协议默认的无限输入区域。
    /// 输入区域是 double-buffered state，由调用方负责随后的 <c>wl_surface.commit</c>。
    /// 只能在 Wayland worker 线程调用。
    /// </summary>
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
