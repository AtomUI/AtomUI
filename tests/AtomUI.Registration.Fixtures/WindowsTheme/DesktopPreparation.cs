// Explicit Windows specialist startup gate; never runs during the measured trajectory.
using Avalonia;
using Avalonia.Controls;

namespace Win31;

internal static class DesktopPreparation
{
    internal static async Task WaitAsync(Window window, Window? backdrop)
    {
        var root = Environment.GetEnvironmentVariable("WIN31_STARTUP_GATE_ROOT");
        if (string.IsNullOrEmpty(root))
        {
            return;
        }

        var hwnd = window.TryGetPlatformHandle()!.Handle;
        var backdropHwnd = backdrop?.TryGetPlatformHandle()?.Handle ?? 0;
        var readyTick = Environment.TickCount64;
        var identity = $"{Environment.ProcessId}\n{hwnd:X}\n{backdropHwnd:X}\n{readyTick}";
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "ready.tmp"), identity);
        File.Move(Path.Combine(root, "ready.tmp"), Path.Combine(root, "ready.txt"));
        var go = Path.Combine(root, "go.txt");
        while (!File.Exists(go))
        {
            if (Environment.TickCount64 - readyTick > 15000)
            {
                throw new TimeoutException("Desktop startup preparation did not complete.");
            }
            await Task.Delay(10);
        }
        if (File.ReadAllText(go) != identity || Win.GetForegroundWindow() != hwnd)
        {
            throw new InvalidOperationException("Desktop startup identity/foreground mismatch.");
        }
        Console.WriteLine($"DESKTOP_PREPARED pid={Environment.ProcessId} hwnd={hwnd:X} readyTick={readyTick} startTick={Environment.TickCount64}");
    }
}
