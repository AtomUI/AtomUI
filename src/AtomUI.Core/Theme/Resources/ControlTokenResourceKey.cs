using SchemaControlTokenIdentity = AtomUI.Theme.Schema.ControlTokenIdentity;

namespace AtomUI.Theme.Resources;

public readonly record struct ControlTokenResourceKey
{
    internal ControlTokenResourceKey(int controlSlot, SharedTokenKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(controlSlot);
        ControlSlot = controlSlot;
        Kind        = kind;
        Identity    = default;
        ControlType = null;
        IsBound     = true;
    }

    private ControlTokenResourceKey(SchemaControlTokenIdentity identity, SharedTokenKind kind)
    {
        ControlSlot = -1;
        Kind        = kind;
        Identity    = identity;
        IdentityOwnerType = identity.OwnerType;
        ControlType = null;
        IsBound     = false;
    }

    private ControlTokenResourceKey(Type controlType, SharedTokenKind kind)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        ControlSlot = -1;
        Kind        = kind;
        Identity    = default;
        ControlType = controlType;
        IsBound     = false;
    }

    internal int ControlSlot { get; }
    internal SharedTokenKind Kind { get; }
    internal SchemaControlTokenIdentity Identity { get; }
    internal Type? IdentityOwnerType { get; }
    internal Type? ControlType { get; }
    internal bool IsBound { get; }

    public static object Global(
        SchemaControlTokenIdentity identity,
        SharedTokenKind kind)
    {
        return new ControlTokenResourceKey(identity, kind);
    }

    internal static object Global(Type controlType, SharedTokenKind kind)
    {
        return new ControlTokenResourceKey(controlType, kind);
    }

    public static object Own(SchemaControlTokenIdentity identity, object resourceKey) =>
        new ControlOwnTokenResourceKey(identity, resourceKey);

    internal static object Own(Type controlType, object resourceKey)
    {
        return new ControlOwnTokenResourceKey(controlType, resourceKey);
    }
}

internal readonly record struct ControlOwnTokenResourceKey
{
    internal ControlOwnTokenResourceKey(Type controlType, object resourceKey)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        ArgumentNullException.ThrowIfNull(resourceKey);
        ControlType = controlType;
        ResourceKey = resourceKey;
    }

    internal ControlOwnTokenResourceKey(SchemaControlTokenIdentity identity, object resourceKey)
    {
        ArgumentNullException.ThrowIfNull(resourceKey);
        Identity = identity;
        IdentityOwnerType = identity.OwnerType;
        ResourceKey = resourceKey;
    }

    internal Type? ControlType { get; }
    internal SchemaControlTokenIdentity Identity { get; }
    internal Type? IdentityOwnerType { get; }
    internal object ResourceKey { get; }
}
