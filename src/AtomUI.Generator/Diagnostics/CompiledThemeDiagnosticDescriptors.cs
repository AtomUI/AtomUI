using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Diagnostics;

#pragma warning disable RS2008
internal static partial class AtomUIDiagnosticDescriptors
{
    internal static readonly DiagnosticDescriptor CompiledThemeBindingFailed = new(
        AtomUIDiagnosticIds.CompiledThemeBindingFailed, "Compiled theme binding failed", "{0}", AtomUIDiagnosticCategories.Token, DiagnosticSeverity.Error, true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);

    // Design language rules of a compiled theme package. The runtime catalog checks the same rules over the merged
    // catalog (Warnings there, because one stray user definition must not stop an application).

    /// <summary>ATMTHM3103: two definitions of one design language in the package share an appearance and density.</summary>
    internal static readonly DiagnosticDescriptor DesignLanguageDuplicateVariant = new(
        AtomUIDiagnosticIds.DesignLanguageDuplicateVariant, "Duplicate design language variant",
        "Design language '{0}' declares more than one definition for the {1}/{2} variant in this package; '{3}' is rejected",
        AtomUIDiagnosticCategories.Token, DiagnosticSeverity.Error, true);

    /// <summary>ATMTHM3104: the definitions of one design language in the package mix Seed and Fixed brand color support.</summary>
    internal static readonly DiagnosticDescriptor DesignLanguageMixedBrandColor = new(
        AtomUIDiagnosticIds.DesignLanguageMixedBrandColor, "Mixed brand color support in a design language",
        "Design language '{0}' mixes Seed and Fixed brand color support in this package; '{1}' declares {2} and is rejected",
        AtomUIDiagnosticCategories.Token, DiagnosticSeverity.Error, true);

    /// <summary>
    /// ATMTHM3105: a design language definition's declared Density does not match its algorithm chain (Compact without
    /// the built-in Compact algorithm, or Normal with it).
    /// </summary>
    internal static readonly DiagnosticDescriptor DesignLanguageDensityMismatch = new(
        AtomUIDiagnosticIds.DesignLanguageDensityMismatch, "Density does not match the algorithm chain",
        "Theme '{0}' of design language '{1}' declares Density=\"{2}\" but its algorithm chain {3} the built-in Compact algorithm",
        AtomUIDiagnosticCategories.Token, DiagnosticSeverity.Warning, true);
}
#pragma warning restore RS2008
