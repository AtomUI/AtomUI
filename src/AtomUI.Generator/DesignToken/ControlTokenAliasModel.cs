using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

internal sealed record ControlTokenAliasInfo(string Name, string CanonicalName);

internal static class ControlTokenAliasModel
{
    private const string AttributeName = "AtomUI.Theme.DesignTokens.ControlTokenAliasAttribute";
    private const string InterfaceName = "AtomUI.Theme.DesignTokens.IControlTokenAliases";

    internal static IReadOnlyList<ControlTokenAliasInfo> Build(INamedTypeSymbol terminal,
        IReadOnlyList<ControlTokenLayerInfo> layers, CancellationToken cancellationToken,
        ICollection<Diagnostic> diagnostics)
    {
        var initialErrors = diagnostics.Count;
        void Error(ISymbol symbol, string detail)
        {
            var location = symbol.Locations.FirstOrDefault(static location => location.IsInSource);
            if (location is null)
            {
                detail += $"; metadata declaration '{symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}'";
            }
            diagnostics.Add(Diagnostic.Create(AtomUIDiagnosticDescriptors.ControlTokenAliasDeclaration,
                location ?? Location.None, terminal.ToDisplayString(), detail));
        }

        var allProperties = layers.SelectMany(static layer => layer.Symbol.GetMembers().OfType<IPropertySymbol>()).ToArray();
        var properties = allProperties.Where(static property => !property.GetAttributes().Any(static attribute =>
                attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.NotTokenDefinitionAttribute))
            .GroupBy(static property => property.Name, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
        var aliases = new Dictionary<string, ControlTokenAliasInfo>(StringComparer.Ordinal);
        foreach (var property in allProperties)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var attributes = property.GetAttributes().Where(static attribute =>
                attribute.AttributeClass?.ToDisplayString() == AttributeName).ToArray();
            if (attributes.Length == 0)
            {
                continue;
            }
            var target = attributes[0].ConstructorArguments.FirstOrDefault().Value as string;
            if (attributes.Length != 1 || target is null || !properties.ContainsKey(property.Name) ||
                !properties.TryGetValue(target, out var canonical) ||
                !SymbolEqualityComparer.IncludeNullability.Equals(property.Type, canonical.Type) ||
                aliases.ContainsKey(property.Name))
            {
                Error(property, $"alias '{property.Name}' requires one existing canonical Own Token of exactly the same type");
                continue;
            }
            aliases.Add(property.Name, new ControlTokenAliasInfo(property.Name, target));
        }
        foreach (var alias in aliases.Values)
        {
            if (aliases.ContainsKey(alias.CanonicalName))
            {
                Error(properties[alias.Name], $"alias '{alias.Name}' must target a canonical field directly; self references, chains and cycles are not supported");
            }
        }

        var implementsInterface = terminal.AllInterfaces.Any(static type => type.ToDisplayString() == InterfaceName);
        if (!terminal.IsAbstract && aliases.Count != 0 &&
            (!implementsInterface || !terminal.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax(cancellationToken)).OfType<ClassDeclarationSyntax>()
                .All(static declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword))))
        {
            Error(terminal, "an alias terminal must be partial and implement IControlTokenAliases");
        }
        if (!terminal.IsAbstract && aliases.Count == 0 && implementsInterface)
        {
            Error(terminal, "IControlTokenAliases requires at least one valid alias declaration");
        }
        if (aliases.Count != 0)
        {
            foreach (var layer in layers)
            {
                if (layer.Symbol.GetMembers("GetAliases").Any() || layer.Symbol.GetMembers().OfType<IMethodSymbol>()
                    .Any(static method => method.ExplicitInterfaceImplementations.Any(static implementation =>
                        implementation.ContainingType.ToDisplayString() == InterfaceName)))
                {
                    Error(layer.Symbol, "the generated GetAliases entry cannot be handwritten");
                }
            }
        }
        return diagnostics.Count == initialErrors
            ? aliases.Values.OrderBy(static alias => alias.Name, StringComparer.Ordinal).ToArray()
            : Array.Empty<ControlTokenAliasInfo>();
    }
}
