using AtomUI.Localization.Data;

namespace AtomUI.Localization;

/// <summary>
/// The translated values of one catalog in one language: one value slot per unit of the catalog.
/// A bundle is compiled into the data of a language module, supplied by a static language pack, or registered
/// by the application through
/// <see cref="ILocalizationBuilder.AddTranslationBundle(TranslationBundleDescriptor)" />.
/// </summary>
public sealed class TranslationBundleDescriptor
{
    private readonly IReadOnlyList<string?> _values;

    /// <summary>
    /// Creates a translation bundle. The values are copied into a read-only list, so later changes to the list
    /// passed in do not affect the bundle.
    /// </summary>
    /// <param name="catalogId">
    /// The identifier of the catalog the bundle supplies, in the
    /// <c>{LanguageModuleId}:{FullyQualifiedMetadataName}</c> format.
    /// </param>
    /// <param name="language">
    /// The language the values are written in. It must not be the default value.
    /// </param>
    /// <param name="sourceKind">
    /// The kind of source the bundle comes from.
    /// </param>
    /// <param name="sourceIdentity">
    /// The identity of the origin that supplies the bundle. It must not be empty or white space.
    /// </param>
    /// <param name="values">
    /// The value for each unit of the catalog, in unit slot order; <see langword="null" /> where the bundle has no
    /// value. It must hold at least one slot, and its count must equal the unit count of the catalog.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sourceIdentity" /> or <paramref name="values" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="catalogId" /> has an invalid format, <paramref name="language" /> is the default value,
    /// <paramref name="sourceIdentity" /> is empty or white space, or <paramref name="values" /> is empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sourceKind" /> is not one of the defined source kinds.
    /// </exception>
    public TranslationBundleDescriptor(
        string catalogId,
        LanguageTag language,
        TranslationSourceKind sourceKind,
        string sourceIdentity,
        IReadOnlyList<string?> values)
    {
        LanguageCatalogIdentity.Validate(catalogId);
        if (language == default)
        {
            throw new ArgumentException("A translation bundle requires a valid language tag.", nameof(language));
        }
        if (sourceKind is not TranslationSourceKind.ModuleBuiltIn and
            not TranslationSourceKind.StaticLanguagePack and
            not TranslationSourceKind.ApplicationOverride)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceKind), sourceKind, "Unknown translation source kind.");
        }

        ArgumentNullException.ThrowIfNull(sourceIdentity);
        if (string.IsNullOrWhiteSpace(sourceIdentity))
        {
            throw new ArgumentException(
                "A translation bundle source identity cannot be empty or whitespace.",
                nameof(sourceIdentity));
        }

        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
        {
            throw new ArgumentException("A translation bundle must contain at least one value slot.", nameof(values));
        }

        var valueArray = new string?[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            valueArray[index] = values[index];
        }

        CatalogId = catalogId;
        Language = language;
        SourceKind = sourceKind;
        SourceIdentity = sourceIdentity;
        _values = Array.AsReadOnly(valueArray);
    }

    internal TranslationBundleDescriptor(LanguageModuleBundle bundle)
    {
        LanguageCatalogIdentity.Validate(bundle.CatalogId);
        CatalogId = bundle.CatalogId;
        Language = LanguageTag.Parse(bundle.Language);
        SourceKind = (TranslationSourceKind)bundle.SourceKind;
        SourceIdentity = bundle.SourceIdentity;
        _values = bundle.Values;
    }

    internal bool IsModuleData => _values is LanguageModuleValues;

    internal bool HasValue(int unitSlot) => _values is LanguageModuleValues values
        ? values.HasValue(unitSlot)
        : _values[unitSlot] is not null;

    /// <summary>
    /// The identifier of the catalog the bundle supplies, in the
    /// <c>{LanguageModuleId}:{FullyQualifiedMetadataName}</c> format.
    /// </summary>
    public string CatalogId { get; }

    /// <summary>
    /// The language the values are written in.
    /// </summary>
    public LanguageTag Language { get; }

    /// <summary>
    /// The kind of source the bundle comes from, as a <see cref="TranslationSourceKind" />.
    /// When bundles of the same language come from different kinds and hold a value for the same unit, the value of
    /// the higher kind is used.
    /// </summary>
    public TranslationSourceKind SourceKind { get; }

    /// <summary>
    /// The identity of the origin that supplies the bundle. The registry names it in diagnostics about the bundle.
    /// </summary>
    public string SourceIdentity { get; }

    /// <summary>
    /// The value for each unit of the catalog in unit slot order; <see langword="null" /> where the bundle holds no
    /// value for the unit. The count must equal the number of units of the catalog.
    /// </summary>
    public IReadOnlyList<string?> Values => _values;
}
