using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AtomUI.Native.Linux;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

[SupportedOSPlatform("linux")]
internal static class WindowUtilsLinux
{
    private const string X11CsdFrameExtentsPropertyName = "_GTK_FRAME_EXTENTS";

    private static void CheckRequest(IntPtr connection, WindowUtilsInterop.xcb_void_cookie_t cookie)
    {
        IntPtr errorPtr = WindowUtilsInterop.xcb_request_check(connection, cookie);
        if (errorPtr != IntPtr.Zero)
        {
            var error = Marshal.PtrToStructure<WindowUtilsInterop.xcb_generic_error_t>(errorPtr);
            Marshal.FreeHGlobal(errorPtr);
            Console.Error.WriteLine($"Shape manipulation error: {error.ErrorCode}");
        }
    }

    public static void ConfigureInitialWindowGeometry(
        IntPtr handle,
        int x,
        int y,
        int width,
        int height,
        int minWidth,
        int minHeight,
        int? maxWidth,
        int? maxHeight)
    {
        if (handle == IntPtr.Zero || width <= 0 || height <= 0)
        {
            return;
        }

        var display = WindowUtilsInterop.XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
        {
            return;
        }

        try
        {
            var flags = WindowUtilsInterop.XSizeHintsFlags.USPosition
                        | WindowUtilsInterop.XSizeHintsFlags.USSize
                        | WindowUtilsInterop.XSizeHintsFlags.PPosition
                        | WindowUtilsInterop.XSizeHintsFlags.PSize
                        | WindowUtilsInterop.XSizeHintsFlags.PMinSize
                        | WindowUtilsInterop.XSizeHintsFlags.PResizeInc;

            var hints = new WindowUtilsInterop.XSizeHints
            {
                X         = x,
                Y         = y,
                Width     = width,
                Height    = height,
                MinWidth  = Math.Max(1, minWidth),
                MinHeight = Math.Max(1, minHeight),
                WidthInc  = 1,
                HeightInc = 1
            };

            if (maxWidth is { } maxW && maxHeight is { } maxH)
            {
                flags |= WindowUtilsInterop.XSizeHintsFlags.PMaxSize;
                hints.MaxWidth  = Math.Max(hints.MinWidth, maxW);
                hints.MaxHeight = Math.Max(hints.MinHeight, maxH);
            }

            hints.Flags = new IntPtr((int)flags);

            WindowUtilsInterop.XSetWMNormalHints(display, handle, ref hints);
            WindowUtilsInterop.XMoveResizeWindow(display, handle, x, y, width, height);
            WindowUtilsInterop.XFlush(display);
        }
        finally
        {
            WindowUtilsInterop.XCloseDisplay(display);
        }
    }

    public static void SetX11CsdFrameExtents(
        IntPtr handle,
        int left,
        int top,
        int right,
        int bottom)
    {
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var display = WindowUtilsInterop.XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
        {
            return;
        }

        try
        {
            var property = WindowUtilsInterop.XInternAtom(display, X11CsdFrameExtentsPropertyName, false);
            if (property == IntPtr.Zero)
            {
                return;
            }

            // _GTK_FRAME_EXTENTS is the historical X11 atom used by Mutter and KWin
            // to account for client-side decoration extents during snapping/placement.
            // The property order is left, right, top, bottom and values are device pixels.
            // Xlib expects format=32 data as C long[], so use IntPtr-sized elements.
            var extents = new[]
            {
                ToCardinal(left),
                ToCardinal(right),
                ToCardinal(top),
                ToCardinal(bottom)
            };

            WindowUtilsInterop.XChangeProperty(
                display,
                handle,
                property,
                new IntPtr((int)WindowUtilsInterop.XCB_ATOM_CARDINAL),
                32,
                WindowUtilsInterop.PropModeReplace,
                extents,
                extents.Length);
            WindowUtilsInterop.XFlush(display);
        }
        finally
        {
            WindowUtilsInterop.XCloseDisplay(display);
        }
    }

    private static IntPtr ToCardinal(int value)
    {
        return new IntPtr(Math.Max(0, value));
    }

