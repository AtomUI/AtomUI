using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Per-target binding source behind <see cref="OwnerTokenResourceExtension"/>.
/// </summary>
/// <remarks>
/// <para>
/// Avalonia 12 keeps <see cref="BindingBase"/> instancing internal, so the extension returns a public
/// <see cref="CompiledBinding"/> rooted at the target (<c>$self</c>) whose single path element is this
/// accessor. Avalonia creates one accessor per binding instance, i.e. per target element, exactly as it
/// does for compiled XAML bindings.
/// </para>
/// <para>
/// The accessor mirrors <c>DynamicResourceExpression</c>/<c>ResourceObservable</c>: it re-reads the
/// resource on <see cref="StyledElement.ResourcesChanged"/> and
/// <see cref="StyledElement.ActualThemeVariantChanged"/>, and additionally switches the resource key when
/// the inherited <see cref="ThemeScope.TokenOwnerProperty"/> changes. The key is the owner's unbound
/// Effective Global key, or the plain <see cref="SharedTokenKind"/> without an owner. An owner the active
/// schema does not register falls back to the plain key instead of throwing, because a primitive must not
/// fail because of its host.
/// </para>
/// </remarks>
internal sealed class OwnerTokenBinding : IPropertyAccessor
{
    private static readonly ClrPropertyInfo s_ownerTokenProperty = new(
        "OwnerToken",
        getter: null,
        setter: null,
        typeof(object));

    private readonly WeakReference<object?> _reference;
    private readonly object _globalKey;
    private readonly SharedTokenKind _kind;
    private Action<object?>? _listener;
    private StyledElement? _target;
    private object? _ownerKey;
    private object? _value = AvaloniaProperty.UnsetValue;

    private OwnerTokenBinding(WeakReference<object?> reference, SharedTokenKind kind)
    {
        _reference = reference;
        _kind      = kind;
        _globalKey = kind;
    }

    public Type PropertyType => typeof(object);

    public object? Value => _value;

    internal static BindingBase Create(SharedTokenKind kind)
    {
        var path = new CompiledBindingPathBuilder()
                   .Self()
                   .Property(s_ownerTokenProperty, (reference, _) => new OwnerTokenBinding(reference, kind))
                   .Build();
        return new CompiledBinding(path)
        {
            Mode = BindingMode.OneWay
        };
    }

    internal static object GetResourceKey(ControlTokenIdentity? owner, SharedTokenKind kind) =>
        owner is { } identity ? ControlTokenResourceKey.Global(identity, kind) : kind;

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
            Publish(AvaloniaProperty.UnsetValue);
            return;
        }

        _target                           =  target;
        target.PropertyChanged            += OnTargetPropertyChanged;
        target.ResourcesChanged           += OnResourcesChanged;
        target.ActualThemeVariantChanged  += OnActualThemeVariantChanged;
        UpdateOwner(target.GetValue(ThemeScope.TokenOwnerProperty));
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

        _listener = null;
        _ownerKey = null;
        _value    = AvaloniaProperty.UnsetValue;
    }

    public void Dispose()
    {
        if (_listener is not null)
        {
            Unsubscribe();
        }
    }

    private void OnTargetPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ThemeScope.TokenOwnerProperty)
        {
            UpdateOwner((ControlTokenIdentity?)e.NewValue);
        }
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e) => Refresh();

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) => Refresh();

    private void UpdateOwner(ControlTokenIdentity? owner)
    {
        // Allocates one boxed key per owner change; refreshes reuse it.
        _ownerKey = owner is null ? null : GetResourceKey(owner, _kind);
        Refresh();
    }

    private void Refresh()
    {
        if (_target is { } target)
        {
            Publish(FindValue(target));
        }
    }

    private object? FindValue(StyledElement target)
    {
        var theme = target.ActualThemeVariant;
        if (_ownerKey is { } ownerKey)
        {
            try
            {
                return target.TryFindResource(ownerKey, theme, out var ownerValue)
                    ? ownerValue
                    : AvaloniaProperty.UnsetValue;
            }
            catch (InvalidOperationException)
            {
                // ThemeTokenResourceProvider rejects identities the active schema does not register.
                // Keep the plain global key for this owner so later refreshes do not throw again.
                _ownerKey = null;
            }
        }

        return target.TryFindResource(_globalKey, theme, out var value)
            ? value
            : AvaloniaProperty.UnsetValue;
    }

    private void Publish(object? value)
    {
        _value = value;
        _listener?.Invoke(value);
    }
}
