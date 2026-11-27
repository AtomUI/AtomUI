using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native.Linux;

/// <summary>
/// Shadow pass-through for a native Wayland windowed Popup: <c>wl_surface.set_input_region</c> shrinks the input
/// region to the content shape while the visible area stays unchanged, so the shadow is still drawn in the same
/// frame and on the same surface as the content. The compositor hands pointer and touch events outside the
/// region to the surface below, across clients.
/// <para>
/// As on X11, the input region is submitted ahead of time: it is recomputed when the layout or the motion state
/// changes and is not submitted again while the content is unchanged. The coordinates are surface logical
/// coordinates, the same as client-area DIPs (buffer scale and viewporter scaling are handled by the compositor),
/// rasterized at a scale of 1.
/// </para>
/// <para>
/// Threading: the protocol objects of the Wayland backend of Avalonia belong only to the <c>AvaloniaWayland</c>
/// worker thread. Region submission is queued into a composition batch through the worker's
/// <c>PostWithCommit</c>, on the same queue and in the same order as Avalonia's own proxy calls. A region changes
/// input only, not drawing. Once the surface is mapped, an extra commit makes it take effect, by the same rule
/// as Avalonia's <c>SetHitTestVisible</c> (<c>CanCommitOutOfBand</c>); before the surface is mapped it takes
/// effect with the first frame. Avalonia 12.1.4 provides no public API for setting input per region, so the
/// worker-side surface and the posting entry point are obtained through reflection. When a member is missing,
/// <see cref="TryCreate" /> returns <c>null</c> and the Popup keeps whole-window input.
/// </para>
/// <para>
/// Before mapping: the Popup is attached in <c>Opened</c> (raised synchronously after <c>PlatformImpl.Show</c>),
/// and the first submission is in the same composition batch as the Popup's first frame and runs before the
/// first buffer attach, so no emptying before the mapping is needed as it is on X11.
/// </para>
/// <para>
/// During a motion no empty region is used; only a 1x1 logical-pixel area at the surface origin is kept (the
/// outermost corner of the shadow, almost fully transparent). Observed on GNOME Mutter: an xdg_popup that was
/// mapped with an empty input region receives no pointer at all after any input region is set later, whereas
/// after the mapping the switch between an empty region and the content shape works. The opening motion starts
/// before the mapping, and a non-empty placeholder region satisfies both needs: not taking the pointer away from
/// the control below while mapping, and letting the input region take effect once the motion has ended.
/// </para>
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class WaylandPopupShadowInputRegion : IPopupShadowHitTest
{
    private const string AssemblyName = "Avalonia.Wayland";
    private const string PopupImplTypeName = "Avalonia.Wayland.PopupImpl";
    private const string PopupProxyTypeName = "Avalonia.Wayland.Server.Persistent.WXdgPopupProxy";
    private const string SurfaceTypeName = "Avalonia.Wayland.Server.Persistent.WSurface";
    private const string ShellSurfaceTypeName = "Avalonia.Wayland.Server.Persistent.WXdgShellSurface";
    private const string WorkerTypeName = "Avalonia.Wayland.Server.WaylandWorker";
    private const string GlobalsTypeName = "Avalonia.Wayland.Server.Transient.WaylandGlobals";
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    // Placeholder input region during a motion; the class comment explains why.
    private static readonly PixelRect[] SuspendedRegion = [new PixelRect(0, 0, 1, 1)];

    // Each member inlines Type.GetType with a constant: caching the Type and then looking up members loses the
    // trimming annotations (IL2075).
    private static readonly Lazy<Type?> PopupImplType = new(() =>
        Type.GetType($"{PopupImplTypeName}, {AssemblyName}"));

    [DynamicDependency("_surfaceProxy", PopupImplTypeName, AssemblyName)]
    private static readonly Lazy<FieldInfo?> SurfaceProxyFieldInfo = new(() =>
        Type.GetType($"{PopupImplTypeName}, {AssemblyName}")?.GetField("_surfaceProxy", Instance));

    [DynamicDependency("_target", PopupProxyTypeName, AssemblyName)]
    private static readonly Lazy<FieldInfo?> ProxyTargetFieldInfo = new(() =>
        Type.GetType($"{PopupProxyTypeName}, {AssemblyName}")?.GetField("_target", Instance));

    [DynamicDependency("get_WlSurface", SurfaceTypeName, AssemblyName)]
    private static readonly Lazy<PropertyInfo?> WlSurfacePropertyInfo = new(() =>
        Type.GetType($"{SurfaceTypeName}, {AssemblyName}")?.GetProperty("WlSurface", Instance));

    [DynamicDependency("get_Globals", SurfaceTypeName, AssemblyName)]
    private static readonly Lazy<PropertyInfo?> GlobalsPropertyInfo = new(() =>
        Type.GetType($"{SurfaceTypeName}, {AssemblyName}")?.GetProperty("Globals", Instance));

    [DynamicDependency("get_Worker", SurfaceTypeName, AssemblyName)]
    private static readonly Lazy<PropertyInfo?> WorkerPropertyInfo = new(() =>
        Type.GetType($"{SurfaceTypeName}, {AssemblyName}")?.GetProperty("Worker", Instance));

    // WXdgShellSurface overrides it as "mapped"; a get through the base PropertyInfo is dispatched through a
    // virtual call.
    [DynamicDependency("get_CanCommitOutOfBand", SurfaceTypeName, AssemblyName)]
    [DynamicDependency("get_CanCommitOutOfBand", ShellSurfaceTypeName, AssemblyName)]
    private static readonly Lazy<PropertyInfo?> CanCommitOutOfBandPropertyInfo = new(() =>
        Type.GetType($"{SurfaceTypeName}, {AssemblyName}")?.GetProperty("CanCommitOutOfBand", Instance));

    [DynamicDependency("PostWithCommit", WorkerTypeName, AssemblyName)]
    private static readonly Lazy<MethodInfo?> PostWithCommitMethodInfo = new(() =>
        Type.GetType($"{WorkerTypeName}, {AssemblyName}")?.GetMethod("PostWithCommit", Instance, [typeof(Action)]));

    [DynamicDependency("get_WlCompositor", GlobalsTypeName, AssemblyName)]
    private static readonly Lazy<PropertyInfo?> WlCompositorPropertyInfo = new(() =>
        Type.GetType($"{GlobalsTypeName}, {AssemblyName}")?.GetProperty("WlCompositor", Instance));

    private readonly TopLevel _popupRoot;
    // The persistent worker-side surface: it stays the same across compositor reconnects, and its WlSurface and
    // Globals are read only on the worker thread.
    private readonly object _surface;
    private readonly Action<Action> _postWithCommit;
    private readonly Func<bool> _isInputSuspended;
    private readonly Func<PopupContentShape?> _getContentShape;
    private bool _hasAppliedRegion;
    // null means the protocol default: an unlimited input region.
    private PixelRect[]? _appliedRegion;
    private bool _isDisposed;

    private WaylandPopupShadowInputRegion(
        TopLevel popupRoot,
        object surface,
        Action<Action> postWithCommit,
        Func<bool> isInputSuspended,
        Func<PopupContentShape?> getContentShape)
    {
        _popupRoot               = popupRoot;
        _surface                 = surface;
        _postWithCommit          = postWithCommit;
        _isInputSuspended        = isInputSuspended;
        _getContentShape         = getContentShape;
        _popupRoot.LayoutUpdated += HandleLayoutUpdated;
        Invalidate();
    }

    public static WaylandPopupShadowInputRegion? TryCreate(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<PopupContentShape?> getContentShape)
    {
        ArgumentNullException.ThrowIfNull(popupRoot);
        ArgumentNullException.ThrowIfNull(isInputSuspended);
        ArgumentNullException.ThrowIfNull(getContentShape);
        if (popupRoot.PlatformImpl is not IPopupImpl platformImpl ||
            PopupImplType.Value is not { } popupImplType ||
            platformImpl.GetType() != popupImplType ||
            SurfaceProxyFieldInfo.Value?.GetValue(platformImpl) is not { } surfaceProxy ||
            ProxyTargetFieldInfo.Value?.GetValue(surfaceProxy) is not { } surface ||
            WorkerPropertyInfo.Value?.GetValue(surface) is not { } worker ||
            PostWithCommitMethodInfo.Value is not { } postWithCommit ||
            WlSurfacePropertyInfo.Value is null ||
            GlobalsPropertyInfo.Value is null ||
            WlCompositorPropertyInfo.Value is null ||
            CanCommitOutOfBandPropertyInfo.Value is null)
        {
            return null;
        }

        return new WaylandPopupShadowInputRegion(
            popupRoot,
            surface,
            postWithCommit.CreateDelegate<Action<Action>>(worker),
            isInputSuspended,
            getContentShape);
    }

    public void Invalidate()
    {
        if (_isDisposed || _popupRoot.PlatformImpl is null)
        {
            return;
        }

        PixelRect[]? region = null;
        if (_isInputSuspended())
        {
            region = SuspendedRegion;
        }
        else if (_getContentShape() is { } shape)
        {
            region = PopupContentRegion.Rasterize(shape, 1.0).ToArray();
        }

        if (_hasAppliedRegion &&
            (_appliedRegion is null
                ? region is null
                : region is not null && _appliedRegion.AsSpan().SequenceEqual(region)))
        {
            return;
        }

        _hasAppliedRegion = true;
        _appliedRegion    = region;
        Submit(region);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed              = true;
        _popupRoot.LayoutUpdated -= HandleLayoutUpdated;
        // When a Popup closes, Avalonia destroys the native window and clears PlatformImpl before it raises
        // Popup.Closed. The region is restored only while the window is still alive, to the region that matches
        // Avalonia's own hit-test state (the protocol default, an unlimited region, when it is hit-testable).
        if (_hasAppliedRegion && _popupRoot.PlatformImpl is not null)
        {
            Submit(_popupRoot.IsHitTestVisible ? null : []);
        }
    }

    private void Submit(PixelRect[]? region)
    {
        _postWithCommit(() => ApplyOnWorker(region));
    }

    // Wayland worker thread.
    private void ApplyOnWorker(PixelRect[]? region)
    {
        // WlSurface is null while disconnected (closed or reconnecting); after a reconnect Avalonia replays the
        // whole-window region for its own hit-test state, and the next Invalidate shrinks it again.
        if (WlSurfacePropertyInfo.Value!.GetValue(_surface) is not WlSurface wlSurface ||
            GlobalsPropertyInfo.Value!.GetValue(_surface) is not { } globals ||
            WlCompositorPropertyInfo.Value!.GetValue(globals) is not WlCompositor wlCompositor ||
            !WaylandWindowUtils.SetInputRectangles(wlSurface, wlCompositor, region))
        {
            return;
        }

        // The input region changes no drawing, so an idle surface may commit no new frame. As with Avalonia's
        // SetHitTestVisible, an extra commit is made so that the region takes effect.
        if (CanCommitOutOfBandPropertyInfo.Value!.GetValue(_surface) is true)
        {
            try
            {
                wlSurface.Commit();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        Invalidate();
    }
}
