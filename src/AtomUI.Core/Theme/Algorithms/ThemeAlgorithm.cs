using System.ComponentModel;
using AtomUI.Theme.Schema;
using AtomUI.Utils;

namespace AtomUI.Theme.Algorithms;

/// <summary>
/// Identifies a theme algorithm by catalog and id, for example <c>AtomUI:Dark</c>.
/// Equality and ordering are ordinal on <see cref="Catalog"/> and then <see cref="Id"/>.
/// </summary>
[TypeConverter(typeof(ThemeAlgorithmTypeConverter))]
public readonly struct ThemeAlgorithm : IEquatable<ThemeAlgorithm>
{
    internal const string BuiltInCatalog = "AtomUI";

    /// <summary>
    /// Creates an algorithm identity. Both parts must be valid theme schema identifiers.
    /// </summary>
    public ThemeAlgorithm(string catalog, string id)
    {
        SchemaIdentifier.Validate(catalog, nameof(catalog));
        SchemaIdentifier.Validate(id, nameof(id));
        Catalog = catalog;
        Id      = id;
    }

    /// <summary>
    /// The catalog that owns the algorithm, for example <c>AtomUI</c>.
    /// </summary>
    public string Catalog { get; }

    /// <summary>
    /// The algorithm id inside its catalog, for example <c>Dark</c>.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The built-in Ant Design light algorithm, <c>AtomUI:Default</c>.
    /// </summary>
    public static ThemeAlgorithm Default { get; } = new(BuiltInCatalog, "Default");

    /// <summary>
    /// The built-in Ant Design dark algorithm, <c>AtomUI:Dark</c>.
    /// </summary>
    public static ThemeAlgorithm Dark { get; } = new(BuiltInCatalog, "Dark");

    /// <summary>
    /// The built-in Ant Design compact algorithm, <c>AtomUI:Compact</c>.
    /// </summary>
    public static ThemeAlgorithm Compact { get; } = new(BuiltInCatalog, "Compact");

    /// <summary>
    /// True for <c>default(ThemeAlgorithm)</c>, which never names a registered algorithm.
    /// </summary>
    internal bool IsUninitialized => Catalog is null;

    /// <summary>
    /// Orders identities ordinally by catalog and then by id.
    /// </summary>
    internal static int Compare(ThemeAlgorithm left, ThemeAlgorithm right)
    {
        var result = string.CompareOrdinal(left.Catalog, right.Catalog);
        return result != 0 ? result : string.CompareOrdinal(left.Id, right.Id);
    }

    public bool Equals(ThemeAlgorithm other) =>
        string.Equals(Catalog, other.Catalog, StringComparison.Ordinal) &&
        string.Equals(Id, other.Id, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ThemeAlgorithm other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Catalog, Id);
    public static bool operator ==(ThemeAlgorithm left, ThemeAlgorithm right) => left.Equals(right);
    public static bool operator !=(ThemeAlgorithm left, ThemeAlgorithm right) => !left.Equals(right);

    public override string ToString() => $"{Catalog}:{Id}";
}
