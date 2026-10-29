using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AtomUI.Native.MacOS;

[SupportedOSPlatform("macos")]
internal static class WindowUtilsInterop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        public double X;
        public double Y;

        public CGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CGSize
    {
        public double Width;
        public double Height;

        public CGSize(double width, double height)
        {
            Width  = width;
            Height = height;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NSPoint
    {
        public double X;
        public double Y;

        public NSPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NSSize
    {
        public double Width;
        public double Height;

        public NSSize(double width, double height)
        {
            Width  = width;
            Height = height;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NSRectFull
    {
        public NSPoint Origin;
        public NSSize Size;

        public NSRectFull(NSPoint origin, NSSize size)
        {
            Origin = origin;
            Size   = size;
        }

        public NSRectFull(double x, double y, double width, double height)
        {
            Origin = new NSPoint(x, y);
            Size   = new NSSize(width, height);
        }
    }

    /// <summary>
    /// macOS AppKit 的 <c>NSWindowButton</c> 枚举，标识窗口标题栏上的标准按钮。
    /// </summary>
    /// <remarks>
    /// 数值来源于 Apple SDK 的 <c>AppKit/NSWindow.h</c>：
    /// <code>
    /// typedef NS_ENUM(NSUInteger, NSWindowButton) {
    ///     NSWindowCloseButton            = 0,
    ///     NSWindowMiniaturizeButton      = 1,
    ///     NSWindowZoomButton             = 2,
    ///     NSWindowToolbarButton          = 3,
    ///     NSWindowDocumentIconButton     = 4,
    ///     NSWindowDocumentVersionsButton = 6,  // 注意：跳过 5
    ///     NSWindowFullScreenButton       = 7   // API_DEPRECATED since 10.12
    /// };
    /// </code>
    /// 常规 <c>NSWindow</c> 上只存在 Close / Miniaturize / Zoom；其余按钮取决于窗口样式
    /// (<c>styleMask</c>)、是否有 toolbar、是否为 document window 等。
    /// </remarks>
    public enum NSWindowButton : long
    {
        CloseButton = 0,
        MiniaturizeButton = 1,
        ZoomButton = 2,
        ToolbarButton = 3,
        DocumentIconButton = 4,
        DocumentVersionsButton = 6,
        FullScreenButton = 7
    }
    
    // Objective-C 运行时函数
    [DllImport("/usr/lib/libobjc.A.dylib")]
    public static extern IntPtr sel_registerName(string selectorName);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    public static extern IntPtr objc_getClass(string className);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    public static extern IntPtr objc_allocateClassPair(IntPtr superclass, string className, IntPtr extraBytes);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool class_addMethod(
        IntPtr cls,
        IntPtr name,
        IntPtr imp,
        [MarshalAs(UnmanagedType.LPStr)] string types);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    public static extern void objc_registerClassPair(IntPtr cls);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    public static extern IntPtr class_createInstance(IntPtr cls, IntPtr extraBytes);

    [DllImport("/usr/lib/libSystem.B.dylib")]
    private static extern IntPtr dlopen(string path, int mode);

    [DllImport("/usr/lib/libSystem.B.dylib")]
    private static extern IntPtr dlsym(IntPtr handle, string symbolName);

    [DllImport("/usr/lib/libSystem.B.dylib")]
    private static extern int dlclose(IntPtr handle);

    // objc_msgSend 函数的不同重载
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr objc_msgSend_intptr(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr objc_msgSend_intptr_long(IntPtr receiver, IntPtr selector, long arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr objc_msgSend_intptr_utf8String(
        IntPtr receiver,
        IntPtr selector,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_void_bool(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_void_intptr(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_void_intptr_intptr_intptr_intptr(
        IntPtr receiver,
        IntPtr selector,
        IntPtr arg1,
        IntPtr arg2,
        IntPtr arg3,
        IntPtr arg4);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_void_intptr_intptr_nuint_intptr(
        IntPtr receiver,
        IntPtr selector,
        IntPtr arg1,
        IntPtr arg2,
        nuint arg3,
        IntPtr arg4);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_void_intptr_intptr(
        IntPtr receiver,
        IntPtr selector,
        IntPtr arg1,
        IntPtr arg2);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern NSRectFull objc_msgSend_rect(IntPtr receiver, IntPtr selector);

    // On macOS x64, large struct returns use objc_msgSend_stret.
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend_stret")]
    public static extern void objc_msgSend_stret_rect(out NSRectFull rect, IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_void_point(IntPtr receiver, IntPtr selector, CGPoint point);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    // NSPoint 为两个 double，x64 与 arm64 都按寄存器返回，不走 objc_msgSend_stret。
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern NSPoint objc_msgSend_point(IntPtr receiver, IntPtr selector);

    // Selector字符串缓存
    public static readonly IntPtr SetIgnoresMouseEventsSelector = sel_registerName("setIgnoresMouseEvents:");
    public static readonly IntPtr IgnoresMouseEventsSelector = sel_registerName("ignoresMouseEvents");
    public static readonly IntPtr MouseLocationOutsideOfEventStreamSelector =
        sel_registerName("mouseLocationOutsideOfEventStream");
    public static readonly IntPtr StandardWindowButtonSelector = sel_registerName("standardWindowButton:");
    public static readonly IntPtr FrameSelector = sel_registerName("frame");
    public static readonly IntPtr SuperviewSelector = sel_registerName("superview");
    public static readonly IntPtr SetFrameOriginSelector = sel_registerName("setFrameOrigin:");
    public static readonly IntPtr IsHiddenSelector = sel_registerName("isHidden");
    public static readonly IntPtr PostsFrameChangedNotificationsSelector =
        sel_registerName("postsFrameChangedNotifications");
    public static readonly IntPtr SetPostsFrameChangedNotificationsSelector =
        sel_registerName("setPostsFrameChangedNotifications:");
    public static readonly IntPtr SetShowsResizeIndicatorSelector = sel_registerName("setShowsResizeIndicator:");
    public static readonly IntPtr UpdateTrackingAreasSelector = sel_registerName("updateTrackingAreas");
    public static readonly IntPtr SetNeedsDisplaySelector = sel_registerName("setNeedsDisplay:");
    public static readonly IntPtr InitSelector = sel_registerName("init");
    public static readonly IntPtr RetainSelector = sel_registerName("retain");
    public static readonly IntPtr ReleaseSelector = sel_registerName("release");
    public static readonly IntPtr StringWithUtf8StringSelector = sel_registerName("stringWithUTF8String:");
    public static readonly IntPtr NotificationCenterDefaultCenterSelector = sel_registerName("defaultCenter");
    public static readonly IntPtr NotificationCenterAddObserverSelector =
        sel_registerName("addObserver:selector:name:object:");
    public static readonly IntPtr NotificationCenterRemoveObserverSelector = sel_registerName("removeObserver:");
    public static readonly IntPtr AddObserverForKeyPathSelector =
        sel_registerName("addObserver:forKeyPath:options:context:");
    public static readonly IntPtr RemoveObserverForKeyPathSelector =
        sel_registerName("removeObserver:forKeyPath:");
    public static readonly IntPtr NSViewFrameDidChangeNotificationName =
        GetAppKitGlobalObject("NSViewFrameDidChangeNotification");

    private static IntPtr GetAppKitGlobalObject(string symbolName)
    {
        const int rtldLazy = 0x1;
        var appKit = dlopen("/System/Library/Frameworks/AppKit.framework/AppKit", rtldLazy);
        if (appKit == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        try
        {
            var symbol = dlsym(appKit, symbolName);
            return symbol == IntPtr.Zero ? IntPtr.Zero : Marshal.ReadIntPtr(symbol);
        }
        finally
        {
            dlclose(appKit);
        }
    }

    public static IntPtr CreateRetainedNSString(string value)
    {
        var stringClass = objc_getClass("NSString");
        if (stringClass == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        var nativeString = objc_msgSend_intptr_utf8String(stringClass, StringWithUtf8StringSelector, value);
        return nativeString == IntPtr.Zero
            ? IntPtr.Zero
            : objc_msgSend_intptr(nativeString, RetainSelector);
    }

    // 辅助方法
    public static IntPtr GetStandardWindowButton(IntPtr window, long buttonType)
    {
        return objc_msgSend_intptr_long(window, StandardWindowButtonSelector, buttonType);
    }

    public static NSRectFull GetFrame(IntPtr view)
    {
        if (view == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid view handle");
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            objc_msgSend_stret_rect(out var frame, view, FrameSelector);
            return frame;
        }

        return objc_msgSend_rect(view, FrameSelector);
    }

    public static NSRectFull GetRect(IntPtr obj, IntPtr selector)
    {
        if (obj == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid object handle");
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            objc_msgSend_stret_rect(out var rect, obj, selector);
            return rect;
        }

        return objc_msgSend_rect(obj, selector);
    }

    public static IntPtr GetSuperview(IntPtr view)
    {
        return objc_msgSend_intptr(view, SuperviewSelector);
    }

    public static void SetFrameOrigin(IntPtr view, CGPoint point)
    {
        if (view == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid view handle");
        }
        objc_msgSend_void_point(view, SetFrameOriginSelector, point);
    }

    public static bool IsHidden(IntPtr view)
    {
        if (view == IntPtr.Zero)
        {
            return true;
        }

        return objc_msgSend_bool(view, IsHiddenSelector);
    }

    public static bool PostsFrameChangedNotifications(IntPtr view)
    {
        if (view == IntPtr.Zero)
        {
            return false;
        }

        return objc_msgSend_bool(view, PostsFrameChangedNotificationsSelector);
    }

    public static void SetPostsFrameChangedNotifications(IntPtr view, bool flag)
    {
        if (view == IntPtr.Zero)
        {
            return;
        }

        objc_msgSend_void_bool(view, SetPostsFrameChangedNotificationsSelector, flag);
    }

    public static void UpdateTrackingAreas(IntPtr view)
    {
        if (view == IntPtr.Zero)
        {
            return;
        }
        objc_msgSend_void(view, UpdateTrackingAreasSelector);
    }

    public static void SetNeedsDisplay(IntPtr view, bool flag)
    {
        if (view == IntPtr.Zero)
        {
            return;
        }
        objc_msgSend_void_bool(view, SetNeedsDisplaySelector, flag);
    }
}
