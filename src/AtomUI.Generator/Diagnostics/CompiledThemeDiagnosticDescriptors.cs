using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Diagnostics;

#pragma warning disable RS2008
internal static partial class AtomUIDiagnosticDescriptors
{
    internal static readonly DiagnosticDescriptor CompiledThemeBindingFailed = new(
        AtomUIDiagnosticIds.CompiledThemeBindingFailed, "Compiled theme binding failed", "{0}", AtomUIDiagnosticCategories.Token, DiagnosticSeverity.Error, true,
        customTags: [WellKnownDiagnosticTags.Telemetry]);
}
#pragma warning restore RS2008
