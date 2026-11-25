using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Native;

internal static class WaylandWindowReflectionExtensions
{
    private const string PopupImplTypeName = "Avalonia.Wayland.PopupImpl";

    [DynamicDependency("_configuredGeometry", PopupImplTypeName, "Avalonia.Wayland")]
    private static readonly Lazy<FieldInfo?> ConfiguredPopupGeometryFieldInfo = new(() =>
        Type.GetType(PopupImplTypeName + ", Avalonia.Wayland")?
            .GetField("_configuredGeometry", BindingFlags.Instance | BindingFlags.NonPublic));

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.NonPublicFields,
        "Avalonia.Wayland.WindowImpl",
        "Avalonia.Wayland")]
    private static readonly Lazy<FieldInfo?> SurfaceProxyFieldInfo = new(() =>
        Type.GetType("Avalonia.Wayland.WindowImpl, Avalonia.Wayland")?.GetField(
            "_surfaceProxy",
            BindingFlags.Instance | BindingFlags.NonPublic));

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.NonPublicFields,
        "Avalonia.Wayland.Server.Persistent.WXdgTopLevelProxy",
        "Avalonia.Wayland")]
    private static readonly Lazy<FieldInfo?> ProxyTargetFieldInfo = new(() =>
        Type.GetType(
                "Avalonia.Wayland.Server.Persistent.WXdgTopLevelProxy, Avalonia.Wayland")
            ?.GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic));

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.PublicProperties,
        "Avalonia.Wayland.Server.Persistent.WSurface",
        "Avalonia.Wayland")]
    private static readonly Lazy<PropertyInfo?> WlSurfacePropertyInfo = new(() =>
        Type.GetType("Avalonia.Wayland.Server.Persistent.WSurface, Avalonia.Wayland")
            ?.GetProperty("WlSurface", BindingFlags.Instance | BindingFlags.Public));

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.PublicProperties,
        "Avalonia.Wayland.Server.Persistent.WSurface",
        "Avalonia.Wayland")]
    private static readonly Lazy<PropertyInfo?> GlobalsPropertyInfo = new(() =>
        Type.GetType("Avalonia.Wayland.Server.Persistent.WSurface, Avalonia.Wayland")
            ?.GetProperty("Globals", BindingFlags.Instance | BindingFlags.Public));

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.PublicProperties,
        "Avalonia.Wayland.Server.Transient.WaylandGlobals",
        "Avalonia.Wayland")]
    private static readonly Lazy<PropertyInfo?> WlCompositorPropertyInfo = new(() =>
        Type.GetType("Avalonia.Wayland.Server.Transient.WaylandGlobals, Avalonia.Wayland")
            ?.GetProperty("WlCompositor", BindingFlags.Instance | BindingFlags.Public));

    internal static bool IsWaylandPopupPositionPending(this PopupRoot root)
    {
        // The first xdg_popup configure establishes the position. PositionChanged
        // only fires if the configured position differs from the initial (0, 0).
        return OperatingSystem.IsLinux() &&
               root.PlatformImpl is { } impl &&
               impl.GetType().FullName == PopupImplTypeName &&
               ConfiguredPopupGeometryFieldInfo.Value is { } field &&
               field.FieldType == typeof(PixelRect?) &&
               field.GetValue(impl) is null;
    }

    public static bool TrySetWaylandInputRectangle(
        this Window window,
        int x,
        int y,
        int width,
        int height)
    {
        if (!OperatingSystem.IsLinux() ||
            window.PlatformImpl is not { } platformImpl ||
            SurfaceProxyFieldInfo.Value?.GetValue(platformImpl) is not { } surfaceProxy ||
            ProxyTargetFieldInfo.Value?.GetValue(surfaceProxy) is not { } surface ||
            WlSurfacePropertyInfo.Value?.GetValue(surface) is not { } wlSurface ||
            GlobalsPropertyInfo.Value?.GetValue(surface) is not { } globals ||
            WlCompositorPropertyInfo.Value?.GetValue(globals) is not { } wlCompositor)
        {
            return false;
        }

        return WaylandWindowUtils.SetInputRectangle(wlSurface, wlCompositor, x, y, width, height);
    }
}
