"""Owned desktop preparation, completed before the fixture releases its startup gate."""
import ctypes as C
from ctypes import wintypes as W
import json
from pathlib import Path
import subprocess
import time


class Mouse(C.Structure):
    _fields_ = [("dx", W.LONG), ("dy", W.LONG), ("data", W.DWORD),
                ("flags", W.DWORD), ("time", W.DWORD), ("extra", C.c_size_t)]


class Input(C.Structure):
    _fields_ = [("kind", W.DWORD), ("mouse", Mouse)]


def prepare(process, identity):
    # GetWindowRect is DPI-virtualized; capture, hit testing and cursor input must
    # use the same physical coordinates as the per-monitor-aware native fixture.
    user = C.WinDLL("user32", use_last_error=True)
    user.SetThreadDpiAwarenessContext.argtypes = [W.HANDLE]
    user.SetThreadDpiAwarenessContext.restype = W.HANDLE
    previous = user.SetThreadDpiAwarenessContext(W.HANDLE(-4))
    if not previous:
        raise OSError(C.get_last_error(), "desktop preparation DPI context")
    try:
        return _prepare(process, identity)
    finally:
        if not user.SetThreadDpiAwarenessContext(previous):
            raise OSError(C.get_last_error(), "restore desktop preparation DPI context")


def _prepare(process, identity):
    pid, hwnd, backdrop, ready_tick = identity
    user = C.WinDLL("user32", use_last_error=True)
    kernel = C.WinDLL("kernel32", use_last_error=True)
    kernel.GetTickCount64.restype = C.c_ulonglong
    user.GetForegroundWindow.restype = W.HWND
    user.GetWindowThreadProcessId.argtypes = [W.HWND, C.POINTER(W.DWORD)]
    user.GetWindowThreadProcessId.restype = W.DWORD
    user.GetWindowRect.argtypes = [W.HWND, C.POINTER(W.RECT)]
    user.GetWindowRect.restype = W.BOOL
    user.SetWindowPos.argtypes = [W.HWND, W.HWND, C.c_int, C.c_int, C.c_int, C.c_int, W.UINT]
    user.SetWindowPos.restype = W.BOOL
    user.SetCursorPos.argtypes = [C.c_int, C.c_int]
    user.SetCursorPos.restype = W.BOOL
    user.WindowFromPoint.argtypes = [W.POINT]
    user.WindowFromPoint.restype = W.HWND
    user.GetAncestor.argtypes = [W.HWND, W.UINT]
    user.GetAncestor.restype = W.HWND
    user.SendInput.argtypes = [W.UINT, C.POINTER(Input), C.c_int]
    user.SendInput.restype = W.UINT

    def owned(handle):
        owner = W.DWORD()
        if not handle or not user.GetWindowThreadProcessId(handle, C.byref(owner)) or owner.value != process.pid:
            raise ValueError("Desktop preparation refuses a foreign HWND")

    now = kernel.GetTickCount64()
    if pid != process.pid or not 0 <= now - ready_tick < 15000 or hwnd == backdrop:
        raise ValueError("Desktop startup identity mismatch")
    owned(hwnd)
    if backdrop:
        owned(backdrop)
    record = dict(pid=pid, hwnd=hex(hwnd), backdrop=hex(backdrop), readyTick=ready_tick,
                  prepareTick=now, clicked=False)
    if user.GetForegroundWindow() != hwnd:
        rect = W.RECT()
        if not user.GetWindowRect(hwnd, C.byref(rect)):
            raise OSError(C.get_last_error(), "owned window bounds")
        if not user.SetWindowPos(hwnd, W.HWND(-1), 0, 0, 0, 0, 0x53):
            raise OSError(C.get_last_error(), "owned startup z-order")
        try:
            x, y = rect.left + 100, rect.top + 20
            if not user.SetCursorPos(x, y) or user.GetAncestor(user.WindowFromPoint(W.POINT(x, y)), 2) != hwnd:
                raise ValueError("Desktop preparation refuses a foreign click target")
            clicks = (Input * 2)()
            clicks[0].mouse.flags, clicks[1].mouse.flags = 2, 4
            sent = user.SendInput(2, clicks, C.sizeof(Input))
            if sent != 2:
                if sent == 1:
                    release = (Input * 1)()
                    release[0].mouse.flags = 4
                    user.SendInput(1, release, C.sizeof(Input))
                raise OSError(C.get_last_error(), "owned title-bar activation")
            record["clicked"] = True
            for _ in range(20):
                if user.GetForegroundWindow() == hwnd:
                    break
                time.sleep(.005)
        finally:
            if not user.SetWindowPos(hwnd, W.HWND(-2), 0, 0, 0, 0, 0x13):
                raise OSError(C.get_last_error(), "restore ordinary window z-order")
    if backdrop and not user.SetWindowPos(backdrop, hwnd, 0, 0, 0, 0, 0x13):
        raise OSError(C.get_last_error(), "owned backdrop immediately behind fixture")
    if not user.SetCursorPos(1, 1) or user.GetForegroundWindow() != hwnd:
        raise ValueError("Desktop startup foreground preparation failed")
    if backdrop:
        record["backdropEvidence"] = verify_backdrop(user, hwnd, backdrop)
    record["preparedTick"] = kernel.GetTickCount64()
    return record


