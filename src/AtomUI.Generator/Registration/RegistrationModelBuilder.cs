using AtomUI.Generator.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using AtomUI.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

internal sealed class RegistrationModelBuilder
{
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
    private readonly Compilation _compilation;
    private readonly string _packageId;
    private readonly string _catalog;
    private readonly Action<Diagnostic> _report;
    private readonly INamedTypeSymbol[] _types;
    private readonly Dictionary<string, PlatformAvailability> _availability = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PlatformAvailability> _resourceAvailability = new(StringComparer.Ordinal);
    private PlatformAvailability? _assemblyAvailability;
    private readonly Dictionary<string, INamedTypeSymbol?> _resolved = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (ThemeAssetInfo Input, XElement Root)> _parsed = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Owner, string Property), List<SemanticThemeAssignment>> _semanticAssignments = new();
    private readonly Dictionary<string, XElement> _themeClasses = new(StringComparer.Ordinal);
    private readonly Dictionary<XElement, string> _documentPaths = new();
    private readonly Dictionary<string, List<(IAssemblySymbol Assembly, string Namespace)>> _xmlNamespaces = new(StringComparer.Ordinal);

    internal RegistrationModelBuilder(Compilation compilation, string packageId, string catalog, Action<Diagnostic> report)
    {
        _compilation = compilation; _packageId = packageId; _catalog = catalog; _report = report;
        _types = Types(compilation.Assembly.GlobalNamespace).ToArray();
        foreach (var assembly in new[] { compilation.Assembly }.Concat(compilation.SourceModule.ReferencedAssemblySymbols))
        {
            foreach (var attribute in assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != "Avalonia.Metadata.XmlnsDefinitionAttribute" ||
                attribute.ConstructorArguments.Length < 2 || attribute.ConstructorArguments[0].Value is not string xml ||
                attribute.ConstructorArguments[1].Value is not string ns)
                {
                    continue;
                }

                if (!_xmlNamespaces.TryGetValue(xml, out var mappings))
                {
                    _xmlNamespaces.Add(xml, mappings = new());
                }

                mappings.Add((assembly, ns));
        }
        }
    }

    // All symbol/XML work is local to this generation transform. Both semantic validation and
    // registration consume these facts; no document is reparsed for an individual semantic owner.
    internal void ReadResources(IReadOnlyList<ThemeAssetInfo> inputs, IReadOnlyDictionary<string, XElement> documents,
        IReadOnlyList<SemanticControlDeclaration> semantics)
    {
        foreach (var input in inputs.OrderBy(a => a.AssetPath, StringComparer.Ordinal))
        {
            if (!documents.TryGetValue(input.Path, out var root))
            {
                continue;
            }

            if (_parsed.ContainsKey(input.AssetPath))
            {
                Error(AtomUIDiagnosticDescriptors.RegistrationIdentityConflict, input, root, input.AssetPath);
            }
            else
            {
                _parsed.Add(input.AssetPath, (input, root));
            }

            _documentPaths[root] = input.AssetPath;
            if (input.ControlThemeClassName is { } name)
            {
                _themeClasses[name] = root;
            }

            ResourceAvailability(input, root);
        }
        ReadSemanticAssignments(new HashSet<string>(semantics.SelectMany(s => s.Parts)
            .Where(p => p.ThemePropertyName is not null).Select(p => p.ThemePropertyName!), StringComparer.Ordinal));
    }

    internal RegistrationPackage Build(List<ControlThemeInfo> tokens, IReadOnlyList<SemanticControlDeclaration> semantics,
        IReadOnlyList<string> globalNames)
    {
        var parsed = _parsed;
        ValidateIncludes(parsed);
        var exports = new Dictionary<string, List<RegistrationExport>>(StringComparer.Ordinal);
        var symbols = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);
        foreach (var pair in parsed)
        {
            var input = pair.Value.Input;
            var root = pair.Value.Root;
            var elements = input.IsResourceDictionary ? root.Elements() : input.IsDefaultTypedControlTheme ? new[] { root } : Enumerable.Empty<XElement>();
            var assetExports = new List<RegistrationExport>();
            foreach (var element in elements)
            {
                if (!IsThemeElement(element))
                {
                    continue;
                }

                var targetText = element.Attribute("TargetType")?.Value;
                if (targetText is null)
                {
                    continue;
                }

                var target = Resolve(element, targetText);
                if (target is null)
                {
                    Error(AtomUIDiagnosticDescriptors.RegistrationAmbiguousThemeExport, input, element, targetText); continue;
                }
                if (!IsStyledElement(target) || !IsAccessible(target))
                {
                    Error(AtomUIDiagnosticDescriptors.RegistrationInaccessibleType, input, element, target.ToDisplayString()); continue;
                }
                var key = element.Attribute(Xaml + "Key")?.Value;
                RegistrationType? keyType = null;
                if (key is null && ReferenceEquals(element, root))
                {
                    keyType = RegistrationType.From(target);
                }
                else if (key is null) { Error(AtomUIDiagnosticDescriptors.RegistrationAmbiguousThemeExport, input, element, "missing x:Key"); continue; }
                else if (key.StartsWith("{x:Type", StringComparison.Ordinal))
                {
                    var keySymbol = Resolve(element, key);
                    if (keySymbol is null || !IsAccessible(keySymbol)) { Error(AtomUIDiagnosticDescriptors.RegistrationInaccessibleType, input, element, key); continue; }
                    keyType = RegistrationType.From(keySymbol); key = null;
                }
                else if (key.StartsWith("{", StringComparison.Ordinal))
                {
                    Error(AtomUIDiagnosticDescriptors.RegistrationAmbiguousThemeExport, input, element, "nonconstant key " + key); continue;
                }
                var fact = RegistrationType.From(target);
                symbols[fact.AssemblyQualifiedName] = target;
                assetExports.Add(new(fact, keyType, key));
                if (IsControl(target) && target.DeclaredAccessibility == Accessibility.Public && SymbolEqualityComparer.Default.Equals(target.ContainingAssembly, _compilation.Assembly) && !tokens.Any(t => t.ControlTypeName == fact.Name))
                {
                    tokens.Add(new ControlThemeInfo(target.ContainingNamespace.ToDisplayString(), target.Name,
                        fact.MetadataName, fact.Name, true, null));
                }
            }
            if (assetExports.Count > 0)
            {
                exports.Add(pair.Key, assetExports);
            }
        }
        ValidateStaticResources(parsed, exports, symbols);
        var assets = new List<RegistrationAsset>();
        foreach (var pair in parsed)
        {
            var input = pair.Value.Input;
            if (!exports.TryGetValue(pair.Key, out var assetExports))
            {
                assetExports = new();
            }

            if (assetExports.Count == 0)
            {
                continue;
            }

            if (!input.HasGeneratedResourceWrapper)
            {
                continue;
            }

            var owners = new List<RegistrationOwner>();
            foreach (var reference in TokenReferences(pair.Value.Root))
            {
                var family = reference.Family;
                var matches = tokens.Where(t => t.HasDescriptor && t.ControlName == family &&
                    NamespaceMatches(reference.Namespace, t.ControlNamespace + ".DesignTokens")).ToArray();
                if (matches.Length == 1)
                {
                    var symbol = MetadataType(matches[0].ControlMetadataName);
                    if (symbol is not null)
                    {
                        var ownerType = RegistrationType.From(symbol);
                        symbols[ownerType.AssemblyQualifiedName] = symbol;
                        owners.Add(new(ownerType, _catalog, family));
                    }
                    continue;
                }
                var extension = matches.Length == 0 ? ResolveXml(reference.Namespace, family + "TokenResourceExtension") : null;
                var owner = extension is null ? null : ResolveTokenOwner(extension, family);
                if (owner is null) { Error(AtomUIDiagnosticDescriptors.RegistrationInvalidTokenOwner, input, reference.Element, reference.Namespace + ":" + family); continue; }
                var foreignOwnerType = RegistrationType.From(owner);
                symbols[foreignOwnerType.AssemblyQualifiedName] = owner;
                owners.Add(new(foreignOwnerType, ThemeGeneratorOptions.GetReferencedControlCatalog(extension!.ContainingAssembly), family));
            }
            var bindings = new List<RegistrationBinding>();
            foreach (var semantic in semantics)
            {
                foreach (var part in semantic.Parts.Where(p => p.ThemePropertyName is not null))
                {
                    foreach (var assignment in SemanticAssignments(semantic.ControlType, part.ThemePropertyName!))
            {
                if (assignment.AssetPath != input.AssetPath || assignment.Target is not { } target)
                        {
                            continue;
                        }

                        bindings.Add(new(new(RegistrationType.From(semantic.ControlType), _catalog, semantic.ControlType.Name), part.ThemePropertyName!, RegistrationType.From(target)));
            }
                }
            }

            var orderedExports = assetExports.OrderBy(e => e.Target.AssemblyQualifiedName, StringComparer.Ordinal)
                .ThenBy(e => e.KeyType is null ? 1 : 0).ThenBy(e => e.KeyType?.AssemblyQualifiedName ?? e.Key, StringComparer.Ordinal).ToArray();
            if (orderedExports.GroupBy(e => e.KeyType?.AssemblyQualifiedName ?? "string:" + e.Key, StringComparer.Ordinal).Any(g => g.Count() > 1))
            {
                Error(AtomUIDiagnosticDescriptors.RegistrationAmbiguousThemeExport, input, pair.Value.Root, "duplicate exported resource key");
            }

            var orderedOwners = owners.Distinct().OrderBy(o => o.Catalog, StringComparer.Ordinal).ThenBy(o => o.Id, StringComparer.Ordinal).ToArray();
            var orderedBindings = bindings.Distinct().OrderBy(b => b.Owner.Catalog, StringComparer.Ordinal).ThenBy(b => b.Owner.Id, StringComparer.Ordinal)
                .ThenBy(b => b.Property, StringComparer.Ordinal).ThenBy(b => b.Target.AssemblyQualifiedName, StringComparer.Ordinal).ToArray();
            var id = _packageId + ":" + input.AssetPath;
            var directClass = DirectResourceClass(input);
            var deferredTarget = directClass is not null && input.IsDefaultTypedControlTheme &&
                                 orderedExports.Length == 1 && orderedExports[0].KeyType == orderedExports[0].Target
                ? orderedExports[0].Target : null;
            var factoryType = directClass is not null && (input.IsResourceDictionary || deferredTarget is not null)
                ? directClass
                : "global::" + GeneratedCodeNamespace.ForAssembly(_compilation.AssemblyName) + "." +
                  ThemeAssetInfo.GetGeneratedResourceClassName(input.AssetPath);
            var asset = new RegistrationAsset(id, new Uri("avares://" + _compilation.AssemblyName + "/" + input.AssetPath).ToString(),
                factoryType,
                assets.Count, new(orderedExports), new(orderedOwners), new(orderedBindings), 0,
                new(AssetAvailability(input, pair.Value.Root, orderedExports, orderedOwners, symbols).Guards()),
                new(pair.Value.Root.DescendantsAndSelf().Where(e => e.Name.LocalName is "ResourceInclude" or "MergeResourceInclude").Select(e => e.Attribute("Source")?.Value ?? "").OrderBy(v => v, StringComparer.Ordinal)),
                new(pair.Value.Root.DescendantsAndSelf().Attributes().SelectMany(a => MarkupResourceKeys(a.Value, "DynamicResource")).Distinct().OrderBy(v => v, StringComparer.Ordinal)));
            assets.Add(asset with
            {
                DeferredThemeTarget = deferredTarget,
                ResourceKeys = deferredTarget is not null
                    ? new ValueArray<RegistrationResourceKey>([new(deferredTarget, null)])
                    : CollectCompleteResourceKeys(input, pair.Value.Root),
                CompiledContractFingerprint = RegistrationFingerprint.ComputeContract(asset, globalNames)
            });
        }
        foreach (var token in tokens.Where(t => t.HasDescriptor))
        {
            var symbol = MetadataType(token.ControlMetadataName);
            if (symbol is not null)
            {
                symbols[RegistrationType.From(symbol).AssemblyQualifiedName] = symbol;
            }
        }
        foreach (var semantic in semantics)
        {
            symbols[RegistrationType.From(semantic.ControlType).AssemblyQualifiedName] = semantic.ControlType;
        }

        var controls = new List<RegistrationControl>();
        foreach (var pair in symbols.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            var symbol = pair.Value;
            var type = RegistrationType.From(symbol);
            var token = tokens.FirstOrDefault(t => t.HasDescriptor && t.ControlTypeName == type.Name);
            var semantic = semantics.FirstOrDefault(s => SymbolEqualityComparer.Default.Equals(s.ControlType, symbol));
            var refs = assets.Where(a => a.Exports.Any(e => e.Target == type) || a.Bindings.Any(b => b.Owner.Type == type)).Select(a => a.Id).ToArray();
            if (token is null && semantic is null && refs.Length == 0)
            {
                continue;
            }

            if (!IsAccessible(symbol)) { _report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationInaccessibleType, symbol.Locations.FirstOrDefault(), type.Name)); continue; }
            var triggers = new List<RegistrationTrigger> { new(type, "control") };
            RegistrationTrigger GeneratedTrigger(string name, string kind)
            {
                var metadataName = name.Substring("global::".Length);
                return new(new(name, metadataName, metadataName + ", " + _compilation.Assembly.Identity.GetDisplayName()), kind);
            }
            if (token is not null)
            {
                var ns = "global::" + token.ControlNamespace + ".DesignTokens.";
                triggers.Add(GeneratedTrigger(ns + token.ResourceExtensionType, "token"));
                triggers.Add(GeneratedTrigger(ns + token.TokenKeyType, "token"));
                if (token.HasOwnToken)
                {
                    triggers.Add(GeneratedTrigger(ns + token.TokenKindType, "token"));
                }
            }
            if (semantic is not null)
            {
                triggers.AddRange(semantic.Parts.Select(p => GeneratedTrigger("global::AtomUI.Theme.Styling." + SemanticPartStyleContract.GetTypeName(semantic, p), "style")));
            }

            controls.Add(new(type, token is null ? null : GeneratedThemeSchemaWriter.GetControlDescriptorFactoryMethodName(token),
                semantic is null ? null : SemanticPartManifestWriter.GetFactoryName(semantic), new(triggers.Select(t => t.Type.Name).Distinct().OrderBy(t => t, StringComparer.Ordinal)),
                new(Availability(symbol).Guards()), new(refs), new(triggers.Distinct().OrderBy(t => t.Type.Name, StringComparer.Ordinal))));
        }
        ApplyBootstrapRegistration(controls, assets);

        foreach (var conflict in tokens.Where(t => t.HasDescriptor).GroupBy(t => t.ControlName, StringComparer.Ordinal).Where(g => g.Select(t => t.ControlTypeName).Distinct().Count() > 1))
        {
            _report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationIdentityConflict, Location.None, _catalog + ":" + conflict.Key));
        }

        foreach (var conflict in controls.GroupBy(RegistrationNames.Proxy, StringComparer.Ordinal).Where(g => g.Select(c => c.Type.AssemblyQualifiedName).Distinct().Count() > 1))
        {
            _report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationIdentityConflict, Location.None, "proxy hash collision: " + conflict.Key));
        }

        foreach (var conflict in assets.GroupBy(a => RegistrationNames.AssetFactory(a.Id), StringComparer.Ordinal).Where(g => g.Select(a => a.Id).Distinct().Count() > 1))
        {
            _report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationIdentityConflict, Location.None, "resource factory hash collision: " + conflict.Key));
        }

        var builderType = _compilation.GetTypeByMetadataName("AtomUI.Registration.ControlPackageRegistrationBuilder");
        return new(_packageId, _compilation.Assembly.Identity.GetDisplayName(), GeneratedCodeNamespace.ForAssembly(_compilation.AssemblyName), new(controls), new(assets), new(globalNames),
            RegistrationFingerprint.ComputeContractPrefix(globalNames), RegistrationFingerprint.ComputeResourceKeyPrefix(globalNames),
            builderType is null ? null : RegistrationType.From(builderType), RegistrationType.From(_compilation.GetSpecialType(SpecialType.System_Void)),
            RegistrationFingerprint.ComputePackageSchema(_packageId, _catalog, controls, tokens, assets))
        {
            // 新生成器引用旧 Core 时保留旧构造入口，不要求旧 Core 含新增工厂 ABI。
            SupportsResourceKeys = _compilation.GetTypeByMetadataName("AtomUI.Registration.ControlThemeResourceRegistration")?
                .GetMembers("CreateGenerated").OfType<IMethodSymbol>().Any(static method =>
                    method.IsStatic && method.DeclaredAccessibility == Accessibility.Public && method.Parameters.Length == 5) == true
        };
    }

    private void ApplyBootstrapRegistration(List<RegistrationControl> controls, IReadOnlyList<RegistrationAsset> assets)
    {
        var marked = _types.Where(type => type.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString() == "AtomUI.Registration.BootstrapRegistrationAttribute")).ToArray();
        if (marked.Length == 0)
        {
            return;
        }

        var byType = controls.ToDictionary(control => control.Type.AssemblyQualifiedName, StringComparer.Ordinal);
        var byAsset = assets.ToDictionary(asset => asset.Id, StringComparer.Ordinal);
        var selected = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Queue<RegistrationControl>();
        void Select(RegistrationType type)
        {
            if (byType.TryGetValue(type.AssemblyQualifiedName, out var control) && selected.Add(type.AssemblyQualifiedName))
            {
                pending.Enqueue(control);
            }
        }
        void SelectAsset(RegistrationAsset asset)
        {
            // Reuse registration ownership facts. This is not a template or application call graph.
            foreach (var export in asset.Exports)
            {
                Select(export.Target);
            }
            foreach (var owner in asset.Owners)
            {
                Select(owner.Type);
            }
            foreach (var binding in asset.Bindings)
            {
                Select(binding.Owner.Type);
            }
        }

        foreach (var symbol in marked)
        {
            var type = RegistrationType.From(symbol);
            if (byType.ContainsKey(type.AssemblyQualifiedName))
            {
                Select(type);
                continue;
            }
            var resourceAssets = _parsed.Values.Where(document =>
                    document.Input.ResourceClassName == symbol.ToDisplayString())
                .Select(document => _packageId + ":" + document.Input.AssetPath)
                .Where(byAsset.ContainsKey).Select(id => byAsset[id]).ToArray();
            if (resourceAssets.Length == 0)
            {
                _report(Diagnostic.Create(AtomUIDiagnosticDescriptors.RegistrationInvalidBootstrap,
                    symbol.Locations.FirstOrDefault(), type.Name));
                continue;
            }
            foreach (var asset in resourceAssets)
            {
                SelectAsset(asset);
            }
        }

        var visitedAssets = new HashSet<string>(StringComparer.Ordinal);
        while (pending.Count != 0)
        {
            foreach (var assetId in pending.Dequeue().Assets)
            {
                if (visitedAssets.Add(assetId))
                {
                    SelectAsset(byAsset[assetId]);
                }
            }
        }
        for (var index = 0; index < controls.Count; index++)
        {
            controls[index] = controls[index] with { IsBootstrap = selected.Contains(controls[index].Type.AssemblyQualifiedName) };
        }
    }

    private ValueArray<RegistrationResourceKey>? CollectCompleteResourceKeys(ThemeAssetInfo input, XElement root)
    {
        // 手写字典可能在构造/重写中增加任意键；导出主题表并不是完整资源键集合。
        if (!input.IsResourceDictionary || DirectResourceClass(input) is not null)
        {
            return null;
        }
        var keys = new HashSet<RegistrationResourceKey>();
        return CollectDictionaryKeys(root, input.AssetPath, keys, new HashSet<string>(StringComparer.Ordinal))
            ? new ValueArray<RegistrationResourceKey>(keys.OrderBy(static key => key.Type is null ? 1 : 0)
                .ThenBy(static key => key.Type?.AssemblyQualifiedName ?? key.Key, StringComparer.Ordinal))
            : null;
    }

    private bool CollectDictionaryKeys(XElement root, string assetPath,
        HashSet<RegistrationResourceKey> keys, HashSet<string> includes)
    {
        if (root.Name.LocalName != "ResourceDictionary" ||
            root.Name.NamespaceName != "https://github.com/avaloniaui" ||
            root.Attribute(Xaml + "Class") is not null || root.Attribute(Xaml + "FactoryMethod") is not null)
        {
            return false;
        }
        foreach (var element in root.Elements())
        {
            if (element.Name.LocalName is "ResourceDictionary.MergedDictionaries" or "ResourceDictionary.ThemeDictionaries")
            {
                foreach (var nested in element.Elements())
                {
                    if (nested.Name.LocalName is "ResourceInclude" or "MergeResourceInclude")
                    {
                        // 同名自定义 provider 可以增加任意键，不能把它的 Source 当作完整键证据。
                        // 非标准 XML 命名空间的 include 保持开放查找。
                        if (nested.Name.NamespaceName != "https://github.com/avaloniaui")
                        {
                            return false;
                        }
                        var source = nested.Attribute("Source")?.Value;
                        var uri = source is null ? null : ResolveInclude(assetPath, source);
                        if (uri is null || !string.Equals(uri.Host, _compilation.AssemblyName, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        var path = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
                        if (!_parsed.TryGetValue(path, out var included) || !includes.Add(path))
                        {
                            return false;
                        }
                        var complete = CollectDictionaryKeys(included.Root, path, keys, includes);
                        includes.Remove(path);
                        if (!complete)
                        {
                            return false;
                        }
                    }
                    else if (!CollectDictionaryKeys(nested, assetPath, keys, includes))
                    {
                        return false;
                    }
                }
                continue;
            }
            var key = element.Attribute(Xaml + "Key")?.Value;
            if (key is null || element.Name.LocalName.Contains('.'))
            {
                return false;
            }
            if (key.StartsWith("{x:Type", StringComparison.Ordinal))
            {
                var type = Resolve(element, key);
                if (type is null || !IsAccessible(type))
                {
                    return false;
                }
                keys.Add(new RegistrationResourceKey(RegistrationType.From(type), null));
            }
            else if (key.StartsWith("{}", StringComparison.Ordinal))
            {
                keys.Add(new RegistrationResourceKey(null, key.Substring(2)));
            }
            else if (key.StartsWith("{", StringComparison.Ordinal))
            {
                return false;
            }
            else
            {
                keys.Add(new RegistrationResourceKey(null, key));
            }
            // 不遍历资源值的内部 Resources；它们具有自己的词法作用域。
        }
        return true;
    }

    private PlatformAvailability Availability(INamedTypeSymbol type)
    {
        var key = RegistrationType.From(type).AssemblyQualifiedName;
        if (!_availability.TryGetValue(key, out var domain))
        {
            _availability.Add(key, domain = PlatformAvailability.FromType(type, _report));
        }

        return domain;
    }

    private PlatformAvailability AssetAvailability(ThemeAssetInfo input, XElement root,
        IReadOnlyList<RegistrationExport> exports, IReadOnlyList<RegistrationOwner> owners,
        IReadOnlyDictionary<string, INamedTypeSymbol> symbols)
    {
        var resource = ResourceAvailability(input, root);
        var domains = exports.Select(e => resource.Intersect(Availability(symbols[e.Target.AssemblyQualifiedName]))).ToArray();
        var domain = domains[0];
        if (domain.IsEmpty)
        {
            Error(AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, input, root,
                "theme asset and its target have no common supported platform domain");
        }

        if (domains.Any(d => !d.Equals(domain)))
        {
            Error(AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, input, root,
                "indivisible theme asset exports incompatible platform availability; split resources or declare a common resource platform domain");
            return domain;
        }
        foreach (var owner in owners)
        {
            var symbol = symbols[owner.Type.AssemblyQualifiedName];
            if (!Availability(symbol).Covers(domain))
            {
                Error(AtomUIDiagnosticDescriptors.RegistrationUnsupportedBackend, input, root,
                    "required Token owner '" + owner.Type.MetadataName + "' is unavailable in part of the asset platform domain; split resources or declare a common resource platform domain");
            }
        }
        return domain;
    }

    private string? DirectResourceClass(ThemeAssetInfo input)
    {
        if (input.ResourceClassName is { } name)
        {
            var type = _compilation.Assembly.GetTypeByMetadataName(name) ??
                _types.SingleOrDefault(candidate => candidate.ToDisplayString() == name);
            if (type is not null && IsAccessible(type) && !type.IsAbstract && !HasConstructionDiagnosticAttribute(type))
            {
                // Qualifying a nested type also references its containing types in C#.
                // The compiled include need not expose their C# call-site diagnostics.
                for (var containing = type.ContainingType; containing is not null; containing = containing.ContainingType)
                {
                    if (HasConstructionDiagnosticAttribute(containing))
                    {
                        return null;
                    }
                }

                var constructor = type.InstanceConstructors.FirstOrDefault(candidate =>
                    candidate.Parameters.Length == 0 &&
                    _compilation.IsSymbolAccessibleWithin(candidate, _compilation.Assembly));
                // ResourceAvailability validates the type, not C# construction. Keep the
                // compiled include fallback when a direct new expression is not legal.
                if (constructor is not null && !HasConstructionDiagnosticAttribute(constructor))
                {
                    var canConstruct = true;
                    var setsRequiredMembers = constructor.GetAttributes().Any(attribute =>
                        attribute.AttributeClass?.ToDisplayString() == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");
                    for (var current = type; current is not null; current = current.BaseType)
                    {
                        if (HasObsoleteAttribute(current) || (!setsRequiredMembers && current.GetMembers().Any(member =>
                                member is IPropertySymbol { IsRequired: true } or IFieldSymbol { IsRequired: true })))
                        {
                            canConstruct = false;
                            break;
                        }

                        // The typed path reproduces only the generated wrapper's inherited
                        // ControlTheme.TargetType assignment, never arbitrary XAML properties.
                        if (input.IsDefaultTypedControlTheme && current.ToDisplayString() != "Avalonia.Styling.ControlTheme" &&
                            current.GetMembers("TargetType").Length != 0)
                        {
                            canConstruct = false;
                            break;
                        }
                    }

                    if (canConstruct)
                    {
                        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }
                }
            }
        }

        return null;
    }

    private static bool HasObsoleteAttribute(ISymbol symbol) => symbol.GetAttributes().Any(attribute =>
        attribute.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");

    private static bool HasConstructionDiagnosticAttribute(ISymbol symbol) => symbol.GetAttributes().Any(attribute =>
        attribute.AttributeClass?.ToDisplayString() is "System.ObsoleteAttribute" or
            "System.Diagnostics.CodeAnalysis.ExperimentalAttribute" or
            "System.Runtime.Versioning.RequiresPreviewFeaturesAttribute");

    private PlatformAvailability ResourceAvailability(ThemeAssetInfo input, XElement root)
    {
        if (_resourceAvailability.TryGetValue(input.AssetPath, out var cached))
        {
            return cached;
        }

        var domain = _assemblyAvailability ??= PlatformAvailability.FromAssembly(_compilation.Assembly, _report);
        if (input.ResourceClassName is { } name)
        {
            // x:Class defines this asset's class in its own assembly. A reference with the same
            // name cannot supply its platform contract. Keep this separate from XML target lookup.
            var type = _compilation.Assembly.GetTypeByMetadataName(name) ??
                _types.SingleOrDefault(candidate => candidate.ToDisplayString() == name);
            var expectedBase = input.IsResourceDictionary ? "Avalonia.Controls.ResourceDictionary" : "Avalonia.Styling.ControlTheme";
            var resourceBase = type;
            while (resourceBase is not null && resourceBase.ToDisplayString() != expectedBase)
            {
                resourceBase = resourceBase.BaseType;
            }

            if (type is null || !IsAccessible(type) || type.IsAbstract || resourceBase is null)
            {
                Error(AtomUIDiagnosticDescriptors.RegistrationInaccessibleType, input, root,
                    "x:Class '" + name + "' in '" + _compilation.Assembly.Identity.GetDisplayName() +
                    "' must resolve to an accessible concrete " + expectedBase + " resource class");
                domain = PlatformAvailability.Empty;
            }
            else
            {
                domain = Availability(type);
            }
        }
        _resourceAvailability.Add(input.AssetPath, domain);
        return domain;
    }

    private bool IsThemeElement(XElement element)
    {
        if (element.Name.LocalName == "ControlTheme" && element.Name.NamespaceName == "https://github.com/avaloniaui")
        {
            return true;
        }

        var symbol = ResolveXml(element.Name.NamespaceName, element.Name.LocalName);
        for (; symbol is not null; symbol = symbol.BaseType)
        {
            if (symbol.ToDisplayString() == "Avalonia.Styling.ControlTheme")
            {
                return true;
            }
        }

        return false;
    }
    private bool NamespaceMatches(string xml, string clrNamespace)
    {
        if (xml == "using:" + clrNamespace || xml == "clr-namespace:" + clrNamespace)
        {
            return true;
        }

        if (xml == "clr-namespace:" + clrNamespace + ";assembly=" + _compilation.AssemblyName)
        {
            return true;
        }

        return _xmlNamespaces.TryGetValue(xml, out var mappings) && mappings.Any(m =>
            SymbolEqualityComparer.Default.Equals(m.Assembly, _compilation.Assembly) && m.Namespace == clrNamespace);
    }

    private static IEnumerable<(XElement Element, string Namespace, string Family)> TokenReferences(XElement root)
    {
        foreach (var element in root.DescendantsAndSelf())
        {
            foreach (var value in element.Attributes().Where(a => !a.IsNamespaceDeclaration).Select(a => a.Value)
                     .Concat(element.Nodes().OfType<XText>().Select(t => t.Value)))
        {
            for (var start = value.IndexOf('{'); start >= 0; start = value.IndexOf('{', start + 1))
            {
                var end = start + 1;
                while (end < value.Length && !char.IsWhiteSpace(value[end]) && value[end] is not (',' or '}'))
                    {
                        end++;
                    }

                    var name = value.Substring(start + 1, end - start - 1);
                var separator = name.IndexOf(':');
                var prefix = separator < 0 ? "" : name.Substring(0, separator);
                name = separator < 0 ? name : name.Substring(separator + 1);
                const string suffix = "TokenResource";
                if (!name.EndsWith(suffix, StringComparison.Ordinal) || name == "SharedTokenResource")
                    {
                        continue;
                    }

                    var xml = (prefix.Length == 0 ? element.GetDefaultNamespace() : element.GetNamespaceOfPrefix(prefix))?.NamespaceName ?? "";
                yield return (element, xml, name.Substring(0, name.Length - suffix.Length));
            }
        }
        }
    }

    private INamedTypeSymbol? ResolveTokenOwner(INamedTypeSymbol extension, string family)
    {
        var ns = extension.ContainingNamespace.ToDisplayString();
        if (ns.EndsWith(".DesignTokens", StringComparison.Ordinal))
        {
            ns = ns.Substring(0, ns.Length - ".DesignTokens".Length);
        }

        return MetadataType(ns + "." + family) ?? _types.SingleOrDefault(t => t.Name == family && IsControl(t));
    }
    private INamedTypeSymbol? Resolve(XElement element, string value)
    {
        value = value.Trim();
        if (value.StartsWith("{x:Type", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal))
        {
            value = value.Substring(7, value.Length - 8).Trim();
        }

        var colon = value.IndexOf(':');
        var prefix = colon < 0 ? "" : value.Substring(0, colon);
        var name = colon < 0 ? value : value.Substring(colon + 1);
        return (colon < 0 ? MetadataType(name) : null) ?? ResolveXml((prefix.Length == 0 ? element.GetDefaultNamespace() : element.GetNamespaceOfPrefix(prefix))?.NamespaceName ?? "", name);
    }
    private INamedTypeSymbol? ResolveXml(string xml, string name)
    {
        if (xml.StartsWith("using:", StringComparison.Ordinal))
        {
            return MetadataType(xml.Substring(6) + "." + name);
        }

        if (xml.StartsWith("clr-namespace:", StringComparison.Ordinal))
        {
            var split = xml.Substring(14).Split(';');
            var assembly = split.Skip(1).FirstOrDefault(s => s.StartsWith("assembly=", StringComparison.Ordinal))?.Substring(9);
            if (assembly is null)
            {
                return MetadataType(split[0] + "." + name);
            }

            var owner = new[] { _compilation.Assembly }.Concat(_compilation.SourceModule.ReferencedAssemblySymbols).SingleOrDefault(a => a.Name == assembly);
            return owner?.GetTypeByMetadataName(split[0] + "." + name);
        }
        if (_xmlNamespaces.TryGetValue(xml, out var mappings))
        {
            var matches = mappings.Select(m => m.Assembly.GetTypeByMetadataName(m.Namespace + "." + name)).Where(t => t is not null).Distinct(SymbolEqualityComparer.Default).ToArray();
            if (matches.Length == 1)
            {
                return matches[0] as INamedTypeSymbol;
            }
        }
        return null;
    }
    private INamedTypeSymbol? MetadataType(string name)
    {
        if (_resolved.TryGetValue(name, out var cached))
        {
            return cached;
        }

        var symbol = _compilation.GetTypeByMetadataName(name);
        if (symbol is null)
        {
            symbol = _types.SingleOrDefault(t => t.ToDisplayString() == name || RegistrationType.MetadataNameOf(t) == name);
        }

        _resolved[name] = symbol;
        return symbol;
    }
    internal static IEnumerable<INamedTypeSymbol> Types(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            foreach (var nested in TypeAndNested(type))
            {
                yield return nested;
            }
        }

        foreach (var child in ns.GetNamespaceMembers())
        {
            foreach (var type in Types(child))
            {
                yield return type;
            }
        }
    }
    private static IEnumerable<INamedTypeSymbol> TypeAndNested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var child in type.GetTypeMembers())
        {
            foreach (var nested in TypeAndNested(child))
            {
                yield return nested;
            }
        }
    }
    private bool IsAccessible(INamedTypeSymbol type) => Accessible(type) && _compilation.IsSymbolAccessibleWithin(type, _compilation.Assembly);
    internal static bool Accessible(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.Arity != 0 || current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal))
            {
                return false;
            }
        }

        return true;
    }
    private static bool IsStyledElement(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == "Avalonia.StyledElement")
            {
                return true;
            }
        }

        return false;
    }
    internal static bool IsControl(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == "Avalonia.Controls.Control")
            {
                return true;
            }
        }

        return false;
    }
    private void ValidateStaticResources(Dictionary<string, (ThemeAssetInfo Input, XElement Root)> parsed,
        IReadOnlyDictionary<string, List<RegistrationExport>> exports, IReadOnlyDictionary<string, INamedTypeSymbol> symbols)
    {
        // An actual x:Type key is an IL type reference, hence a TypeMap condition. Only a default
        // export whose key and target are that exact Type is guaranteed to be selected by it.
        // Keep normal ambient lookup/overrides: this proves retention without inserting a local
        // dictionary which would shadow an application's replacement theme. String keys and
        // arbitrary Type-key/TargetType pairings do not have this evidence.
        var typedProviders = exports.SelectMany(pair => pair.Value.Where(e => e.KeyType == e.Target &&
                parsed[pair.Key].Input.HasGeneratedResourceWrapper).Select(e => (Path: pair.Key, Export: e)))
            .GroupBy(p => p.Export.Target.AssemblyQualifiedName, StringComparer.Ordinal)
            .ToDictionary(g => "T\0" + g.Key, g => g.ToArray(), StringComparer.Ordinal);
        var domains = new Dictionary<string, PlatformAvailability[]>(StringComparer.Ordinal);
        PlatformAvailability[] Domains(string path)
        {
            if (domains.TryGetValue(path, out var cached))
            {
                return cached;
            }

            var input = parsed[path].Input;
            var resource = ResourceAvailability(input, parsed[path].Root);
            var result = exports.TryGetValue(path, out var targets)
                ? targets.Select(t => resource.Intersect(Availability(symbols[t.Target.AssemblyQualifiedName]))).ToArray()
                : parsed[path].Root.Attribute("TargetType") is { } target && Resolve(parsed[path].Root, target.Value) is { } owner
                    ? new[] { resource.Intersect(Availability(owner)) } : new[] { resource };
            domains.Add(path, result);
            return result;
        }

        foreach (var pair in parsed)
        {
            foreach (var element in pair.Value.Root.DescendantsAndSelf())
            {
                foreach (var key in element.Attributes().Where(a => !a.IsNamespaceDeclaration).SelectMany(a => MarkupResourceKeys(a.Value, "StaticResource"))
                     .Concat(element.Name.LocalName == "StaticResource" && element.Attribute("ResourceKey") is { } resourceKey ? new[] { resourceKey.Value } : Array.Empty<string>()))
        {
            var normalizedKey = NormalizeResourceKey(element, key);
            var source = FindResource(element, normalizedKey);
            var typedCondition = source.Element is null && !source.Opaque && typedProviders.TryGetValue(normalizedKey, out var providers) &&
                providers.Any(provider => Domains(pair.Key).All(consumer => Domains(provider.Path).All(producer => producer.Covers(consumer))));
            if (source.Element is null && !source.Opaque && !typedCondition)
                    {
                        Error(AtomUIDiagnosticDescriptors.RegistrationInvalidResourceDependency, pair.Value.Input, element,
                    "StaticResource '" + key + "' has no visible lexical/include source or exact available default type-key condition; add a normal ResourceInclude");
                    }
                }
            }
        }
    }

    internal sealed record SemanticThemeAssignment(string AssetPath, INamedTypeSymbol? Target);

    internal IEnumerable<SemanticThemeAssignment> SemanticAssignments(INamedTypeSymbol owner, string property) =>
        _semanticAssignments.TryGetValue((RegistrationType.From(owner).AssemblyQualifiedName, property), out var facts)
            ? facts : Enumerable.Empty<SemanticThemeAssignment>();

    private void ReadSemanticAssignments(HashSet<string> properties)
    {
        if (properties.Count == 0)
        {
            return;
        }

        foreach (var pair in _parsed)
        {
            foreach (var element in pair.Value.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName == "Setter" && element.Parent is { } theme && IsThemeElement(theme))
            {
                if (element.Attribute("Property") is { } property && properties.Contains(property.Value) && theme.Attribute("TargetType") is { } target)
                    {
                        Add(Resolve(theme, target.Value), property.Value, element, element.Attribute("Value")?.Value,
                        element.Elements().FirstOrDefault());
                    }

                    continue;
            }
            if (element.Name.LocalName.Contains("."))
                {
                    continue;
                }

                if (!element.Attributes().Any(a => properties.Contains(a.Name.LocalName)) &&
                !element.Elements().Any(e => properties.Any(p => e.Name.LocalName.EndsWith("." + p, StringComparison.Ordinal))))
                {
                    continue;
                }

                var owner = ResolveXml(element.Name.NamespaceName, element.Name.LocalName);
            if (owner is null || !IsStyledElement(owner))
                {
                    continue;
                }

                foreach (var attribute in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
                {
                    Add(owner, attribute.Name.LocalName, element, attribute.Value, null);
                }

                foreach (var property in element.Elements().Where(e => e.Name.LocalName.Contains(".")))
            {
                var separator = property.Name.LocalName.LastIndexOf('.');
                var declaringType = ResolveXml(property.Name.NamespaceName, property.Name.LocalName.Substring(0, separator));
                if (declaringType is null || !SemanticPartTypeResolver.IsAssignableTo(owner, declaringType))
                    {
                        continue;
                    }

                    Add(owner, property.Name.LocalName.Substring(separator + 1), property, null, property.Elements().FirstOrDefault(), declaringType);
            }

            void Add(INamedTypeSymbol? ownerType, string name, XElement context, string? value, XElement? valueElement,
                INamedTypeSymbol? declaringType = null)
            {
                if (ownerType is null || !properties.Contains(name))
                    {
                        return;
                    }
                    // Only actual ControlTheme properties constitute ownership evidence. Attached
                    // properties and selectors require their own typed contract; never infer an owner.
                    var property = SemanticThemePropertyResolver.FindInstanceProperty(declaringType ?? ownerType, name);
                var semanticProperty = SemanticThemePropertyResolver.FindInstanceProperty(ownerType, name);
                if (!SemanticThemePropertyResolver.IsSameSlot(property, semanticProperty) ||
                    !SymbolEqualityComparer.Default.Equals(property!.Type, _compilation.GetTypeByMetadataName("Avalonia.Styling.ControlTheme")))
                    {
                        return;
                    }

                    var resolved = ResolveThemeValue(context, value, valueElement, new HashSet<XElement>());
                var key = (RegistrationType.From(ownerType).AssemblyQualifiedName, name);
                if (!_semanticAssignments.TryGetValue(key, out var list))
                    {
                        _semanticAssignments.Add(key, list = new());
                    }

                    list.Add(new(pair.Key, resolved));
            }
        }
        }
    }

    private INamedTypeSymbol? ResolveThemeValue(XElement context, string? value, XElement? element, HashSet<XElement> seen)
    {
        if (value is not null)
        {
            var keys = MarkupResourceKeys(value, "StaticResource").ToArray();
            if (keys.Length != 1 || !value.Trim().StartsWith("{StaticResource ", StringComparison.Ordinal))
            {
                return null;
            }

            element = FindResource(context, NormalizeResourceKey(context, keys[0])).Element;
        }
        if (element is null || !seen.Add(element))
        {
            return null;
        }

        if (element.Name.LocalName == "Setter.Value")
        {
            return ResolveThemeValue(element, null, element.Elements().SingleOrDefault(), seen);
        }

        if (element.Name.LocalName == "StaticResource" && element.Attribute("ResourceKey") is { } key)
        {
            return ResolveThemeValue(element, null, FindResource(element, NormalizeResourceKey(element, key.Value)).Element, seen);
        }

        if (!IsThemeElement(element))
        {
            return null;
        }

        if (element.Attribute("TargetType") is { } target)
        {
            return Resolve(element, target.Value);
        }

        var themeClass = ResolveXml(element.Name.NamespaceName, element.Name.LocalName);
        return themeClass is not null && _themeClasses.TryGetValue(themeClass.ToDisplayString(), out var declaration)
            ? ResolveThemeValue(declaration, null, declaration, seen) : null;
    }

    private (XElement? Element, bool Opaque) FindResource(XElement context, string key)
    {
        foreach (var ancestor in context.AncestorsAndSelf())
        {
            if (ancestor.Name.LocalName == "ResourceDictionary" || ancestor.Name.LocalName.EndsWith(".Resources", StringComparison.Ordinal))
            {
                var found = FindInScope(ancestor, key, new(StringComparer.Ordinal));
                if (found.Element is not null || found.Opaque)
                {
                    return found;
                }
            }
            foreach (var scope in ancestor.Elements().Where(e => e.Name.LocalName.EndsWith(".Resources", StringComparison.Ordinal)))
            {
                var found = FindInScope(scope, key, new(StringComparer.Ordinal));
                if (found.Element is not null || found.Opaque)
                {
                    return found;
                }
            }
        }
        return default;
    }

    private (XElement? Element, bool Opaque) FindInScope(XElement scope, string key, HashSet<string> seen)
    {
        // A .Resources property can contain either implicit entries or one explicit dictionary.
        // The latter is the same resource scope, not an entry keyed by its element name.
        if (scope.Name.LocalName.EndsWith(".Resources", StringComparison.Ordinal) &&
            scope.Elements().Count() == 1 && scope.Elements().First() is { } dictionary &&
            dictionary.Name.LocalName == "ResourceDictionary" && dictionary.Attribute(Xaml + "Key") is null)
        {
            return FindInScope(dictionary, key, seen);
        }

        var local = scope.Elements().LastOrDefault(e => e.Attribute(Xaml + "Key") is { } ownKey && NormalizeResourceKey(e, ownKey.Value) == key);
        if (local is not null)
        {
            return (local, false);
        }

        foreach (var include in scope.Elements().Where(e => e.Name.LocalName.EndsWith(".MergedDictionaries", StringComparison.Ordinal)).Elements().Reverse())
        {
            if (include.Name.LocalName == "ResourceDictionary")
            {
                var inline = FindInScope(include, key, seen);
                if (inline.Element is not null || inline.Opaque)
                {
                    return inline;
                }

                continue;
            }
            if (include.Name.LocalName is not ("ResourceInclude" or "MergeResourceInclude"))
            {
                continue;
            }

            var source = include.Attribute("Source")?.Value;
            if (source is null || source.StartsWith("{", StringComparison.Ordinal))
            {
                continue;
            }

            var root = scope.AncestorsAndSelf().Last();
            if (!_documentPaths.TryGetValue(root, out var owner))
            {
                continue;
            }

            var uri = ResolveInclude(owner, source);
            if (uri is null)
            {
                continue;
            }

            if (!string.Equals(uri.Host, _compilation.AssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                // Explicit binary includes are real dependencies, but their key/target contents
                // are opaque here. Avalonia validates the resource at load time.
                if (IsReferencedAssembly(uri.Host))
                {
                    return (null, true);
                }

                continue;
            }
            var path = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
            if (seen.Add(path) && _parsed.TryGetValue(path, out var target))
            {
                var found = FindInScope(target.Root, key, seen);
                if (found.Element is not null || found.Opaque)
                {
                    return found;
                }
            }
        }
        return default;
    }

    private string NormalizeResourceKey(XElement context, string key) =>
        key.StartsWith("{x:Type", StringComparison.Ordinal) && Resolve(context, key) is { } type
            ? "T\0" + RegistrationType.From(type).AssemblyQualifiedName
            : "S\0" + key;

    private static IEnumerable<string> MarkupResourceKeys(string value, string extension)
    {
        var prefix = "{" + extension + " ";
        for (var start = value.IndexOf(prefix, StringComparison.Ordinal); start >= 0; start = value.IndexOf(prefix, start + prefix.Length, StringComparison.Ordinal))
        {
            var depth = 1;
            var end = start + prefix.Length;
            for (; end < value.Length; end++)
            {
                if (value[end] == '{')
                {
                    depth++;
                }
                else if (value[end] == '}' && --depth == 0)
                {
                    break;
                }
            }
            if (end >= value.Length)
            {
                continue;
            }

            var key = value.Substring(start + prefix.Length, end - start - prefix.Length).Trim();
            if (key.StartsWith("ResourceKey=", StringComparison.Ordinal))
            {
                key = key.Substring(12).Trim();
            }

            yield return key;
        }
    }

    private bool IsReferencedAssembly(string name) => _compilation.SourceModule.ReferencedAssemblySymbols.Any(
        assembly => string.Equals(assembly.Name, name, StringComparison.OrdinalIgnoreCase));

    private Uri? ResolveInclude(string assetPath, string source) =>
        Uri.TryCreate("avares://" + _compilation.AssemblyName + "/" + assetPath, UriKind.Absolute, out var parent) &&
        Uri.TryCreate(parent, source, out var result) && result.Scheme == "avares" ? result : null;

    private void ValidateIncludes(Dictionary<string, (ThemeAssetInfo Input, XElement Root)> parsed)
    {
        var edges = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var pair in parsed)
        {
            var refs = new List<string>();
            foreach (var include in pair.Value.Root.DescendantsAndSelf().Where(e => e.Name.LocalName is "ResourceInclude" or "MergeResourceInclude"))
            {
                var source = include.Attribute("Source")?.Value;
                if (source is null || source.StartsWith("{", StringComparison.Ordinal))
                { Error(AtomUIDiagnosticDescriptors.RegistrationInvalidResourceDependency, pair.Value.Input, include, "include requires a constant URI"); continue; }
                var uri = ResolveInclude(pair.Key, source);
                if (uri is null) { Error(AtomUIDiagnosticDescriptors.RegistrationInvalidResourceDependency, pair.Value.Input, include, "unsupported or invalid include URI: " + source); continue; }
                if (!string.Equals(uri.Host, _compilation.AssemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsReferencedAssembly(uri.Host))
                    {
                        Error(AtomUIDiagnosticDescriptors.RegistrationInvalidResourceDependency, pair.Value.Input, include, "include assembly cannot be resolved: " + source);
                    }

                    continue;
                }
                var path = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
                if (!parsed.ContainsKey(path)) { Error(AtomUIDiagnosticDescriptors.RegistrationInvalidResourceDependency, pair.Value.Input, include, source); continue; }
                refs.Add(path);
            }
            foreach (var element in pair.Value.Root.Descendants())
            {
                var type = ResolveXml(element.Name.NamespaceName, element.Name.LocalName);
                if (type is null)
                {
                    continue;
                }

                foreach (var target in parsed.Where(p => p.Value.Input.ControlThemeClassName == type.ToDisplayString()))
                {
                    refs.Add(target.Key);
                }
            }
            edges.Add(pair.Key, refs.Distinct(StringComparer.Ordinal).ToList());
        }
        var visited = new HashSet<string>(StringComparer.Ordinal); var active = new HashSet<string>(StringComparer.Ordinal);
        void Visit(string path)
        {
            if (active.Contains(path)) { Error(AtomUIDiagnosticDescriptors.RegistrationInvalidResourceDependency, parsed[path].Input, parsed[path].Root, "include cycle: " + path); return; }
            if (!visited.Add(path))
            {
                return;
            }

            active.Add(path); foreach (var target in edges[path])
            {
                Visit(target);
            }

            active.Remove(path);
        }
        foreach (var path in edges.Keys)
        {
            Visit(path);
        }
    }
    private void Error(DiagnosticDescriptor descriptor, ThemeAssetInfo input, XElement element, string message)
    {
        var line = (IXmlLineInfo)element;
        var position = new LinePosition(Math.Max(0, line.LineNumber - 1), Math.Max(0, line.LinePosition - 1));
        _report(Diagnostic.Create(descriptor, Location.Create(input.Path, new TextSpan(input.Source.Lines.GetPosition(position), 0), new(position, position)), message));
    }
}
