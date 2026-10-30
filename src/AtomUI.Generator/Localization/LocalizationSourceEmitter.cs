using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Localization;

internal static class LocalizationSourceEmitter
{
    internal static void Emit(
        SourceProductionContext context,
        LocalizationGenerationResult result)
    {
        foreach (var diagnostic in result.Diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }

        if (result.Plan.Catalogs.IsEmpty)
        {
            return;
        }

        foreach (var catalog in result.Plan.Catalogs.Where(static catalog => catalog.OwnsCatalog))
        {
            LanguageCatalogSourceWriter.Write(context, result.AssemblyName, catalog);
        }

        LanguageModuleSourceWriter.Write(
            context,
            result.AssemblyName,
            result.Plan.Catalogs);
        ApplicationLanguageBootstrapWriter.Write(
            context,
            result.AssemblyName,
            result.Plan.Catalogs,
            result.Plan.ApplicationHost);
    }
}
