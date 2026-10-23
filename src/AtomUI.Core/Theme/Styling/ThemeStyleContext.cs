using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleContext : AvaloniaObject
{
    internal static readonly StyledProperty<ThemeStyleActivation?> ActivationProperty =
        AvaloniaProperty.Register<ThemeStyleContext, ThemeStyleActivation?>("Activation");

    private readonly HashSet<ThemeStyleHostLease> _hosts = new(ReferenceEqualityComparer.Instance);
    private ThemeStyleUpdate? _pendingUpdate;

    internal ThemeStyleContext(IReadOnlyList<ThemeStyleAsset>? assets = null)
    {
        Current = new ThemeStyleGeneration(assets ?? Array.Empty<ThemeStyleAsset>());
        SetValue(ActivationProperty, Current.Activation);
    }

    internal ThemeStyleGeneration Current { get; private set; }
    internal long HostRevision { get; private set; }
    internal bool HasPendingUpdate => _pendingUpdate is not null;

    internal ThemeStyleContextStamp CaptureStamp() => new(this, _hosts);

    internal ThemeStyleUpdate Prepare(IReadOnlyList<ThemeStyleAsset> assets)
    {
        Dispatcher.UIThread.VerifyAccess();
        ArgumentNullException.ThrowIfNull(assets);
        if (_pendingUpdate is not null)
        {
            throw new InvalidOperationException("The previous style update must be restored or retired first.");
        }
        if (_hosts.Any(static host => !host.IsReady))
        {
            throw new InvalidOperationException("A style host is still attaching or awaiting cleanup.");
        }

        var generation = new ThemeStyleGeneration(assets);
        var update = new ThemeStyleUpdate(this, generation, _hosts.ToArray());
        _pendingUpdate = update;
        try
        {
            update.PrepareHosts();
        }
        catch
        {
            Complete(update);
            throw;
        }
        return update;
    }

    internal void Register(ThemeStyleHostLease host)
    {
        if (_hosts.Any(static existing => !existing.IsReady))
        {
            throw new InvalidOperationException("An existing style host has not finished attaching or cleaning up.");
        }
        _hosts.Add(host);
        HostRevision++;
    }

    internal bool HasAncestorHost(IStyleHost host)
    {
        for (var parent = host.StylingParent; parent is not null; parent = parent.StylingParent)
        {
            if (_hosts.Any(lease => lease.OwnsHost(parent)))
            {
                return true;
            }
        }
        return false;
    }

    internal void Unregister(ThemeStyleHostLease host)
    {
        if (_hosts.Remove(host))
        {
            HostRevision++;
        }
    }

    internal void Commit(ThemeStyleGeneration generation)
    {
        Current = generation;
    }

    internal void Complete(ThemeStyleUpdate update)
    {
        if (ReferenceEquals(_pendingUpdate, update))
        {
            _pendingUpdate = null;
        }
    }
}

internal sealed class ThemeStyleGeneration
{
    internal ThemeStyleGeneration(IReadOnlyList<ThemeStyleAsset> assets)
    {
        var copy = assets.ToArray();
        if (copy.Any(static asset => asset is null))
        {
            throw new ArgumentException("A style generation cannot contain a null asset.", nameof(assets));
        }
        Assets = Array.AsReadOnly(copy);
    }

    internal ThemeStyleActivation Activation { get; } = new();
    internal IReadOnlyList<ThemeStyleAsset> Assets { get; }
}
