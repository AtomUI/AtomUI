using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Diagnostics;

#pragma warning disable RS2008 // Matches the central AtomUI diagnostic registry until release tracking is enabled.
internal static partial class AtomUIDiagnosticDescriptors
{
    internal static readonly DiagnosticDescriptor RegistrationAmbiguousThemeExport = CreateRegistrationDiagnostic(AtomUIDiagnosticIds.RegistrationAmbiguousThemeExport, "Theme export is ambiguous", "Theme export is ambiguous: {0}");
    internal static readonly DiagnosticDescriptor RegistrationInvalidTokenOwner = CreateRegistrationDiagnostic(AtomUIDiagnosticIds.RegistrationInvalidTokenOwner, "Token owner cannot be resolved", "Token owner cannot be resolved to an accessible control: {0}");
    internal static readonly DiagnosticDescriptor RegistrationInaccessibleType = CreateRegistrationDiagnostic(AtomUIDiagnosticIds.RegistrationInaccessibleType, "Registration type is inaccessible", "Registration type is inaccessible or open: {0}");
    internal static readonly DiagnosticDescriptor RegistrationInvalidResourceDependency = CreateRegistrationDiagnostic(AtomUIDiagnosticIds.RegistrationInvalidResourceDependency, "Resource dependency is invalid", "Resource dependency is invalid: {0}");
    internal static readonly DiagnosticDescriptor RegistrationIdentityConflict = CreateRegistrationDiagnostic(AtomUIDiagnosticIds.RegistrationIdentityConflict, "Registration identity conflicts", "Registration identity is conflicting or invalid: {0}");
    internal static readonly DiagnosticDescriptor RegistrationUnsupportedBackend = CreateRegistrationDiagnostic(AtomUIDiagnosticIds.RegistrationUnsupportedBackend, "Registration ABI or platform is unsupported", "Registration ABI or platform is unsupported: {0}");
    private static DiagnosticDescriptor CreateRegistrationDiagnostic(string id, string title, string message) => new(id, title, message, AtomUIDiagnosticCategories.Registration, DiagnosticSeverity.Error, true, customTags: [WellKnownDiagnosticTags.Telemetry]);
}
