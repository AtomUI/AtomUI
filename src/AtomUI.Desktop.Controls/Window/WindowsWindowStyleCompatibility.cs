using System.Runtime.CompilerServices;
using AtomUI.Native;
using Avalonia.Logging;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls;

/// <summary>Owns native style compatibility for all application windows, including ordinary Avalonia windows.</summary>
internal sealed class WindowsWindowStyleCompatibility : IDisposable
{
    private readonly ConditionalWeakTable<AvaloniaWindow, IDisposable> _hooks = new();
    private readonly IDisposable _openedSubscription;
    private readonly IDisposable _closedSubscription;
    private bool _isDisposed;
    private bool _hasReportedCompatibilityFailure;

    public WindowsWindowStyleCompatibility()
    {
        // The routed event precedes styling, initial layout and PlatformImpl.Show; CLR Opened is too late.
        _openedSubscription = AvaloniaWindow.WindowOpenedEvent.AddClassHandler<AvaloniaWindow>((window, _) => Attach(window));
        _closedSubscription = AvaloniaWindow.WindowClosedEvent.AddClassHandler<AvaloniaWindow>((window, _) => Detach(window));
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        _openedSubscription.Dispose();
        _closedSubscription.Dispose();
        foreach (var entry in _hooks)
        {
            entry.Value.Dispose();
        }
        _hooks.Clear();
    }

    private void Attach(AvaloniaWindow window)
    {
        if (!_isDisposed && !_hooks.TryGetValue(window, out _) &&
            NativeWindowStyles.TryAttachWindowsBorderlessStyleHook(window, ReportFailure) is { } hook)
        {
            _hooks.Add(window, hook);
        }
    }

    private void Detach(AvaloniaWindow window)
    {
        if (_hooks.TryGetValue(window, out var hook))
        {
            hook.Dispose();
            _hooks.Remove(window);
        }
    }

    private void ReportFailure(string reason)
    {
        if (_hasReportedCompatibilityFailure)
        {
            return;
        }
        _hasReportedCompatibilityFailure = true;
        Logger.TryGet(LogEventLevel.Warning, AtomUILogArea.System)?.Log(
            typeof(WindowsWindowStyleCompatibility), "Windows borderless window style compatibility failed: {Reason}", reason);
    }
}
