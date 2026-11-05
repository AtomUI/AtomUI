using Avalonia;
using Avalonia.Styling;

namespace AtomUI.Native;

/// <summary>
/// Acquires the shared style-retirement installation before an owner exposes Styles.
/// Window, Popup and theme-manager lifetimes own independent reference-counted leases.
/// </summary>
internal sealed class AvaloniaStyleRemovalHook : IDisposable
{
    private IDisposable? _lease;

    public AvaloniaStyleRemovalHook(StyledElement owner) : this((IStyleHost)owner) { }

    public AvaloniaStyleRemovalHook(IStyleHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        _lease = StyleRemovalHook.Attach(host);
    }

    public void Dispose()
    {
        _lease?.Dispose();
        _lease = null;
    }
}
