using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AtomUI.Generator.Diagnostics;

namespace AtomUI.Generator;

internal class ControlTokenPropertyWalker : CSharpSyntaxWalker
{
    public ControlTokenInfo ControlTokenInfo { get; }
    private readonly SemanticModel _semanticModel;
    private readonly CancellationToken _cancellationToken;

    public ControlTokenPropertyWalker(
        SemanticModel semanticModel,
        CancellationToken cancellationToken = default)
    {
        _semanticModel = semanticModel;
        _cancellationToken = cancellationToken;
        ControlTokenInfo = new ControlTokenInfo();
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        // Properties are collected from symbols in VisitClassDeclaration so inherited
        // Token definitions and their concrete types use one deterministic path.
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        ControlTokenInfo.TokenName = node.Identifier.Text;
        ControlTokenInfo.DeclarationLocation = node.Identifier.GetLocation();

        var classDeclaredSymbol = _semanticModel.GetDeclaredSymbol(node, _cancellationToken);
        if (classDeclaredSymbol is not null)
        {
            ControlTokenInfo.TokenNamespace = classDeclaredSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : classDeclaredSymbol.ContainingNamespace.ToDisplayString();
            ClassifyRole(node, classDeclaredSymbol);
            ReadTokenName(node, classDeclaredSymbol);
            var inheritance = ControlTokenInheritanceModel.Resolve(
                classDeclaredSymbol,
                _cancellationToken);
            ControlTokenInfo.Diagnostics.AddRange(inheritance.Diagnostics);
            if (inheritance.IsValid)
            {
                AddTokenProperties(classDeclaredSymbol, inheritance.Layers);
                ControlTokenInfo.Aliases = ControlTokenAliasModel.Build(classDeclaredSymbol,
                    inheritance.Layers, _cancellationToken, ControlTokenInfo.Diagnostics);
                ControlTokenInfo.DefaultPlan = ControlTokenDefaultModel.Build(classDeclaredSymbol,
                    inheritance.Layers, _semanticModel.Compilation, _cancellationToken,
                    ControlTokenInfo.Diagnostics, ControlTokenInfo.Aliases, out var dependencyDefaults);
                if (!dependencyDefaults)
                {
                    ControlTokenInfo.Diagnostics.AddRange(
                        ControlTokenCalculationValidator.Validate(inheritance.Layers,
                            _semanticModel.Compilation, _cancellationToken));
                }
            }
        }
    }

