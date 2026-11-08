using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Per-target binding source behind <see cref="TokenTransitions"/>.
/// </summary>
/// <remarks>
/// <para>
/// Like <see cref="OwnerTokenBinding"/>, the markup returns a public <see cref="CompiledBinding"/> rooted at the target
/// (<c>$self</c>) whose single path element is this accessor, so Avalonia creates one accessor per target. A Setter
/// instances the binding per styled control, which is what makes per-control durations possible although the
/// transitions themselves are not in the visual tree and have no resource host.
/// </para>
/// <para>
/// The accessor resolves every declared duration Token on the target: the owner's unbound Effective Global key when the
/// inherited <see cref="ThemeScope.TokenOwnerProperty"/> names a registered control, the plain
/// <see cref="SharedTokenKind"/> otherwise. It refreshes on <see cref="StyledElement.ResourcesChanged"/> (theme publish,
/// local scope changes, attach), <see cref="StyledElement.ActualThemeVariantChanged"/> and owner changes, and publishes
/// the factory's <see cref="Avalonia.Animation.Transitions"/> for the resolved durations. When a declared Token does not
/// resolve (the target is not attached yet) no transitions are published. A refresh that resolves the published
/// durations again does not allocate and does not republish.
/// </para>
/// </remarks>
internal sealed class TransitionDurationBinding : IPropertyAccessor
{
    private const int StackDurationCapacity = 8;

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

    private TransitionDurationBinding(WeakReference<object?> reference, TokenTransitionsFactory factory)
    {
        _reference = reference;
        _factory   = factory;
    }

    public Type PropertyType => typeof(object);

    public object? Value => _value;

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
        target.PropertyChanged           += OnTargetPropertyChanged;
        target.ResourcesChanged          += OnResourcesChanged;
        target.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        SetOwner(target.GetValue(ThemeScope.TokenOwnerProperty));
        _value = Resolve(target);
        _listener(_value);
    }

    public void Unsubscribe()
    {
        if (_target is { } target)
        {
            target.PropertyChanged           -= OnTargetPropertyChanged;
            target.ResourcesChanged          -= OnResourcesChanged;
            target.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
            _target                          =  null;
        }

        _listener  = null;
        _owner     = null;
        _ownerKeys = null;
        _value     = AvaloniaProperty.UnsetValue;
    }

    public void Dispose()
    {
        if (_listener is not null)
        {
            Unsubscribe();
        }
    }

    /// <summary>Re-resolves the durations and publishes the transitions if they changed.</summary>
    internal void Refresh()
    {
        if (_target is not { } target)
        {
            return;
        }

        var value = Resolve(target);
        if (ReferenceEquals(value, _value))
        {
            return;
        }

        _value = value;
        _listener?.Invoke(value);
    }

    private void OnTargetPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ThemeScope.TokenOwnerProperty)
        {
            SetOwner((ControlTokenIdentity?)e.NewValue);
            Refresh();
        }
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e) => Refresh();

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) => Refresh();

    private void SetOwner(ControlTokenIdentity? owner)
    {
        _owner     = owner;
        _ownerKeys = owner is { } identity ? _factory.GetOwnerKeys(identity) : null;
    }

    private object Resolve(StyledElement target)
    {
        var kinds = _factory.Kinds;
        var keys = _ownerKeys is { } ownerKeys && !OwnerTokenBinding.IsKnownUnregistered(target, _owner!.Value)
            ? ownerKeys
            : _factory.GlobalKeys;
        Span<TimeSpan> durations = kinds.Length <= StackDurationCapacity
            ? stackalloc TimeSpan[StackDurationCapacity]
            : new TimeSpan[kinds.Length];
        durations = durations[..kinds.Length];

        var themeVariant = target.ActualThemeVariant;
        for (var index = 0; index < kinds.Length; index++)
        {
            if (!target.TryFindResource(keys[index], themeVariant, out var value))
            {
                return AvaloniaProperty.UnsetValue;
            }
            if (value is not TimeSpan duration)
            {
                throw new InvalidOperationException(
                    $"Token '{kinds[index]}' declared by TokenTransitions.Duration is not a duration " +
                    $"(resolved '{value?.GetType().Name ?? "null"}').");
            }
            durations[index] = duration;
        }

        return _factory.GetTransitions(durations);
    }
}
