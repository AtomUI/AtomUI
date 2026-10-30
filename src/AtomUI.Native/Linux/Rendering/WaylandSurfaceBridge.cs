using Avalonia;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using NWayland.Interop;
using NWayland.Protocols.Wayland;

namespace AtomUI.Native;

// Desktop binds the version-specific Avalonia members. Native only invokes these operations
// from the Wayland worker; it performs no reflection discovery or backend selection.
internal sealed class WaylandSurfaceBridge
{
    internal required Func<bool> IsConfigured { get; init; }
    internal required Func<WaylandFrameSnapshot> CaptureSnapshot { get; init; }
    internal required Func<WlCallback?> GetCallback { get; init; }
    internal required Func<WlCallback, IWlEventsListener> GetListener { get; init; }
    internal required Action<WlCallback, IWlEventsListener> SetListener { get; init; }
    internal required Action<WlCallback> CancelFailedCallback { get; init; }
    internal required Action<IRenderTarget.RenderTargetSceneInfo> BeforeAttach { get; init; }
    internal required Func<IGlPlatformSurfaceRenderTarget, WaylandEglTargetBridge> BindTarget { get; init; }
    internal required Action<IDisposable> Register { get; init; }
    internal required Action<IDisposable> Unregister { get; init; }
    internal required Action Wakeup { get; init; }
    internal required Action<Exception> ReportFailure { get; init; }
    internal required Func<WlSurface?> GetWlSurface { get; init; }
    internal WaylandPresentationTiming? Presentation { get; init; }
}

internal sealed record WaylandEglTargetBridge(Action<PixelSize> EnsureWindow, Func<EglSurface?> GetSurface);

internal readonly record struct WaylandFrameSnapshot(
    object? Connection,
    object? Surface,
    uint? ConfigureSerial,
    double Scale,
    Thickness ShadowExtents);
