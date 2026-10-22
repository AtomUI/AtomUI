namespace AtomUI.Generator.Diagnostics;

internal static class AtomUIDiagnosticIds
{
    public const string CompiledThemeBindingFailed = "ATOMUITOKEN002";
    public const string RegistrationAmbiguousThemeExport = "ATOMUIREG001";
    public const string RegistrationInvalidTokenOwner = "ATOMUIREG002";
    public const string RegistrationInaccessibleType = "ATOMUIREG003";
    public const string RegistrationInvalidResourceDependency = "ATOMUIREG004";
    public const string RegistrationIdentityConflict = "ATOMUIREG005";
    public const string RegistrationUnsupportedBackend = "ATOMUIREG006";
    public const string RegistrationUnloweredTypeMapAccessor = "ATOMUIREG007";
    // Retired diagnostic: package collection no longer has backend-specific startup partitions.
    public const string RegistrationStagingFallback = "ATOMUIREG009";
    // Retired diagnostic: control fragments no longer have a bootstrap phase; all fragments defer to the startup commit.
    public const string RegistrationInvalidBootstrap = "ATOMUIREG010";
    public const string StartupManifestInvalid = "ATOMUIREG011";
    public const string StartupPackageConflict = "ATOMUIREG012";
    public const string StartupThemeContractMismatch = "ATOMUIREG013";
    public const string StartupLanguageContractMismatch = "ATOMUIREG014";
    public const string StartupImageManualHost = "ATOMUIREG015";
    public const string AotMissingGeneratedAccessor = "ATOMUIAOT001";
    public const string AotMissingGeneratedPath = "ATOMUIAOT002";
    public const string AotUnverifiableDataMemberPath = "ATOMUIAOT003";
    public const string ScopedResourceHostInvalidClassShape = "ATOMUIGEN001";
    public const string ScopedResourceHostRequiresAvaloniaObject = "ATOMUIGEN002";
    public const string ScopedResourceHostRejectsVisualTarget = "ATOMUIGEN003";
    public const string ScopedResourceHostRejectsExistingResourceHost = "ATOMUIGEN004";
    public const string ControlTokenInvalidName = "ATOMUIGEN012";
    public const string ControlTokenInheritance = "ATOMUIGEN013";
    public const string ControlTokenMissingControl = "ATOMUIGEN014";
    public const string ControlTokenAmbiguousControl = "ATOMUIGEN015";
    public const string ControlTokenGlobalNameConflict = "ATOMUIGEN019";
    public const string ControlTokenMustBeSealed = "ATOMUIGEN020";
    public const string ControlTokenGenericLayer = "ATOMUIGEN021";
    public const string ControlTokenPropertyConflict = "ATOMUIGEN022";
    public const string ControlTokenInvalidProperty = "ATOMUIGEN023";
    public const string ControlTokenInvalidCalculationChain = "ATOMUIGEN024";
    public const string AbstractControlTokenInvalidName = "ATOMUIGEN025";
    public const string ControlTokenNestedType = "ATOMUIGEN026";
    public const string SemanticPartInvalidDeclaration = "ATOMUIGEN027";
    public const string SemanticPartDuplicateDeclaration = "ATOMUIGEN028";
    public const string SemanticPartInvalidContractType = "ATOMUIGEN029";
    public const string SemanticPartInvalidThemeContract = "ATOMUIGEN030";
    public const string SemanticPartMissingSince = "ATOMUIGEN031";
    public const string SemanticPartTemplateCardinalityMismatch = "ATOMUIGEN032";
    public const string SemanticPartTemplateContractTypeMismatch = "ATOMUIGEN033";
    public const string SemanticPartMissingControlTemplate = "ATOMUIGEN034";
    public const string SemanticPartTemplateNodeConflict = "ATOMUIGEN035";
    public const string SemanticPartTemplateMarkerNotStatic = "ATOMUIGEN036";
    public const string SemanticPartStyleTypeConflict = "ATOMUIGEN037";
    public const string SemanticPartInvalidSince = "ATOMUIGEN038";
    public const string StartupHookMissingAttach = "ATOMUIGEN039";
    public const string ControlTokenDefaultVersion = "ATOMUIGEN040";
    public const string ControlTokenDefaultDeclaration = "ATOMUIGEN041";
    public const string ControlTokenDefaultSignature = "ATOMUIGEN042";
    public const string ControlTokenDefaultDependency = "ATOMUIGEN043";
    public const string ControlTokenAliasDeclaration = "ATOMUIGEN044";
    public const string LocalizationInvalidLanguageData = "ATOMUILOC001";
    public const string LocalizationDuplicateLanguageData = "ATOMUILOC002";
    public const string LocalizationInvalidCatalog = "ATOMUILOC003";
    public const string LocalizationInvalidCatalogUnit = "ATOMUILOC004";
    public const string LocalizationInvalidXliff = "ATOMUILOC005";
    public const string LocalizationCatalogXliffMismatch = "ATOMUILOC006";
    public const string LocalizationInvalidTranslation = "ATOMUILOC007";
    public const string LocalizationInvalidApplicationHost = "ATOMUILOC008";
    public const string LocalizationInvalidLanguagePackage = "ATOMUILOC009";
}
