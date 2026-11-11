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
/// the inherited <see cref="ThemeScope.TokenOwnerProperty"/> changes (observed through
/// <see cref="TokenOwnerListeners"/>, not through the target's <c>PropertyChanged</c>). The key is the owner's
/// unbound Effective Global key, or the plain <see cref="SharedTokenKind"/> without an owner.
/// </para>
/// <para>
/// A <c>ResourcesChanged</c> raised by a theme publish is checked against the publish's
/// <see cref="ThemePublishDelta"/> first: when neither the owner's effective value of the Token nor (without a
/// registered owner) the global value changed, nothing is resolved. Every other notification (attach, a local
/// resource change, a variant switch, an owner change, a publish whose delta is unknown) resolves fully.
/// </para>
/// <para>
/// On every full refresh the owner is checked against the registry of the target's <see cref="ThemeContext"/>
/// (cached per snapshot on the context); an owner that registry does not register uses the plain key, because a
/// primitive must not fail because of its host. A later snapshot that registers the owner is picked up on its
/// publish. Without a resolvable context the owner key is used and the provider decides, like any other resource
/// lookup; lookup failures (e.g. a failing Control snapshot evaluation) propagate unchanged.
/// </para>
/// </remarks>
internal sealed class OwnerTokenBinding : IPropertyAccessor, ITokenOwnerListener
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
    private ControlTokenIdentity? _owner;
    private object? _ownerKey;
    private object? _value = AvaloniaProperty.UnsetValue;
    private bool _published;
    private ThemePublishTracker _publishes;

    private OwnerTokenBinding(WeakReference<object?> reference, SharedTokenKind kind)
    {
        _reference = reference;
        _kind      = kind;
        _globalKey = kind;
    }

    public Type PropertyType => typeof(object);

    public object? Value => _value;

    /// <summary>How often the accessor resolved its resource; contract tests pin the publish early-exit with it.</summary>
    internal int ResolveCount { get; private set; }

    internal static BindingBase Create(SharedTokenKind kind, BindingPriority priority)
    {
        var path = new CompiledBindingPathBuilder()
                   .Self()
                   .Property(s_ownerTokenProperty, (reference, _) => new OwnerTokenBinding(reference, kind))
                   .Build();
        return new CompiledBinding(path)
        {
            Mode     = BindingMode.OneWay,
            Priority = priority
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

        _target                          =  target;
        TokenOwnerListeners.Add(target, this);
        target.ResourcesChanged          += OnResourcesChanged;
        target.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        UpdateOwner(target.GetValue(ThemeScope.TokenOwnerProperty));
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
        _ownerKey  = null;
        _value     = AvaloniaProperty.UnsetValue;
        _published = false;
        _publishes = default;
    }

    public void Dispose()
    {
        if (_listener is not null)
        {
            Unsubscribe();
        }
    }

    void ITokenOwnerListener.OnTokenOwnerChanged(ControlTokenIdentity? owner) => UpdateOwner(owner);

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        if (_target is not { } target)
        {
            return;
        }

        var context = ThemeScope.ResolveContext(target);
        var delta = _publishes.Observe(context);
        if (delta is not null && !IsKeyChanged(delta, context!))
        {
            return;
        }
        Refresh(target, context);
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) => Refresh();

    private void UpdateOwner(ControlTokenIdentity? owner)
    {
        // Allocates one boxed key per owner change; refreshes reuse it.
        _owner    = owner;
        _ownerKey = owner is null ? null : GetResourceKey(owner, _kind);
        Refresh();
    }

    /// <summary>Re-resolves the owner value and publishes it only if it differs from the published one.</summary>
    /// <remarks>
    /// A theme publish refreshes every owner binding in the tree while most tokens keep their value; the resolved
    /// values are immutable or per-consumer frozen copies that compare by value, so an equal result is the same value
    /// and republishing it would only make Avalonia re-evaluate the property for no change.
    /// </remarks>
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
        var value = FindValue(target, context);
        if (_published && Equals(value, _value))
        {
            return;
        }

        Publish(value);
    }

    /// <summary>Creates a standalone accessor for <paramref name="target"/>; contract tests drive it directly.</summary>
    internal static OwnerTokenBinding CreateAccessor(object target, SharedTokenKind kind) =>
        new(new WeakReference<object?>(target), kind);

    private bool IsKeyChanged(ThemePublishDelta delta, ThemeContext context)
    {
        if (_ownerKey is not null && context.TryGetRegisteredOwnerSlot(_owner!.Value, out var slot))
        {
            return delta.IsOwnerGlobalChanged(slot, _globalKey);
        }
        return delta.IsGlobalChanged(_globalKey);
    }

    private object? FindValue(StyledElement target, ThemeContext? context)
    {
        ResolveCount++;
        var key = _ownerKey is { } ownerKey && !IsKnownUnregistered(context, _owner!.Value)
            ? ownerKey
            : _globalKey;
        return target.TryFindResource(key, target.ActualThemeVariant, out var value)
            ? value
            : AvaloniaProperty.UnsetValue;
    }

    internal static bool IsKnownUnregistered(ThemeContext? context, ControlTokenIdentity owner) =>
        context is not null && !context.TryGetRegisteredOwnerSlot(owner, out _);

    private void Publish(object? value)
    {
        _value     = value;
        _published = true;
        _listener?.Invoke(value);
    }
}
