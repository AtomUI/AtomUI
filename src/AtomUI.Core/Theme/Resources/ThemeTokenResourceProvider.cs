using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

internal sealed class ThemeTokenResourceProvider : ResourceProvider
{
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

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        var snapshot = Snapshot;
        if (key is ExplicitTokenResourceKey explicitKey)
        {
            return TryGetExplicitResource(snapshot, explicitKey.ResourceKey, theme, out value);
        }
        if (key is ControlTokenResourceKey controlKey)
        {
            var controlSlot = ResolveGlobalControlSlot(snapshot, controlKey);

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

            snapshot.Registry.NotifyControlFirstUse(descriptor.Slot);
            var found = snapshot.Controls[descriptor.Slot]
                                   .ControlResources
                                   .TryGetValue(ownKey.ResourceKey, out value);
            value = ThemeResourceValue.CloneForConsumer(value);
            return found;
        }

        if (snapshot.GlobalResources.TryGetValue(key, out value))
        {
            value = ThemeResourceValue.CloneForConsumer(value);
            return true;
        }
        if (snapshot.Registry.TryGetControlResourceSlot(key, out var resourceControlSlot) &&
            (uint)resourceControlSlot < (uint)snapshot.Controls.Count)
        {
            snapshot.Registry.NotifyControlFirstUse(resourceControlSlot);
            var found = snapshot.Controls[resourceControlSlot].ControlResources.TryGetValue(key, out value);
            value = ThemeResourceValue.CloneForConsumer(value);
            return found;
        }

        value = null;
        return false;
    }

    private static int ResolveGlobalControlSlot(ThemeSnapshot snapshot, ControlTokenResourceKey controlKey)
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
        return controlSlot;
    }

    private bool TryGetExplicitResource(ThemeSnapshot snapshot, object key, ThemeVariant? theme, out object? value)
    {
        if (key is ControlTokenResourceKey globalKey)
        {
            var globalSlot = ResolveGlobalControlSlot(snapshot, globalKey);
            if ((uint)globalSlot >= (uint)snapshot.Controls.Count)
            {
                value = null;
                return false;
            }
            var globalDescriptor = snapshot.Registry.Controls[globalSlot];
            var globalResourceKey = snapshot.Registry.GetSharedResourceKey(globalKey.Kind);
            var configured = snapshot.EffectiveConfig.GlobalTokens.Any(candidate =>
                Equals(candidate.Descriptor.ResourceKey, globalResourceKey));
            if (snapshot.EffectiveConfig.TryGetControl(globalDescriptor.Identity, out var globalConfig))
            {
                configured |= globalConfig.GlobalTokens.Any(candidate => Equals(candidate.Descriptor.ResourceKey, globalResourceKey));
            }
            if (!configured)
            {
                value = AvaloniaProperty.UnsetValue;
                return true;
            }
            return TryGetResource(key, theme, out value);
        }

        var resourceKey = key is ControlOwnTokenResourceKey owned ? owned.ResourceKey : key;
        if (!snapshot.Registry.TryGetControlResourceSlot(resourceKey, out var slot))
        {
            throw new InvalidOperationException("WhenExplicit requires a registered Control Global or Own Token resource.");
        }
        var descriptor = snapshot.Registry.Controls[slot];
        if (key is ControlOwnTokenResourceKey ownerKey)
        {
            ControlTokenDescriptor? declared;
            var registered = ownerKey.ControlType is { } controlType
                ? snapshot.Registry.TryGetControl(controlType, out declared)
                : snapshot.Registry.TryGetControl(ownerKey.Identity, out declared);
            if (!registered || declared?.Slot != slot)
            {
                throw new InvalidOperationException("The explicit Own Token resource does not belong to its declared owner.");
            }
        }
        var token = descriptor.OwnTokens.Single(candidate => Equals(candidate.ResourceKey, resourceKey));
        var canonical = descriptor.GetCanonicalOwnToken(token);
        if (!snapshot.EffectiveConfig.TryGetControl(descriptor.Identity, out var config) ||
            !config.OwnTokens.Any(candidate => candidate.Descriptor.Slot == canonical.Slot))
        {
            // Found-but-unset stops outer resource-host fallback. Consumers must
            // select their fallback explicitly: Unset alone does not preserve an
            // inherited target value in every native binding expression.
            value = AvaloniaProperty.UnsetValue;
            return true;
        }
        return TryGetResource(key, theme, out value);
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
