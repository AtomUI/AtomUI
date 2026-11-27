using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AtomUI.Native.Linux;

/// <summary>
/// Holds the process-wide XCB connection.
/// <para>
/// The connection is independent of Avalonia's main connection. It is created lazily and reused under a lock,
/// so that SHAPE calls do not handshake with the X server each time and do not contend for the lock of
/// Avalonia's main loop. The result of probing whether the SHAPE extension is available is cached as well.
/// </para>
/// <para>
/// Callers must hold <c>lock(SyncRoot)</c> while using <see cref="Connection" />.
/// </para>
/// </summary>
[SupportedOSPlatform("linux")]
internal static class XcbConnectionHolder
{
    private static readonly object s_lock = new();
    private static IntPtr s_connection;
    private static bool s_init;
    private static bool s_shapeSupported;

    /// <summary>
    /// Lock that serializes multithreaded access to <see cref="Connection" />.
    /// </summary>
    public static object SyncRoot => s_lock;

    /// <summary>
    /// The shared XCB connection, other than <see cref="IntPtr.Zero" /> once it has been established.
    /// Hold <c>lock(SyncRoot)</c> while using it.
    /// </summary>
    public static IntPtr Connection
    {
        get
        {
            EnsureInit();
            return s_connection;
        }
    }

    /// <summary>
    /// Whether the X server exposes the SHAPE extension and the local connection has been established.
    /// </summary>
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
                // Open a dedicated connection instead of reusing Avalonia's main connection.
                // This avoids contending for the lock with the X11 event loop on the main thread.
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
