using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Diagnostics;

#pragma warning disable RS2008
internal static partial class AtomUIDiagnosticDescriptors
{
    internal static readonly DiagnosticDescriptor StartupManifestInvalid = Startup(
        AtomUIDiagnosticIds.StartupManifestInvalid, "Invalid startup manifest", "Startup manifest: {0}. Rebuild the named package with the current AtomUI generator.");
    internal static readonly DiagnosticDescriptor StartupPackageConflict = Startup(
        AtomUIDiagnosticIds.StartupPackageConflict, "Startup package conflict", "Startup package: {0}. Correct the conflicting package, owner or asset declaration.");
    internal static readonly DiagnosticDescriptor StartupThemeContractMismatch = Startup(
        AtomUIDiagnosticIds.StartupThemeContractMismatch, "Startup theme contract mismatch", "Startup theme: {0}. Rebuild the theme against the application's Token and owner contracts.");
    internal static readonly DiagnosticDescriptor StartupLanguageContractMismatch = Startup(
        AtomUIDiagnosticIds.StartupLanguageContractMismatch, "Startup language contract mismatch", "Startup language: {0}. Supply compatible translations for every declared supported language.");
    internal static readonly DiagnosticDescriptor StartupImageManualHost = new(
        AtomUIDiagnosticIds.StartupImageManualHost, "Startup image requires manual registration",
        "Application '{0}' cannot receive a generated startup image host. Make it a top-level non-generic partial class, or call UseStartupImage(GeneratedApplicationStartupImage.Instance) from its AtomUI builder configuration.",
        AtomUIDiagnosticCategories.Registration, DiagnosticSeverity.Warning, true);

    private static DiagnosticDescriptor Startup(string id, string title, string message) => new(
        id, title, message, AtomUIDiagnosticCategories.Registration, DiagnosticSeverity.Error, true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);
}
#pragma warning restore RS2008
