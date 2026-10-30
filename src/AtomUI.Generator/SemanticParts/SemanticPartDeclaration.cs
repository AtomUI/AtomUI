using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class SemanticPartDeclaration
{
    internal SemanticPartDeclaration(
        string name,
        string path,
        string? selectorClass,
        string? selectorRoute,
        ITypeSymbol? contractType,
        int cardinality,
        int customization,
        string? themePropertyName,
        bool crossVisualRoot,
        string? since,
        bool runtimeCreated,
        Location location,
        ITypeSymbol? themeTargetType = null,
        bool crossNestedOwners = false,
        bool restHidden = false)
    {
        Name = name;
        Path = path;
        SelectorClass = selectorClass;
        SelectorRoute = selectorRoute;
        ContractType = contractType;
        Cardinality = cardinality;
        Customization = customization;
        ThemePropertyName = themePropertyName;
        CrossVisualRoot = crossVisualRoot;
        Since = since;
        RuntimeCreated = runtimeCreated;
        CrossNestedOwners = crossNestedOwners;
        RestHidden = restHidden;
        Location = location;
        ThemeTargetType = themeTargetType;
    }

    internal string Name { get; }
    internal string Path { get; }
    internal string? SelectorClass { get; }
    internal string? SelectorRoute { get; }
    internal ITypeSymbol? ContractType { get; }
    internal int Cardinality { get; }
    internal int Customization { get; }
    internal string? ThemePropertyName { get; }
    internal bool CrossVisualRoot { get; }
    internal string? Since { get; }
    internal bool RuntimeCreated { get; }
    internal bool CrossNestedOwners { get; }
    internal bool RestHidden { get; }
    internal Location Location { get; }
    internal ITypeSymbol? ThemeTargetType { get; }

    internal SemanticPartDeclaration WithThemeTargetType(ITypeSymbol? targetType)
    {
        return new SemanticPartDeclaration(
            Name,
            Path,
            SelectorClass,
            SelectorRoute,
            ContractType,
            Cardinality,
            Customization,
            ThemePropertyName,
            CrossVisualRoot,
            Since,
            RuntimeCreated,
            Location,
            targetType,
            CrossNestedOwners,
            RestHidden);
    }

    internal SemanticPartDeclaration WithSelectorRoute(string selectorRoute)
    {
        return new SemanticPartDeclaration(
            Name,
            Path,
            SelectorClass,
            selectorRoute,
            ContractType,
            Cardinality,
            Customization,
            ThemePropertyName,
            CrossVisualRoot,
            Since,
            RuntimeCreated,
            Location,
            ThemeTargetType,
            CrossNestedOwners,
            RestHidden);
    }
}

internal sealed class SemanticControlDeclaration
{
    private const int SelectorCustomization = 1;

    private SemanticControlDeclaration(
        INamedTypeSymbol controlType,
        IReadOnlyList<SemanticPartDeclaration> parts,
        Location location)
    {
        ControlType = controlType;
        Parts = parts;
        Location = location;
    }

    internal INamedTypeSymbol ControlType { get; }
    internal IReadOnlyList<SemanticPartDeclaration> Parts { get; }
    internal Location Location { get; }

    internal static SemanticControlDeclaration Create(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var controlType = (INamedTypeSymbol)context.TargetSymbol;
        var parts = context.Attributes
                           .Select(attribute => CreatePart(attribute, context.TargetNode.GetLocation(), cancellationToken))
                           .OrderBy(static part => part.Path, StringComparer.Ordinal)
                           .ToArray();
        return new SemanticControlDeclaration(
            controlType,
            parts,
            context.TargetNode.GetLocation());
    }

    internal static SemanticControlDeclaration Create(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        var location = type.Locations.FirstOrDefault() ?? Location.None;
        var parts = type.GetAttributes().Where(attribute => attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.SemanticPartAttribute)
            .Select(attribute => CreatePart(attribute, location, cancellationToken)).OrderBy(part => part.Path, StringComparer.Ordinal).ToArray();
        return new SemanticControlDeclaration(type, parts, location);
    }

    internal static SemanticControlDeclaration Merge(IEnumerable<SemanticControlDeclaration> declarations)
    {
        var declarationArray = declarations
                               .OrderBy(static declaration =>
                                   declaration.Location.SourceTree?.FilePath ?? string.Empty,
                                   StringComparer.Ordinal)
                               .ThenBy(static declaration => declaration.Location.SourceSpan.Start)
                               .ToArray();
        var first = declarationArray[0];
        return new SemanticControlDeclaration(
            first.ControlType,
            declarationArray.SelectMany(static declaration => declaration.Parts)
                            .OrderBy(static part => part.Path, StringComparer.Ordinal)
                            .ToArray(),
            first.Location);
    }

    internal SemanticControlDeclaration WithParts(IReadOnlyList<SemanticPartDeclaration> parts)
    {
        return new SemanticControlDeclaration(ControlType, parts, Location);
    }

    private static SemanticPartDeclaration CreatePart(
        AttributeData attribute,
        Location fallbackLocation,
        CancellationToken cancellationToken)
    {
        var name = attribute.ConstructorArguments.Length == 1
            ? attribute.ConstructorArguments[0].Value as string ?? string.Empty
            : string.Empty;
        var path = name;
        string? selectorClass = null;
        string? selectorRoute = null;
        ITypeSymbol? contractType = null;
        var cardinality = 0;
        var customization = SelectorCustomization;
        string? themePropertyName = null;
        var crossVisualRoot = false;
        string? since = null;
        var runtimeCreated = false;
        var crossNestedOwners = false;
        var restHidden = false;

        foreach (var argument in attribute.NamedArguments)
        {
            switch (argument.Key)
            {
                case "Path":
                    path = argument.Value.Value as string ?? string.Empty;
                    break;
                case "SelectorClass":
                    selectorClass = argument.Value.Value as string;
                    break;
                case "SelectorRoute":
                    selectorRoute = argument.Value.Value as string;
                    break;
                case "ContractType":
                    contractType = argument.Value.Value as ITypeSymbol;
                    break;
                case "Cardinality":
                    cardinality = Convert.ToInt32(argument.Value.Value, System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Customization":
                    customization = Convert.ToInt32(argument.Value.Value, System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "ThemePropertyName":
                    themePropertyName = argument.Value.Value as string;
                    break;
                case "CrossVisualRoot":
                    crossVisualRoot = argument.Value.Value is true;
                    break;
                case "Since":
                    since = argument.Value.Value as string;
                    break;
                case "RuntimeCreated":
                    runtimeCreated = argument.Value.Value is true;
                    break;
                case "CrossNestedOwners":
                    crossNestedOwners = argument.Value.Value is true;
                    break;
                case "RestHidden":
                    restHidden = argument.Value.Value is true;
                    break;
            }
        }

        var location = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? fallbackLocation;
        return new SemanticPartDeclaration(
            name,
            path,
            selectorClass,
            selectorRoute,
            contractType,
            cardinality,
            customization,
            themePropertyName,
            crossVisualRoot,
            since,
            runtimeCreated,
            location,
            null,
            crossNestedOwners,
            restHidden);
    }
}