    /// <summary>
    /// Gets the default title bar height of the Linux system, from Avalonia's WindowDecorationMargin.
    /// </summary>
    /// <returns>
    /// <c>null</c> when not on Linux, when the window has no handle, or when the top margin is not positive.
    /// </returns>
    public static double? GetSystemTitleBarHeightLinux(Window window)
    {
        if (!OperatingSystem.IsLinux())
        {
            return null;
        }

        // Use the WindowDecorationMargin property that Avalonia provides.
        // Its Top value is the title bar height.
        var margin = window.WindowDecorationMargin;
        return margin.Top > 0 ? margin.Top : null;
    }
    
    /// <summary>
    /// Sets the input region of an X11 window to a rectangle, through the XCB SHAPE extension.
    /// It cuts the shadow buffer zone, which is visually transparent, out of the input area, so that mouse events
    /// on the shadow pass through to the windows below.
    /// All coordinates are <b>device pixels</b> (native X11 coordinates), not DIPs.
    /// The connection comes from <see cref="XcbConnectionHolder" />.
    /// </summary>
    public static void SetInputRectangle(IntPtr handle, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        SetInputRectangles(handle, [new PixelRect(x, y, width, height)]);
    }

    /// <summary>
    /// Sets the input region of the window to the union of several rectangles, in device pixels.
    /// An empty list means an empty input region: the whole window is transparent to the mouse.
    /// </summary>
    public static void SetInputRectangles(IntPtr handle, IReadOnlyList<PixelRect> rectangles)
    {
        if (handle == IntPtr.Zero)
        {
            return;
        }

        if (!XcbConnectionHolder.IsShapeSupported)
        {
            return;
        }

        uint windowId = (uint)handle.ToInt64();
        var rectSize = Marshal.SizeOf<WindowUtilsInterop.xcb_rectangle_t>();
        IntPtr rectsPtr = Marshal.AllocHGlobal(Math.Max(1, rectangles.Count) * rectSize);
        try
        {
            for (var i = 0; i < rectangles.Count; i++)
            {
                var source = rectangles[i];
                var rect = new WindowUtilsInterop.xcb_rectangle_t
                {
                    X      = (short)Math.Clamp(source.X, short.MinValue, short.MaxValue),
                    Y      = (short)Math.Clamp(source.Y, short.MinValue, short.MaxValue),
                    Width  = (ushort)Math.Clamp(source.Width, 0, ushort.MaxValue),
                    Height = (ushort)Math.Clamp(source.Height, 0, ushort.MaxValue)
                };
                Marshal.StructureToPtr(rect, rectsPtr + i * rectSize, false);
            }

            lock (XcbConnectionHolder.SyncRoot)
            {
                var connection = XcbConnectionHolder.Connection;
                var cookie = WindowUtilsInterop.xcb_shape_rectangles_checked(
                    connection,
                    WindowUtilsInterop.xcb_shape_op_t.XCB_SHAPE_SO_SET,
                    WindowUtilsInterop.xcb_shape_kind_t.XCB_SHAPE_SK_INPUT,
                    (byte)WindowUtilsInterop.xcb_clip_ordering_t.XCB_CLIP_ORDERING_UNSORTED,
                    windowId,
                    0,
                    0,
                    (uint)rectangles.Count,
                    rectsPtr
                );

                CheckRequest(connection, cookie);
                WindowUtilsInterop.xcb_flush(connection);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(rectsPtr);
        }
    }

    /// <summary>
    /// Clears the input shape and restores the default input region of the X server: the whole window, which
    /// follows the window size.
    /// </summary>
    public static void ClearInputShape(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !XcbConnectionHolder.IsShapeSupported)
        {
            return;
        }

        lock (XcbConnectionHolder.SyncRoot)
        {
            var connection = XcbConnectionHolder.Connection;
            var cookie = WindowUtilsInterop.xcb_shape_mask_checked(
                connection,
                WindowUtilsInterop.xcb_shape_op_t.XCB_SHAPE_SO_SET,
                WindowUtilsInterop.xcb_shape_kind_t.XCB_SHAPE_SK_INPUT,
                (uint)handle.ToInt64(),
                0,
                0,
                WindowUtilsInterop.XCB_NONE);

            CheckRequest(connection, cookie);
            WindowUtilsInterop.xcb_flush(connection);
        }
    }

    /// <summary>
    /// Resets the input region of the window to the whole client area.
    /// </summary>
    public static void ResetInputRegion(IntPtr handle, int width, int height)
    {
        SetInputRectangle(handle, 0, 0, width, height);
    }
}
