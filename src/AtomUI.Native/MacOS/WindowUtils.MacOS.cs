using System.Diagnostics;
using System.Runtime.Versioning;
using AtomUI.Native.MacOS;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

internal static partial class WindowExtensions
{
    [SupportedOSPlatform("macos")]
    private static void SetWindowIgnoreMouseEventsMacOS(IntPtr handle, bool flag)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        WindowUtilsInterop.objc_msgSend_void_bool(handle, WindowUtilsInterop.SetIgnoresMouseEventsSelector, flag);
    }
    
    [SupportedOSPlatform("macos")]
    private static bool IsWindowIgnoreMouseEventsMacOS(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        return WindowUtilsInterop.objc_msgSend_bool(handle, WindowUtilsInterop.IgnoresMouseEventsSelector);
    }
    
    [SupportedOSPlatform("macos")]
    public static void SetMacOSOptionButtonsPosition(this WindowBase window, double x, double y, double spacing = 10.0)
    {
        var handle = window.PlatformImpl?.Handle?.Handle;
        if (Design.IsDesignMode)
        {
            return;
        }
        Debug.Assert(handle is not null);
        if (handle is null || handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        // 获取三个标准按钮
        IntPtr closeButton    = WindowUtilsInterop.GetStandardWindowButton(handle.Value, (long)WindowUtilsInterop.NSWindowButton.CloseButton);
        IntPtr minimizeButton = WindowUtilsInterop.GetStandardWindowButton(handle.Value, (long)WindowUtilsInterop.NSWindowButton.MiniaturizeButton);
        IntPtr zoomButton     = WindowUtilsInterop.GetStandardWindowButton(handle.Value, (long)WindowUtilsInterop.NSWindowButton.ZoomButton);
        
        double offset = x;
        
        // 调整关闭按钮
        if (closeButton != IntPtr.Zero)
        {
            WindowUtilsInterop.NSRectFull frame = WindowUtilsInterop.GetFrame(closeButton);
            frame.Origin.X = offset;
            frame.Origin.Y = -y; // Cocoa坐标系中Y轴向下为正
            WindowUtilsInterop.SetFrame(closeButton, frame);
            offset += spacing + frame.Size.Width;
        }
        
        // 调整最小化按钮
        if (minimizeButton != IntPtr.Zero)
        {
            WindowUtilsInterop.NSRectFull frame = WindowUtilsInterop.GetFrame(minimizeButton);
            frame.Origin.X = offset;
            frame.Origin.Y = -y;
            WindowUtilsInterop.SetFrame(minimizeButton, frame);
            offset += spacing + frame.Size.Width;
        }
        
        // 调整缩放按钮
        if (zoomButton != IntPtr.Zero)
        {
            WindowUtilsInterop.NSRectFull frame = WindowUtilsInterop.GetFrame(zoomButton);
            frame.Origin.X = offset;
            frame.Origin.Y = -y;
            WindowUtilsInterop.SetFrame(zoomButton, frame);
        }

        // 强制重绘标题栏
        if (closeButton != IntPtr.Zero)
        {
            IntPtr superview = WindowUtilsInterop.GetSuperview(closeButton);
            if (superview != IntPtr.Zero)
            {
                WindowUtilsInterop.SetNeedsDisplay(superview, true);
            }
        }
    }

    [SupportedOSPlatform("macos")]
    public static Size GetMacOSOptionsSize(this WindowBase window, double spacing = 10.0)
    {
        var handle = window.PlatformImpl?.Handle?.Handle;
        if (Design.IsDesignMode)
        {
            return default;
        }
        Debug.Assert(handle is not null);
        if (handle is null || handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        IntPtr closeButton    = WindowUtilsInterop.GetStandardWindowButton(handle.Value, (long)WindowUtilsInterop.NSWindowButton.CloseButton);
        IntPtr minimizeButton = WindowUtilsInterop.GetStandardWindowButton(handle.Value, (long)WindowUtilsInterop.NSWindowButton.MiniaturizeButton);
        IntPtr zoomButton     = WindowUtilsInterop.GetStandardWindowButton(handle.Value, (long)WindowUtilsInterop.NSWindowButton.ZoomButton);
        
        double totalWidth = 0;
        double maxHeight  = 0;
        // 计算关闭按钮
        if (closeButton != IntPtr.Zero)
        {
            WindowUtilsInterop.NSRectFull frame = WindowUtilsInterop.GetFrame(closeButton);
            totalWidth += spacing + frame.Size.Width;
            maxHeight  =  Math.Max(maxHeight, frame.Size.Height);
        }

        // 计算最小化按钮
        if (minimizeButton != IntPtr.Zero)
        {
            WindowUtilsInterop.NSRectFull frame = WindowUtilsInterop.GetFrame(minimizeButton);
            totalWidth += spacing + frame.Size.Width;
            maxHeight  =  Math.Max(maxHeight, frame.Size.Height);
        }
        // 计算缩放按钮
        if (zoomButton != IntPtr.Zero)
        {
            WindowUtilsInterop.NSRectFull frame = WindowUtilsInterop.GetFrame(zoomButton);
            totalWidth += frame.Size.Width;
            maxHeight  =  Math.Max(maxHeight, frame.Size.Height);
        }

        return new Size(totalWidth, maxHeight);
    }

    [SupportedOSPlatform("macos")]
    public static void SetMacOSWindowClosable(this WindowBase window, bool flag)
    {
        var handle = window.PlatformImpl?.Handle?.Handle;
        if (Design.IsDesignMode)
        {
            return;
        }
        Debug.Assert(handle is not null);
        if (handle is null || handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        // 获取关闭按钮
        IntPtr closeButton = WindowUtilsInterop.GetStandardWindowButton(handle.Value, (long)WindowUtilsInterop.NSWindowButton.CloseButton);
            
        if (closeButton != IntPtr.Zero)
        {
            // 重新设置样式掩码以刷新按钮状态
            ulong styleMask       = WindowUtilsInterop.GetStyleMask(handle.Value);
            var   selSetStyleMask = WindowUtilsInterop.SetStyleMaskSelector;
            WindowUtilsInterop.objc_msgSend_void_ulong(handle.Value, selSetStyleMask, styleMask);
            // 设置按钮启用状态
            WindowUtilsInterop.SetEnabled(closeButton, flag);
        }
    }
}
