using System.ComponentModel;

namespace AtomUI.Theme.Schema;

/// <summary>
/// The intermediate fingerprint state of the list of global Token names. The generator computes it at compile
/// time and embeds it as a constant; at runtime a <see cref="SchemaFingerprintBuilder" /> restored from it
/// hashes only the fields of one theme asset, so every asset skips hashing the same global names again. Only
/// generated code creates and consumes it. The value 0 means that nothing was hashed, as in the builder.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct SchemaPrefixState : IEquatable<SchemaPrefixState>
{
    /// <summary>
    /// Captures a raw fingerprint state.
    /// </summary>
    /// <param name="value">
    /// The raw state of a <see cref="SchemaFingerprintBuilder" />, as <see cref="SchemaFingerprintBuilder.Save" />
    /// returns it; 0 means that nothing was hashed.
    /// </param>
    public SchemaPrefixState(ulong value)
    {
        Value = value;
    }

    /// <summary>
    /// The raw fingerprint state, or 0 when nothing has been hashed.
    /// </summary>
    public ulong Value { get; }

    /// <summary>
    /// Returns whether both states hold the same raw value.
    /// </summary>
    public bool Equals(SchemaPrefixState other) => Value == other.Value;

    /// <summary>
    /// Returns whether the object is a <see cref="SchemaPrefixState" /> with the same raw value.
    /// </summary>
    public override bool Equals(object? obj) => obj is SchemaPrefixState other && Equals(other);

    /// <summary>
    /// Returns the hash code of the raw value.
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Returns whether two states hold the same raw value.
    /// </summary>
    public static bool operator ==(SchemaPrefixState left, SchemaPrefixState right) => left.Equals(right);

    /// <summary>
    /// Returns whether two states hold different raw values.
    /// </summary>
    public static bool operator !=(SchemaPrefixState left, SchemaPrefixState right) => !left.Equals(right);
}
