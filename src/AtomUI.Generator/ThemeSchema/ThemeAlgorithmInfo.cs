using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class ThemeAlgorithmInfo
{
    private const string ThemeAlgorithmInterface = "global::AtomUI.Theme.Algorithms.IThemeAlgorithm";

    internal ThemeAlgorithmInfo(
        string algorithm,
        int algorithmValue,
        int revision,
        string appearanceEffect,
        string typeName)
    {
        Algorithm = algorithm;
        AlgorithmValue = algorithmValue;
        Revision = revision;
        AppearanceEffect = appearanceEffect;
        TypeName = typeName;
    }

    public string Algorithm { get; }
    public int AlgorithmValue { get; }
    public int Revision { get; }
    public string AppearanceEffect { get; }
    public string TypeName { get; }

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
            attribute.ConstructorArguments.Length != 3 ||
            attribute.ConstructorArguments[0] is not
            {
                Kind: TypedConstantKind.Enum,
                Type: INamedTypeSymbol algorithmType,
                Value: int algorithmValue
            } ||
            attribute.ConstructorArguments[1].Value is not int revision ||
            revision <= 0)
        {
            return null;
        }

        var algorithm = algorithmType.GetMembers()
                                     .OfType<IFieldSymbol>()
                                     .SingleOrDefault(field =>
                                         field.HasConstantValue &&
                                         field.ConstantValue is int value &&
                                         value == algorithmValue);
        if (algorithm is null)
        {
            return null;
        }

        var appearanceValue = attribute.ConstructorArguments[2].Value switch
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
                algorithm.Name,
                algorithmValue,
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
