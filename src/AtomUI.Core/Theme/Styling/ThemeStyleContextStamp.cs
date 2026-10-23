namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleContextStamp
{
    private readonly ThemeStyleContext _context;
    private readonly ThemeStyleGeneration _generation;
    private readonly long _hostRevision;
    private readonly (ThemeStyleHostLease Host, ThemeStyleHostStamp Stamp)[] _hosts;

    internal ThemeStyleContextStamp(ThemeStyleContext context, IEnumerable<ThemeStyleHostLease> hosts)
    {
        _context = context;
        _generation = context.Current;
        _hostRevision = context.HostRevision;
        _hosts = hosts.Select(static host => (host, host.CaptureStamp())).ToArray();
    }

    internal bool IsCurrent => !_context.HasPendingUpdate && ReferenceEquals(_context.Current, _generation) &&
                               _context.HostRevision == _hostRevision &&
                               _hosts.All(static item => item.Host.Matches(item.Stamp));
}
