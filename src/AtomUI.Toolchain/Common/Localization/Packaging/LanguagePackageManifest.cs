namespace AtomUI.Build.Tasks.LocalizationBuild;

internal sealed class LanguagePackageManifest
{
    internal LanguagePackageManifest(
        string packageId,
        string language,
        IReadOnlyList<LanguagePackageCatalogEntry> catalogs)
    {
        PackageId = packageId;
        Language = language;
        Catalogs = catalogs;
    }

    internal string PackageId { get; }

    internal string Language { get; }

    internal IReadOnlyList<LanguagePackageCatalogEntry> Catalogs { get; }
}

internal sealed class LanguagePackageCatalogEntry
{
    internal LanguagePackageCatalogEntry(
        string moduleId,
        string catalogId,
        LanguagePackageContractValidation contractValidation,
        string path,
        string sourceFingerprint)
    {
        ModuleId = moduleId;
        CatalogId = catalogId;
        ContractValidation = contractValidation;
        Path = path;
        SourceFingerprint = sourceFingerprint;
    }

    internal string ModuleId { get; }

    internal string CatalogId { get; }

    internal LanguagePackageContractValidation ContractValidation { get; }

    internal string Path { get; }

    internal string SourceFingerprint { get; }
}
