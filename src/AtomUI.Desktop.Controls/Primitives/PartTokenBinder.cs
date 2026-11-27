using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// One inheritable theme input and the owner Token name it is mapped from.
/// </summary>
internal readonly record struct PartTokenMapping(AvaloniaProperty Property, string Token);

/// <summary>
/// Maps a part's inheritable theme inputs from its Token owner's Own and effective global Tokens when no attached
/// owner provides them: runtime dialog surfaces whose dialog is not styled in a tree, a picker presenter shown without
/// its picker, a standalone cover. The owner is the part's forwarded <c>ThemeScope.TokenOwner</c>; a part created
/// alone takes <c>fallbackOwnerType</c>'s registered identity (the identity the owner's default ControlTheme would
/// inject) as its owner. Inputs share one resource observer and use the owner's Own keys (by Token name) or effective
/// global keys at Style priority — the priority the owner's
/// ControlTheme setters reach through inheritance — so the value resolves through the part's own resource chain like an
/// AXAML TokenResource, follows publishes and variants, and a local value on the part still wins. The binding set is
/// rebuilt only when the owner identity changes, new bindings first. When the part inherits an owner that is
/// <c>fallbackOwnerType</c> or <c>alternateOwnerType</c> (it sits in the owner's styled tree), any owner while
/// <c>fallbackOwnerType</c> is null, or its runtime host is attached, the owner's setters are authoritative and
/// nothing is mapped. An owner inherited from an ancestor of another type is replaced with the fallback identity.
/// </summary>
internal static class PartTokenBinder
{
    /// <summary>
    /// Starts the input mapping described on this class for <paramref name="part" /> and returns a handle. Disposing
    /// the handle removes the bindings, the tree and owner subscriptions and any fallback owner the mapping set.
    /// </summary>
    /// <param name="part">
    /// The hosted or standalone part whose inputs are mapped.
    /// </param>
    /// <param name="host">
    /// The owner element when the part has one at runtime (the dialog of a surface); while it is attached to a logical
    /// tree its own ControlTheme setters reach the part through the presenter's inheritance parent.
    /// </param>
    /// <param name="fallbackOwnerType">
    /// The design owner of a part created alone, or null.
    /// </param>
    /// <param name="inputs">
    /// The inheritable inputs and the owner Token names they are mapped from.
    /// </param>
    /// <param name="alternateOwnerType">
    /// Another registered control whose theme provides these inputs.
    /// </param>
    internal static IDisposable Bind(
        Control part,
        Control? host,
        Type? fallbackOwnerType,
        IReadOnlyList<PartTokenMapping> inputs,
        Type? alternateOwnerType = null)
    {
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(inputs);
        return new Binding(part, host, fallbackOwnerType, inputs, alternateOwnerType);
    }

    /// <summary>
    /// Binds a part's own (non-inherited) properties to the Own Tokens of the owner it sits under. Unlike
    /// <see cref="Bind" />, the owner's ControlTheme provides nothing: the part resolves the values itself, so they
    /// reach only the part instead of inheriting through the owner's whole tree. <paramref name="inputsByOwner" /> lists,
    /// per owner control type, the properties and the owner's Token keys (its generated <c>TokenKind</c> values, which
    /// keeps the reads visible to the Token consumption analysis). Values are bound only while the part is attached and
    /// its <c>ThemeScope.TokenOwner</c> is a registered control assignable to one of those types; otherwise the
    /// properties keep their defaults, as they did when no such owner supplied the inputs.
    /// </summary>
    internal static IDisposable BindFromOwner(
        Control part,
        IReadOnlyList<(Type OwnerType, IReadOnlyList<(AvaloniaProperty Property, object Key)> Inputs)> inputsByOwner)
    {
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(inputsByOwner);
        return new OwnerBinding(part, inputsByOwner);
    }

    private sealed class OwnerBinding : IDisposable
    {
        private readonly Control _part;
        private readonly IReadOnlyList<(Type OwnerType, IReadOnlyList<(AvaloniaProperty Property, object Key)> Inputs)> _inputsByOwner;
        private readonly IDisposable _ownerSubscription;
        private OwnerTokenPropertyBindings? _bindings;
        private ControlTokenIdentity? _boundOwner;
        private bool _disposed;

