using AtomUI.Generator.Localization.Catalog;
using AtomUI.Generator.Localization.Xliff;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.Localization;

internal static class LocalizationCompilationProvider
{
    internal static IncrementalValueProvider<LocalizationGenerationResult> Create(
        IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
            "AtomUI.Localization.LanguageCatalogAttribute",
            static (_, _) => true,
            static (attributeContext, _) => new LanguageCatalogTarget(
                (INamedTypeSymbol)attributeContext.TargetSymbol,
                attributeContext.Attributes[0]));
        var catalogs = targets.Combine(context.AnalyzerConfigOptionsProvider)
                              .Select(static (input, cancellationToken) =>
                                  LanguageCatalogSymbolParser.Parse(
                                      input.Left,
                                      LanguageGeneratorOptions.GetModuleId(
                                          input.Right,
                                          input.Left.Symbol.ContainingAssembly.Name),
                                      cancellationToken))
                              .WithTrackingName("LocalizationCatalogs");
        var languageFiles = context.AdditionalTextsProvider
                                   .Combine(context.AnalyzerConfigOptionsProvider)
                                   .Where(static input => LanguageGeneratorOptions.IsLanguageFile(
                                       input.Left,
                                       input.Right))
                                   .Select(static (input, cancellationToken) =>
                                       LocalizationPipeline.ParseLanguageFile(
                                           input.Left,
                                           input.Right,
                                           cancellationToken))
                                   .WithTrackingName("LocalizationLanguageFiles");
        var applicationHosts = context.SyntaxProvider.CreateSyntaxProvider(
                                          static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                                          static (syntaxContext, cancellationToken) =>
                                              ApplicationLanguageHostInfo.TryCreate(
                                                  syntaxContext,
                                                  cancellationToken))
                                      .Where(static host => host is not null)
                                      .Select(static (host, _) => host!)
                                      .WithTrackingName("LocalizationApplicationHosts");
        return context.CompilationProvider
                                       .Combine(catalogs.Collect())
                                       .Combine(languageFiles.Collect())
                                       .Combine(applicationHosts.Collect())
                                       .Combine(context.AnalyzerConfigOptionsProvider)
                                       .Select(static (input, _) =>
                                       {
                                           var generationInputs = input.Left;
                                           var compilation = generationInputs.Left.Left.Left;
                                           var assemblyName = compilation.AssemblyName;
                                           return LocalizationPipeline.Compile(
                                               assemblyName,
                                               generationInputs.Left.Left.Right,
                                               generationInputs.Left.Right,
                                               compilation,
                                               generationInputs.Right);
                                       })
                                       .WithTrackingName("LocalizationCompilation");
    }
}
