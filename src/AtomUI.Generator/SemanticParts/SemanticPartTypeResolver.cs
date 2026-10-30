using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class SemanticPartTypeResolver
{
    private const string XmlnsDefinitionAttribute = "Avalonia.Metadata.XmlnsDefinitionAttribute";

    private readonly Compilation _compilation;
    private readonly IReadOnlyList<INamedTypeSymbol> _sourceControls;
    private readonly IReadOnlyList<IAssemblySymbol> _assemblies;
    private readonly Dictionary<string, List<(IAssemblySymbol Assembly, string ClrNamespace)>> _xmlnsMappings =
        new(StringComparer.Ordinal);
    private readonly Dictionary<ThemeAssetTargetTypeReference, INamedTypeSymbol?> _targetTypes = new();
    private readonly Dictionary<(string XmlNamespace, string TypeName), INamedTypeSymbol?> _xmlTypes = new();

    internal SemanticPartTypeResolver(
        Compilation compilation,
        IReadOnlyList<INamedTypeSymbol> sourceControls)
    {
        _compilation = compilation;
        _sourceControls = sourceControls;
        _assemblies = GetAssemblies(compilation).ToArray();
        BuildXmlnsMappings();
    }

    internal INamedTypeSymbol? ResolveTargetType(ThemeAssetTargetTypeReference reference)
    {
        if (_targetTypes.TryGetValue(reference, out var cached))
        {
            return cached;
        }

        INamedTypeSymbol? resolved = null;
        if (TryParseTypeReference(reference.Value, out var prefix, out var typeName) &&
            reference.Namespaces.TryGetValue(prefix, out var xmlNamespace))
        {
            resolved = ResolveXmlType(xmlNamespace, typeName);
        }

        _targetTypes.Add(reference, resolved);
        return resolved;
    }

    internal INamedTypeSymbol? ResolveMarkerType(ThemeAssetSemanticMarkerInfo marker)
    {
        return ResolveXmlType(marker.XmlNamespace, marker.TypeName);
    }

    internal INamedTypeSymbol? ResolveMetadataName(string? fullName)
    {
        return fullName is null ? null : _compilation.GetTypeByMetadataName(fullName);
    }

    internal static bool IsAssignableTo(ITypeSymbol type, ITypeSymbol contractType)
    {
        for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, contractType))
            {
                return true;
            }
        }
        return false;
    }

    private INamedTypeSymbol? ResolveXmlType(string xmlNamespace, string typeName)
    {
        var key = (xmlNamespace, typeName);
        if (_xmlTypes.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var resolved = ResolveXmlTypeCore(xmlNamespace, typeName);
        _xmlTypes.Add(key, resolved);
        return resolved;
    }

    private INamedTypeSymbol? ResolveXmlTypeCore(string xmlNamespace, string typeName)
    {
        if (xmlNamespace.StartsWith("using:", StringComparison.Ordinal))
        {
            return _compilation.GetTypeByMetadataName($"{xmlNamespace.Substring("using:".Length)}.{typeName}");
        }
        if (xmlNamespace.StartsWith("clr-namespace:", StringComparison.Ordinal))
        {
            return ResolveClrNamespaceType(xmlNamespace.Substring("clr-namespace:".Length), typeName);
        }

        if (_xmlnsMappings.TryGetValue(xmlNamespace, out var mappings))
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Assembly.GetTypeByMetadataName($"{mapping.ClrNamespace}.{typeName}") is { } resolved)
                {
                    return resolved;
                }
            }
        }

        var matches = ControlThemeModelBuilder.FindPublicControlsByName(
            _compilation,
            _sourceControls,
            typeName).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private INamedTypeSymbol? ResolveClrNamespaceType(string value, string typeName)
    {
        var segments = value.Split(';');
        var clrNamespace = segments[0];
        var assemblyName = segments.Skip(1)
                                   .Select(static segment => segment.Trim())
                                   .FirstOrDefault(static segment => segment.StartsWith(
                                       "assembly=",
                                       StringComparison.OrdinalIgnoreCase));
        IAssemblySymbol? assembly = _compilation.Assembly;
        if (assemblyName is not null)
        {
            var name = assemblyName.Substring("assembly=".Length);
            assembly = _assemblies.FirstOrDefault(candidate => string.Equals(
                candidate.Identity.Name,
                name,
                StringComparison.OrdinalIgnoreCase));
        }

        return assembly?.GetTypeByMetadataName($"{clrNamespace}.{typeName}");
    }

    private void BuildXmlnsMappings()
    {
        foreach (var assembly in _assemblies)
        {
            foreach (var attribute in assembly.GetAttributes())
            {
                if (!string.Equals(
                        attribute.AttributeClass?.ToDisplayString(),
                        XmlnsDefinitionAttribute,
                        StringComparison.Ordinal) ||
                    attribute.ConstructorArguments.Length < 2 ||
                    attribute.ConstructorArguments[0].Value is not string xmlNamespace ||
                    attribute.ConstructorArguments[1].Value is not string clrNamespace ||
                    string.IsNullOrWhiteSpace(clrNamespace))
                {
                    continue;
                }

                if (!_xmlnsMappings.TryGetValue(xmlNamespace, out var mappings))
                {
                    mappings = new List<(IAssemblySymbol Assembly, string ClrNamespace)>();
                    _xmlnsMappings.Add(xmlNamespace, mappings);
                }
                mappings.Add((assembly, clrNamespace));
            }
        }
    }

    private static IEnumerable<IAssemblySymbol> GetAssemblies(Compilation compilation)
    {
        yield return compilation.Assembly;
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            yield return assembly;
        }
    }

    private static bool TryParseTypeReference(string value, out string prefix, out string typeName)
    {
        var reference = value.Trim();
        if (reference.StartsWith("{x:Type", StringComparison.Ordinal) &&
            reference.EndsWith("}", StringComparison.Ordinal))
        {
            reference = reference.Substring("{x:Type".Length, reference.Length - "{x:Type".Length - 1).Trim();
        }

        var colon = reference.IndexOf(':');
        if (colon < 0)
        {
            prefix = string.Empty;
            typeName = reference;
            return typeName.Length != 0;
        }

        prefix = reference.Substring(0, colon);
        typeName = reference.Substring(colon + 1);
        return prefix.Length != 0 && typeName.Length != 0;
    }
}
