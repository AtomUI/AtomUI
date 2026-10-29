using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Animations;
using AtomUI.Native;
using Avalonia;
using Avalonia.Logging;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using NWayland;
using NWayland.Interop;
using NWayland.Protocols.PresentationTime;
using NWayland.Protocols.Wayland;

namespace AtomUI;

// Deliberately version-bound. These members are framework integration details, never control API.
internal sealed class WaylandFramePacingCompatibility
{
    private const string SurfaceTypeName = "Avalonia.Wayland.Server.Persistent.WSurface";
    private const string ShellTypeName = "Avalonia.Wayland.Server.Persistent.WXdgShellSurface";
    private const string EglSurfaceTypeName = "Avalonia.Wayland.Server.Transient.Rendering.WaylandEglWsiSurface";
    private const string EglTargetTypeName = EglSurfaceTypeName + ".RenderTarget";
    private const string WorkerTypeName = "Avalonia.Wayland.Server.WaylandWorker";
    private const string GlobalsTypeName = "Avalonia.Wayland.Server.Transient.WaylandGlobals";
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private readonly Type _eglSurfaceType;
    private readonly Type _topLevelType;
    private readonly Type _popupType;
    private readonly FieldInfo _surface;
    private readonly FieldInfo _frameCallback;
    private readonly FieldInfo _listener;
    private readonly FieldInfo _configured;
    private readonly FieldInfo _configureSerial;
    private readonly FieldInfo _scale;
    private readonly FieldInfo _shadowExtents;
    private readonly FieldInfo _eglSurface;
    private readonly PropertyInfo _connection;
    private readonly PropertyInfo _wlSurface;
    private readonly PropertyInfo _globals;
    private readonly PropertyInfo _worker;
    private readonly MethodInfo _beforeAttach;
    private readonly MethodInfo _register;
    private readonly MethodInfo _unregister;
    private readonly MethodInfo _wakeup;
    private readonly MethodInfo _ensureWindow;
    private readonly PropertyInfo _registry;
    private readonly FieldInfo _knownGlobals;
    private readonly ConditionalWeakTable<IPlatformRenderSurface, IPlatformRenderSurface> _surfaces = new();
    private readonly ConditionalWeakTable<object, WaylandSurfaceFrameState> _states = new();
    private readonly ConditionalWeakTable<object, StrongBox<WaylandPresentationTiming?>> _presentations = new();

