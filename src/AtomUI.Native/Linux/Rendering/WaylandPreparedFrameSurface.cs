using System.Diagnostics;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using static Avalonia.OpenGL.GlConsts;

namespace AtomUI.Native;

internal sealed class WaylandPreparedFrameSurface : IGlPlatformSurface
{
    private readonly IGlPlatformSurface _original;
    private readonly WaylandSurfaceBridge _bridge;
    private readonly WaylandFramePacer _pacer;
    private readonly WaylandSurfaceFrameState _state;

    internal WaylandPreparedFrameSurface(IGlPlatformSurface original, WaylandSurfaceFrameState state)
    {
        _original = original;
        _state = state;
        _bridge = state.Bridge;
        _pacer = state.Pacer;
    }

    public bool IsReady => !_state.IsDisposed && (_pacer.IsFailed
        ? _original.IsReady
        : _bridge.IsConfigured() && _pacer.CanPrepare);

    public IGlPlatformSurfaceRenderTarget CreateGlRenderTarget(IGlContext context)
    {
        _state.VerifyAccess();
        ObjectDisposedException.ThrowIf(_state.IsDisposed, this);
        var original = _original.CreateGlRenderTarget(context);
        if (_pacer.IsFailed || context is not EglContext egl)
        {
            return original;
        }

        try
        {
            return new Target(this, original, egl, _bridge.BindTarget(original));
        }
        catch
        {
            original.Dispose();
            throw;
        }
    }

    private sealed class Target : IGlPlatformSurfaceRenderTarget
    {
        private readonly WaylandPreparedFrameSurface _surface;
        private readonly IGlPlatformSurfaceRenderTarget _original;
        private readonly EglContext _context;
        private readonly WaylandEglTargetBridge _bridge;
        private readonly WaylandEglSubmission _submission;
        private bool _disposed;
        private bool _invalidated;

        internal Target(WaylandPreparedFrameSurface surface, IGlPlatformSurfaceRenderTarget original,
            EglContext context, WaylandEglTargetBridge bridge)
        {
            _surface = surface;
            _original = original;
            _context = context;
            _bridge = bridge;
            _submission = new WaylandEglSubmission(context.EglInterface, context.NotifyContextLost);
        }

        public PlatformRenderTargetState State
        {
            get
            {
                if (_disposed || _surface._state.IsDisposed)
                {
                    return PlatformRenderTargetState.Disposed;
                }

                if (_invalidated || _original.State.IsCorrupted || _context.IsLost || _surface._pacer.IsFailed)
                {
                    return PlatformRenderTargetState.Corrupted;
                }

                return _surface.IsReady
                    ? PlatformRenderTargetState.Ready
                    : PlatformRenderTargetState.NotReadyWillWakeupRenderLoop;
            }
        }

        public IGlPlatformSurfaceRenderingSession BeginDraw(IRenderTarget.RenderTargetSceneInfo sceneInfo)
        {
            _surface._state.VerifyAccess();
            var state = State;
            if (state.IsCorrupted)
            {
                throw new RenderTargetCorruptedException();
            }

            if (!state.IsReady)
            {
                throw new RenderTargetNotReadyException();
            }

            var size = new PixelSize(Math.Max(1, sceneInfo.Size.Width), Math.Max(1, sceneInfo.Size.Height));
            _bridge.EnsureWindow(size);
            var eglSurface = _bridge.GetSurface() ?? throw new RenderTargetCorruptedException();
            var restore = _context.MakeCurrent(eglSurface);
            try
            {
                var gl = _context.GlInterface;
                gl.BindFramebuffer(GL_FRAMEBUFFER, 0);
                if (_context.Version.Type == GlProfileType.OpenGL)
                {
                    gl.Viewport(0, 0, size.Width, size.Height);
                    if (gl.IsReadBufferAvailable)
                    {
                        gl.ReadBuffer(GL_BACK);
                    }
                    if (gl.IsWriteBufferAvailable)
                    {
                        gl.WriteBuffer(GL_BACK);
                    }
                    if (gl.IsDrawBufferAvailable)
                    {
                        gl.DrawBuffer(GL_BACK);
                    }
                }

                // The scene content is fixed once drawing starts. Schedule this frame's presentation now so the UI
                // spends the whole draw producing the next frame instead of waiting for this draw to finish.
                var presentation = _surface._bridge.Presentation;
                presentation?.OnFrameStarted();
                return new Session(this, eglSurface, sceneInfo, size, restore, _surface._bridge.CaptureSnapshot(),
                    presentation?.OnFramePrepared(Stopwatch.GetTimestamp()));
            }
            catch
            {
                restore.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _surface._state.VerifyAccess();
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _surface._pacer.Cancel(this);
            _original.Dispose();
        }

        private sealed class Session(
            Target owner,
            EglSurface eglSurface,
            IRenderTarget.RenderTargetSceneInfo sceneInfo,
            PixelSize size,
            IDisposable restore,
            WaylandFrameSnapshot snapshot,
            long? predictedPresentation) : IGlPlatformSurfaceRenderingSession, IWaylandPreparedFrame
        {
            private bool _drawingCompleted;
            private bool _released;

            public IGlContext Context => owner._context;
            public PixelSize Size => size;
            public double Scaling => sceneInfo.Scaling;
            public bool IsYFlipped => false;
            public object Owner => owner;
            public bool IsValid => !owner._disposed && !owner._invalidated && !owner._surface._state.IsDisposed &&
                                   !owner._original.State.IsCorrupted && !owner._context.IsLost &&
                                   owner._surface._bridge.IsConfigured() &&
                                   snapshot == owner._surface._bridge.CaptureSnapshot();

            public void Dispose()
            {
                if (_drawingCompleted)
                {
                    return;
                }

                _drawingCompleted = true;
                try
                {
                    try
                    {
                        owner._context.GlInterface.Flush();
                    }
                    finally
                    {
                        restore.Dispose();
                    }

                    owner._surface._pacer.CompleteFrame(this);
                }
                catch
                {
                    // An abandoned frame never reaches Submit or Invalidate; keep the in-flight count exact.
                    Release();
                    throw;
                }
            }

            public void Submit()
            {
                owner._surface._state.VerifyAccess();
                var bridge = owner._surface._bridge;
                if (bridge.GetCallback() is not null)
                {
                    throw new InvalidOperationException("A Wayland callback must complete before submitting the prepared frame.");
                }

                var committed = false;
                using var current = owner._context.MakeCurrent(eglSurface);
                try
                {
                    var display = owner._context.Display;
                    display.EglInterface.SwapInterval(display.Handle, 0);
                    bridge.BeforeAttach(sceneInfo);
                    if (bridge.Presentation is { } presentation && bridge.GetWlSurface() is { } wlSurface)
                    {
                        presentation.RequestFeedback(wlSurface, predictedPresentation);
                    }
                    owner._surface._state.InstallCallbackRelay();
                    owner._submission.SwapBuffers(display.Handle, eglSurface.DangerousGetHandle());
                    committed = true;
                }
                finally
                {
                    Release();
                    if (!committed && bridge.GetCallback() is { } staged)
                    {
                        // This callback was created by this failed submission, while no earlier
                        // callback was pending. Retire only that exact object; never clear a live
                        // callback merely to make the surface appear ready.
                        bridge.CancelFailedCallback(staged);
                    }
                }
            }

            public void Invalidate()
            {
                owner._invalidated = true;
                Release();
            }

            private void Release()
            {
                if (!_released)
                {
                    _released = true;
                    owner._surface._bridge.Presentation?.OnFrameReleased();
                }
            }
        }
    }
}
