using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using AtomUI.Generator.Localization.Xliff;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Localization.Catalog;

internal static class CatalogCompilationPlanner
{
    internal static CatalogPlanningResult Plan(
        CatalogSymbolIndex index,
        ImmutableArray<LanguageFileInput> inputs)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var dormantInputs = ImmutableArray.CreateBuilder<LanguageInputResolution>();
        var inputsByCatalog = index.Entries.ToDictionary(
            static entry => entry.Catalog.Key,
            static _ => new List<LanguageInputResolution>());

        foreach (var input in OrderInputs(inputs))
        {
            var catalogKey = new CatalogKey(input.ModuleId, input.Document.File.Id);
            if (!index.TryGet(catalogKey, out var catalogEntry))
            {
                if (input.SourceKind == LanguageFileSourceKind.StaticLanguagePack &&
                    !index.ContainsModule(input.ModuleId))
                {
                    dormantInputs.Add(new LanguageInputResolution(
                        input,
                        LanguageInputActivationState.Dormant,
                        catalog: null));
                    continue;
                }

                diagnostics.Add(Mismatch(
                    input,
                    catalogKey,
                    input.SourceKind == LanguageFileSourceKind.StaticLanguagePack
                        ? $"referenced Catalog '{catalogKey.FileId}' could not be found"
                        : $"file id '{catalogKey.FileId}' does not match a Catalog in module '{catalogKey.ModuleId}'"));
                continue;
            }

            inputsByCatalog[catalogKey].Add(new LanguageInputResolution(
                input,
                LanguageInputActivationState.Active,
                catalogEntry.Catalog));
        }

        var workItems = index.Entries
                             .Select(entry => new CatalogCompilationWorkItem(
                                 entry,
                                 inputsByCatalog[entry.Catalog.Key]
                                     .OrderBy(static resolution => resolution.Input.ModuleId, StringComparer.Ordinal)
                                     .ThenBy(static resolution => resolution.Input.Document.File.Id, StringComparer.Ordinal)
                                     .ThenBy(static resolution => GetLanguage(resolution.Input), StringComparer.Ordinal)
                                     .ThenBy(static resolution => resolution.Input.SourceKind)
                                     .ThenBy(static resolution => resolution.Input.SourceIdentity, StringComparer.Ordinal)
                                     .ThenBy(static resolution => resolution.Input.Path, StringComparer.Ordinal)
                                     .ToImmutableArray()))
                             .OrderBy(static item => item.Catalog.CatalogId, StringComparer.Ordinal)
                             .ToImmutableArray();

        return new CatalogPlanningResult(
            workItems,
            dormantInputs.OrderBy(static resolution => resolution.Input.ModuleId, StringComparer.Ordinal)
                         .ThenBy(static resolution => resolution.Input.Document.File.Id, StringComparer.Ordinal)
                         .ThenBy(static resolution => GetLanguage(resolution.Input), StringComparer.Ordinal)
                         .ThenBy(static resolution => resolution.Input.SourceKind)
                         .ThenBy(static resolution => resolution.Input.SourceIdentity, StringComparer.Ordinal)
                         .ThenBy(static resolution => resolution.Input.Path, StringComparer.Ordinal)
                         .ToImmutableArray(),
            diagnostics.OrderBy(static diagnostic => diagnostic.Location.GetLineSpan().Path, StringComparer.Ordinal)
                       .ThenBy(static diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line)
                       .ThenBy(static diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Character)
                       .ThenBy(static diagnostic => diagnostic.Id, StringComparer.Ordinal)
                       .ThenBy(static diagnostic => diagnostic.GetMessage(), StringComparer.Ordinal)
                       .ToImmutableArray());
    }

    private static IOrderedEnumerable<LanguageFileInput> OrderInputs(
        ImmutableArray<LanguageFileInput> inputs)
    {
        return inputs.OrderBy(static input => input.ModuleId, StringComparer.Ordinal)
                     .ThenBy(static input => input.Document.File.Id, StringComparer.Ordinal)
                     .ThenBy(static input => GetLanguage(input), StringComparer.Ordinal)
                     .ThenBy(static input => input.SourceKind)
                     .ThenBy(static input => input.SourceIdentity, StringComparer.Ordinal)
                     .ThenBy(static input => input.Path, StringComparer.Ordinal);
    }

    private static string GetLanguage(LanguageFileInput input)
    {
        return input.Document.TargetLanguage ?? input.Document.SourceLanguage;
    }

    private static Diagnostic Mismatch(
        LanguageFileInput input,
        CatalogKey catalogKey,
        string reason)
    {
        return Diagnostic.Create(
            AtomUIDiagnosticDescriptors.LocalizationCatalogXliffMismatch,
            LocalizationDiagnosticFactory.CreateLocation(input.Path, input.Text, 1, 1),
            input.Path,
            catalogKey.ToString(),
            reason);
    }
}
