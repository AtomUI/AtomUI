using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

internal sealed class ControlTokenLayerInfo
{
    internal ControlTokenLayerInfo(INamedTypeSymbol symbol, bool isTerminal)
    {
        Symbol = symbol;
        IsTerminal = isTerminal;
    }

    internal INamedTypeSymbol Symbol { get; }
    internal bool IsTerminal { get; }
}

internal sealed class FlattenedControlTokenProperty
{
    internal FlattenedControlTokenProperty(
        string name,
        string valueType,
        string declaringType,
        string terminalType,
        Location? location,
        bool isInherited,
        string? reservedReason = null)
    {
        Name = name;
        ValueType = valueType;
        DeclaringType = declaringType;
        TerminalType = terminalType;
        Location = location;
        IsInherited = isInherited;
        ReservedReason = reservedReason;
    }

    internal string Name { get; }
    internal string ValueType { get; }
    internal string DeclaringType { get; }
    internal string TerminalType { get; }
    internal Location? Location { get; }
    internal bool IsInherited { get; }
    internal string? ReservedReason { get; }
}

internal sealed class ControlTokenInheritanceResult
{
    internal ControlTokenInheritanceResult(
        IReadOnlyList<ControlTokenLayerInfo> layers,
        IReadOnlyList<Diagnostic> diagnostics)
    {
        Layers = layers;
        Diagnostics = diagnostics;
    }

    internal IReadOnlyList<ControlTokenLayerInfo> Layers { get; }
    internal IReadOnlyList<Diagnostic> Diagnostics { get; }
    internal bool IsValid => Diagnostics.Count == 0;
}

internal static class ControlTokenInheritanceModel
{
    internal const string RootTypeName =
        "global::AtomUI.Theme.DesignTokens.AbstractControlDesignToken";

    internal static ControlTokenInheritanceResult Resolve(
        INamedTypeSymbol tokenType,
        CancellationToken cancellationToken)
    {
        var layers = new List<ControlTokenLayerInfo>();
        var diagnostics = new List<Diagnostic>();
        var current = tokenType;
        var isTerminal = !tokenType.IsAbstract;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            layers.Add(new ControlTokenLayerInfo(current, isTerminal &&
                                                          SymbolEqualityComparer.Default.Equals(current, tokenType)));

            if (current.SpecialType == SpecialType.System_Object)
            {
                diagnostics.Add(CreateInheritanceDiagnostic(
                    current,
                    $"the chain must reach {RootTypeName}",
                    cancellationToken));
                break;
            }

            if (!HasOwnControlDesignTokenAttribute(current))
            {
                diagnostics.Add(CreateInheritanceDiagnostic(
                    current,
                    "every non-root layer must declare [ControlDesignToken]",
                    cancellationToken));
                break;
            }

            if (!SymbolEqualityComparer.Default.Equals(current, tokenType) && !current.IsAbstract)
            {
                diagnostics.Add(CreateInheritanceDiagnostic(
                    current,
                    "an intermediate Control design token layer must be abstract",
                    cancellationToken));
                break;
            }

            if (!SymbolEqualityComparer.Default.Equals(current, tokenType) && IsEffectivelyGeneric(current))
            {
                diagnostics.Add(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.ControlTokenGenericLayer,
                    GetTypeLocation(current, cancellationToken),
                    GetDiagnosticTypeName(current)));
                break;
            }

            if (!SymbolEqualityComparer.Default.Equals(current, tokenType) && current.ContainingType is not null)
            {
                diagnostics.Add(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.ControlTokenNestedType,
                    GetTypeLocation(current, cancellationToken),
                    GetDiagnosticTypeName(current)));
                break;
            }

            if (!SymbolEqualityComparer.Default.Equals(current, tokenType) &&
                !HasValidTokenName(current.Name))
            {
                diagnostics.Add(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.AbstractControlTokenInvalidName,
                    GetTypeLocation(current, cancellationToken),
                    GetDiagnosticTypeName(current)));
                break;
            }

            var baseType = current.BaseType;
            if (baseType is null)
            {
                diagnostics.Add(CreateInheritanceDiagnostic(
                    current,
                    $"the chain must reach {RootTypeName}",
                    cancellationToken));
                break;
            }

            if (string.Equals(
                    baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    RootTypeName,
                    StringComparison.Ordinal))
            {
                layers.Reverse();
                return new ControlTokenInheritanceResult(layers, diagnostics);
            }

            if (baseType.SpecialType == SpecialType.System_Object)
            {
                diagnostics.Add(CreateInheritanceDiagnostic(
                    current,
                    $"the chain must reach {RootTypeName}",
                    cancellationToken));
                break;
            }

            current = baseType;
        }

        layers.Reverse();
        return new ControlTokenInheritanceResult(layers, diagnostics);
    }

    private static bool HasOwnControlDesignTokenAttribute(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                TargetMarkConstants.ControlDesignTokenAttribute,
                StringComparison.Ordinal));
    }

    private static bool HasValidTokenName(string name)
    {
        const string suffix = "Token";
        return name.EndsWith(suffix, StringComparison.Ordinal) && name.Length != suffix.Length;
    }

    internal static bool IsEffectivelyGeneric(INamedTypeSymbol symbol)
    {
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            if (current.Arity != 0)
            {
                return true;
            }
        }

        return false;
    }

    internal static string GetDiagnosticTypeName(INamedTypeSymbol symbol)
    {
        return symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static Diagnostic CreateInheritanceDiagnostic(
        INamedTypeSymbol invalidLayer,
        string reason,
        CancellationToken cancellationToken)
    {
        return Diagnostic.Create(
            AtomUIDiagnosticDescriptors.ControlTokenInheritance,
            GetTypeLocation(invalidLayer, cancellationToken),
            GetDiagnosticTypeName(invalidLayer),
            reason);
    }

    private static Location? GetTypeLocation(
        INamedTypeSymbol symbol,
        CancellationToken cancellationToken)
    {
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
        return syntax is TypeDeclarationSyntax declaration
            ? declaration.Identifier.GetLocation()
            : symbol.Locations.FirstOrDefault(static location => location.IsInSource);
    }
}
