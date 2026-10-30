using AtomUI.Localization.Data;

namespace AtomUI.Localization;

public sealed class TranslationBundleDescriptor
{
    private readonly IReadOnlyList<string?> _values;

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

    public string CatalogId { get; }

    public LanguageTag Language { get; }

    public TranslationSourceKind SourceKind { get; }

    public string SourceIdentity { get; }

    public IReadOnlyList<string?> Values => _values;
}