    [DynamicDependency("_surface", EglSurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_frameCallback", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_currentScale", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_Connection", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_WlSurface", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_Globals", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("get_Worker", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("RegisterRenderTarget", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("UnregisterRenderTarget", SurfaceTypeName, "Avalonia.Wayland")]
    [DynamicDependency("OnBeforeNewBufferAttached", ShellTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_initialConfigureAcknowledged", ShellTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_pendingAckSerial", ShellTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_shadowExtents", ShellTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_eglSurface", EglTargetTypeName, "Avalonia.Wayland")]
    [DynamicDependency("EnsureWindow", EglTargetTypeName, "Avalonia.Wayland")]
    [DynamicDependency("WakeupRenderLoop", WorkerTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_listener", typeof(WlProxy))]
    [DynamicDependency("get_Registry", GlobalsTypeName, "Avalonia.Wayland")]
    [DynamicDependency("_knownGlobals", GlobalsTypeName, "Avalonia.Wayland")]
    private WaylandFramePacingCompatibility()
    {
        var eglSurfaceType = Type.GetType("Avalonia.Wayland.Server.Transient.Rendering.WaylandEglWsiSurface, Avalonia.Wayland", true)!;
        _eglSurfaceType = eglSurfaceType;
        _topLevelType = Type.GetType("Avalonia.Wayland.Server.Persistent.WXdgTopLevel, Avalonia.Wayland", true)!;
        _popupType = Type.GetType("Avalonia.Wayland.Server.Persistent.WXdgPopup, Avalonia.Wayland", true)!;
        var surfaceType = Type.GetType("Avalonia.Wayland.Server.Persistent.WSurface, Avalonia.Wayland", true)!;
        var shellType = Type.GetType("Avalonia.Wayland.Server.Persistent.WXdgShellSurface, Avalonia.Wayland", true)!;
        var targetType = Type.GetType("Avalonia.Wayland.Server.Transient.Rendering.WaylandEglWsiSurface+RenderTarget, Avalonia.Wayland", true)!;
        var workerType = Type.GetType("Avalonia.Wayland.Server.WaylandWorker, Avalonia.Wayland", true)!;
        var globalsType = Type.GetType("Avalonia.Wayland.Server.Transient.WaylandGlobals, Avalonia.Wayland", true)!;

        _surface = eglSurfaceType.GetField("_surface", Instance) ?? throw new MissingFieldException(EglSurfaceTypeName, "_surface");
        _frameCallback = surfaceType.GetField("_frameCallback", Instance) ?? throw new MissingFieldException(SurfaceTypeName, "_frameCallback");
        _scale = surfaceType.GetField("_currentScale", Instance) ?? throw new MissingFieldException(SurfaceTypeName, "_currentScale");
        _listener = typeof(WlProxy).GetField("_listener", Instance) ?? throw new MissingFieldException(typeof(WlProxy).FullName, "_listener");
        _configured = shellType.GetField("_initialConfigureAcknowledged", Instance) ?? throw new MissingFieldException(ShellTypeName, "_initialConfigureAcknowledged");
        _configureSerial = shellType.GetField("_pendingAckSerial", Instance) ?? throw new MissingFieldException(ShellTypeName, "_pendingAckSerial");
        _shadowExtents = shellType.GetField("_shadowExtents", Instance) ?? throw new MissingFieldException(ShellTypeName, "_shadowExtents");
        _eglSurface = targetType.GetField("_eglSurface", Instance) ?? throw new MissingFieldException(EglTargetTypeName, "_eglSurface");
        _connection = surfaceType.GetProperty("Connection", Instance) ?? throw new MissingMemberException(SurfaceTypeName, "Connection");
        _wlSurface = surfaceType.GetProperty("WlSurface", Instance) ?? throw new MissingMemberException(SurfaceTypeName, "WlSurface");
        _globals = surfaceType.GetProperty("Globals", Instance) ?? throw new MissingMemberException(SurfaceTypeName, "Globals");
        _worker = surfaceType.GetProperty("Worker", Instance) ?? throw new MissingMemberException(SurfaceTypeName, "Worker");
        _beforeAttach = shellType.GetMethod("OnBeforeNewBufferAttached", Instance, [typeof(IRenderTarget.RenderTargetSceneInfo)]) ?? throw new MissingMethodException(ShellTypeName, "OnBeforeNewBufferAttached");
        _register = surfaceType.GetMethod("RegisterRenderTarget", Instance, [typeof(IDisposable)]) ?? throw new MissingMethodException(SurfaceTypeName, "RegisterRenderTarget");
        _unregister = surfaceType.GetMethod("UnregisterRenderTarget", Instance, [typeof(IDisposable)]) ?? throw new MissingMethodException(SurfaceTypeName, "UnregisterRenderTarget");
        _wakeup = workerType.GetMethod("WakeupRenderLoop", Instance, Type.EmptyTypes) ?? throw new MissingMethodException(WorkerTypeName, "WakeupRenderLoop");
        _ensureWindow = targetType.GetMethod("EnsureWindow", Instance, [typeof(PixelSize)]) ?? throw new MissingMethodException(EglTargetTypeName, "EnsureWindow");
        _registry = globalsType.GetProperty("Registry", Instance) ?? throw new MissingMemberException(GlobalsTypeName, "Registry");
        _knownGlobals = globalsType.GetField("_knownGlobals", Instance) ?? throw new MissingFieldException(GlobalsTypeName, "_knownGlobals");

        if (_frameCallback.FieldType != typeof(WlCallback) || _frameCallback.IsInitOnly ||
            _listener.FieldType != typeof(IWlEventsListener) || _listener.IsInitOnly ||
            _configured.FieldType != typeof(bool) || _configureSerial.FieldType != typeof(uint?) ||
            _scale.FieldType != typeof(double) || _shadowExtents.FieldType != typeof(Thickness?) ||
            _eglSurface.FieldType != typeof(EglSurface) || _surface.FieldType != surfaceType ||
            _beforeAttach.ReturnType != typeof(void) || _ensureWindow.ReturnType != typeof(void) ||
            _registry.PropertyType != typeof(WlRegistry) ||
            _knownGlobals.FieldType != typeof(Dictionary<string, (uint name, uint version)>))
        {
            throw new NotSupportedException("The Wayland frame-pacing member signatures do not match the validated ABI.");
        }
    }

    internal static bool TryCreate([NotNullWhen(true)] out WaylandFramePacingCompatibility? compatibility, out string reason)
    {
        compatibility = null;
        var version = typeof(WaylandPlatformOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var nwayland = typeof(WlProxy).Assembly.GetName().Version;
        if (!OperatingSystem.IsLinux() || version != "12.1.4+2d79ba55fae1b1e71c69f93a668080c9029e6a31" || nwayland != new Version(0, 11, 0, 0))
        {
            reason = $"Unvalidated Wayland/NWayland assembly versions: {version}, {nwayland}.";
            return false;
        }

        try
        {
            compatibility = new WaylandFramePacingCompatibility();
            reason = "Validated Avalonia Wayland EGL frame-pacing adapter.";
            return true;
        }
        catch (Exception error) when (error is TypeLoadException or MissingMemberException or NotSupportedException or MemberAccessException)
        {
            reason = error.Message;
            return false;
        }
    }

    internal IPlatformRenderSurface Wrap(IPlatformRenderSurface surface)
    {
        if (surface.GetType() != _eglSurfaceType)
        {
            return surface;
        }

        return _surfaces.GetValue(surface, CreateSurface);
    }

    private IPlatformRenderSurface CreateSurface(IPlatformRenderSurface original)
    {
        var surface = _surface.GetValue(original)!;
        if (surface.GetType() != _topLevelType && surface.GetType() != _popupType)
        {
            return original;
        }

        if (_states.TryGetValue(surface, out var previous) && previous.IsDisposed)
        {
            _states.Remove(surface);
        }

        var state = _states.GetValue(surface, CreateState);
        return new WaylandPreparedFrameSurface((IGlPlatformSurface)original, state);
    }

    private WaylandSurfaceFrameState CreateState(object surface)
    {
        var wakeup = _wakeup.CreateDelegate<Action>(_worker.GetValue(surface)!);
        var bridge = new WaylandSurfaceBridge
        {
            IsConfigured = () => _globals.GetValue(surface) is not null && _wlSurface.GetValue(surface) is not null && (bool)_configured.GetValue(surface)!,
            CaptureSnapshot = () => new WaylandFrameSnapshot(_connection.GetValue(surface), _wlSurface.GetValue(surface),
                (uint?)_configureSerial.GetValue(surface), (double)_scale.GetValue(surface)!, (Thickness?)_shadowExtents.GetValue(surface) ?? default),
            GetCallback = () => (WlCallback?)_frameCallback.GetValue(surface),
            GetListener = callback => (IWlEventsListener?)_listener.GetValue(callback) ?? throw new InvalidOperationException("The Wayland callback listener has already been released."),
            SetListener = (callback, listener) => _listener.SetValue(callback, listener),
            CancelFailedCallback = callback =>
            {
                if (ReferenceEquals(_frameCallback.GetValue(surface), callback))
                {
                    callback.Dispose();
                    _frameCallback.SetValue(surface, null);
                }
            },
            BeforeAttach = _beforeAttach.CreateDelegate<Action<IRenderTarget.RenderTargetSceneInfo>>(surface),
            BindTarget = target => new WaylandEglTargetBridge(_ensureWindow.CreateDelegate<Action<PixelSize>>(target), () => (EglSurface?)_eglSurface.GetValue(target)),
            Register = _register.CreateDelegate<Action<IDisposable>>(surface),
            Unregister = _unregister.CreateDelegate<Action<IDisposable>>(surface),
            Wakeup = wakeup,
            ReportFailure = error => Logger.TryGet(LogEventLevel.Error, AtomUILogArea.System)?.Log(
                typeof(WaylandFramePacingCompatibility), "Wayland prepared-frame submission failed; retiring its target: {Error}", error),
            GetWlSurface = () => (WlSurface?)_wlSurface.GetValue(surface),
            Presentation = _globals.GetValue(surface) is { } globals
                ? _presentations.GetValue(globals, value => new StrongBox<WaylandPresentationTiming?>(BindPresentation(value))).Value
                : null
        };
        return new WaylandSurfaceFrameState(bridge);
    }

    // Avalonia does not bind wp_presentation; bind it once per connection from the advertised globals.
    private WaylandPresentationTiming? BindPresentation(object globals)
    {
        var known = (Dictionary<string, (uint name, uint version)>)_knownGlobals.GetValue(globals)!;
        var descriptor = WpPresentation.ProxyType.Interface;
        if (!known.TryGetValue(descriptor.Name, out var global))
        {
            return null;
        }
        var registry = (WlRegistry)_registry.GetValue(globals)!;
        var version = Math.Min(global.version, (uint)descriptor.Version);
        var timing = new WaylandPresentationTiming(listener => registry.Bind<WpPresentation>(global.name, version, listener));
        MotionFrameTiming.Clock = new WaylandMotionPresentationClock(timing);
        return timing;
    }
}
