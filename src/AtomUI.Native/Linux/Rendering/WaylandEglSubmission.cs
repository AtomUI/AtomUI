using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;

namespace AtomUI.Native;

internal sealed unsafe class WaylandEglSubmission
{
    private readonly EglInterface _egl;
    private readonly Action? _notifyContextLost;
    private readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int> _swapBuffers;

    internal WaylandEglSubmission(EglInterface egl, Action? notifyContextLost = null)
    {
        _egl = egl;
        _notifyContextLost = notifyContextLost;
        // Resolve against the active driver's loader; do not load another EGL implementation.
        fixed (byte* name = "eglSwapBuffers\0"u8)
        {
            _swapBuffers = (delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int>)egl.GetProcAddress((IntPtr)name);
        }

        if (_swapBuffers == null)
        {
            throw new OpenGlException("The active EGL implementation does not expose eglSwapBuffers.");
        }
    }

    internal void SwapBuffers(IntPtr display, IntPtr surface)
    {
        // Avalonia's current void wrapper loses EGLBoolean. Keep the result so a failed
        // deferred commit cannot leave an uncommitted callback blocking the next target.
        if (_swapBuffers(display, surface) == 0)
        {
            var error = _egl.GetError();
            if (error == EglConsts.EGL_CONTEXT_LOST)
            {
                _notifyContextLost?.Invoke();
                throw new PlatformGraphicsContextLostException();
            }

            throw new OpenGlException($"Wayland eglSwapBuffers failed (EGL error 0x{error:X}).");
        }
    }
}
