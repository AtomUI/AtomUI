using AtomUI.Theme.Build;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class ThemeAlgorithmInfo
{
    private const string ThemeAlgorithmInterface = "global::AtomUI.Theme.Algorithms.IThemeAlgorithm";

    internal ThemeAlgorithmInfo(
        string catalog,
        string id,
        int revision,
        string appearanceEffect,
        string typeName)
    {
        Catalog = catalog;
        Id = id;
        Revision = revision;
        AppearanceEffect = appearanceEffect;
        TypeName = typeName;
    }

    public string Catalog { get; }
    public string Id { get; }
    public int Revision { get; }
    public string AppearanceEffect { get; }
    public string TypeName { get; }

    // Same order as the runtime ThemeAlgorithm.Compare, so generated registrations arrive sorted.
    internal static IEnumerable<ThemeAlgorithmInfo> Order(IEnumerable<ThemeAlgorithmInfo> algorithms) =>
        algorithms.OrderBy(static algorithm => algorithm.Catalog, StringComparer.Ordinal)
                  .ThenBy(static algorithm => algorithm.Id, StringComparer.Ordinal)
                  .ThenBy(static algorithm => algorithm.TypeName, StringComparer.Ordinal);

    internal static ThemeAlgorithmInfo? Create(GeneratorAttributeSyntaxContext context) =>
        context.TargetSymbol is INamedTypeSymbol type ? Create(type) : null;

    internal static ThemeAlgorithmInfo? Create(INamedTypeSymbol type)
    {
        if (
            type.IsAbstract ||
            type.IsGenericType ||
            !ImplementsThemeAlgorithm(type))
        {
            return null;
        }

        var attribute = type.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.ThemeAlgorithmAttribute);
        if (attribute is null ||
            attribute.ConstructorArguments.Length != 4 ||
            attribute.ConstructorArguments[0].Value is not string catalog ||
            attribute.ConstructorArguments[1].Value is not string id ||
            !ThemeBuildIdentifier.IsValid(catalog) ||
            !ThemeBuildIdentifier.IsValid(id) ||
            attribute.ConstructorArguments[2].Value is not int revision ||
            revision <= 0)
        {
            return null;
        }

        var appearanceValue = attribute.ConstructorArguments[3].Value switch
        {
            byte value  => value,
            short value => value,
            int value   => value,
            _           => -1
        };

        var appearance = appearanceValue switch
        {
            0 => "Preserve",
            1 => "Light",
            2 => "Dark",
            _ => null
        };
        if (appearance is null)
        {
            return null;
        }

        var parameterless = type.InstanceConstructors.FirstOrDefault(static constructor =>
            constructor.DeclaredAccessibility == Accessibility.Public && constructor.Parameters.Length == 0);
        if (parameterless is not null)
        {
            return new ThemeAlgorithmInfo(
                catalog,
                id,
                revision,
                appearance,
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        return null;
    }

    private static bool ImplementsThemeAlgorithm(ITypeSymbol type)
    {
        if (type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ThemeAlgorithmInterface)
        {
            return true;
        }

        foreach (var interfaceType in type.AllInterfaces)
        {
            if (interfaceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ThemeAlgorithmInterface)
            {
                return true;
            }
        }

        return false;
    }
}
