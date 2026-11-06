using AtomUI.Theme.Styling;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AtomUI.Theme;

internal partial class ThemeManager
{
    private readonly HashSet<ThemeStyleHostLease> _styleHosts = new(ReferenceEqualityComparer.Instance);
    private Func<IStyleHost, IDisposable>? _styleHostCompatibilityFactory;

    internal void InstallStyleHostCompatibility(Func<IStyleHost, IDisposable> factory)
    {
        Dispatcher.UIThread.VerifyAccess();
        ArgumentNullException.ThrowIfNull(factory);
        if (_styleHostCompatibilityFactory is not null)
        {
            if (_styleHostCompatibilityFactory != factory)
            {
                throw new InvalidOperationException("Style host compatibility is already installed.");
            }
            return;
        }
        var attached = new List<ThemeStyleHostLease>();
        try
        {
            foreach (var host in _styleHosts)
            {
                host.AttachCompatibility(factory);
                attached.Add(host);
            }
            _styleHostCompatibilityFactory = factory;
        }
        catch
        {
            foreach (var host in attached)
            {
                host.ReleaseCompatibility();
            }
            throw;
        }
    }

    internal void RegisterStyleHost(ThemeStyleHostLease host)
    {
        if (_styleHostCompatibilityFactory is { } factory)
        {
            host.AttachCompatibility(factory);
        }
        _styleHosts.Add(host);
    }

    internal void UnregisterStyleHost(ThemeStyleHostLease host)
    {
        host.ReleaseCompatibility();
        _styleHosts.Remove(host);
    }
}
