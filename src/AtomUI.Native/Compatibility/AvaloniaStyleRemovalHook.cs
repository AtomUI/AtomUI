using Avalonia;

namespace AtomUI.Native;

/// <summary>
/// Acquires the shared style-retirement installation before Window exposes Styles.
/// The window and theme-manager lifetimes own independent reference-counted leases.
/// </summary>
internal sealed class AvaloniaStyleRemovalHook : IDisposable
{
    private IDisposable? _lease;

    public AvaloniaStyleRemovalHook(StyledElement owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        _lease = StyleRemovalHook.Attach(owner);
    }

    public void Dispose()
    {
        _lease?.Dispose();
        _lease = null;
    }
}
