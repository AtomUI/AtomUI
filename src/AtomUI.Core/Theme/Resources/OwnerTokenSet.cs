using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Reads several global Design Tokens through the owner of one target element, with one set of target subscriptions.
/// </summary>
/// <remarks>
/// <para>
/// Each entry resolves exactly like an <see cref="OwnerTokenBinding"/> on the same target: the owner's unbound Effective
/// Global key, or the plain <see cref="SharedTokenKind"/> without an owner or with an owner the target's registry does not
/// register. The set re-resolves on <see cref="StyledElement.ResourcesChanged"/> (checking a theme publish's
/// <see cref="ThemePublishDelta"/> per entry first), on <see cref="StyledElement.ActualThemeVariantChanged"/> and when
/// the inherited <see cref="ThemeScope.TokenOwnerProperty"/> changes.
/// </para>
/// <para>
/// Controls use it for theme variables they compute from several Tokens: one subscription per element instead of one
/// binding per Token keeps attach cost and handler chains flat. The set only listens to its target, so it lives and dies
/// with it; <see cref="Dispose"/> exists for owners that replace it.
/// </para>
/// <para>
/// An entry resolves on its first <see cref="GetValue"/>, like a style setter binding that only starts when its style
/// activates: variables read only in an interaction state cost nothing until that state occurs. Only entries that have
/// been read follow later changes and report them; an unread entry has nothing to report.
/// </para>
/// </remarks>
internal sealed class OwnerTokenSet : ITokenOwnerListener, IDisposable
{
    private readonly StyledElement _target;
    private readonly Action<int, object?, object?> _changed;
    private readonly SharedTokenKind?[] _kinds;
    private readonly object?[] _ownerKeys;
    private readonly object?[] _values;
    private readonly bool[] _resolved;
    private ControlTokenIdentity? _owner;
    private ThemePublishTracker _publishes;
    private bool _subscribed;

    /// <param name="target">The element whose owner, resources and theme variant the entries follow.</param>
    /// <param name="count">The number of entries.</param>
    /// <param name="changed">Called with the entry index, the old and the new value whenever an entry's value changes.</param>
    internal OwnerTokenSet(StyledElement target, int count, Action<int, object?, object?> changed)
    {
        _target    = target;
        _changed   = changed;
        _kinds     = new SharedTokenKind?[count];
        _ownerKeys = new object?[count];
        _values    = new object?[count];
        _resolved  = new bool[count];
        _owner     = target.GetValue(ThemeScope.TokenOwnerProperty);
        TokenOwnerListeners.Add(target, this);
        target.ResourcesChanged          += OnResourcesChanged;
        target.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        _subscribed                      =  true;
    }

    /// <summary>
    /// The value of an entry, resolved now if it has not been read yet; null when it has no Token or the Token does not
    /// resolve. A first read reports no change: the reader receives the value directly.
    /// </summary>
    internal object? GetValue(int index)
    {
        if (!_resolved[index])
        {
            var context = ThemeScope.ResolveContext(_target);
            if (Array.IndexOf(_resolved, true) < 0)
            {
                // The first read entry fixes the snapshot later publishes are compared against, as a binding's first
                // resolve does. Entries read later resolve against the current snapshot; a publish whose notification
                // is still pending compares from the recorded one and only refreshes entries whose key changed.
                _publishes.MarkCurrent(context);
            }

            _resolved[index] = true;
            _values[index]   = Resolve(index, context);
        }

        return _values[index];
    }

    /// <summary>How often the set resolved an entry; contract tests pin the publish early-exit with it.</summary>
    internal int ResolveCount { get; private set; }

    /// <summary>
    /// Points an entry at <paramref name="kind"/> (null clears it). An entry that has been read resolves again now and
    /// reports a change; an unread one resolves on its first read.
    /// </summary>
    internal void SetKind(int index, SharedTokenKind? kind)
    {
        if (_kinds[index] == kind)
        {
            return;
        }

        _kinds[index]     = kind;
        _ownerKeys[index] = kind is { } token && _owner is { } owner ? OwnerTokenBinding.GetResourceKey(owner, token) : null;
        if (_resolved[index])
        {
            Refresh(index, ThemeScope.ResolveContext(_target));
        }
    }

    public void Dispose()
    {
        if (!_subscribed)
        {
            return;
        }

        _subscribed = false;
        TokenOwnerListeners.Remove(_target, this);
        _target.ResourcesChanged          -= OnResourcesChanged;
        _target.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
    }

    void ITokenOwnerListener.OnTokenOwnerChanged(ControlTokenIdentity? owner)
    {
        _owner = owner;
        for (var i = 0; i < _kinds.Length; i++)
        {
            _ownerKeys[i] = _kinds[i] is { } token && owner is { } identity
                ? OwnerTokenBinding.GetResourceKey(identity, token)
                : null;
        }

        RefreshAll();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) => RefreshAll();

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        var context = ThemeScope.ResolveContext(_target);
        var delta   = _publishes.Observe(context);
        for (var i = 0; i < _kinds.Length; i++)
        {
            if (_resolved[i] && _kinds[i] is { } kind && (delta is null || IsKeyChanged(i, kind, delta, context!)))
            {
                Refresh(i, context);
            }
        }
    }

    private void RefreshAll()
    {
        var context = ThemeScope.ResolveContext(_target);
        _publishes.MarkCurrent(context);
        for (var i = 0; i < _kinds.Length; i++)
        {
            if (_resolved[i])
            {
                Refresh(i, context);
            }
        }
    }

    private bool IsKeyChanged(int index, SharedTokenKind kind, ThemePublishDelta delta, ThemeContext context)
    {
        if (_ownerKeys[index] is not null && context.TryGetRegisteredOwnerSlot(_owner!.Value, out var slot))
        {
            return delta.IsOwnerGlobalChanged(slot, kind);
        }
        return delta.IsGlobalChanged(kind);
    }

    private object? Resolve(int index, ThemeContext? context)
    {
        if (_kinds[index] is not { } kind)
        {
            return null;
        }

        ResolveCount++;
        var key = _ownerKeys[index] is { } ownerKey && !OwnerTokenBinding.IsKnownUnregistered(context, _owner!.Value)
            ? ownerKey
            : kind;
        return _target.TryFindResource(key, _target.ActualThemeVariant, out var value) ? value : null;
    }

    private void Refresh(int index, ThemeContext? context)
    {
        var value = Resolve(index, context);
        var old   = _values[index];
        if (Equals(old, value))
        {
            return;
        }

        _values[index] = value;
        _changed(index, old, value);
    }
}