    private void ClassifyRole(ClassDeclarationSyntax node, INamedTypeSymbol classSymbol)
    {
        ControlTokenInfo.Role = classSymbol.IsAbstract
            ? ControlTokenRole.AbstractLayer
            : ControlTokenRole.Terminal;

        if (ControlTokenInheritanceModel.IsEffectivelyGeneric(classSymbol))
        {
            ControlTokenInfo.Diagnostics.Add(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.ControlTokenGenericLayer,
                node.Identifier.GetLocation(),
                ControlTokenInheritanceModel.GetDiagnosticTypeName(classSymbol)));
        }
        else if (classSymbol.ContainingType is not null)
        {
            ControlTokenInfo.Diagnostics.Add(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.ControlTokenNestedType,
                node.Identifier.GetLocation(),
                ControlTokenInheritanceModel.GetDiagnosticTypeName(classSymbol)));
        }

        if (ControlTokenInfo.IsTerminal && !classSymbol.IsSealed)
        {
            ControlTokenInfo.Diagnostics.Add(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.ControlTokenMustBeSealed,
                node.Identifier.GetLocation(),
                classSymbol.Name));
        }
    }

    private void ReadTokenName(ClassDeclarationSyntax node, INamedTypeSymbol classSymbol)
    {
        const string suffix = "Token";
        if (!classSymbol.Name.EndsWith(suffix, StringComparison.Ordinal) ||
            classSymbol.Name.Length == suffix.Length)
        {
            ControlTokenInfo.Diagnostics.Add(Diagnostic.Create(
                classSymbol.IsAbstract
                    ? AtomUIDiagnosticDescriptors.AbstractControlTokenInvalidName
                    : AtomUIDiagnosticDescriptors.ControlTokenInvalidName,
                node.Identifier.GetLocation(),
                classSymbol.IsAbstract
                    ? ControlTokenInheritanceModel.GetDiagnosticTypeName(classSymbol)
                    : classSymbol.Name));
            return;
        }

        if (ControlTokenInfo.IsTerminal)
        {
            ControlTokenInfo.ControlName = classSymbol.Name.Substring(0, classSymbol.Name.Length - suffix.Length);
        }
    }

    private void AddTokenProperties(
        INamedTypeSymbol terminalType,
        IReadOnlyList<ControlTokenLayerInfo> layers)
    {
        var flattened = new Dictionary<string, FlattenedControlTokenProperty>(StringComparer.Ordinal);
        var inheritedMemberTypes = GetInheritedMemberTypes(layers[0].Symbol.BaseType);
        var terminalTypeName = terminalType.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType);
        foreach (var layer in layers)
        {
            var declaredMembers = layer.Symbol.GetMembers()
                                       .Where(static member => !member.IsImplicitlyDeclared)
                                       .ToArray();
            var conflictingNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in declaredMembers)
            {
                if (conflictingNames.Contains(member.Name))
                {
                    continue;
                }

                if (flattened.TryGetValue(member.Name, out var inheritedToken))
                {
                    AddPropertyConflict(
                        member.Name,
                        member.Locations.FirstOrDefault(),
                        inheritedToken.DeclaringType,
                        layer.Symbol);
                    conflictingNames.Add(member.Name);
                    continue;
                }

                if (member is IPropertySymbol property &&
                    !HasNotTokenDefinition(property) &&
                    inheritedMemberTypes.TryGetValue(property.Name, out var inheritedMemberType))
                {
                    AddPropertyConflict(
                        property.Name,
                        property.Locations.FirstOrDefault(),
                        inheritedMemberType,
                        layer.Symbol);
                    conflictingNames.Add(property.Name);
                }
            }

            foreach (var property in declaredMembers.OfType<IPropertySymbol>())
            {
                if (conflictingNames.Contains(property.Name) || HasNotTokenDefinition(property))
                {
                    continue;
                }

                if (!TryValidateProperty(property, out var reason))
                {
                    ControlTokenInfo.Diagnostics.Add(Diagnostic.Create(
                        AtomUIDiagnosticDescriptors.ControlTokenInvalidProperty,
                        property.Locations.FirstOrDefault(),
                        layer.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        property.Name,
                        reason));
                    continue;
                }

                var candidate = new FlattenedControlTokenProperty(
                    property.Name,
                    property.Type.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType),
                    layer.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    terminalTypeName,
                    property.Locations.FirstOrDefault(),
                    !SymbolEqualityComparer.Default.Equals(layer.Symbol, terminalType));
                if (flattened.TryGetValue(property.Name, out var existing))
                {
                    AddPropertyConflict(
                        property.Name,
                        candidate.Location,
                        existing.DeclaringType,
                        layer.Symbol);
                    continue;
                }

                flattened.Add(property.Name, candidate);
            }

            foreach (var member in declaredMembers)
            {
                if (!inheritedMemberTypes.ContainsKey(member.Name))
                {
                    inheritedMemberTypes.Add(
                        member.Name,
                        layer.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
        }

        if (ControlTokenInfo.Diagnostics.Count != 0)
        {
            return;
        }

        foreach (var property in flattened.Values)
        {
            ControlTokenInfo.Tokens.Add(new TokenName(property.Name, string.Empty));
            ControlTokenInfo.SchemaTokens.Add(new SchemaTokenInfo(
                property.Name,
                property.ValueType,
                property.TerminalType,
                SchemaTokenStage.Control));
        }
    }

    private void AddPropertyConflict(
        string memberName,
        Location? location,
        string inheritedDeclaringType,
        INamedTypeSymbol currentLayer)
    {
        ControlTokenInfo.Diagnostics.Add(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.ControlTokenPropertyConflict,
            location,
            memberName,
            inheritedDeclaringType,
            currentLayer.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
    }

    private static Dictionary<string, string> GetInheritedMemberTypes(INamedTypeSymbol? baseType)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var current = baseType; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers().Where(static member => !member.IsImplicitlyDeclared))
            {
                if (!result.ContainsKey(member.Name))
                {
                    result.Add(
                        member.Name,
                        current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
        }

        return result;
    }

    private bool TryValidateProperty(IPropertySymbol property, out string reason)
    {
        if (property.IsStatic)
        {
            reason = "static properties are not supported";
            return false;
        }
        if (property.IsIndexer)
        {
            reason = "indexers are not supported";
            return false;
        }
        if (property.ExplicitInterfaceImplementations.Length != 0)
        {
            reason = "explicit interface implementations are not supported";
            return false;
        }
        if (property.IsVirtual || property.IsAbstract || property.IsOverride)
        {
            reason = "virtual, abstract, and override properties are not supported";
            return false;
        }
        if (property.IsRequired)
        {
            reason = "required properties are not supported by the generated parameterless factory";
            return false;
        }
        if (property.GetMethod is null || property.SetMethod is null)
        {
            reason = "both getter and setter are required";
            return false;
        }
        if (property.SetMethod.IsInitOnly)
        {
            reason = "init-only setters are not supported by generated runtime writers";
            return false;
        }
        if (!_semanticModel.Compilation.IsSymbolAccessibleWithin(
                property.GetMethod,
                _semanticModel.Compilation.Assembly) ||
            !_semanticModel.Compilation.IsSymbolAccessibleWithin(
                property.SetMethod,
                _semanticModel.Compilation.Assembly))
        {
            reason = "getter and setter must be accessible to generated code";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool HasNotTokenDefinition(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.NotTokenDefinitionAttribute)
            {
                return true;
            }
        }

        return false;
    }

}
