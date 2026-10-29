using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native.Linux;

/// <summary>
/// 原生 Wayland 窗口型 Popup 的阴影穿透：用 <c>wl_surface.set_input_region</c> 把输入区域收缩到内容形状，
/// 可见区域不变，阴影仍与内容同一 surface 同帧绘制；compositor 把区域外的指针与触摸交给下方 surface，跨客户端生效。
/// <para>
/// 与 X11 相同，输入区域是预先提交的区域：布局变化与动效状态变化时重新计算，内容未变时不重复提交。
/// 坐标是 surface 逻辑坐标（与客户区 DIP 一致，buffer scale 与 viewporter 缩放由 compositor 处理），按缩放 1 光栅化。
/// </para>
/// <para>
/// 线程模型：Avalonia Wayland 后端的协议对象只属于 <c>AvaloniaWayland</c> worker 线程。区域提交通过 worker 的
/// <c>PostWithCommit</c> 排入合成批次，与 Avalonia 自身 proxy 调用走同一队列、保持先后顺序；区域只改输入不改绘制，
/// surface 已映射时按 Avalonia <c>SetHitTestVisible</c> 同样的规则（<c>CanCommitOutOfBand</c>）额外提交一次使其生效，
/// 未映射时随首帧提交生效。Avalonia 12.1.4 未提供按区域设置输入的公开 API，因此 worker 侧 surface 与投递入口经反射取得，
/// 成员缺失时 <see cref="TryCreate"/> 返回 <c>null</c>，Popup 保持整窗输入。
/// </para>
/// <para>
/// 映射前：Popup 在 <c>Opened</c>（同步发生在 <c>PlatformImpl.Show</c> 之后）挂接，首次提交与 Popup 首帧处于同一合成批次，
/// 先于首个 buffer attach 执行，因此不需要 X11 那样的映射前置空。
/// </para>
/// <para>
/// 动效期间不使用空区域，而是只保留 surface 原点处 1×1 逻辑像素（阴影最外角，几乎全透明）：GNOME Mutter 实测，
/// 以空输入区域映射的 xdg_popup 之后再设置任何输入区域都不再接收指针（映射后空区域与内容形状之间切换正常）。
/// 打开动效开始于映射之前，用非空占位区域可同时满足“映射时不抢走下方控件的指针”与“动效结束后输入区域生效”。
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

    // 动效期间的占位输入区域，原因见类注释
    private static readonly PixelRect[] SuspendedRegion = [new PixelRect(0, 0, 1, 1)];

    // 每个成员内联 Type.GetType(常量)：缓存 Type 再查成员会丢失 trimming 注解（IL2075）
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

    // WXdgShellSurface 覆盖为“已映射”；经基类 PropertyInfo 取值按虚调用分派
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
    // worker 侧持久 surface：跨 compositor 重连保持不变，其 WlSurface/Globals 只在 worker 线程读取
    private readonly object _surface;
    private readonly Action<Action> _postWithCommit;
    private readonly Func<bool> _isInputSuspended;
    private readonly Func<PopupContentShape?> _getContentShape;
    private bool _hasAppliedRegion;
    // null 表示协议默认的无限输入区域
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
        // Popup 关闭时 Avalonia 先销毁原生窗口并置空 PlatformImpl，再触发 Popup.Closed；
        // 窗口仍存活时才恢复为 Avalonia 自身命中状态对应的区域（可命中为协议默认的无限区域）
        if (_hasAppliedRegion && _popupRoot.PlatformImpl is not null)
        {
            Submit(_popupRoot.IsHitTestVisible ? null : []);
        }
    }

    private void Submit(PixelRect[]? region)
    {
        _postWithCommit(() => ApplyOnWorker(region));
    }

    // Wayland worker 线程
    private void ApplyOnWorker(PixelRect[]? region)
    {
        // 已断开（关闭或重连中）时 WlSurface 为 null；重连后 Avalonia 按自身命中状态重放整窗级区域，下次 Invalidate 再收缩
        if (WlSurfacePropertyInfo.Value!.GetValue(_surface) is not WlSurface wlSurface ||
            GlobalsPropertyInfo.Value!.GetValue(_surface) is not { } globals ||
            WlCompositorPropertyInfo.Value!.GetValue(globals) is not WlCompositor wlCompositor ||
            !WaylandWindowUtils.SetInputRectangles(wlSurface, wlCompositor, region))
        {
            return;
        }

        // 输入区域不改变绘制，空闲 surface 可能不再提交新帧；与 Avalonia SetHitTestVisible 一样额外 commit 使其生效
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
