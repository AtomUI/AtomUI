using SchemaControlTokenIdentity = AtomUI.Theme.Schema.ControlTokenIdentity;

namespace AtomUI.Theme.Resources;

/// <summary>
/// A resource key for a global Token that is looked up through the Effective Global Token of a control: the
/// control's own value of the Token is used when it has one, otherwise the global resource.
/// </summary>
/// <remarks>
/// A bound key stores the slot of one control in one snapshot. An unbound key stores a control identity or a
/// control type instead, and is resolved against the active theme snapshot at each lookup.
/// </remarks>
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

    /// <summary>
    /// Creates the key for the global Token <paramref name="kind" /> as it is effective for the control that
    /// <paramref name="identity" /> identifies. Looking the key up throws an
    /// <see cref="InvalidOperationException" /> when that control is not registered in the active theme schema.
    /// </summary>
    /// <param name="identity">
    /// The identity of the control that owns the Token.
    /// </param>
    /// <param name="kind">
    /// The global Token to read.
    /// </param>
    /// <returns>
    /// A resource key for resource lookups and <c>DynamicResource</c> bindings.
    /// </returns>
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

    /// <summary>
    /// Creates the key for the Own Token <paramref name="resourceKey" /> of the control that
    /// <paramref name="identity" /> identifies. Looking the key up throws an
    /// <see cref="InvalidOperationException" /> when that control is not registered in the active theme schema or
    /// the Token is not one of its Own Tokens.
    /// </summary>
    /// <param name="identity">
    /// The identity of the control that owns the Token.
    /// </param>
    /// <param name="resourceKey">
    /// The resource key of an Own Token of that control.
    /// </param>
    /// <returns>
    /// A resource key for resource lookups and <c>DynamicResource</c> bindings.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="resourceKey" /> is <see langword="null" />.
    /// </exception>
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
