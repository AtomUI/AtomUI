using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Reads several Design Token resources through one target element, with one set of target subscriptions.
/// </summary>
/// <remarks>
/// <para>
/// An inherited entry resolves like an <see cref="OwnerTokenBinding" /> on the same target: the owner's unbound
/// Effective Global key, or the plain <see cref="SharedTokenKind" /> without an owner or with an owner the target's
/// registry does not register. The set re-resolves on <see cref="StyledElement.ResourcesChanged" /> (checking a theme
/// publish's <see cref="ThemePublishDelta" /> per entry first), on <see cref="StyledElement.ActualThemeVariantChanged"
/// /> and when the inherited <see cref="ThemeScope.TokenOwnerProperty" /> changes. Fixed entries use the original
/// resource key.
/// </para>
/// <para>
/// Controls use it for theme variables they compute from several Tokens: one subscription per element instead of one
/// binding per Token keeps attach cost and handler chains flat. The set only listens to its target, so it lives and dies
/// with it; <see cref="Dispose" /> exists for owners that replace it.
/// </para>
/// <para>
/// An entry resolves on its first <see cref="GetValue" />, like a style setter binding that only starts when its style
/// activates: variables read only in an interaction state cost nothing until that state occurs. Only entries that have
/// been read follow later changes and report them; an unread entry has nothing to report.
/// </para>
/// </remarks>
internal sealed class OwnerTokenSet : ITokenOwnerListener, IDisposable
{
    private readonly StyledElement _target;
    private readonly Action<int, object?, object?>? _changed;
    private readonly Action? _groupChanged;
    private readonly SharedTokenKind?[] _kinds;
    private readonly object?[] _keys;
    private readonly object?[] _values;
    private readonly object?[]? _pendingValues;
    private readonly bool[] _resolved;
    private ControlTokenIdentity? _owner;
    private ThemePublishTracker _publishes;
    private bool _subscribed;

    /// <param name="target">
    /// The element whose owner, resources and theme variant the entries follow.
    /// </param>
    /// <param name="count">
    /// The number of entries.
    /// </param>
    /// <param name="changed">
    /// Called with the entry index, the old and the new value whenever an entry's value changes.
    /// </param>
    internal OwnerTokenSet(StyledElement target, int count, Action<int, object?, object?> changed)
        : this(target, count, changed ?? throw new ArgumentNullException(nameof(changed)), null)
    {
    }

    /// <summary>
    /// Reports one change after all changed values of a refresh are available to the consumer.
    /// </summary>
    internal OwnerTokenSet(StyledElement target, int count, Action changed)
        : this(target, count, null, changed ?? throw new ArgumentNullException(nameof(changed)))
    {
    }

    private OwnerTokenSet(StyledElement target, int count, Action<int, object?, object?>? changed, Action? groupChanged)
    {
        _target        = target;
        _changed       = changed;
        _groupChanged  = groupChanged;
        _kinds         = new SharedTokenKind?[count];
        _keys          = new object?[count];
        _values        = new object?[count];
        _pendingValues = groupChanged is null ? null : new object?[count];
        _resolved      = new bool[count];
        _owner         = target.GetValue(ThemeScope.TokenOwnerProperty);
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

            _values[index]   = Resolve(index, context);
            _resolved[index] = true;
        }

