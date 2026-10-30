namespace AtomUI.Theme.Schema;

public readonly struct ControlTokenIdentity : IEquatable<ControlTokenIdentity>
{
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

    public static ControlTokenIdentity ForControl(Type controlType, string catalog, string id) =>
        new(controlType, catalog, id);

    internal Type? OwnerType { get; }
    public string Catalog { get; }
    public string Id { get; }

    public bool Equals(ControlTokenIdentity other) =>
        string.Equals(Catalog, other.Catalog, StringComparison.Ordinal) &&
        string.Equals(Id, other.Id, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ControlTokenIdentity other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Catalog, Id);
    public static bool operator ==(ControlTokenIdentity left, ControlTokenIdentity right) => left.Equals(right);
    public static bool operator !=(ControlTokenIdentity left, ControlTokenIdentity right) => !left.Equals(right);

    public override string ToString() => $"{Catalog}:{Id}";
}
