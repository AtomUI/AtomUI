using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Controls.Platform;

namespace AtomUI.Native.Windows;

/// <summary>Completes borderless top-level HWND styles before redirection-surface creation.</summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsBorderlessStyleHook : IDisposable
{
    private readonly IWin32OptionsTopLevelImpl _implementation;
    private readonly Action<string> _reportFailure;
    private bool _isDisposed;

    private WindowsBorderlessStyleHook(IWin32OptionsTopLevelImpl implementation, Action<string> reportFailure)
    {
        _implementation = implementation;
        _reportFailure = reportFailure;
        // Avalonia takes the multicast delegate's final return value when any callback sets handled.
        // This observer never handles a message; prepend it to preserve existing handlers' results.
        _implementation.WndProcHookCallback = (Win32Properties.CustomWndProcHookCallback)HandleWndProc +
                                             _implementation.WndProcHookCallback;
    }

    public static WindowsBorderlessStyleHook? TryCreate(Window window, Action<string> reportFailure)
    {
        if (window.PlatformImpl is not IWin32OptionsTopLevelImpl implementation ||
            window.TryGetPlatformHandle() is not { HandleDescriptor: "HWND", Handle: var handle } ||
            handle == IntPtr.Zero)
        {
            return null;
        }
        var hook = new WindowsBorderlessStyleHook(implementation, reportFailure);
        try
        {
            // Properties assigned before Show may already have produced a borderless HWND.
            var style = ReadWindowStyle(handle, WindowUtilsInterop.GWL_STYLE);
            var completed = hook.CompleteStyle(handle, style);
            if (completed != style)
            {
                var previous = WindowUtilsInterop.SetWindowLong(handle, WindowUtilsInterop.GWL_STYLE, unchecked((int)completed));
                if (previous == 0 && Marshal.GetLastPInvokeError() is var error && error != 0)
                {
                    throw new Win32Exception(error);
                }
            }
            return hook;
        }
        catch (Win32Exception error)
        {
            hook.ReportNativeFailure(error);
            return null;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        // TopLevel clears PlatformImpl before raising WindowClosedEvent. Remove from the original owner.
        _implementation.WndProcHookCallback -= HandleWndProc;
    }

    private IntPtr HandleWndProc(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_isDisposed)
        {
            return IntPtr.Zero;
        }
        if (message is WindowUtilsInterop.WM_DESTROY or WindowUtilsInterop.WM_NCDESTROY)
        {
            Dispose();
            return IntPtr.Zero;
        }
        if (message != WindowUtilsInterop.WM_STYLECHANGING ||
            unchecked((int)wParam.ToInt64()) != WindowUtilsInterop.GWL_STYLE || lParam == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }
        try
        {
            // All style writes pass here, including SetFullScreen and restoration of ordinary borderless
            // windows. Transform the pending value, not a requested managed state or a saved style snapshot.
            // No second SetWindowLong/SetWindowPos, deferred refresh, or surface-creation retry is needed.
            var styles = Marshal.PtrToStructure<WindowUtilsInterop.STYLESTRUCT>(lParam);
            var completed = CompleteStyle(hwnd, styles.StyleNew);
            if (completed != styles.StyleNew)
            {
                styles.StyleNew = completed;
                Marshal.StructureToPtr(styles, lParam, false);
            }
        }
        catch (Win32Exception error)
        {
            ReportNativeFailure(error);
        }
        return IntPtr.Zero;
    }

    private uint CompleteStyle(IntPtr hwnd, uint style)
    {
        if ((style & (WindowUtilsInterop.WS_CHILD | WindowUtilsInterop.WS_POPUP | WindowUtilsInterop.WS_OVERLAPPEDWINDOW)) != 0)
        {
            return style;
        }
        var exStyle = ReadWindowStyle(hwnd, WindowUtilsInterop.GWL_EXSTYLE);
        return (exStyle & WindowUtilsInterop.WS_EX_NOREDIRECTIONBITMAP) == 0
            ? style | WindowUtilsInterop.WS_POPUP
            : style;
    }

    private void ReportNativeFailure(Win32Exception error)
    {
        Dispose();
        _reportFailure($"Unable to complete the native borderless window style: {error.Message} ({error.NativeErrorCode}).");
    }

    private static uint ReadWindowStyle(IntPtr hwnd, int index)
    {
        var value = WindowUtilsInterop.GetWindowLong(hwnd, index);
        if (value == 0 && Marshal.GetLastPInvokeError() is var error && error != 0)
        {
            throw new Win32Exception(error);
        }
        return unchecked((uint)value);
    }
}
