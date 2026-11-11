using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;

namespace AtomUI.Theme;

/// <summary>
/// What one commit of a <see cref="ThemeContext"/> changed, expressed in resource keys, so a per-target binding that
/// is notified by the publish can skip re-resolving a key whose value did not change.
/// </summary>
/// <remarks>
/// <para>
/// Global keys are compared by value between the previous and the current snapshot once, on first use. Control keys
/// are decided per slot: a slot whose cell is shared by both snapshots, or whose configuration reads the root globals
/// in both, follows the global key; a slot that is materialized in both snapshots compares the two values; anything
/// else (a cold cell, an unknown key) counts as changed, so the answer is conservative: "unchanged" is only reported
/// when it is certain.
/// </para>
/// <para>
/// <see cref="Everything"/> is used when the two snapshots are not comparable (no previous snapshot, a different
/// schema registry, a different appearance); <see cref="Nothing"/> is handed to a binding that already handled the
/// current publish.
/// </para>
/// </remarks>
internal sealed class ThemePublishDelta
{
    internal static readonly ThemePublishDelta Everything = new(null, null, everything: true, nothing: false);
    internal static readonly ThemePublishDelta Nothing = new(null, null, everything: false, nothing: true);

    private readonly ThemeSnapshot? _previous;
    private readonly ThemeSnapshot? _current;
    private readonly bool _everything;
    private readonly bool _nothing;
    private HashSet<object>? _changedGlobalKeys;

    private ThemePublishDelta(ThemeSnapshot? previous, ThemeSnapshot? current, bool everything, bool nothing)
    {
        _previous   = previous;
        _current    = current;
        _everything = everything;
        _nothing    = nothing;
    }

    internal static ThemePublishDelta Between(ThemeSnapshot? previous, ThemeSnapshot current)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (previous is null ||
            previous.RegistryRevision != current.RegistryRevision ||
            previous.Controls.Count != current.Controls.Count ||
            previous.Appearance != current.Appearance)
        {
            return new ThemePublishDelta(null, current, everything: true, nothing: false);
        }
        return new ThemePublishDelta(previous, current, everything: false, nothing: false);
    }

    /// <summary>The snapshot this delta leads to; null for the two sentinels.</summary>
    internal ThemeSnapshot? Current => _current;

    /// <summary>The snapshot this delta compares against; null for the sentinels and for <see cref="IsEverything"/>.</summary>
    internal ThemeSnapshot? Previous => _previous;

    internal bool IsEverything => _everything;

    /// <summary>Whether the global value stored under <paramref name="resourceKey"/> changed.</summary>
    internal bool IsGlobalChanged(object resourceKey)
    {
        if (_nothing)
        {
            return false;
        }
        if (_everything)
        {
            return true;
        }
        return (_changedGlobalKeys ??= ComputeChangedGlobalKeys()).Contains(resourceKey);
    }

    /// <summary>
    /// Whether the Effective Global value of <paramref name="globalResourceKey"/> for the control in
    /// <paramref name="slot"/> changed.
    /// </summary>
    internal bool IsOwnerGlobalChanged(int slot, object globalResourceKey)
    {
        if (_nothing)
        {
            return false;
        }
        if (_everything || (uint)slot >= (uint)_current!.Controls.Count)
        {
            return true;
        }

        var previousCell = _previous!.Controls.GetCell(slot);
        var currentCell  = _current.Controls.GetCell(slot);
        if (ReferenceEquals(previousCell, currentCell) ||
            (previousCell.UsesRootSharedResources && currentCell.UsesRootSharedResources))
        {
            // Same delta on both sides (or no delta at all): only the global value can have changed.
            return IsGlobalChanged(globalResourceKey);
        }
        if (previousCell.MaterializedValue is { } previousValue && currentCell.MaterializedValue is { } currentValue)
        {
            var hadPrevious = previousValue.TryGetSharedResource(globalResourceKey, _previous.GlobalResources, out var previous);
            var hasCurrent  = currentValue.TryGetSharedResource(globalResourceKey, _current.GlobalResources, out var current);
            return hadPrevious != hasCurrent || !Equals(previous, current);
        }
        return true;
    }

    /// <summary>Whether the Own value stored under <paramref name="ownResourceKey"/> of the control in <paramref name="slot"/> changed.</summary>
    internal bool IsOwnChanged(int slot, object ownResourceKey)
    {
        if (_nothing)
        {
            return false;
        }
        if (_everything || (uint)slot >= (uint)_current!.Controls.Count)
        {
            return true;
        }

        var previousCell = _previous!.Controls.GetCell(slot);
        var currentCell  = _current.Controls.GetCell(slot);
        if (ReferenceEquals(previousCell, currentCell))
        {
            return false;
        }
        if (previousCell.MaterializedValue is { } previousValue && currentCell.MaterializedValue is { } currentValue)
        {
            var hadPrevious = previousValue.ControlResources.TryGetValue(ownResourceKey, out var previous);
            var hasCurrent  = currentValue.ControlResources.TryGetValue(ownResourceKey, out var current);
            return hadPrevious != hasCurrent || !Equals(previous, current);
        }
        return true;
    }

    /// <summary>
    /// Whether the value a resource lookup of <paramref name="key"/> through the current snapshot would return
    /// changed. Keys the theme does not own (and owners the registry does not register) count as changed.
    /// </summary>
    internal bool IsChanged(object key)
    {
        if (_nothing)
        {
            return false;
        }
        if (_everything)
        {
            return true;
        }

        var registry = _current!.Registry;
        switch (key)
        {
            case ControlTokenResourceKey controlKey:
            {
                int slot;
                if (controlKey.IsBound)
                {
                    slot = controlKey.ControlSlot;
                }
                else if (controlKey.ControlType is { } controlType)
                {
                    if (!registry.TryGetControl(controlType, out var descriptor))
                    {
                        return true;
                    }
                    slot = descriptor.Slot;
                }
                else
                {
                    if (!registry.TryGetControl(controlKey.Identity, out var descriptor))
                    {
                        return true;
                    }
                    slot = descriptor.Slot;
                }
                return IsOwnerGlobalChanged(slot, registry.GetSharedResourceKey(controlKey.Kind));
            }
            case ControlOwnTokenResourceKey ownKey:
            {
                var registered = ownKey.ControlType is { } controlType
                    ? registry.TryGetControl(controlType, out var descriptor)
                    : registry.TryGetControl(ownKey.Identity, out descriptor);
                return !registered || descriptor is null || IsOwnChanged(descriptor.Slot, ownKey.ResourceKey);
            }
            default:
                if (_current.GlobalResources.ContainsKey(key))
                {
                    return IsGlobalChanged(key);
                }
                if (registry.TryGetControlResourceSlot(key, out var ownSlot))
                {
                    return IsOwnChanged(ownSlot, key);
                }
                return true;
        }
    }

    private HashSet<object> ComputeChangedGlobalKeys()
    {
        var changed = new HashSet<object>();
        var previous = _previous!.GlobalResources;
        var current  = _current!.GlobalResources;
        if (ReferenceEquals(previous, current))
        {
            return changed;
        }
        foreach (var resource in current)
        {
            if (!previous.TryGetValue(resource.Key, out var previousValue) || !Equals(previousValue, resource.Value))
            {
                changed.Add(resource.Key);
            }
        }
        foreach (var key in previous.Keys)
        {
            if (!current.ContainsKey(key))
            {
                changed.Add(key);
            }
        }
        return changed;
    }
}

