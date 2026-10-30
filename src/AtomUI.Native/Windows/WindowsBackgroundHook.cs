using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Native.Windows;

[SupportedOSPlatform("windows")]
internal sealed class WindowsBackgroundHook : INativeWindowBackgroundHook
{
    private readonly Window _window;
    private Color? _backgroundColor;
    private IntPtr _backgroundBrush;
    private bool _isSizeMoving;
    private bool _isDisposed;

    public WindowsBackgroundHook(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        Win32Properties.AddWndProcHookCallback(_window, HandleWndProc);
    }

    public void SetBackgroundColor(Color? color)
    {
        if (_isDisposed)
        {
            return;
        }

        var effectiveColor = color is { A: > 0 } value
            ? Color.FromRgb(value.R, value.G, value.B)
            : (Color?)null;
        if (_backgroundColor == effectiveColor)
        {
            return;
        }

        ReleaseBackgroundBrush();
        _backgroundColor = effectiveColor;
        if (effectiveColor is { } brushColor)
        {
            _backgroundBrush = WindowUtilsInterop.CreateSolidBrush(ToColorRef(brushColor));
        }
    }

    public void PaintClientArea()
    {
        if (_isDisposed ||
            _backgroundBrush == IntPtr.Zero ||
            _window.TryGetPlatformHandle() is not { Handle: var handle } ||
            handle == IntPtr.Zero)
        {
            return;
        }

        PaintClientArea(handle);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        Win32Properties.RemoveWndProcHookCallback(_window, HandleWndProc);
        ReleaseBackgroundBrush();
    }

    private IntPtr HandleWndProc(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (_isDisposed || _backgroundBrush == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        switch (msg)
        {
            case WindowUtilsInterop.WM_ENTERSIZEMOVE:
                _isSizeMoving = true;
                break;
            case WindowUtilsInterop.WM_EXITSIZEMOVE:
                _isSizeMoving = false;
                break;
            case WindowUtilsInterop.WM_ERASEBKGND:
                // live resize 期间不填充：RedirectionSurface 下 GDI 填充会先于渲染帧被 DWM 合成，
                // 表现为整窗闪背景色。此时交还 Avalonia 默认行为（窗口类无背景画刷，不擦除）。
                if (_isSizeMoving)
                {
                    break;
                }
                if (wParam != IntPtr.Zero && TryFillClientArea(hWnd, wParam))
                {
                    handled = true;
                    return new IntPtr(1);
                }
                break;
            case WindowUtilsInterop.WM_SHOWWINDOW:
                if (wParam != IntPtr.Zero)
                {
                    PaintClientArea(hWnd);
                }
                break;
        }

        return IntPtr.Zero;
    }

    private void PaintClientArea(IntPtr hWnd)
    {
        var hdc = WindowUtilsInterop.GetDC(hWnd);
        if (hdc == IntPtr.Zero)
        {
            return;
        }

        try
        {
            TryFillClientArea(hWnd, hdc);
        }
        finally
        {
            WindowUtilsInterop.ReleaseDC(hWnd, hdc);
        }
    }

    private bool TryFillClientArea(IntPtr hWnd, IntPtr hdc)
    {
        if (!WindowUtilsInterop.GetClientRect(hWnd, out var clientRect) ||
            clientRect.Width <= 0 ||
            clientRect.Height <= 0)
        {
            return false;
        }

        return WindowUtilsInterop.FillRect(hdc, ref clientRect, _backgroundBrush) != 0;
    }

    private void ReleaseBackgroundBrush()
    {
        if (_backgroundBrush == IntPtr.Zero)
        {
            return;
        }

        WindowUtilsInterop.DeleteObject(_backgroundBrush);
        _backgroundBrush = IntPtr.Zero;
    }

    private static int ToColorRef(Color color)
    {
        return color.R | (color.G << 8) | (color.B << 16);
    }
}
