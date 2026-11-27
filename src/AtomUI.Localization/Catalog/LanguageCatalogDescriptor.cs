namespace AtomUI.Localization;

/// <summary>
/// Describes one language catalog: its identifier, the enum type that names its resource units, and the units in
/// the order in which translation bundles store their values.
/// </summary>
public abstract class LanguageCatalogDescriptor
{
    private readonly IReadOnlyList<LanguageCatalogUnitDescriptor> _units;

    private protected LanguageCatalogDescriptor(
        string catalogId,
        Type resourceKindType,
        IReadOnlyList<LanguageCatalogUnitDescriptor> units,
        bool retainUnits = false)
    {
        LanguageCatalogIdentity.Validate(catalogId);
        ArgumentNullException.ThrowIfNull(resourceKindType);
        ArgumentNullException.ThrowIfNull(units);
        if (units.Count == 0)
        {
            throw new ArgumentException("A language Catalog must contain at least one unit.", nameof(units));
        }

        if (retainUnits)
        {
            CatalogId = catalogId;
            ResourceKindType = resourceKindType;
            _units = units;
            return;
        }

        var unitArray = new LanguageCatalogUnitDescriptor[units.Count];
        var unitKeys = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < units.Count; index++)
        {
            var unit = units[index] ?? throw new ArgumentException(
                "A language Catalog cannot contain a null unit descriptor.",
                nameof(units));
            if (!unitKeys.Add(unit.Key))
            {
                throw new ArgumentException(
                    $"Language unit Key '{unit.Key}' is duplicated in Catalog '{catalogId}'.",
                    nameof(units));
            }

            unitArray[index] = unit;
        }

        CatalogId = catalogId;
        ResourceKindType = resourceKindType;
        _units = Array.AsReadOnly(unitArray);
    }

    /// <summary>
    /// The identifier of the catalog, in the <c>{LanguageModuleId}:{FullyQualifiedMetadataName}</c> format.
    /// It must be unique among the registered catalogs.
    /// </summary>
    public string CatalogId { get; }

    /// <summary>
    /// The enum type whose values name the resource units of the catalog.
    /// Two registered catalogs must not share an enum type, except for catalogs decoded from module data, which all
    /// report <c>Enum</c>.
    /// </summary>
    public Type ResourceKindType { get; }

    /// <summary>
    /// The units of the catalog in the order that gives each unit its slot: a translation bundle stores the value
    /// for a unit at the index of the unit in this list.
    /// </summary>
    public IReadOnlyList<LanguageCatalogUnitDescriptor> Units => _units;

    internal abstract bool TryGetUnitSlot(object resourceKey, out int unitSlot);
}

/// <summary>
/// A language catalog whose units are found through the values of an enum. Register it with
/// <see cref="ILocalizationBuilder.AddCatalog(LanguageCatalogDescriptor)" />.
/// </summary>
/// <typeparam name="TResourceKind">
/// The enum type that names the resource units of the catalog.
/// </typeparam>
public sealed class LanguageCatalogDescriptor<TResourceKind> : LanguageCatalogDescriptor
    where TResourceKind : struct, Enum
{
    private readonly Func<TResourceKind, int> _unitSlotResolver;

    /// <summary>
    /// Creates a catalog whose units are found through the values of an enum.
    /// </summary>
    /// <param name="catalogId">
    /// The identifier of the catalog, in the <c>{LanguageModuleId}:{FullyQualifiedMetadataName}</c> format.
    /// </param>
    /// <param name="units">
    /// The units of the catalog in slot order. It must hold at least one unit, no <see langword="null" /> entry and
    /// no two units with the same key.
    /// </param>
    /// <param name="unitSlotResolver">
    /// Maps a resource key to the index of its unit in <paramref name="units" />. Return any value outside
    /// <see cref="LanguageCatalogDescriptor.Units" /> for a key the catalog does not hold;
    /// <see cref="TryGetUnitSlot(TResourceKind, out int)" /> then returns <see langword="false" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="units" /> or <paramref name="unitSlotResolver" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="catalogId" /> has an invalid format, <paramref name="units" /> is empty, contains a
    /// <see langword="null" /> entry or a duplicate key.
    /// </exception>
    public LanguageCatalogDescriptor(
        string catalogId,
        IReadOnlyList<LanguageCatalogUnitDescriptor> units,
        Func<TResourceKind, int> unitSlotResolver)
        : base(catalogId, typeof(TResourceKind), units)
    {
        _unitSlotResolver = unitSlotResolver ?? throw new ArgumentNullException(nameof(unitSlotResolver));
    }

    /// <summary>
    /// Maps a resource key to the slot of its unit.
    /// </summary>
    /// <param name="resourceKey">
    /// The resource key to resolve.
    /// </param>
    /// <param name="unitSlot">
    /// The index of the unit in <see cref="LanguageCatalogDescriptor.Units" /> when the key is found; otherwise the
    /// value the resolver returned, which lies outside that list.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the key belongs to the catalog; otherwise <see langword="false" />.
    /// </returns>
    public bool TryGetUnitSlot(TResourceKind resourceKey, out int unitSlot)
    {
        unitSlot = _unitSlotResolver(resourceKey);
        return (uint)unitSlot < (uint)Units.Count;
    }

    internal override bool TryGetUnitSlot(object resourceKey, out int unitSlot)
    {
        if (resourceKey is TResourceKind typedKey)
        {
            return TryGetUnitSlot(typedKey, out unitSlot);
        }

        unitSlot = -1;
        return false;
    }
}

internal static class LanguageCatalogIdentity
{
    internal static void Validate(string catalogId)
    {
        ArgumentNullException.ThrowIfNull(catalogId);
        if (string.IsNullOrWhiteSpace(catalogId) ||
            catalogId[0] == ':' ||
            catalogId[^1] == ':' ||
            catalogId.IndexOf(':') < 0)
        {
            throw new ArgumentException(
                "A Catalog ID must use the '{LanguageModuleId}:{FullyQualifiedMetadataName}' format.",
                nameof(catalogId));
        }
    }
}
