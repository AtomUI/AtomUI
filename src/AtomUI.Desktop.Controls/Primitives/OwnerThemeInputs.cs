using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>One inheritable theme input and the owner Token name it is mapped from.</summary>
internal readonly record struct OwnerThemeInput(AvaloniaProperty Property, string Token);

/// <summary>
/// Maps a part's inheritable theme inputs from its Token owner's Own and effective global Tokens when no attached
/// owner provides them: runtime dialog surfaces whose dialog is not styled in a tree, a picker presenter shown without
/// its picker, a standalone cover. The owner is the part's forwarded <c>ThemeScope.TokenOwner</c>; a part created
/// alone takes <c>fallbackOwnerType</c>'s registered identity (the identity the owner's default ControlTheme would
/// inject) as its owner. Each input is delivered by <see cref="TokenResourceBinder.CreateTokenResourceBinding"/> with
/// the owner's Own key (by Token name) or effective global key at Style priority — the priority the owner's
/// ControlTheme setters reach through inheritance — so the value resolves through the part's own resource chain like an
/// AXAML TokenResource, follows publishes and variants, and a local value on the part still wins. The binding set is
/// rebuilt only when the owner identity changes, new bindings first. When the part inherits its owner from an attached
/// ancestor (it sits in the owner's styled tree) or its runtime host is attached, the owner's setters are authoritative
/// and nothing is mapped.
/// </summary>
internal static class OwnerThemeInputs
{
    /// <param name="part">The hosted or standalone part whose inputs are mapped.</param>
    /// <param name="host">
    /// The owner element when the part has one at runtime (the dialog of a surface); while it is attached to a logical
    /// tree its own ControlTheme setters reach the part through the presenter's inheritance parent.
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
        private readonly Control _part;
        private readonly Control? _host;
        private readonly Type? _fallbackOwnerType;
        private readonly IReadOnlyList<OwnerThemeInput> _inputs;
        private readonly IDisposable _ownerSubscription;
        private IDisposable?[]? _bindings;
        private ControlTokenIdentity? _boundOwner;
        private IDisposable? _fallbackOwner;
        private bool _disposed;

        internal Binding(Control part, Control? host, Type? fallbackOwnerType, IReadOnlyList<OwnerThemeInput> inputs)
        {
            _part = part;
            _host = host;
            _fallbackOwnerType = fallbackOwnerType;
            _inputs = inputs;
            part.AttachedToLogicalTree += HandleTreeChanged;
            part.DetachedFromLogicalTree += HandleTreeChanged;
            if (host is not null)
            {
                host.AttachedToLogicalTree += HandleTreeChanged;
                host.DetachedFromLogicalTree += HandleTreeChanged;
            }
            _ownerSubscription = part.GetObservable(ThemeScope.TokenOwnerProperty).Subscribe(_ => Refresh());
            Refresh();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _part.AttachedToLogicalTree -= HandleTreeChanged;
            _part.DetachedFromLogicalTree -= HandleTreeChanged;
            if (_host is not null)
            {
                _host.AttachedToLogicalTree -= HandleTreeChanged;
                _host.DetachedFromLogicalTree -= HandleTreeChanged;
            }
            _ownerSubscription.Dispose();
            Release();
        }

        private void HandleTreeChanged(object? sender, LogicalTreeAttachmentEventArgs e) => Refresh();

        private void Refresh()
        {
            if (_disposed)
            {
                return;
            }
            if (!((ILogical)_part).IsAttachedToLogicalTree ||
                (_host is { } host && ((ILogical)host).IsAttachedToLogicalTree))
            {
                Release();
                return;
            }
            var owner = ThemeScope.GetTokenOwner(_part);
            if (owner is not null && !_part.IsSet(ThemeScope.TokenOwnerProperty))
            {
                // Inherited from an attached owner ancestor: that owner's ControlTheme setters provide the inputs.
                Release();
                return;
            }
            var context = ThemeScope.ResolveContext(_part);
            if (context is null)
            {
                Release();
                return;
            }
            var registry = context.Snapshot.Registry;
            if (owner is null)
            {
                if (_fallbackOwnerType is not null && registry.TryGetControl(_fallbackOwnerType, out var fallback))
                {
                    // A part created alone becomes its design owner's part; setting the owner raises the owner
                    // subscription, which runs this refresh again with the owner in place.
                    var previous = _fallbackOwner;
                    _fallbackOwner = _part.SetValue(ThemeScope.TokenOwnerProperty, fallback.Identity, BindingPriority.Style);
                    previous?.Dispose();
                    return;
                }
                Release();
                return;
            }
            if (owner == _boundOwner && _bindings is not null)
            {
                return;
            }
            if (!registry.TryGetControl(owner.Value, out var descriptor))
            {
                Release();
                return;
            }
            var bindings = new IDisposable?[_inputs.Count];
            for (var index = 0; index < _inputs.Count; index++)
            {
                var (property, token) = _inputs[index];
                var key = ResolveKey(descriptor, owner.Value, token);
                bindings[index] = key is null
                    ? null
                    : TokenResourceBinder.CreateTokenResourceBinding(_part, _part, property, key, BindingPriority.Style);
            }
            // New bindings first: observers never see the attached-property default between two owners.
            var replaced = _bindings;
            _bindings = bindings;
            _boundOwner = owner;
            DisposeAll(replaced);
        }

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

        private void Release()
        {
            var bindings = _bindings;
            _bindings = null;
            _boundOwner = null;
            DisposeAll(bindings);
            _fallbackOwner?.Dispose();
            _fallbackOwner = null;
        }

        private static void DisposeAll(IDisposable?[]? bindings)
        {
            if (bindings is null)
            {
                return;
            }
            foreach (var binding in bindings)
            {
                binding?.Dispose();
            }
        }
    }
}
