using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
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
