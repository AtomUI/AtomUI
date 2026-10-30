using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.Localization.Catalog;

internal static class LanguageCatalogSymbolParser
{
    internal static LanguageCatalogParseResult Parse(
        LanguageCatalogTarget target,
        string moduleId,
        CancellationToken cancellationToken)
    {
        var symbol = target.Symbol;
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var typeLocation = GetTypeIdentifierLocation(symbol, cancellationToken);
        if (symbol.TypeKind != TypeKind.Enum || symbol.IsGenericType)
        {
            diagnostics.Add(InvalidCatalog(
                symbol,
                typeLocation,
                "the target must be a non-generic enum"));
            return new LanguageCatalogParseResult(null, diagnostics.ToImmutable());
        }

        if (!IsEffectivelyPublic(symbol))
        {
            diagnostics.Add(InvalidCatalog(
                symbol,
                typeLocation,
                "the Catalog enum and all containing types must be public"));
        }

        if (symbol.GetAttributes().Any(static attribute =>
                attribute.AttributeClass?.ToDisplayString() == "System.FlagsAttribute"))
        {
            diagnostics.Add(InvalidCatalog(
                symbol,
                typeLocation,
                "Flags Catalog enums are not supported"));
        }

        var fields = symbol.GetMembers()
                           .OfType<IFieldSymbol>()
                           .Where(static field => !field.IsImplicitlyDeclared && field.HasConstantValue)
                           .ToArray();
        if (fields.Length == 0)
        {
            diagnostics.Add(InvalidCatalog(
                symbol,
                typeLocation,
                "the Catalog must declare at least one resource unit"));
        }

        var units = ImmutableArray.CreateBuilder<LanguageCatalogUnitInfo>();
        foreach (var field in fields)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var declaration = field.DeclaringSyntaxReferences
                                   .Select(reference => reference.GetSyntax(cancellationToken))
                                   .OfType<EnumMemberDeclarationSyntax>()
                                   .FirstOrDefault();
            if (declaration?.EqualsValue is not null)
            {
                diagnostics.Add(InvalidUnit(
                    symbol,
                    field,
                    declaration.EqualsValue.Value.GetLocation(),
                    "the unit must not declare an explicit numeric value"));
                continue;
            }

            units.Add(new LanguageCatalogUnitInfo(field.Name, field.Locations[0], Convert.ToInt32(field.ConstantValue, System.Globalization.CultureInfo.InvariantCulture)));
        }

        if (diagnostics.Count > 0)
        {
            return new LanguageCatalogParseResult(null, diagnostics.ToImmutable());
        }

        var namespaceName = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();
        var metadataName = GetMetadataName(symbol);
        var typeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var catalog = new LanguageCatalogInfo(
            moduleId,
            metadataName,
            namespaceName,
            typeName,
            units.OrderBy(static unit => unit.Key, StringComparer.Ordinal).ToImmutableArray(),
            typeLocation);
        return new LanguageCatalogParseResult(catalog, ImmutableArray<Diagnostic>.Empty);
    }

    private static bool IsEffectivelyPublic(INamedTypeSymbol symbol)
    {
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }
        }

        return true;
    }

    private static string GetMetadataName(INamedTypeSymbol symbol)
    {
        var typeNames = new Stack<string>();
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            typeNames.Push(current.MetadataName);
        }

        var typeName = string.Join("+", typeNames);
        return symbol.ContainingNamespace.IsGlobalNamespace
            ? typeName
            : $"{symbol.ContainingNamespace.ToDisplayString()}.{typeName}";
    }

    private static Location GetTypeIdentifierLocation(
        INamedTypeSymbol symbol,
        CancellationToken cancellationToken)
    {
        var declaration = symbol.DeclaringSyntaxReferences
                                .Select(reference => reference.GetSyntax(cancellationToken))
                                .OfType<BaseTypeDeclarationSyntax>()
                                .FirstOrDefault();
        return declaration?.Identifier.GetLocation() ??
               symbol.Locations.FirstOrDefault() ??
               Location.None;
    }

    private static Diagnostic InvalidCatalog(
        INamedTypeSymbol catalog,
        Location location,
        string reason)
    {
        return Diagnostic.Create(
            AtomUIDiagnosticDescriptors.LocalizationInvalidCatalog,
            location,
            catalog.Name,
            reason);
    }

    private static Diagnostic InvalidUnit(
        INamedTypeSymbol catalog,
        IFieldSymbol field,
        Location location,
        string reason)
    {
        return Diagnostic.Create(
            AtomUIDiagnosticDescriptors.LocalizationInvalidCatalogUnit,
            location,
            field.Name,
            catalog.ToDisplayString(),
            reason);
    }
}

internal sealed class LanguageCatalogTarget
{
    internal LanguageCatalogTarget(INamedTypeSymbol symbol, AttributeData attribute)
    {
        Symbol = symbol;
        Attribute = attribute;
    }

    internal INamedTypeSymbol Symbol { get; }

    internal AttributeData Attribute { get; }
}
