using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using AtomUI.Generator.Localization.Xliff;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Localization.Catalog;

internal sealed class CatalogSymbolIndex
{
    private readonly Dictionary<CatalogKey, CatalogSymbolEntry> _catalogs;
    private readonly HashSet<string> _moduleIds;

    private CatalogSymbolIndex(
        Dictionary<CatalogKey, CatalogSymbolEntry> catalogs,
        HashSet<string> moduleIds)
    {
        _catalogs = catalogs;
        _moduleIds = moduleIds;
    }

    internal static CatalogSymbolIndexResult Create(
        ImmutableArray<LanguageCatalogInfo> ownedCatalogs,
        ImmutableArray<LanguageFileInput> languageInputs,
        Compilation? compilation)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var catalogs = new Dictionary<CatalogKey, CatalogSymbolEntry>();
        var moduleIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var catalog in ownedCatalogs.OrderBy(static item => item.CatalogId, StringComparer.Ordinal))
        {
            moduleIds.Add(catalog.ModuleId);
            AddCatalog(
                catalogs,
                diagnostics,
                new CatalogSymbolEntry(catalog, ownsCatalog: true),
                catalog.Location);
        }

        if (compilation is not null)
        {
            foreach (var moduleId in languageInputs
                         .Select(static input => input.ModuleId)
                         .Distinct(StringComparer.Ordinal)
                         .OrderBy(static moduleId => moduleId, StringComparer.Ordinal))
            {
                if (ReferencedLanguageCatalogResolver.ContainsModule(compilation, moduleId))
                {
                    moduleIds.Add(moduleId);
                }
            }

            var resolvedKeys = new HashSet<CatalogKey>();
            foreach (var input in languageInputs
                         .OrderBy(static item => item.ModuleId, StringComparer.Ordinal)
                         .ThenBy(static item => item.Document.File.Id, StringComparer.Ordinal)
                         .ThenBy(static item => item.Path, StringComparer.Ordinal))
            {
                var key = new CatalogKey(input.ModuleId, input.Document.File.Id);
                if (catalogs.ContainsKey(key))
                {
                    continue;
                }

                if (!resolvedKeys.Add(key))
                {
                    continue;
                }

                if (!ReferencedLanguageCatalogResolver.TryResolve(
                        compilation,
                        input,
                        out var referencedCatalog,
                        out var resolutionError,
                        out var failureKind))
                {
                    if (failureKind == ReferencedLanguageCatalogResolutionFailureKind.CatalogNotFound)
                    {
                        continue;
                    }

                    diagnostics.Add(Mismatch(input, key, resolutionError));
                    continue;
                }

                moduleIds.Add(referencedCatalog!.ModuleId);
                AddCatalog(
                    catalogs,
                    diagnostics,
                    new CatalogSymbolEntry(referencedCatalog!, ownsCatalog: false),
                    referencedCatalog!.Location);
            }
        }

        return new CatalogSymbolIndexResult(
            new CatalogSymbolIndex(catalogs, moduleIds),
            diagnostics.OrderBy(static diagnostic => diagnostic.Location.GetLineSpan().Path, StringComparer.Ordinal)
                       .ThenBy(static diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line)
                       .ThenBy(static diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Character)
                       .ThenBy(static diagnostic => diagnostic.Id, StringComparer.Ordinal)
                       .ThenBy(static diagnostic => diagnostic.GetMessage(), StringComparer.Ordinal)
                       .ToImmutableArray());
    }

    internal bool TryGet(CatalogKey key, out CatalogSymbolEntry entry)
    {
        return _catalogs.TryGetValue(key, out entry!);
    }

    internal bool ContainsModule(string moduleId)
    {
        return _moduleIds.Contains(moduleId);
    }

    internal ImmutableArray<CatalogSymbolEntry> Entries
    {
        get
        {
            return _catalogs.Values
                            .OrderBy(static entry => entry.Catalog.CatalogId, StringComparer.Ordinal)
                            .ToImmutableArray();
        }
    }

    private static void AddCatalog(
        Dictionary<CatalogKey, CatalogSymbolEntry> catalogs,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        CatalogSymbolEntry entry,
        Location location)
    {
        if (!catalogs.TryGetValue(entry.Catalog.Key, out var existing))
        {
            catalogs.Add(entry.Catalog.Key, entry);
            return;
        }

        diagnostics.Add(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.LocalizationCatalogXliffMismatch,
            location,
            entry.Catalog.MetadataName,
            entry.Catalog.CatalogId,
            $"Catalog identity conflicts with '{existing.Catalog.TypeName}'"));
    }

    private static Diagnostic Mismatch(
        LanguageFileInput input,
        CatalogKey key,
        string reason)
    {
        return Diagnostic.Create(
            AtomUIDiagnosticDescriptors.LocalizationCatalogXliffMismatch,
            LocalizationDiagnosticFactory.CreateLocation(input.Path, input.Text, 1, 1),
            input.Path,
            key.ToString(),
            reason);
    }
}

internal sealed class CatalogSymbolEntry
{
    internal CatalogSymbolEntry(LanguageCatalogInfo catalog, bool ownsCatalog)
    {
        Catalog = catalog;
        OwnsCatalog = ownsCatalog;
    }

    internal LanguageCatalogInfo Catalog { get; }

    internal bool OwnsCatalog { get; }
}

internal sealed class CatalogSymbolIndexResult
{
    internal CatalogSymbolIndexResult(
        CatalogSymbolIndex index,
        ImmutableArray<Diagnostic> diagnostics)
    {
        Index = index;
        Diagnostics = diagnostics;
    }

    internal CatalogSymbolIndex Index { get; }

    internal ImmutableArray<Diagnostic> Diagnostics { get; }
}
