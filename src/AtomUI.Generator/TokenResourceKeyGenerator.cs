using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

[Generator]
public class TokenResourceKeyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        // Attribute providers cache metadata names only. Bind their symbols against this compilation
        // inside one transform, and detach every result before returning to the incremental graph.
        IncrementalValueProvider<System.Collections.Immutable.ImmutableArray<string>> Declarations(string attribute) =>
            initContext.SyntaxProvider.ForAttributeWithMetadataName(attribute, static (_, _) => true,
                static (context, _) => RegistrationType.MetadataNameOf((INamedTypeSymbol)context.TargetSymbol)).Collect();
        var declarations = Declarations(TargetMarkConstants.GlobalDesignTokenAttribute)
            .Combine(Declarations(TargetMarkConstants.ControlDesignTokenAttribute))
            .Combine(Declarations(TargetMarkConstants.ThemeAlgorithmAttribute))
            .Combine(Declarations(TargetMarkConstants.SemanticPartAttribute));
        var assets = initContext.AdditionalTextsProvider
            .Where(static text => text.Path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
            .Combine(initContext.AnalyzerConfigOptionsProvider)
            .Where(static input =>
            {
                input.Right.GetOptions(input.Left).TryGetValue("build_metadata.AdditionalFiles.Link", out var link);
                return ThemeAssetInfo.IsThemeAssetPath(string.IsNullOrWhiteSpace(link) ? input.Left.Path : link!);
            })
            .Select(static (input, token) =>
            {
                input.Right.GlobalOptions.TryGetValue("build_property.AtomUIThemeAssetProjectDirectory", out var directory);
                input.Right.GetOptions(input.Left).TryGetValue("build_metadata.AdditionalFiles.Link", out var link);
                return new ThemeAssetInput(input.Left.Path, input.Left.GetText(token)?.ToString() ?? "", directory, link);
            })
            .Collect();
        var inputs = initContext.CompilationProvider.Combine(initContext.AnalyzerConfigOptionsProvider).Combine(declarations).Combine(assets);
        var themeInputs = initContext.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(AtomUI.Theme.Build.CompiledThemeInput.Extension, StringComparison.OrdinalIgnoreCase))
            .Select(static (file, token) => (file.Path, Text: file.GetText(token)?.ToString() ?? "")).Collect();
        var resourceIdInputs = initContext.AdditionalTextsProvider
            .Where(static file => string.Equals(System.IO.Path.GetFileName(file.Path), "token-resource-ids.tsv", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, token) => (file.Path, Text: file.GetText(token)?.ToString() ?? "")).Collect();
        var output = inputs.Combine(themeInputs).Combine(StartupReferenceReader.Create(initContext))
            .Combine(Localization.LocalizationCompilationProvider.Create(initContext))
            .Combine(resourceIdInputs)
            .Select(static (combined, cancellationToken) =>
        {
            var state = combined.Left;
            var input = state.Left.Left.Left;
            var themeInputs = state.Left.Left.Right;
            var references = state.Left.Right;
            var localization = state.Right;
            var compilation = input.Left.Left.Left;
            var options = input.Left.Left.Right;
            var declarations = input.Left.Right;
            var documents = new Dictionary<string, System.Xml.Linq.XElement>(StringComparer.Ordinal);
            var assets = input.Right.Select(asset =>
            {
                var info = ThemeAssetInfo.Create(asset.AsAdditionalText(), asset.Directory, asset.Link,
                    cancellationToken, out var root);
                if (root is not null && !documents.ContainsKey(info.Path))
                {
                    documents.Add(info.Path, root);
                }

                return info;
            }).ToArray();
            var context = new GenerationOutput();
            var resourceIds = TokenResourceOrdinals.Read(combined.Right, context.ReportDiagnostic, cancellationToken);
            if (compilation.GetTypeByMetadataName("AtomUI.Theme.Resources.TokenResourceExtension`1") is null)
            {
                return context.Freeze();
            }

            var assemblyName = compilation.AssemblyName ?? "AtomUI";
            var packageId = ThemeGeneratorOptions.GetPackageId(options, assemblyName);
            var catalog = ThemeGeneratorOptions.GetControlCatalog(options, assemblyName);
            var info = new TokenInfo();
            foreach (var name in declarations.Left.Left.Left.Distinct(StringComparer.Ordinal))
            {
                var symbol = compilation.GetTypeByMetadataName(name);
                if (symbol is null)
                {
                    continue;
                }

                foreach (var reference in symbol.DeclaringSyntaxReferences)
                {
                    var walker = new TokenPropertyWalker(compilation.GetSemanticModel(reference.SyntaxTree));
                    walker.Visit(reference.GetSyntax(cancellationToken));
                    foreach (var token in walker.TokenNames)
                    {
                        info.Tokens.Add(new(token, walker.TokenResourceCatalog!));
                    }

                    info.SchemaTokens.UnionWith(walker.SchemaTokens);
                }
            }
            var ownTokens = new List<ControlTokenInfo>();
            foreach (var name in declarations.Left.Left.Right.Distinct(StringComparer.Ordinal))
            {
                var symbol = compilation.GetTypeByMetadataName(name);
                var reference = symbol?.DeclaringSyntaxReferences.FirstOrDefault();
                if (reference is null)
                {
                    continue;
                }

                var walker = new ControlTokenPropertyWalker(compilation.GetSemanticModel(reference.SyntaxTree), cancellationToken);
                walker.Visit(reference.GetSyntax(cancellationToken));
                ownTokens.Add(walker.ControlTokenInfo);
            }
            var algorithms = declarations.Left.Right.Distinct(StringComparer.Ordinal).Select(compilation.GetTypeByMetadataName)
                .Where(static symbol => symbol is not null).Select(static symbol => ThemeAlgorithmInfo.Create(symbol!)).Where(static algorithm => algorithm is not null).Select(static algorithm => algorithm!).ToArray();
            var semanticDeclarations = declarations.Right.Distinct(StringComparer.Ordinal).Select(compilation.GetTypeByMetadataName)
                .Where(static symbol => symbol is not null).Select(symbol => SemanticControlDeclaration.Create(symbol!, cancellationToken)).ToArray();
            var globalNames = compilation.GetTypeByMetadataName("AtomUI.Theme.Resources.SharedTokenKind")?.GetMembers().OfType<IFieldSymbol>()
                .Where(static field => field.HasConstantValue && field.Name != "value__")
                .OrderBy(static field => field.Name, StringComparer.Ordinal)
                .Select(static field => field.Name).ToArray() ?? Array.Empty<string>();
            if (info.SchemaTokens.Count > 0)
            {
                globalNames = info.SchemaTokens.OrderBy(t => t.Name, StringComparer.Ordinal).Select(t => t.Name).ToArray();
            }

            info.AvailableGlobalTokenNames.UnionWith(globalNames);
            info.AvailableGlobalTokenNames.UnionWith(info.Tokens.Select(t => t.Name));
            var controlInfos = ControlThemeModelBuilder.Build(compilation, ownTokens,
                info.AvailableGlobalTokenNames, context.ReportDiagnostic).ToList();
            var hasSemanticRuntime = compilation.GetTypeByMetadataName("AtomUI.Theme.Schema.ControlSemanticDescriptor") is not null;
            var resourceFacts = new RegistrationModelBuilder(compilation, packageId, catalog, context.ReportDiagnostic);
            resourceFacts.ReadResources(assets, documents, semanticDeclarations);
            var semanticControls = hasSemanticRuntime ? SemanticPartModelBuilder.Build(compilation, semanticDeclarations, assets, resourceFacts, context.ReportDiagnostic) : Array.Empty<SemanticControlDeclaration>();
            RegistrationPackage? ownRegistration = null;
            if (compilation.GetTypeByMetadataName("AtomUI.Registration.ControlPackageMarkerAttribute") is not null)
            {
                controlInfos.RemoveAll(static control => !control.HasOwnToken);
                var package = resourceFacts.Build(controlInfos, semanticControls, globalNames);
                ownRegistration = package;
                if (package.SupportsStartupManifest || package.Controls.Count != 0 || package.Assets.Count != 0)
                {
                    if (RegistrationFrameworkResolver.TryResolve(compilation, options.GlobalOptions, out var framework, out var error))
                    {
                        TypeMapRegistrationWriter.Write(context, package,
                            supportsTypeMap: framework == AtomUI.Build.Tasks.Registration.RegistrationFrameworkFamily.OfficialTypeMap);
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Diagnostics.AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, Location.None, error));
                    }
                }
            }
            ThemeBuildContracts? contracts = null;
            AtomUI.Theme.Definitions.Compiled.CompiledThemeManifest? ownTheme = null;
            if (ThemeBuildContracts.HasAbi(compilation))
            {
                contracts = ThemeBuildContracts.Build(compilation, catalog, info.SchemaTokens, controlInfos, context);
                contracts.Write(context);
                ownTheme = CompiledThemePackageWriter.Write(context, compilation, contracts, themeInputs, assemblyName, packageId);
            }
            else if (themeInputs.Length != 0)
            {
                CompiledThemePackageWriter.Error(context, null, 0, 0, "Compiled theme inputs require generated ABI v1.");
            }
            info.ControlThemeInfos.AddRange(controlInfos);
            ControlTokenDefaultsWriter.Write(context, controlInfos);
            ControlTokenAliasesWriter.Write(context, controlInfos);
            ControlTokenGlobalValuesWriter.Write(context, compilation, info.SchemaTokens);
            StartupImageWriter.Write(context, compilation, ownRegistration, references, localization, contracts, ownTheme);
            ThemeControlCatalogMetadataWriter.Write(context, catalog);
            if (info.SchemaTokens.Count > 0 || controlInfos.Count > 0 || algorithms.Length > 0)
            {
                new GeneratedThemeSchemaWriter(context, assemblyName, catalog, info.SchemaTokens, controlInfos, algorithms, ThemeBuildContracts.HasAbi(compilation)).Write();
            }

            if (hasSemanticRuntime)
            {
                new SemanticPartManifestWriter(context, assemblyName, catalog, semanticControls,
                !SemanticPartStyleContract.HasCanonicalXmlnsDefinition(compilation.Assembly)).Write();
            }

            new ResourceKeyClassWriter(context, info, catalog, compilation.AssemblyName ?? "", resourceIds).Write();
            return context.Freeze();
        }).WithTrackingName("ControlRegistrationOutput");
        initContext.RegisterImplementationSourceOutput(output, static (context, output) => output.Write(context));
    }
}
