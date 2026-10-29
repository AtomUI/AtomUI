using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AtomUI.Native.Linux;

[SupportedOSPlatform("linux")]
internal static class WindowUtilsInterop
{
    #region XCB基本类型和常量

    [StructLayout(LayoutKind.Sequential)]
    internal struct xcb_void_cookie_t { public uint Sequence; }

    // 矩形结构
    [StructLayout(LayoutKind.Sequential)]
    internal struct xcb_rectangle_t
    {
        public short X;
        public short Y;
        public ushort Width;
        public ushort Height;
    }

    // Atom 常量
    internal const uint XCB_ATOM_CARDINAL = 6;

    // xcb_pixmap_t None：SHAPE mask 传 None 时恢复默认形状（整个窗口）
    internal const uint XCB_NONE = 0;

    // XChangeProperty mode
    internal const int PropModeReplace = 0;

    // 枚举类型（对应C中的typedef enum）
    internal enum xcb_shape_op_t : byte
    {
        XCB_SHAPE_SO_SET = 0
    }
    
    internal enum xcb_shape_kind_t : byte
    {
        XCB_SHAPE_SK_INPUT = 2
    }
    
    internal enum xcb_clip_ordering_t : byte
    {
        XCB_CLIP_ORDERING_UNSORTED = 0
    }
    
    // 错误处理
    [StructLayout(LayoutKind.Sequential)]
    internal struct xcb_generic_error_t
    {
        public byte ResponseType;
        public byte ErrorCode;
        public ushort Sequence;
        public uint ResourceId;
        public ushort MinorCode;
        public byte MajorCode;
        public byte Pad0;
        public uint FullSequence;
    }

    [Flags]
    internal enum XSizeHintsFlags
    {
        USPosition = 1 << 0,
        USSize     = 1 << 1,
        PPosition  = 1 << 2,
        PSize      = 1 << 3,
        PMinSize   = 1 << 4,
        PMaxSize   = 1 << 5,
        PResizeInc = 1 << 6
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XSizeHints
    {
        public IntPtr Flags;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int MinWidth;
        public int MinHeight;
        public int MaxWidth;
        public int MaxHeight;
        public int WidthInc;
        public int HeightInc;
        public int MinAspectX;
        public int MinAspectY;
        public int MaxAspectX;
        public int MaxAspectY;
        public int BaseWidth;
        public int BaseHeight;
        public int WinGravity;
    }
    
    #endregion
        
    #region XCB核心函数P/Invoke声明
        
    [DllImport("libxcb.so.1", EntryPoint = "xcb_connect")]
    internal static extern IntPtr xcb_connect(string display, IntPtr screen);
        
    [DllImport("libxcb.so.1", EntryPoint = "xcb_connection_has_error")]
    internal static extern int xcb_connection_has_error(IntPtr connection);
        
    [DllImport("libxcb.so.1", EntryPoint = "xcb_disconnect")]
    internal static extern void xcb_disconnect(IntPtr connection);
        
    [DllImport("libxcb.so.1", EntryPoint = "xcb_flush")]
    internal static extern int xcb_flush(IntPtr connection);
        
    [DllImport("libxcb.so.1", EntryPoint = "xcb_request_check")]
    internal static extern IntPtr xcb_request_check(IntPtr connection, xcb_void_cookie_t cookie);
        
    #endregion

    #region Xlib核心函数P/Invoke声明

    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    internal static extern IntPtr XOpenDisplay(IntPtr displayName);

    [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
    internal static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6", EntryPoint = "XFlush")]
    internal static extern int XFlush(IntPtr display);

    [DllImport("libX11.so.6", EntryPoint = "XMoveResizeWindow")]
    internal static extern int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);

    [DllImport("libX11.so.6", EntryPoint = "XSetWMNormalHints")]
    internal static extern int XSetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints);

    [DllImport("libX11.so.6", EntryPoint = "XInternAtom", CharSet = CharSet.Ansi)]
    internal static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

    [DllImport("libX11.so.6", EntryPoint = "XChangeProperty")]
    internal static extern int XChangeProperty(
        IntPtr display,
        IntPtr window,
        IntPtr property,
        IntPtr type,
        int format,
        int mode,
        IntPtr[] data,
        int elementCount);

    #endregion
        
    #region 形状扩展函数
        
    // 查询形状扩展版本
    [DllImport("libxcb-shape.so.0", EntryPoint = "xcb_shape_query_version")]
    internal static extern uint xcb_shape_query_version(IntPtr connection);
        
    [DllImport("libxcb-shape.so.0", EntryPoint = "xcb_shape_query_version_reply")]
    internal static extern IntPtr xcb_shape_query_version_reply(
        IntPtr connection, uint cookie, IntPtr error);
        
    [DllImport("libxcb-shape.so.0", EntryPoint = "xcb_shape_rectangles_checked")]
    internal static extern xcb_void_cookie_t xcb_shape_rectangles_checked(
        IntPtr c,
        xcb_shape_op_t operation,          // xcb_shape_op_t (实际是byte)
        xcb_shape_kind_t destination_kind, // xcb_shape_kind_t (实际是byte)
        byte ordering,                     // uint8_t
        uint destination_window,
        short x_offset,                    // int16_t
        short y_offset,                    // int16_t
        uint rectangles_len,               // uint32_t
        IntPtr rectangles                  // const xcb_rectangle_t* (指针)
    );

    [DllImport("libxcb-shape.so.0", EntryPoint = "xcb_shape_mask_checked")]
    internal static extern xcb_void_cookie_t xcb_shape_mask_checked(
        IntPtr c,
        xcb_shape_op_t operation,
        xcb_shape_kind_t destination_kind,
        uint destination_window,
        short x_offset,
        short y_offset,
        uint source_bitmap                 // xcb_pixmap_t
    );
    #endregion
}
