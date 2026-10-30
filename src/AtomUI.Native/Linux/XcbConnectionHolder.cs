using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AtomUI.Native.Linux;

/// <summary>
/// 进程级别的独立 XCB 连接持有者。
///
/// 持有一条与 Avalonia 主连接相互独立的 XCB 连接，懒初始化、加锁复用，
/// 避免每次 SHAPE 调用都反复握手 X 服务器，也避免与 Avalonia 主循环抢锁。
///
/// 同时缓存 SHAPE 扩展可用性探测结果。
///
/// 调用方在使用 <see cref="Connection"/> 时必须 <c>lock(SyncRoot)</c>。
/// </summary>
[SupportedOSPlatform("linux")]
internal static class XcbConnectionHolder
{
    private static readonly object s_lock = new();
    private static IntPtr s_connection;
    private static bool s_init;
    private static bool s_shapeSupported;

    /// <summary>多线程串行化访问 <see cref="Connection"/> 的同步锁。</summary>
    public static object SyncRoot => s_lock;

    /// <summary>
    /// 共享 XCB 连接（已成功建立时非 <see cref="IntPtr.Zero"/>）。
    /// 使用时必须先 <c>lock(SyncRoot)</c>。
    /// </summary>
    public static IntPtr Connection
    {
        get
        {
            EnsureInit();
            return s_connection;
        }
    }

    /// <summary>X 服务器是否暴露 SHAPE 扩展且本地连接已成功建立。</summary>
    public static bool IsShapeSupported
    {
        get
        {
            EnsureInit();
            return s_shapeSupported;
        }
    }

    private static void EnsureInit()
    {
        if (s_init)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_init)
            {
                return;
            }

            try
            {
                // 自己开独立连接，不复用 Avalonia 主连接 ——
                // 避免和主线程的 X11 事件循环抢锁。
                s_connection = WindowUtilsInterop.xcb_connect(string.Empty, IntPtr.Zero);
                if (s_connection != IntPtr.Zero
                    && WindowUtilsInterop.xcb_connection_has_error(s_connection) == 0)
                {
                    s_shapeSupported = QueryShapeExtension(s_connection);
                }
                else if (s_connection != IntPtr.Zero)
                {
                    WindowUtilsInterop.xcb_disconnect(s_connection);
                    s_connection = IntPtr.Zero;
                }
            }
            catch (DllNotFoundException)
            {
                s_shapeSupported = false;
            }
            catch (EntryPointNotFoundException)
            {
                s_shapeSupported = false;
            }
            finally
            {
                s_init = true;
            }
        }
    }

    private static bool QueryShapeExtension(IntPtr connection)
    {
        uint   shapeCookie   = WindowUtilsInterop.xcb_shape_query_version(connection);
        IntPtr errorPtr      = IntPtr.Zero;
        IntPtr shapeReplyPtr = WindowUtilsInterop.xcb_shape_query_version_reply(connection, shapeCookie, errorPtr);

        if (errorPtr != IntPtr.Zero)
        {
            var error = Marshal.PtrToStructure<WindowUtilsInterop.xcb_generic_error_t>(errorPtr);
            Console.Error.WriteLine($"SHAPE extension error: {error.ErrorCode}");
            Marshal.FreeHGlobal(errorPtr);
            return false;
        }

        if (shapeReplyPtr == IntPtr.Zero)
        {
            return false;
        }

        Marshal.FreeHGlobal(shapeReplyPtr);
        return true;
    }
}
