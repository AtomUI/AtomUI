using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleHostLease : IDisposable
{
    private IStyleHost? _host;
    private ThemeContext? _context;
    private readonly List<Styles> _mounted = [];
    private bool _disposed;

    private ThemeStyleHostLease(IStyleHost host, ThemeContext context)
    {
        _host = host;
        _context = context;
    }

    internal Styles Current { get; private set; } = null!;
    internal bool IsReady => !_disposed && Current is not null;
    internal bool OwnsHost(IStyleHost host) => IsReady && ReferenceEquals(_host, host);
    private IStyleHost Host => _host ?? throw new ObjectDisposedException(nameof(ThemeStyleHostLease));

    internal ThemeStyleHostStamp CaptureStamp() => new(Host);

    internal bool Matches(ThemeStyleHostStamp stamp) => IsReady && stamp.Matches(Host);

    internal static ThemeStyleHostLease Attach(IStyleHost host, ThemeContext context)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(context);
        if (host is not AvaloniaObject owner)
        {
            throw new ArgumentException("A managed style host must be an AvaloniaObject.", nameof(host));
        }
        owner.VerifyAccess();
        context.Manager.EnsurePresentationHostReady();
        var lease = new ThemeStyleHostLease(host, context);
        var group = lease.Prepare(context.Styles.Current);
        try
        {
            context.Styles.Register(lease);
            lease.Mount(group);
            lease.Current = group;
            return lease;
        }
        catch (Exception exception)
        {
            try
            {
                lease.Dispose();
            }
            catch (Exception cleanupException)
            {
                context.Manager.RetainPresentationCleanup(lease, "StyleHost.Attach", cleanupException);
                throw new AggregateException("Style host attachment and restoration failed.", exception, cleanupException);
            }
            throw;
        }
    }

    internal Styles Prepare(ThemeStyleGeneration generation)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var anchor = _host is Application
            ? new Style(selector => selector.Is<TopLevel>().PropertyEquals(StyledElement.ParentProperty, null))
            : new Style();
        if (_host is Application)
        {
            anchor.Setters.Add(new Setter(ThemeScope.ContextProperty, _context));
        }

        // This is a runtime-selected Context, not a ControlTemplate relationship.
        // A typed observable avoids reflection; native Style frames own the subscription.
        anchor.Setters.Add(new Setter(ThemeStyleQualification.ActiveProperty,
            _context!.Styles.GetObservable(ThemeStyleContext.ActivationProperty).ToBinding()));
        var group = new Styles { anchor };
        // Root Window leases can already see this Context's Application styles. Duplicating
        // them locally would silently outrank application overrides. Other cross-root owners
        // have no such styling ancestor and need their own independent graphs.
        if (!_context.Styles.HasAncestorHost(Host))
        {
            foreach (var asset in generation.Assets)
            {
                group.Add(asset.Create(generation.Activation));
            }
        }
        return group;
    }

    internal void Mount(Styles group)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // AddOwner/observers may throw after the native collection has already changed.
        _mounted.Add(group);
        var currentIndex = Host.Styles.IndexOf(Current);
        if (currentIndex >= 0)
        {
            Host.Styles.Insert(currentIndex + 1, group);
        }
        else
        {
            Host.Styles.Add(group);
        }
    }

    internal void Commit(Styles group)
    {
        Current = group;
    }

    internal void Remove(Styles group)
    {
        if (_host is null)
        {
            return;
        }
        var errors = new List<Exception>();
        TryCleanup(() => Host.Styles.Remove(group), errors);
        // Native removal can stop halfway through the child owners after an observer throws.
        if (_host is IResourceHost resourceHost)
        {
            ReleaseOwners(group, resourceHost, errors);
        }
        TryCleanup(Refresh, errors);
        if (!Host.Styles.Contains(group) && errors.Count == 0)
        {
            _mounted.Remove(group);
        }
        ThrowCleanupErrors(errors);
    }

    public void Dispose()
    {
        if (_host is null)
        {
            return;
        }
        ((AvaloniaObject)_host).VerifyAccess();
        _disposed = true;
        var errors = new List<Exception>();
        foreach (var group in _mounted.AsEnumerable().Reverse().ToArray())
        {
            TryCleanup(() => Remove(group), errors);
        }
        if (errors.Count == 0)
        {
            _context!.Styles.Unregister(this);
            Current = null!;
            _host = null;
            _context = null;
        }
        ThrowCleanupErrors(errors);
    }

    private void Refresh()
    {
        if (Host.Styles.Contains(Current))
        {
            // Native Add invalidates inheritance children (including template nodes), whereas
            // Remove only detaches logical children. Keep the original native rule ordering.
            var index = Host.Styles.IndexOf(Current);
            var errors = new List<Exception>();
            TryCleanup(() => Host.Styles.RemoveAt(index), errors);
            if (!Host.Styles.Contains(Current))
            {
                if (_host is IResourceHost resourceHost)
                {
                    ReleaseOwners(Current, resourceHost, errors);
                }
                TryCleanup(() => Host.Styles.Insert(Math.Min(index, Host.Styles.Count), Current), errors);
            }
            ThrowCleanupErrors(errors);
            return;
        }

        // Final detach has no retained graph. This selector-free Style only matches the host,
        // so its own removal cannot leave template frames. It publishes no public resources.
        var refresh = new Style();
        refresh.Setters.Add(new Setter(ThemeStyleQualification.ActiveProperty,
            (_host as StyledElement)?.GetValue(ThemeStyleQualification.ActiveProperty)));
        try
        {
            Host.Styles.Add(refresh);
        }
        finally
        {
            Host.Styles.Remove(refresh);
        }
    }

    private static void ReleaseOwners(IStyle style, IResourceHost host, List<Exception> errors)
    {
        if (style is IResourceProvider provider)
        {
            TryCleanup(() => provider.RemoveOwner(host), errors);
        }
        foreach (var child in style.Children)
        {
            ReleaseOwners(child, host, errors);
        }
    }

    private static void TryCleanup(Action action, List<Exception> errors)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            errors.Add(exception);
        }
    }

    private static void ThrowCleanupErrors(List<Exception> errors)
    {
        if (errors.Count != 0)
        {
            throw new AggregateException("Managed style cleanup was incomplete; retry is required.", errors);
        }
    }
}
