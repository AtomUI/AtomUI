using System.Runtime.CompilerServices;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Per-target binding source behind <see cref="TokenTransitions" />.
/// </summary>
/// <remarks>
/// <para>
/// Like <see cref="OwnerTokenBinding" />, the markup returns a public <see cref="CompiledBinding" /> rooted at the
/// target (<c>$self</c>) whose single path element is this accessor, so Avalonia creates one accessor per target. A
/// Setter instances the binding per styled control, which is what makes per-control durations possible although the
/// transitions themselves are not in the visual tree and have no resource host.
/// </para>
/// <para>
/// The accessor resolves every declared duration Token on the target: the owner's unbound Effective Global key when the
/// inherited <see cref="ThemeScope.TokenOwnerProperty" /> names a registered control, the plain <see
/// cref="SharedTokenKind" /> otherwise. It refreshes on <see cref="StyledElement.ResourcesChanged" /> (theme publish,
/// local scope changes, attach), <see cref="StyledElement.ActualThemeVariantChanged" /> and owner changes (observed
/// through <see cref="TokenOwnerListeners" />), and publishes the factory's <see cref="Avalonia.Animation.Transitions"
/// /> for the resolved durations. A <c>ResourcesChanged</c> raised by a theme publish whose <see
/// cref="ThemePublishDelta" /> shows none of the declared durations changed for this owner resolves nothing. When a
/// declared Token does not resolve (the target is not attached yet) no transitions are published. A refresh that
/// resolves the published durations again does not allocate and does not republish.
/// </para>
/// </remarks>
internal sealed class TransitionDurationBinding : IPropertyAccessor, ITokenOwnerListener
{
    private const int InlineCapacity = 8;

    [InlineArray(InlineCapacity)]
    private struct ResolvedValues
    {
        private TokenTransitionsFactory.TokenValue _first;
    }

    private static readonly ClrPropertyInfo s_tokenTransitionsProperty = new(
        "TokenTransitions",
        getter: null,
        setter: null,
        typeof(object));

    private readonly WeakReference<object?> _reference;
    private readonly TokenTransitionsFactory _factory;
    private Action<object?>? _listener;
    private StyledElement? _target;
    private ControlTokenIdentity? _owner;
    private object[]? _ownerKeys;
    private object? _value = AvaloniaProperty.UnsetValue;
    private ThemePublishTracker _publishes;

    private TransitionDurationBinding(WeakReference<object?> reference, TokenTransitionsFactory factory)
    {
        _reference = reference;
        _factory   = factory;
    }

    public Type PropertyType => typeof(object);

    public object? Value => _value;

    /// <summary>
    /// How often the accessor resolved its durations; contract tests pin the publish early-exit with it.
    /// </summary>
    internal int ResolveCount { get; private set; }

    internal static BindingBase Create(TokenTransitionsFactory factory, BindingPriority priority)
    {
        var path = new CompiledBindingPathBuilder()
                   .Self()
                   .Property(s_tokenTransitionsProperty,
                       (reference, _) => new TransitionDurationBinding(reference, factory))
                   .Build();
        return new CompiledBinding(path)
        {
            Mode     = BindingMode.OneWay,
            Priority = priority
        };
    }

    public bool SetValue(object? value, BindingPriority priority) => false;

    public void Subscribe(Action<object?> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        if (_listener is not null)
        {
            throw new InvalidOperationException("A member accessor can be subscribed to only once.");
        }

        _listener = listener;
        if (!_reference.TryGetTarget(out var source) || source is not StyledElement target)
        {
            _listener(AvaloniaProperty.UnsetValue);
            return;
        }

        _target                          =  target;
        TokenOwnerListeners.Add(target, this);
        target.ResourcesChanged          += OnResourcesChanged;
        target.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        SetOwner(target.GetValue(ThemeScope.TokenOwnerProperty));
        var context = ThemeScope.ResolveContext(target);
        _publishes.MarkCurrent(context);
        _value = Resolve(target, context);
        _listener(_value);
    }

    public void Unsubscribe()
    {
        if (_target is { } target)
        {
            TokenOwnerListeners.Remove(target, this);
            target.ResourcesChanged          -= OnResourcesChanged;
            target.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
            _target                          =  null;
        }

        _listener  = null;
        _owner     = null;
        _ownerKeys = null;
        _value     = AvaloniaProperty.UnsetValue;
        _publishes = default;
    }

    public void Dispose()
    {
        if (_listener is not null)
        {
            Unsubscribe();
        }
    }

    /// <summary>
    /// Re-resolves the durations and publishes the transitions if they changed.
    /// </summary>
    internal void Refresh()
    {
        if (_target is { } target)
        {
            Refresh(target, ThemeScope.ResolveContext(target));
        }
    }

    private void Refresh(StyledElement target, ThemeContext? context)
    {
        _publishes.MarkCurrent(context);
        var value = Resolve(target, context);
        if (ReferenceEquals(value, _value))
        {
            return;
        }

        _value = value;
        _listener?.Invoke(value);
    }

    void ITokenOwnerListener.OnTokenOwnerChanged(ControlTokenIdentity? owner)
    {
        SetOwner(owner);
        Refresh();
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        if (_target is not { } target)
        {
            return;
        }

        var context = ThemeScope.ResolveContext(target);
        var delta = _publishes.Observe(context);
        if (delta is not null && !AreKeysChanged(delta, context!))
        {
            return;
        }
        Refresh(target, context);
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) => Refresh();

    private void SetOwner(ControlTokenIdentity? owner)
    {
        _owner     = owner;
        _ownerKeys = owner is { } identity ? _factory.GetOwnerKeys(identity) : null;
    }

    private bool AreKeysChanged(ThemePublishDelta delta, ThemeContext context)
    {
        var globalKeys = _factory.GlobalKeys;
        if (_ownerKeys is not null && context.TryGetRegisteredOwnerSlot(_owner!.Value, out var slot))
        {
            foreach (var key in globalKeys)
            {
                if (delta.IsOwnerGlobalChanged(slot, key))
                {
                    return true;
                }
            }
            return false;
        }
        foreach (var key in globalKeys)
        {
            if (delta.IsGlobalChanged(key))
            {
                return true;
            }
        }
        return false;
    }

    private object Resolve(StyledElement target, ThemeContext? context)
    {
        ResolveCount++;
        var inputs = _factory.Inputs;
        var keys = _ownerKeys is { } ownerKeys && !OwnerTokenBinding.IsKnownUnregistered(context, _owner!.Value)
            ? ownerKeys
            : _factory.GlobalKeys;
        ResolvedValues storage = default;
        Span<TokenTransitionsFactory.TokenValue> values = inputs.Length <= InlineCapacity
            ? storage[..inputs.Length]
            : new TokenTransitionsFactory.TokenValue[inputs.Length];

        var themeVariant = target.ActualThemeVariant;
        for (var index = 0; index < inputs.Length; index++)
        {
            if (!target.TryFindResource(keys[index], themeVariant, out var value))
            {
                return AvaloniaProperty.UnsetValue;
            }
            var input = inputs[index];
            if (input.IsEasing && value is Easing or null)
            {
                values[index] = new(default, (Easing?)value);
            }
            else if (!input.IsEasing && value is TimeSpan duration)
            {
                values[index] = new(duration, null);
            }
            else
            {
                var channel = input.IsEasing ? "Easing" : "Duration";
                throw new InvalidOperationException(
                    $"Token '{input.Kind}' declared by TokenTransitions.{channel} has an incompatible value " +
                    $"of type '{value?.GetType().Name ?? "null"}'.");
            }
        }

        return _factory.GetTransitions(values);
    }
}