def verify_backdrop(user, hwnd, backdrop):
    """Require a covered, visibly composed background before the no-input run."""
    main, background = W.RECT(), W.RECT()
    if not user.GetWindowRect(hwnd, C.byref(main)) or not user.GetWindowRect(backdrop, C.byref(background)):
        raise OSError(C.get_last_error(), "desktop backdrop bounds")
    if not (background.left < main.left and background.top < main.top and
            background.right > main.right and background.bottom > main.bottom):
        raise ValueError("Owned backdrop does not cover the complete native window")
    points = [((background.left + main.left) // 2, (main.top + main.bottom) // 2),
              ((background.right + main.right) // 2, (main.top + main.bottom) // 2),
              ((main.left + main.right) // 2, (background.top + main.top) // 2),
              ((main.left + main.right) // 2, (background.bottom + main.bottom) // 2)]
    user.GetDC.argtypes = [W.HWND]
    user.GetDC.restype = W.HDC
    user.ReleaseDC.argtypes = [W.HWND, W.HDC]
    gdi = C.WinDLL("gdi32", use_last_error=True)
    gdi.GetPixel.argtypes = [W.HDC, C.c_int, C.c_int]
    gdi.GetPixel.restype = W.DWORD
    dwm = C.WinDLL("dwmapi", use_last_error=True)
    dwm.DwmFlush.argtypes = []
    dwm.DwmFlush.restype = C.c_long
    dc = user.GetDC(None)
    if not dc:
        raise OSError(C.get_last_error(), "desktop backdrop DC")
    try:
        deadline = time.monotonic() + 2
        while True:
            if dwm.DwmFlush() < 0:
                raise RuntimeError("Desktop backdrop composition flush failed")
            samples = [dict(x=x, y=y, root=hex(user.GetAncestor(user.WindowFromPoint(W.POINT(x, y)), 2) or 0),
                            rgb=hex(gdi.GetPixel(dc, x, y))) for x, y in points]
            if all(sample["root"] == hex(backdrop) and sample["rgb"] == "0x303030" for sample in samples):
                return dict(main=[main.left, main.top, main.right, main.bottom],
                            background=[background.left, background.top, background.right, background.bottom],
                            samples=samples)
            if time.monotonic() >= deadline:
                raise ValueError("Owned backdrop has not presented a uniform visible background: " + json.dumps(samples))
            time.sleep(.01)
    finally:
        user.ReleaseDC(None, dc)


def run_prepared(command, cwd, environment, output, timeout, directory):
    """One explicit ready/go exchange; no window operations after go is issued."""
    directory = Path(directory)
    directory.mkdir()
    environment = dict(environment, WIN31_STARTUP_GATE_ROOT=str(directory))
    started = time.monotonic()
    record = {}
    with subprocess.Popen(command, cwd=cwd, env=environment, stdout=output, stderr=subprocess.STDOUT) as process:
        try:
            ready = directory / "ready.txt"
            while not ready.is_file():
                if process.poll() is not None:
                    raise RuntimeError("Fixture exited before its mandatory desktop startup gate (exit " +
                                       str(process.returncode) + ")")
                if time.monotonic() - started > 15:
                    raise TimeoutError("Fixture did not open its desktop startup gate")
                time.sleep(.01)
            text = ready.read_text(encoding="utf-8")
            fields = text.splitlines()
            if len(fields) != 4:
                raise ValueError("Malformed desktop startup identity")
            identity = (int(fields[0]), int(fields[1], 16), int(fields[2], 16), int(fields[3]))
            record = prepare(process, identity)
            output.write("DESKTOP_GATE " + json.dumps(record, sort_keys=True) + "\n")
            output.flush()
            temporary = directory / "go.tmp"
            temporary.write_text(text, encoding="utf-8", newline="")
            temporary.replace(directory / "go.txt")
            code = process.wait(timeout=max(.1, timeout - (time.monotonic() - started)))
            return subprocess.CompletedProcess(command, code)
        except BaseException:
            if process.poll() is None:
                process.kill()
                process.wait()
            raise
        finally:
            (directory / "preparation.json").write_text(json.dumps(record, indent=2), encoding="utf-8")
