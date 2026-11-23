using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

internal sealed class ThemeTokenResourceProvider : ResourceProvider
{
    // Scope this probe to one key in a synchronous resource walk.
    [ThreadStatic]
    private static object? s_explicitTokenKey;

    private static readonly object s_implicitGlobalToken = new();

    private ThemeSnapshot? _snapshot;
    private readonly ThemeContext? _context;

    internal ThemeTokenResourceProvider(ThemeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _snapshot = snapshot;
    }

    internal ThemeTokenResourceProvider(ThemeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ThemeSnapshot Snapshot => _context?.Snapshot ?? Volatile.Read(ref _snapshot)!;

    public override bool HasResources => true;

    internal readonly record struct ExplicitTokenLookup(object Key, object? FallbackResourceKey = null, object? DefaultValue = null);

    internal static bool TryFindExplicitTokenResource(StyledElement target, object key, out object? value)
    {
        var previousKey = s_explicitTokenKey;
        s_explicitTokenKey = key;
        try
        {
            if (target.TryFindResource(key, target.ActualThemeVariant, out value) &&
                !ReferenceEquals(value, s_implicitGlobalToken))
            {
                return true;
            }

            value = null;
            return false;
        }
        finally
        {
            s_explicitTokenKey = previousKey;
        }
    }


    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        var snapshot = Snapshot;
        // Every resource lookup passes here; read the thread-static probe once and compare only while one is active.
        var explicitTokenKey = s_explicitTokenKey;
        var isExplicitProbe = explicitTokenKey is not null && Equals(key, explicitTokenKey);
        if (key is ControlTokenResourceKey controlKey)
        {
            var controlSlot = controlKey.ControlSlot;
            if (!controlKey.IsBound)
            {
                ControlTokenDescriptor? descriptor;
                if (controlKey.ControlType is { } controlType)
                {
                    if (!snapshot.Registry.TryGetControl(controlType, out descriptor))
                    {
                        throw new InvalidOperationException(
                            $"Control type '{controlType.FullName}' is not registered in the active theme schema.");
                    }
                }
                else if (!snapshot.Registry.TryGetControl(controlKey.Identity, out descriptor))
                {
                    throw new InvalidOperationException(
                        $"Control Token identity '{controlKey.Identity}' is not registered in the active theme schema.");
                }
                controlSlot = descriptor.Slot;
            }

            if ((uint)controlSlot >= (uint)snapshot.Controls.Count)
            {
                value = null;
                return false;
            }

            var resourceKey = snapshot.Registry.GetSharedResourceKey(controlKey.Kind);
            snapshot.Registry.NotifyControlFirstUse(controlSlot);
            var cell = snapshot.Controls.GetCell(controlSlot);
            var found = cell.UsesRootSharedResources
                ? snapshot.GlobalResources.TryGetValue(resourceKey, out value)
                : cell.GetValue().TryGetSharedResource(resourceKey, snapshot.GlobalResources, out value);
            if (found && isExplicitProbe &&
                !IsExplicitControlGlobalToken(snapshot, controlSlot, resourceKey))
            {
                // Stop at the winning resource provider even when its value is implicit.
                value = s_implicitGlobalToken;
                return true;
            }
            value = ThemeResourceValue.CloneForConsumer(value);
            return found;
        }

        if (key is ControlOwnTokenResourceKey ownKey)
        {
            ControlTokenDescriptor? descriptor;
            var registered = ownKey.ControlType is { } controlType
                ? snapshot.Registry.TryGetControl(controlType, out descriptor)
                : snapshot.Registry.TryGetControl(ownKey.Identity, out descriptor);
            if (!registered || descriptor is null)
            {
                throw new InvalidOperationException(
                    $"Control owner '{ownKey.ControlType?.FullName ?? ownKey.Identity.ToString()}' is not registered in the active theme schema.");
            }
            if (!snapshot.Registry.TryGetControlResourceSlot(ownKey.ResourceKey, out var ownControlSlot) ||
                ownControlSlot != descriptor.Slot)
            {
                throw new InvalidOperationException(
                    $"Own Token resource key '{ownKey.ResourceKey}' does not belong to Control type " +
                    $"'{descriptor.ControlType.FullName}'.");
            }

            if (isExplicitProbe &&
                !IsExplicitOwnToken(snapshot, descriptor.Slot, ownKey.ResourceKey))
            {
                value = null;
                return false;
            }

            snapshot.Registry.NotifyControlFirstUse(descriptor.Slot);
            var found = snapshot.Controls[descriptor.Slot]
                                   .ControlResources
                                   .TryGetValue(ownKey.ResourceKey, out value);
            value = ThemeResourceValue.CloneForConsumer(value);
            return found;
        }

        if (snapshot.GlobalResources.TryGetValue(key, out value))
        {
            value = isExplicitProbe && !ContainsToken(snapshot.EffectiveConfig.GlobalTokens, key)
                ? s_implicitGlobalToken
                : ThemeResourceValue.CloneForConsumer(value);
            return true;
        }
        if (snapshot.Registry.TryGetControlResourceSlot(key, out var resourceControlSlot) &&
            (uint)resourceControlSlot < (uint)snapshot.Controls.Count)
        {
            if (isExplicitProbe && !IsExplicitOwnToken(snapshot, resourceControlSlot, key))
            {
                value = null;
                return false;
            }

            snapshot.Registry.NotifyControlFirstUse(resourceControlSlot);
            var found = snapshot.Controls[resourceControlSlot].ControlResources.TryGetValue(key, out value);
            value = ThemeResourceValue.CloneForConsumer(value);
            return found;
        }

        value = null;
        return false;
    }

    private static bool IsExplicitControlGlobalToken(ThemeSnapshot snapshot, int controlSlot, object key)
    {
        var config = snapshot.EffectiveConfig;
        config.TryGetControl(snapshot.Registry.GetControlIdentity(controlSlot), out var control);
        return (control is not null && ContainsToken(control.GlobalTokens, key)) ||
               (control is not { IsOwnEvaluationUnit: true } && ContainsToken(config.GlobalTokens, key));
    }

    private static bool IsExplicitOwnToken(ThemeSnapshot snapshot, int controlSlot, object key) =>
        snapshot.EffectiveConfig.TryGetControl(snapshot.Registry.GetControlIdentity(controlSlot), out var control) &&
        ContainsToken(control.OwnTokens, key);

    private static bool ContainsToken(IReadOnlyList<NormalizedTokenValue> tokens, object key)
    {
        foreach (var token in tokens)
        {
            if (Equals(key, token.Descriptor.ResourceKey))
            {
                return true;
            }
        }

        return false;
    }

    internal void PrepareSnapshot(ThemeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (_context is not null)
        {
            throw new InvalidOperationException("A context-backed resource provider is updated through its ThemeContext.");
        }
        Volatile.Write(ref _snapshot, snapshot);
    }

    internal void PublishSnapshotChanged()
    {
        RaiseResourcesChanged();
    }

    public void ReplaceSnapshot(ThemeSnapshot snapshot)
    {
        PrepareSnapshot(snapshot);
        PublishSnapshotChanged();
    }
}