        internal OwnerBinding(
            Control part,
            IReadOnlyList<(Type OwnerType, IReadOnlyList<(AvaloniaProperty Property, object Key)> Inputs)> inputsByOwner)
        {
            _part = part;
            _inputsByOwner = inputsByOwner;
            part.AttachedToLogicalTree += HandleTreeChanged;
            part.DetachedFromLogicalTree += HandleTreeChanged;
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
            // Detached parts hold no resource or owner-listener subscriptions.
            if (!((ILogical)_part).IsAttachedToLogicalTree ||
                ThemeScope.ResolveContext(_part) is not { } context ||
                ThemeScope.GetTokenOwner(_part) is not { } owner ||
                !context.Snapshot.Registry.TryGetControl(owner, out var descriptor) ||
                FindInputs(descriptor.ControlType) is not { } inputs)
            {
                Release();
                return;
            }
            if (owner == _boundOwner && _bindings is not null)
            {
                return;
            }
            var bindings = new OwnerTokenPropertyBindings(_part, inputs.Count, BindingPriority.Style);
            try
            {
                for (var index = 0; index < inputs.Count; index++)
                {
                    var (property, key) = inputs[index];
                    bindings.Bind(index, property, key);
                }
            }
            catch
            {
                bindings.Dispose();
                throw;
            }
            // New bindings first: observers never see the property default between two owners.
            var replaced = _bindings;
            _bindings = bindings;
            _boundOwner = owner;
            replaced?.Dispose();
        }

        private IReadOnlyList<(AvaloniaProperty Property, object Key)>? FindInputs(Type controlType)
        {
            foreach (var (ownerType, inputs) in _inputsByOwner)
            {
                if (ownerType.IsAssignableFrom(controlType))
                {
                    return inputs;
                }
            }
            return null;
        }

        private void Release()
        {
            var bindings = _bindings;
            _bindings = null;
            _boundOwner = null;
            bindings?.Dispose();
        }
    }

    private sealed class Binding : IDisposable
    {
        private readonly Control _part;
        private readonly Control? _host;
        private readonly Type? _fallbackOwnerType;
        private readonly Type? _alternateOwnerType;
        private readonly IReadOnlyList<PartTokenMapping> _inputs;
        private readonly IDisposable _ownerSubscription;
        private OwnerTokenPropertyBindings? _bindings;
        private ControlTokenIdentity? _boundOwner;
        private IDisposable? _fallbackOwner;
        private bool _disposed;

        internal Binding(Control part, Control? host, Type? fallbackOwnerType, IReadOnlyList<PartTokenMapping> inputs,
            Type? alternateOwnerType)
        {
            _part = part;
            _host = host;
            _fallbackOwnerType = fallbackOwnerType;
            _alternateOwnerType = alternateOwnerType;
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
            var context = ThemeScope.ResolveContext(_part);
            if (context is null)
            {
                Release();
                return;
            }
            var registry = context.Snapshot.Registry;
            var owner = ThemeScope.GetTokenOwner(_part);
            if (owner is { } inheritedOwner && !_part.IsSet(ThemeScope.TokenOwnerProperty))
            {
                // Forwarded owners use a local value. For inheritance, only a compatible ControlTheme supplies
                // the expected inputs. Standalone covers and time presenters can inherit the surrounding Window.
                if (_fallbackOwnerType is null ||
                    (registry.TryGetControl(inheritedOwner, out var ancestor) &&
                     (_fallbackOwnerType.IsAssignableFrom(ancestor.ControlType) ||
                      _alternateOwnerType?.IsAssignableFrom(ancestor.ControlType) == true)))
                {
                    Release();
                    return;
                }
                owner = null;
            }
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
            var bindings = new OwnerTokenPropertyBindings(_part, _inputs.Count, BindingPriority.Style);
            try
            {
                for (var index = 0; index < _inputs.Count; index++)
                {
                    var (property, token) = _inputs[index];
                    if (ResolveKey(descriptor, owner.Value, token) is { } key)
                    {
                        bindings.Bind(index, property, key);
                    }
                }
            }
            catch
            {
                bindings.Dispose();
                throw;
            }
            // New bindings first: observers never see the attached-property default between two owners.
            var replaced = _bindings;
            _bindings = bindings;
            _boundOwner = owner;
            replaced?.Dispose();
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
            var fallbackOwner = _fallbackOwner;
            _bindings = null;
            _boundOwner = null;
            _fallbackOwner = null;
            bindings?.Dispose();
            fallbackOwner?.Dispose();
        }
    }
}
