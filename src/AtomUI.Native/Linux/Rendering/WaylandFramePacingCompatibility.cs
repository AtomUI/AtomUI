using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using NWayland;
using NWayland.Interop;
using NWayland.Protocols.PresentationTime;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native;

// Bind the required private member contract; package versions are diagnostic context only.
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
    private readonly Action<Exception> _reportFailure;
    private readonly Action<WaylandPresentationTiming> _presentationTimingBound;

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
    private WaylandFramePacingCompatibility(Action<Exception> reportFailure, Action<WaylandPresentationTiming> presentationTimingBound)
    {
        _reportFailure = reportFailure;
        _presentationTimingBound = presentationTimingBound;
        var eglSurfaceType = Type.GetType("Avalonia.Wayland.Server.Transient.Rendering.WaylandEglWsiSurface, Avalonia.Wayland", true)!;
        _eglSurfaceType = eglSurfaceType;
        _topLevelType = Type.GetType("Avalonia.Wayland.Server.Persistent.WXdgTopLevel, Avalonia.Wayland", true)!;
        _popupType = Type.GetType("Avalonia.Wayland.Server.Persistent.WXdgPopup, Avalonia.Wayland", true)!;
        var surfaceType = Type.GetType("Avalonia.Wayland.Server.Persistent.WSurface, Avalonia.Wayland", true)!;
        var shellType = Type.GetType("Avalonia.Wayland.Server.Persistent.WXdgShellSurface, Avalonia.Wayland", true)!;
        var targetType = Type.GetType("Avalonia.Wayland.Server.Transient.Rendering.WaylandEglWsiSurface+RenderTarget, Avalonia.Wayland", true)!;
        var workerType = Type.GetType("Avalonia.Wayland.Server.WaylandWorker, Avalonia.Wayland", true)!;
        var globalsType = Type.GetType("Avalonia.Wayland.Server.Transient.WaylandGlobals, Avalonia.Wayland", true)!;
        var connectionType = Type.GetType("Avalonia.Wayland.Server.Interop.WaylandConnection, Avalonia.Wayland", true)!;

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

        if (!typeof(IGlPlatformSurface).IsAssignableFrom(eglSurfaceType) ||
            !typeof(IGlPlatformSurfaceRenderTarget).IsAssignableFrom(targetType) ||
            !surfaceType.IsAssignableFrom(shellType) || !shellType.IsAssignableFrom(_topLevelType) ||
            !shellType.IsAssignableFrom(_popupType))
        {
            throw new NotSupportedException("The Wayland EGL surface/render-target or persistent shell inheritance contract changed.");
        }
        WaylandMemberContract.ReadField(_surface, eglSurfaceType, "_surface", surfaceType);
        WaylandMemberContract.WriteField(_frameCallback, surfaceType, "_frameCallback", typeof(WlCallback));
        WaylandMemberContract.WriteField(_listener, typeof(WlProxy), "_listener", typeof(IWlEventsListener));
        WaylandMemberContract.ReadField(_configured, shellType, "_initialConfigureAcknowledged", typeof(bool));
        WaylandMemberContract.ReadField(_configureSerial, shellType, "_pendingAckSerial", typeof(uint?));
        WaylandMemberContract.ReadField(_scale, surfaceType, "_currentScale", typeof(double));
        WaylandMemberContract.ReadField(_shadowExtents, shellType, "_shadowExtents", typeof(Thickness?));
        WaylandMemberContract.ReadField(_eglSurface, targetType, "_eglSurface", typeof(EglSurface));
        WaylandMemberContract.ReadProperty(_connection, surfaceType, "Connection", connectionType);
        WaylandMemberContract.ReadProperty(_wlSurface, surfaceType, "WlSurface", typeof(WlSurface));
        WaylandMemberContract.ReadProperty(_globals, surfaceType, "Globals", globalsType);
        WaylandMemberContract.ReadProperty(_worker, surfaceType, "Worker", workerType);
        WaylandMemberContract.InstanceMethod(_beforeAttach, shellType, "OnBeforeNewBufferAttached", typeof(void), typeof(IRenderTarget.RenderTargetSceneInfo));
        WaylandMemberContract.InstanceMethod(_register, surfaceType, "RegisterRenderTarget", typeof(void), typeof(IDisposable));
        WaylandMemberContract.InstanceMethod(_unregister, surfaceType, "UnregisterRenderTarget", typeof(void), typeof(IDisposable));
        WaylandMemberContract.InstanceMethod(_wakeup, workerType, "WakeupRenderLoop", typeof(void));
        WaylandMemberContract.InstanceMethod(_ensureWindow, targetType, "EnsureWindow", typeof(void), typeof(PixelSize));
        WaylandMemberContract.ReadProperty(_registry, globalsType, "Registry", typeof(WlRegistry));
        WaylandMemberContract.ReadField(_knownGlobals, globalsType, "_knownGlobals", typeof(Dictionary<string, (uint name, uint version)>));
        WaylandMemberContract.InstanceMethod(typeof(IWlEventsListener).GetMethod(nameof(IWlEventsListener.DispatchEvent), [typeof(WlEventArgs)]),
            typeof(IWlEventsListener), nameof(IWlEventsListener.DispatchEvent), typeof(void), typeof(WlEventArgs));
        if (!typeof(IWlEventsListener).IsAssignableFrom(typeof(WaylandFrameCallbackRelay)) ||
            !typeof(WlProxy).IsAssignableFrom(typeof(WlCallback)))
        {
            throw new NotSupportedException("The NWayland callback/listener type contract changed.");
        }
    }

    internal static bool TryCreate(Action<Exception> reportFailure, Action<WaylandPresentationTiming> presentationTimingBound,
        [NotNullWhen(true)] out WaylandFramePacingCompatibility? compatibility, out string reason)
    {
        compatibility = null;
        try
        {
            compatibility = new WaylandFramePacingCompatibility(reportFailure, presentationTimingBound);
            reason = "Required Wayland EGL/NWayland member contracts bound. " + DescribeVersions();
            return true;
        }
        catch (Exception error) when (error is TypeLoadException or MissingMemberException or NotSupportedException or MemberAccessException or AmbiguousMatchException)
        {
            reason = error.Message + " " + DescribeVersions();
            return false;
        }
    }

    private static string DescribeVersions() =>
        $"Backend={Type.GetType("Avalonia.WaylandPlatformOptions, Avalonia.Wayland")?.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}; NWayland={typeof(WlProxy).Assembly.GetName().Version}.";

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
            ReportFailure = _reportFailure,
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
        _presentationTimingBound(timing);
        return timing;
    }
}
