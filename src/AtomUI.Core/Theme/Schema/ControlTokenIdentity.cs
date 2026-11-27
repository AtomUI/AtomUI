namespace AtomUI.Theme.Schema;

/// <summary>
/// The catalog-qualified id of a control Token owner, which value packages, theme assets and theme
/// configuration use to name the control's Tokens. Two identities are equal when their catalogs and ids are
/// equal, whatever their owner types.
/// </summary>
public readonly struct ControlTokenIdentity : IEquatable<ControlTokenIdentity>
{
    /// <summary>
    /// Creates an identity without an owner type; <see cref="ForControl" /> creates one that also carries the
    /// owning control type.
    /// </summary>
    /// <param name="catalog">
    /// The catalog the Token owner belongs to.
    /// </param>
    /// <param name="id">
    /// The id of the Token owner within the catalog.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="catalog" /> or <paramref name="id" /> is blank or not a valid theme schema identifier.
    /// </exception>
    public ControlTokenIdentity(string catalog, string id)
    {
        SchemaIdentifier.Validate(catalog, nameof(catalog));
        SchemaIdentifier.Validate(id, nameof(id));
        Catalog = catalog;
        Id      = id;
    }

    private ControlTokenIdentity(Type ownerType, string catalog, string id) : this(catalog, id)
    {
        ArgumentNullException.ThrowIfNull(ownerType);
        if (!typeof(Avalonia.Controls.Control).IsAssignableFrom(ownerType))
        {
            throw new ArgumentException("Token owner must derive from Avalonia.Controls.Control.", nameof(ownerType));
        }
        OwnerType = ownerType;
    }

    /// <summary>
    /// Creates the identity of the Token class owned by <paramref name="controlType" />. Theme assets hash the
    /// owner type of every Token owner they require, so identities handed to them must carry an owner type.
    /// </summary>
    /// <param name="controlType">
    /// The control type that owns the Token class; it must derive from <see cref="Avalonia.Controls.Control" />.
    /// </param>
    /// <param name="catalog">
    /// The catalog the Token owner belongs to.
    /// </param>
    /// <param name="id">
    /// The id of the Token owner within the catalog.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="controlType" /> does not derive from <see cref="Avalonia.Controls.Control" />, or
    /// <paramref name="catalog" /> or <paramref name="id" /> is blank or not a valid theme schema identifier.
    /// </exception>
    public static ControlTokenIdentity ForControl(Type controlType, string catalog, string id) =>
        new(controlType, catalog, id);

    /// <summary>
    /// The control type that owns the Token class, or <see langword="null" /> for an identity made with the
    /// constructor. The registry reads it for startup checks, schema fingerprints and diagnostics.
    /// </summary>
    internal Type? OwnerType { get; }
    /// <summary>
    /// The catalog the Token owner belongs to. Equality and the order of Token owners in fingerprints compare it
    /// ordinally.
    /// </summary>
    public string Catalog { get; }
    /// <summary>
    /// The id of the Token owner within its catalog. Equality compares it ordinally; the order of Token
    /// owners in fingerprints uses it after the catalog.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Returns whether both identities have the same catalog and id, compared ordinally. The owner types take
    /// no part in it.
    /// </summary>
    public bool Equals(ControlTokenIdentity other) =>
        string.Equals(Catalog, other.Catalog, StringComparison.Ordinal) &&
        string.Equals(Id, other.Id, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether the object is a <see cref="ControlTokenIdentity" /> that equals this one; any other value
    /// gives <see langword="false" />.
    /// </summary>
    public override bool Equals(object? obj) => obj is ControlTokenIdentity other && Equals(other);
    /// <summary>
    /// Returns a hash of the catalog and id, consistent with <see cref="Equals(ControlTokenIdentity)" />. It is
    /// randomized per process, so it is not stable across runs.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Catalog, Id);
    /// <summary>
    /// Returns whether two identities have the same catalog and id.
    /// </summary>
    public static bool operator ==(ControlTokenIdentity left, ControlTokenIdentity right) => left.Equals(right);
    /// <summary>
    /// Returns whether two identities differ in catalog or id.
    /// </summary>
    public static bool operator !=(ControlTokenIdentity left, ControlTokenIdentity right) => !left.Equals(right);

    /// <summary>
    /// Returns the identity as <c>catalog:id</c>.
    /// </summary>
    public override string ToString() => $"{Catalog}:{Id}";
}
