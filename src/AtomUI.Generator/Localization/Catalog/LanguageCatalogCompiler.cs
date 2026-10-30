using System.Collections.Immutable;
using AtomUI.Generator.Localization.Xliff;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Localization.Catalog;

internal static class LanguageCatalogCompiler
{
    internal static LanguageCatalogCompilationResult Compile(
        ImmutableArray<LanguageCatalogParseResult> catalogResults,
        ImmutableArray<LanguageFileInputResult> fileResults)
    {
        return Compile(catalogResults, fileResults, compilation: null);
    }

    internal static LanguageCatalogCompilationResult Compile(
        ImmutableArray<LanguageCatalogParseResult> catalogResults,
        ImmutableArray<LanguageFileInputResult> fileResults,
        Compilation? compilation)
    {
        var result = LocalizationPipeline.Compile(
            assemblyName: null,
            catalogResults,
            fileResults,
            compilation,
            ImmutableArray<ApplicationLanguageHostInfo>.Empty);
        return new LanguageCatalogCompilationResult(
            result.Plan.Catalogs,
            result.Diagnostics);
    }
}
