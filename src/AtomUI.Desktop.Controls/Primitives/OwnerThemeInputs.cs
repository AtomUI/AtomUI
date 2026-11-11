using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// Maps a part's inheritable theme inputs from its Token owner's Own and effective global Tokens when the part is
/// not a visual descendant of the owner's styled tree: runtime dialog surfaces, a picker presenter shown without its
/// picker, a standalone cover. The owner is the part's forwarded or inherited <c>ThemeScope.TokenOwner</c>;
/// <c>fallbackOwnerType</c> names the design owner of a part created alone. Each input is resolved by Token name
/// against the owner's descriptor (Own Token first, then the owner's effective global) through the part's
/// <c>ThemeContext</c>, so scoped <c>ThemeConfigProvider</c> overrides of <c>Controls[owner]</c> apply, and is
/// published at Style priority — the priority the owner's ControlTheme setters reach through inheritance — so a
/// local value on the part still wins. Values follow owner changes and theme publishes while the part is attached.
/// </summary>
/// <summary>One inheritable theme input and the owner Token name it is mapped from.</summary>
internal readonly record struct OwnerThemeInput(AvaloniaProperty Property, string Token);

internal static class OwnerThemeInputs
{
    /// <param name="part">The hosted or standalone part whose inputs are mapped.</param>
    /// <param name="host">
    /// The owner element when the part has one at runtime (the dialog of a surface). While that host is attached to a
    /// logical tree its own ControlTheme setters reach the part through the presenter's inheritance parent and are
    /// authoritative (they resolve in the host's scope); the mapping then stays out of the way.
    /// </param>
    /// <param name="fallbackOwnerType">The design owner of a part created alone, or null.</param>
    /// <param name="inputs">The inheritable inputs and the owner Token names they are mapped from.</param>
    internal static IDisposable Bind(
        Control part,
        Control? host,
        Type? fallbackOwnerType,
        IReadOnlyList<OwnerThemeInput> inputs)
    {
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(inputs);
        return new Binding(part, host, fallbackOwnerType, inputs);
    }

    private sealed class Binding : IDisposable
    {
        private static readonly ThemeTokenResolver s_resolver = new();

        private readonly Control _part;
        private readonly Control? _host;
        private readonly Type? _fallbackOwnerType;
        private readonly IReadOnlyList<OwnerThemeInput> _inputs;
        private readonly IDisposable?[] _values;
        private readonly IDisposable _ownerSubscription;
        private IDisposable? _contextSubscription;
        private IDisposable? _fallbackOwner;
        private bool _disposed;

