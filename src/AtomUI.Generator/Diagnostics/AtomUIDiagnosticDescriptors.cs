using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Diagnostics;

#pragma warning disable RS2008
internal static partial class AtomUIDiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ControlTokenDefaultVersion = DefaultDiagnostic(
        AtomUIDiagnosticIds.ControlTokenDefaultVersion, "Control default evaluation contract is invalid");
    public static readonly DiagnosticDescriptor ControlTokenDefaultDeclaration = DefaultDiagnostic(
        AtomUIDiagnosticIds.ControlTokenDefaultDeclaration, "Control default declaration is invalid");
    public static readonly DiagnosticDescriptor ControlTokenDefaultSignature = DefaultDiagnostic(
        AtomUIDiagnosticIds.ControlTokenDefaultSignature, "Control default signature is invalid");
    public static readonly DiagnosticDescriptor ControlTokenDefaultDependency = DefaultDiagnostic(
        AtomUIDiagnosticIds.ControlTokenDefaultDependency, "Control default dependency is invalid");
    public static readonly DiagnosticDescriptor ControlTokenAliasDeclaration = new(
        AtomUIDiagnosticIds.ControlTokenAliasDeclaration, "Control Token alias declaration is invalid",
        "Control '{0}' alias contract is invalid: {1}", AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: [WellKnownDiagnosticTags.Telemetry]);

    private static DiagnosticDescriptor DefaultDiagnostic(string id, string title) => new(id, title,
        "Control '{0}' default contract is invalid: {1}", AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor AotMissingGeneratedAccessor = new(
        AtomUIDiagnosticIds.AotMissingGeneratedAccessor,
        "AOT-sensitive data member path requires generated accessor",
        "AOT-sensitive data member path '{0}' on '{1}' requires [GenerateDataMemberAccessors] or an IDataMemberAccessorDescriptor",
        AtomUIDiagnosticCategories.Aot,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor AotMissingGeneratedPath = new(
        AtomUIDiagnosticIds.AotMissingGeneratedPath,
        "AOT-sensitive data member path is not generated",
        "AOT-sensitive data member path '{0}' on '{1}' is not included in generated accessors",
        AtomUIDiagnosticCategories.Aot,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor AotUnverifiableDataMemberPath = new(
        AtomUIDiagnosticIds.AotUnverifiableDataMemberPath,
        "AOT-sensitive data member path cannot be verified",
        "AOT-sensitive data member path '{0}' cannot be verified at compile time; use nameof(Type.Property) or an IDataMemberAccessorDescriptor",
        AtomUIDiagnosticCategories.Aot,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ScopedResourceHostInvalidClassShape = new(
        AtomUIDiagnosticIds.ScopedResourceHostInvalidClassShape,
        "Scoped resource host target must be a non-generic partial class",
        "Type '{0}' must be a non-generic partial top-level class to use [GenerateScopedResourceHost]",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ScopedResourceHostRequiresAvaloniaObject = new(
        AtomUIDiagnosticIds.ScopedResourceHostRequiresAvaloniaObject,
        "Scoped resource host target must inherit AvaloniaObject",
        "Type '{0}' must inherit AvaloniaObject to use [GenerateScopedResourceHost]",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ScopedResourceHostRejectsVisualTarget = new(
        AtomUIDiagnosticIds.ScopedResourceHostRejectsVisualTarget,
        "Scoped resource host target must be non-visual",
        "Type '{0}' inherits a visual type and must not use [GenerateScopedResourceHost]",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ScopedResourceHostRejectsExistingResourceHost = new(
        AtomUIDiagnosticIds.ScopedResourceHostRejectsExistingResourceHost,
        "Scoped resource host target already implements resource host interfaces",
        "Type '{0}' already implements IResourceHost or IThemeVariantHost and must not use [GenerateScopedResourceHost]",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenInvalidName = new(
        AtomUIDiagnosticIds.ControlTokenInvalidName,
        "Control design token name does not follow convention",
        "Control design token type '{0}' must end with 'Token' and have a non-empty Control name",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenInheritance = new(
        AtomUIDiagnosticIds.ControlTokenInheritance,
        "Control design token inheritance chain is invalid",
        "Control design token inheritance chain is invalid at '{0}': {1}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenMissingControl = new(
        AtomUIDiagnosticIds.ControlTokenMissingControl,
        "Control design token has no matching Control",
        "Control design token type '{0}' requires a matching public Control named '{1}'",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenAmbiguousControl = new(
        AtomUIDiagnosticIds.ControlTokenAmbiguousControl,
        "Control design token matches more than one Control",
        "Control design token type '{0}' matches more than one public Control named '{1}'",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenGlobalNameConflict = new(
        AtomUIDiagnosticIds.ControlTokenGlobalNameConflict,
        "Control Own Token conflicts with a Global Token",
        "Control '{0}' Own Token '{1}' conflicts with a Global Token",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenMustBeSealed = new(
        AtomUIDiagnosticIds.ControlTokenMustBeSealed,
        "Concrete Control design token must be sealed",
        "Concrete Control design token type '{0}' must be sealed",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenGenericLayer = new(
        AtomUIDiagnosticIds.ControlTokenGenericLayer,
        "Control design token layer must be non-generic",
        "Control design token type '{0}' must be non-generic",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenPropertyConflict = new(
        AtomUIDiagnosticIds.ControlTokenPropertyConflict,
        "Control design token member conflicts with an inherited token name",
        "Control Own Token name '{0}' conflicts across the inheritance chain ('{1}' and '{2}')",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenInvalidProperty = new(
        AtomUIDiagnosticIds.ControlTokenInvalidProperty,
        "Control design token property shape is invalid",
        "Property '{1}' on Control design token type '{0}' cannot define an Own Token: {2}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenInvalidCalculationChain = new(
        AtomUIDiagnosticIds.ControlTokenInvalidCalculationChain,
        "Control design token calculation chain is invalid",
        "CalculateTokenValues on Control design token type '{0}' has an invalid calculation chain: {1}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor AbstractControlTokenInvalidName = new(
        AtomUIDiagnosticIds.AbstractControlTokenInvalidName,
        "Abstract Control design token name does not follow convention",
        "The abstract Control design token type '{0}' must end with 'Token'",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor ControlTokenNestedType = new(
        AtomUIDiagnosticIds.ControlTokenNestedType,
        "Control design token type must be top-level",
        "Control design token type '{0}' must be declared as a top-level class",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);
    public static readonly DiagnosticDescriptor SemanticPartInvalidDeclaration = new(
        AtomUIDiagnosticIds.SemanticPartInvalidDeclaration,
        "Semantic Part declaration is invalid",
        "Semantic Part '{0}' on Control '{1}' is invalid: {2}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartDuplicateDeclaration = new(
        AtomUIDiagnosticIds.SemanticPartDuplicateDeclaration,
        "Semantic Part declaration is duplicated",
        "Semantic Part '{0}' on Control '{1}' duplicates {2} '{3}'",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartInvalidContractType = new(
        AtomUIDiagnosticIds.SemanticPartInvalidContractType,
        "Semantic Part ContractType is invalid",
        "Semantic Part '{0}' on Control '{1}' requires a public Avalonia StyledElement ContractType",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartInvalidThemeContract = new(
        AtomUIDiagnosticIds.SemanticPartInvalidThemeContract,
        "Semantic Part Theme contract is invalid",
        "Semantic Part '{0}' on Control '{1}' has an invalid strongly typed ControlTheme contract: {2}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartMissingSince = new(
        AtomUIDiagnosticIds.SemanticPartMissingSince,
        "Semantic Part should declare its introduction version",
        "Semantic Part '{0}' on Control '{1}' does not declare Since",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartInvalidSince = new(
        AtomUIDiagnosticIds.SemanticPartInvalidSince,
        "Semantic Part Since must be a three-part release version",
        "Semantic Part '{0}' on Control '{1}' declares Since '{2}', which is not a major.minor.patch release version such as '6.2.0'",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartTemplateCardinalityMismatch = new(
        AtomUIDiagnosticIds.SemanticPartTemplateCardinalityMismatch,
        "Semantic Part marker cardinality does not match the declaration",
        "Control '{0}' template '{1}' contains {2} marker(s) for Semantic Part '{3}', expected {4}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartTemplateContractTypeMismatch = new(
        AtomUIDiagnosticIds.SemanticPartTemplateContractTypeMismatch,
        "Semantic Part marker type does not satisfy ContractType",
        "Control '{0}' template '{1}' maps Semantic Part '{2}' to '{3}', which is not assignable to '{4}'",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartMissingControlTemplate = new(
        AtomUIDiagnosticIds.SemanticPartMissingControlTemplate,
        "Semantic Part Control has no analyzable ControlTemplate",
        "Control '{0}' declares static Semantic Parts but no applicable leaf ControlTemplate was found",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartTemplateNodeConflict = new(
        AtomUIDiagnosticIds.SemanticPartTemplateNodeConflict,
        "Semantic Part template node has conflicting roles",
        "Control '{0}' template '{1}' assigns one '{2}' node to multiple Semantic Parts: {3}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartTemplateMarkerNotStatic = new(
        AtomUIDiagnosticIds.SemanticPartTemplateMarkerNotStatic,
        "Semantic Part marker must be statically enabled",
        "Control '{0}' template '{1}' marker '{2}' on '{3}' must use a static true Classes.<name> value",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor SemanticPartStyleTypeConflict = new(
        AtomUIDiagnosticIds.SemanticPartStyleTypeConflict,
        "Semantic Part Style type identity conflicts",
        "Semantic Part '{0}' on Control '{1}' generates Style type '{2}', which conflicts with {3}",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor StartupHookMissingAttach = new(
        AtomUIDiagnosticIds.StartupHookMissingAttach,
        "Application overrides RegisterServices without the AtomUI startup hook",
        "Application '{0}' overrides RegisterServices without calling ApplicationStartup.Attach(this); call it after base.RegisterServices()",
        AtomUIDiagnosticCategories.Generator,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationInvalidLanguageData = new(
        AtomUIDiagnosticIds.LocalizationInvalidLanguageData,
        "Pinned language data record is invalid",
        "Language data '{0}' line {1} is invalid: {2}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationDuplicateLanguageData = new(
        AtomUIDiagnosticIds.LocalizationDuplicateLanguageData,
        "Pinned language data record is duplicated",
        "Language data '{0}' line {1} duplicates {2}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationInvalidCatalog = new(
        AtomUIDiagnosticIds.LocalizationInvalidCatalog,
        "Language Catalog declaration is invalid",
        "Language Catalog '{0}' is invalid: {1}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationInvalidCatalogUnit = new(
        AtomUIDiagnosticIds.LocalizationInvalidCatalogUnit,
        "Language Catalog unit is invalid",
        "Language Catalog unit '{0}' in '{1}' is invalid: {2}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationInvalidXliff = new(
        AtomUIDiagnosticIds.LocalizationInvalidXliff,
        "XLIFF language document is invalid",
        "XLIFF language document '{0}' is invalid: {1}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationCatalogXliffMismatch = new(
        AtomUIDiagnosticIds.LocalizationCatalogXliffMismatch,
        "XLIFF does not match its Language Catalog",
        "XLIFF language document '{0}' does not match Catalog '{1}': {2}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationInvalidTranslation = new(
        AtomUIDiagnosticIds.LocalizationInvalidTranslation,
        "XLIFF translation message is invalid",
        "Translation unit '{0}' for language '{1}' is invalid: {2}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    public static readonly DiagnosticDescriptor LocalizationInvalidApplicationHost = new(
        AtomUIDiagnosticIds.LocalizationInvalidApplicationHost,
        "Application cannot host generated localization bootstrap",
        "Application type '{0}' cannot host generated localization bootstrap: {1}",
        AtomUIDiagnosticCategories.Localization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);
}
#pragma warning restore RS2008