        return _values[index];
    }

    /// <summary>
    /// How often the set resolved an entry; contract tests pin the publish early-exit with it.
    /// </summary>
    internal int ResolveCount { get; private set; }

    /// <summary>
    /// Points an entry at <paramref name="kind" /> (null clears it). An entry that has been read resolves again now and
    /// reports a change; an unread one resolves on its first read.
    /// </summary>
    internal void SetKind(int index, SharedTokenKind? kind)
    {
        if (_kinds[index] == kind && (kind.HasValue || _keys[index] is null))
        {
            return;
        }

        var key = kind is { } token && _owner is { } owner ? OwnerTokenBinding.GetResourceKey(owner, token) : null;
        SetEntry(index, kind, key);
    }

    /// <summary>
    /// Points an entry at the exact resource key used by the original consumer.
    /// </summary>
    internal void SetResourceKey(int index, object? key)
    {
        if (_kinds[index] is null && Equals(_keys[index], key))
        {
            return;
        }

        SetEntry(index, null, key);
    }

    private void SetEntry(int index, SharedTokenKind? kind, object? key)
    {
        var previousKind = _kinds[index];
        var previousKey  = _keys[index];
        _kinds[index] = kind;
        _keys[index]  = key;
        if (!_resolved[index])
        {
            return;
        }

        object? value;
        try
        {
            value = Resolve(index, ThemeScope.ResolveContext(_target));
        }
        catch
        {
            _kinds[index] = previousKind;
            _keys[index]  = previousKey;
            throw;
        }

        if (UpdateValue(index, value, _values))
        {
            _groupChanged?.Invoke();
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
        _publishes = default;
    }

    void ITokenOwnerListener.OnTokenOwnerChanged(ControlTokenIdentity? owner)
    {
        _owner = owner;
        for (var i = 0; i < _kinds.Length; i++)
        {
            if (_kinds[i] is { } token)
            {
                _keys[i] = owner is { } identity ? OwnerTokenBinding.GetResourceKey(identity, token) : null;
            }
        }

        RefreshAll();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) => RefreshAll();

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        var context = ThemeScope.ResolveContext(_target);
        var delta   = _publishes.Observe(context);
        if (!ReferenceEquals(delta, ThemePublishDelta.Nothing))
        {
            RefreshEntries(context, delta);
        }
    }

    private void RefreshAll()
    {
        var context = ThemeScope.ResolveContext(_target);
        _publishes.MarkCurrent(context);
        RefreshEntries(context, null);
    }

    private void RefreshEntries(ThemeContext? context, ThemePublishDelta? delta)
    {
        if (_pendingValues is not { } pending)
        {
            ReadEntries(context, delta, _values);
            return;
        }

        // Stage the group so a lookup failure leaves the last complete values available for a retry.
        Array.Copy(_values, pending, _values.Length);
        bool changed;
        try
        {
            changed = ReadEntries(context, delta, pending);
            if (changed)
            {
                Array.Copy(pending, _values, pending.Length);
            }
        }
        catch
        {
            // A failed lookup may have followed an owner or variant change. The next notification must re-read every entry.
            _publishes = default;
            throw;
        }
        finally
        {
            Array.Clear(pending);
        }

        if (changed)
        {
            _groupChanged!.Invoke();
        }
    }

    private bool ReadEntries(ThemeContext? context, ThemePublishDelta? delta, object?[] values)
    {
        var changed = false;
        for (var i = 0; i < _kinds.Length; i++)
        {
            if (_resolved[i] && (_kinds[i].HasValue || _keys[i] is not null) &&
                (delta is null || IsKeyChanged(i, delta, context!)))
            {
                changed |= UpdateValue(i, Resolve(i, context), values);
            }
        }
        return changed;
    }

    private bool IsKeyChanged(int index, ThemePublishDelta delta, ThemeContext context)
    {
        if (_kinds[index] is not { } kind)
        {
            return delta.IsChanged(_keys[index]!);
        }
        if (_keys[index] is not null && context.TryGetRegisteredOwnerSlot(_owner!.Value, out var slot))
        {
            return delta.IsOwnerGlobalChanged(slot, kind);
        }
        return delta.IsGlobalChanged(kind);
    }

    private object? Resolve(int index, ThemeContext? context)
    {
        var key = _keys[index];
        if (_kinds[index] is { } kind && (key is null || OwnerTokenBinding.IsKnownUnregistered(context, _owner!.Value)))
        {
            key = kind;
        }
        if (key is null)
        {
            return null;
        }

        ResolveCount++;
        if (key is ThemeTokenResourceProvider.ExplicitTokenLookup lookup)
        {
            if (ThemeTokenResourceProvider.TryFindExplicitTokenResource(_target, lookup.Key, out var explicitValue))
            {
                return explicitValue;
            }
            if (lookup.FallbackResourceKey is null)
            {
                return lookup.DefaultValue;
            }
            return _target.TryFindResource(lookup.FallbackResourceKey, _target.ActualThemeVariant, out var fallback)
                ? fallback
                : lookup.DefaultValue;
        }

        return _target.TryFindResource(key, _target.ActualThemeVariant, out var value) ? value : null;
    }

    private bool UpdateValue(int index, object? value, object?[] values)
    {
        var old = values[index];
        if (Equals(old, value))
        {
            return false;
        }

        values[index] = value;
        _changed?.Invoke(index, old, value);
        return true;
    }
}
