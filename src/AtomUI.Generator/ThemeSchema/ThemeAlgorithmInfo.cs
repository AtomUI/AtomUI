using AtomUI.Generator.Diagnostics;
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

    /// <summary>
    /// Validates one <c>[ThemeAlgorithm]</c> class. A declaration that cannot be registered is reported as
    /// ATOMUITOKEN002 at the attribute (as <c>[ControlTokenAlgorithm]</c> does) and skipped; without
    /// <paramref name="reportDiagnostic"/> it is only skipped.
    /// </summary>
    internal static ThemeAlgorithmInfo? Create(
        INamedTypeSymbol type,
        Action<Diagnostic>? reportDiagnostic = null,
        CancellationToken cancellationToken = default)
    {
        var attribute = type.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.ThemeAlgorithmAttribute);
        if (attribute is null)
        {
            return null;
        }

        var typeName = type.ToDisplayString();
        if (type.TypeKind != TypeKind.Class ||
            type.IsAbstract ||
            type.IsGenericType ||
            !ImplementsThemeAlgorithm(type))
        {
            Report(reportDiagnostic, type, attribute, cancellationToken,
                $"'{typeName}' must be a non-abstract, non-generic class that implements IThemeAlgorithm.");
            return null;
        }

        if (attribute.ConstructorArguments.Length != 4 ||
            attribute.ConstructorArguments[0].Value is not string catalog ||
            attribute.ConstructorArguments[1].Value is not string id ||
            !ThemeBuildIdentifier.IsValid(catalog) ||
            !ThemeBuildIdentifier.IsValid(id) ||
            attribute.ConstructorArguments[2].Value is not int revision ||
            revision <= 0)
        {
            Report(reportDiagnostic, type, attribute, cancellationToken,
                $"'{typeName}' declares a theme algorithm with an invalid catalog, id or revision.");
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
            Report(reportDiagnostic, type, attribute, cancellationToken,
                $"'{typeName}' declares a theme algorithm with an invalid appearance effect.");
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

        Report(reportDiagnostic, type, attribute, cancellationToken,
            $"'{typeName}' must have a public parameterless constructor.");
        return null;
    }

    private static void Report(
        Action<Diagnostic>? reportDiagnostic,
        INamedTypeSymbol type,
        AttributeData attribute,
        CancellationToken cancellationToken,
        string message)
    {
        if (reportDiagnostic is null)
        {
            return;
        }

        var location = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ??
                       type.Locations.FirstOrDefault(static candidate => candidate.IsInSource) ??
                       Location.None;
        reportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed, location, message));
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
