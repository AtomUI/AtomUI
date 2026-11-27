using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme;

/// <summary>
/// Binds a <see cref="TopLevel" /> to one <see cref="ThemeContext" /> for the lifetime of the lease.
/// <c>Attach</c> merges a <see cref="ThemeContextResourceBridge" /> of the context into the resources of the
/// host, sets the theme scope context and the requested theme variant of the host, registers the lease with the
/// context, and disposes a previous lease of the same host. <c>Dispose</c> removes the bridge and clears these
/// three values while the lease is still stored on the host; a window host disposes the lease when it closes.
/// AtomUI.Desktop.Controls uses it in <c>Window</c>.
/// </summary>
internal sealed class ThemeContextLease : IDisposable
{
    private static readonly AttachedProperty<ThemeContextLease?> LeaseProperty =
        AvaloniaProperty.RegisterAttached<ThemeConfigProvider, TopLevel, ThemeContextLease?>(
            "ThemeContextLease");

    private TopLevel? _host;
    private ThemeContext? _ownerContext;
    private ThemeContextResourceBridge? _bridge;

    private ThemeContextLease(
        TopLevel host,
        ThemeContext ownerContext,
        ThemeContextResourceBridge bridge)
    {
        _host         = host;
        _ownerContext = ownerContext;
        _bridge       = bridge;
        ownerContext.Published += HandleContextPublished;
        if (host is Window window)
        {
            window.Closed += HandleWindowClosed;
        }
    }

    internal static ThemeContextLease Attach(
        TopLevel host,
        ThemeContext ownerContext)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(ownerContext);

        var previous = host.GetValue(LeaseProperty);
        var hadPreviousLeaseValue = host.IsSet(LeaseProperty);
        var previousContext = host.GetValue(ThemeScope.ContextProperty);
        var hadPreviousContextValue = host.IsSet(ThemeScope.ContextProperty);
        var previousVariant = host.GetValue(TopLevel.RequestedThemeVariantProperty);
        var hadPreviousVariantValue = host.IsSet(TopLevel.RequestedThemeVariantProperty);
        var bridge = new ThemeContextResourceBridge(ownerContext);
        ThemeContextLease? lease = null;
        var registeredWithManager = false;
        try
        {
            host.Resources.MergedDictionaries.Add(bridge);
            host.SetValue(ThemeScope.ContextProperty, ownerContext);
            host.SetCurrentValue(
                TopLevel.RequestedThemeVariantProperty,
                ToAvaloniaVariant(ownerContext.Appearance));
            lease = new ThemeContextLease(host, ownerContext, bridge);
            host.SetValue(LeaseProperty, lease);
            ownerContext.Manager.RegisterContextLease(lease);
            registeredWithManager = true;
            bridge.RestartListening();
        }
        catch
        {
            if (hadPreviousLeaseValue)
            {
                CleanupBoundary(() => host.SetValue(LeaseProperty, previous));
            }
            else
            {
                CleanupBoundary(() => host.ClearValue(LeaseProperty));
            }
            if (hadPreviousContextValue)
            {
                CleanupBoundary(() => host.SetValue(ThemeScope.ContextProperty, previousContext));
            }
            else
            {
                CleanupBoundary(() => host.ClearValue(ThemeScope.ContextProperty));
            }
            if (hadPreviousVariantValue)
            {
                CleanupBoundary(() =>
                    host.SetValue(TopLevel.RequestedThemeVariantProperty, previousVariant));
            }
            else
            {
                CleanupBoundary(() => host.ClearValue(TopLevel.RequestedThemeVariantProperty));
            }
            CleanupBoundary(() => host.Resources.MergedDictionaries.Remove(bridge));
            CleanupBoundary(bridge.Dispose);
            if (lease is not null)
            {
                CleanupBoundary(lease.ReleaseSubscriptions);
                if (registeredWithManager)
                {
                    CleanupBoundary(() => ownerContext.Manager.UnregisterContextLease(lease));
                }
            }
            throw;
        }

        previous?.Dispose();
        return lease;
    }

    internal bool IsOwnedBy(ThemeContext context)
    {
        return ReferenceEquals(_ownerContext, context);
    }

    public void Dispose()
    {
        var host = Interlocked.Exchange(ref _host, null);
        var context = Interlocked.Exchange(ref _ownerContext, null);
        var bridge = Interlocked.Exchange(ref _bridge, null);
        if (host is null)
        {
            return;
        }

        if (context is not null)
        {
            CleanupBoundary(() => context.Published -= HandleContextPublished);
            CleanupBoundary(() => context.Manager.UnregisterContextLease(this));
        }
        if (host is Window window)
        {
            CleanupBoundary(() => window.Closed -= HandleWindowClosed);
        }
        if (bridge is not null)
        {
            CleanupBoundary(() => host.Resources.MergedDictionaries.Remove(bridge));
            CleanupBoundary(bridge.Dispose);
        }

        var ownsHostState = false;
        CleanupBoundary(() => ownsHostState = ReferenceEquals(host.GetValue(LeaseProperty), this));
        if (ownsHostState)
        {
            CleanupBoundary(() => host.ClearValue(LeaseProperty));
            CleanupBoundary(() => host.ClearValue(ThemeScope.ContextProperty));
            CleanupBoundary(() => host.ClearValue(TopLevel.RequestedThemeVariantProperty));
        }
    }

    private void HandleContextPublished(object? sender, EventArgs args)
    {
        var host = _host;
        var context = _ownerContext;
        if (host is not null && context is not null)
        {
            host.SetCurrentValue(
                TopLevel.RequestedThemeVariantProperty,
                ToAvaloniaVariant(context.Appearance));
        }
    }

    private void HandleWindowClosed(object? sender, EventArgs args)
    {
        Dispose();
    }

    private void ReleaseSubscriptions()
    {
        if (_ownerContext is not null)
        {
            CleanupBoundary(() => _ownerContext.Published -= HandleContextPublished);
        }
        if (_host is Window window)
        {
            CleanupBoundary(() => window.Closed -= HandleWindowClosed);
        }
    }

    private static void CleanupBoundary(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private static ThemeVariant ToAvaloniaVariant(ThemeAppearance appearance)
    {
        return appearance == ThemeAppearance.Dark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
    }
}