        internal Binding(Control part, Control? host, Type? fallbackOwnerType, IReadOnlyList<OwnerThemeInput> inputs)
        {
            _part = part;
            _host = host;
            _fallbackOwnerType = fallbackOwnerType;
            _inputs = inputs;
            _values = new IDisposable?[inputs.Count];
            part.AttachedToLogicalTree += HandleAttached;
            part.DetachedFromLogicalTree += HandleDetached;
            part.ResourcesChanged += HandleResourcesChanged;
            if (host is not null)
            {
                host.AttachedToLogicalTree += HandleHostTreeChanged;
                host.DetachedFromLogicalTree += HandleHostTreeChanged;
            }
            _ownerSubscription = part.GetObservable(ThemeScope.TokenOwnerProperty).Subscribe(_ => Refresh());
            if (((ILogical)part).IsAttachedToLogicalTree)
            {
                HandleAttached(part, null);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _part.AttachedToLogicalTree -= HandleAttached;
            _part.DetachedFromLogicalTree -= HandleDetached;
            _part.ResourcesChanged -= HandleResourcesChanged;
            if (_host is not null)
            {
                _host.AttachedToLogicalTree -= HandleHostTreeChanged;
                _host.DetachedFromLogicalTree -= HandleHostTreeChanged;
            }
            _ownerSubscription.Dispose();
            _contextSubscription?.Dispose();
            _contextSubscription = null;
            Clear();
        }

        private void HandleAttached(object? sender, LogicalTreeAttachmentEventArgs? e)
        {
            _contextSubscription?.Dispose();
            // Scoped publishes reach the part through its ThemeContext, not through the logical resource chain.
            _contextSubscription = s_resolver.Subscribe(_part, _ => Refresh());
            Refresh();
        }

        private void HandleDetached(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            _contextSubscription?.Dispose();
            _contextSubscription = null;
            Clear();
        }

        private void HandleResourcesChanged(object? sender, ResourcesChangedEventArgs e) => Refresh();

        private void HandleHostTreeChanged(object? sender, LogicalTreeAttachmentEventArgs e) => Refresh();

        private void Refresh()
        {
            if (_disposed || !((ILogical)_part).IsAttachedToLogicalTree)
            {
                return;
            }
            if (_host is { } host && ((ILogical)host).IsAttachedToLogicalTree)
            {
                // The styled host passes its inputs down itself (presenters inherit from an attached dialog).
                Clear();
                return;
            }
            var context = ThemeScope.ResolveContext(_part);
            if (context is null)
            {
                Clear();
                return;
            }
            var snapshot = context.Snapshot;
            var identity = ThemeScope.GetTokenOwner(_part);
            if (identity is null && FallbackIdentity(snapshot) is { } fallback)
            {
                // A part created alone becomes its design owner's part: the same identity the owner's default
                // ControlTheme injects, so its OwnerTokenResource reads and TokenTransitions follow that owner too.
                // Setting it raises the owner subscription, which runs this refresh again with the owner in place.
                _fallbackOwner?.Dispose();
                _fallbackOwner = _part.SetValue(ThemeScope.TokenOwnerProperty, fallback, BindingPriority.Style);
                return;
            }
            if (identity is not { } owner || !snapshot.Registry.TryGetControl(owner, out var descriptor))
            {
                Clear();
                return;
            }
            for (var index = 0; index < _inputs.Count; index++)
            {
                var (property, token) = _inputs[index];
                var key = ResolveKey(descriptor, owner, token);
                if (key is not null &&
                    context.ResourceProvider.TryGetResource(key, _part.ActualThemeVariant, out var value) &&
                    value is not null)
                {
                    _values[index]?.Dispose();
                    _values[index] = _part.SetValue(property, Convert(value, property.PropertyType), BindingPriority.Style);
                }
                else
                {
                    _values[index]?.Dispose();
                    _values[index] = null;
                }
            }
        }

        private ControlTokenIdentity? FallbackIdentity(AtomUI.Theme.Compilation.ThemeSnapshot snapshot) =>
            _fallbackOwnerType is not null && snapshot.Registry.TryGetControl(_fallbackOwnerType, out var descriptor)
                ? descriptor.Identity
                : null;

        private static object? ResolveKey(ControlTokenDescriptor descriptor, ControlTokenIdentity owner, string token)
        {
            foreach (var own in descriptor.OwnTokens)
            {
                if (string.Equals(own.Name, token, StringComparison.Ordinal))
                {
                    return ControlTokenResourceKey.Own(owner, own.ResourceKey);
                }
            }
            return Enum.TryParse<SharedTokenKind>(token, out var kind)
                ? ControlTokenResourceKey.Global(owner, kind)
                : null;
        }

        // Owner themes assign these Tokens through AXAML Setters, which convert; the direct resource value does not.
        private static object Convert(object value, Type targetType)
        {
            if (typeof(IBrush).IsAssignableFrom(targetType) && value is Color color)
            {
                return new ImmutableSolidColorBrush(color);
            }
            if (targetType == typeof(Thickness) && value is double uniform)
            {
                return new Thickness(uniform);
            }
            if (targetType == typeof(CornerRadius) && value is double radius)
            {
                return new CornerRadius(radius);
            }
            return value;
        }

        private void Clear()
        {
            for (var index = 0; index < _values.Length; index++)
            {
                _values[index]?.Dispose();
                _values[index] = null;
            }
            _fallbackOwner?.Dispose();
            _fallbackOwner = null;
        }
    }
}
