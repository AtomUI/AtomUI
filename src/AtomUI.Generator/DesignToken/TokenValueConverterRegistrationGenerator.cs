using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

/// <summary>
/// Generates <c>TokenValueConverterRegistry.g.cs</c>, the registry of every type marked with
/// <c>[TokenValueConverter]</c>.
/// </summary>
[Generator]
public class TokenValueConverterRegistrationGenerator : IIncrementalGenerator
{
    private const string TokenValueConverterInterface = "global::AtomUI.Theme.DesignTokens.ITokenValueConverter";

    /// <summary>
    /// Registers the implementation source output that collects the types marked with <c>[TokenValueConverter]</c>,
    /// sorted and de-duplicated by type name, and writes the registry when there is at least one.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var converterTypesProvider = initContext.SyntaxProvider.ForAttributeWithMetadataName(
            TargetMarkConstants.TokenValueConverterAttribute,
            static (node, token) => true,
            static (context, token) => GetConverterTypeName(context))
            .Where(static typeName => !string.IsNullOrWhiteSpace(typeName))
            .Collect()
            .Select(static (typeNames, token) => typeNames.Distinct().OrderBy(typeName => typeName).ToArray());

        initContext.RegisterImplementationSourceOutput(converterTypesProvider, static (context, converterTypes) =>
        {
            if (converterTypes.Length == 0)
            {
                return;
            }

            var classWriter = new TokenValueConverterRegistryClassWriter(context, converterTypes!);
            classWriter.Write();
        });
    }

    private static string? GetConverterTypeName(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        if (typeSymbol.TypeKind != TypeKind.Class ||
            typeSymbol.IsAbstract ||
            typeSymbol.IsGenericType ||
            !ImplementsTokenValueConverter(typeSymbol))
        {
            return null;
        }

        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static bool ImplementsTokenValueConverter(INamedTypeSymbol typeSymbol)
    {
        foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
        {
            if (interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == TokenValueConverterInterface)
            {
                return true;
            }
        }

        return false;
    }
}