/// <summary>
/// Per-binding record of the snapshot the binding last resolved against. From a <c>ResourcesChanged</c> handler,
/// <see cref="Observe"/> tells the binding whether this notification belongs to a theme publish whose
/// <see cref="ThemePublishDelta"/> it may consult: the context's last commit must lead from exactly that snapshot to
/// the context's current one (null otherwise: not a publish, or the binding is not current with the delta's
/// previous snapshot, so it must refresh fully). A notification while the context still holds the snapshot the
/// binding resolved against yields <see cref="ThemePublishDelta.Nothing"/>.
/// </summary>
/// <remarks>
/// Snapshot identity, not <see cref="ThemeContext.PublishVersion"/>, decides: in a root transaction every scoped
/// context is committed before the root publishes and its own publish raises no <c>ResourcesChanged</c>, so a binding
/// inside a scope sees the root's notification while its context's version has not moved yet. The committed snapshot
/// has, and that is what the binding's value depends on.
/// </remarks>
internal struct ThemePublishTracker
{
    private ThemeContext? _context;
    private ThemeSnapshot? _snapshot;

    /// <summary>Records that the binding has just resolved its value against <paramref name="context"/>'s current snapshot.</summary>
    internal void MarkCurrent(ThemeContext? context)
    {
        _context  = context;
        _snapshot = context?.Snapshot;
    }

    internal ThemePublishDelta? Observe(ThemeContext? context)
    {
        if (context is null || !ThemeContext.IsPublishing)
        {
            MarkCurrent(context);
            return null;
        }

        var snapshot = context.Snapshot;
        if (ReferenceEquals(_context, context) && ReferenceEquals(_snapshot, snapshot))
        {
            return ThemePublishDelta.Nothing;
        }

        var delta = context.LastPublish;
        var usable = ReferenceEquals(_context, context) &&
                     ReferenceEquals(delta.Current, snapshot) &&
                     ReferenceEquals(delta.Previous, _snapshot)
            ? delta
            : null;
        _context  = context;
        _snapshot = snapshot;
        return usable;
    }
}
