using AtomUI.Theme;
using Avalonia;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// Forwards a host's Token owner to a part that lives outside the host's logical subtree (popup presenters,
/// runtime dialog surfaces, viewers the host takes over), so the part's <c>OwnerTokenResource</c> reads and
/// <c>TokenTransitions</c> resolve to the host. The forwarded value is the host's inherited or injected
/// <c>ThemeScope.TokenOwner</c>; when the host itself is not styled in a tree (a dialog opened through a placement
/// target, a picker whose presenter is shown alone) it is the registered identity of the host's type, the same
/// identity the default ControlTheme would inject, resolved through the part's or host's theme context once either
/// is attached. Disposing clears the forwarded value so the part returns to inheritance.
/// </summary>
internal static class TokenOwnerForwarding
{
    public static IDisposable Forward(Control host, Control part)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(part);
        return new Forwarded(host, part);
    }

    private sealed class Forwarded : IDisposable
    {
        private readonly Control _host;
        private readonly Control _part;
        private readonly IDisposable _hostOwnerSubscription;
        private bool _disposed;

        internal Forwarded(Control host, Control part)
        {
            _host = host;
            _part = part;
            _part.AttachedToLogicalTree += HandlePartAttached;
            _hostOwnerSubscription = host.GetObservable(ThemeScope.TokenOwnerProperty).Subscribe(_ => Apply());
            Apply();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _part.AttachedToLogicalTree -= HandlePartAttached;
            _hostOwnerSubscription.Dispose();
            _part.ClearValue(ThemeScope.TokenOwnerProperty);
        }

        private void HandlePartAttached(object? sender, LogicalTreeAttachmentEventArgs e) => Apply();

        private void Apply()
        {
            if (_disposed)
            {
                return;
            }
            var owner = ThemeScope.GetTokenOwner(_host) ?? RegisteredIdentity();
            if (owner is null)
            {
                // No host owner and no registered identity: a local null would block the owner the part can still
                // inherit from its own ancestors, so the part is left to inheritance until a host owner appears.
                _part.ClearValue(ThemeScope.TokenOwnerProperty);
                return;
            }
            _part.SetValue(ThemeScope.TokenOwnerProperty, owner);
        }

        private ControlTokenIdentity? RegisteredIdentity()
        {
            var context = ThemeScope.ResolveContext(_part) ?? ThemeScope.ResolveContext(_host);
            if (context is null)
            {
                return null;
            }
            var registry = context.Snapshot.Registry;
            for (var type = _host.GetType(); type is not null && type != typeof(Control); type = type.BaseType)
            {
                if (registry.TryGetControl(type, out var descriptor))
                {
                    return descriptor.Identity;
                }
            }
            return null;
        }
    }
}
