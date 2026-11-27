using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.ThemeConsumption;

/// <summary>
/// ATOMUIGEN047: control code must not write LocalValue into visual properties;
/// use <c>SetCurrentValue</c> or a Template-priority write so user values and theme Setters keep precedence.
/// <para>
/// A visual StyledProperty is a static field of type <c>StyledProperty&lt;T&gt;</c> / <c>AvaloniaProperty&lt;T&gt;</c>
/// (also through subclasses) named <c>&lt;Name&gt;Property</c> where <c>Name</c> is one of Background, Foreground,
/// BorderBrush, BorderThickness, CornerRadius, Padding, Margin, FontSize, FontWeight, Opacity, Fill, Stroke,
/// StrokeThickness, BoxShadow and <c>T</c> (nullable unwrapped) is IBrush, Color, Thickness, CornerRadius, double,
/// FontWeight or BoxShadows.
/// </para>
/// <para>
/// Reported: <c>SetValue(visualProperty, value)</c> without a priority argument or with
/// <c>BindingPriority.LocalValue</c> (constructors included), except inside a property <c>set</c> accessor (the CLR
/// wrapper of a StyledProperty); and plain assignments <c>x.Background = value</c> to the CLR property of such a
/// StyledProperty on a <c>Control</c>-derived receiver, except inside constructors and inside object initialisers of
/// newly created objects. Not detected: compound assignments, <c>SetValue</c> with a priority held in a variable,
/// writes through <c>AvaloniaProperty</c> (non-generic) arguments or reflection, <c>Bind</c> calls. Assemblies that do
/// not consume <c>SharedTokenKind</c>, the assembly defining it and <c>[ControlDesignToken]</c> types are exempt.
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LocalValueVisualWriteAnalyzer : DiagnosticAnalyzer
{
    private static readonly HashSet<string> s_visualPropertyNames = new(StringComparer.Ordinal)
    {
        "Background", "Foreground", "BorderBrush", "BorderThickness", "CornerRadius", "Padding", "Margin",
        "FontSize", "FontWeight", "Opacity", "Fill", "Stroke", "StrokeThickness", "BoxShadow"
    };

    private static readonly string[] s_valueTypeMetadataNames =
    [
        "Avalonia.Media.IBrush", "Avalonia.Media.Color", "Avalonia.Thickness", "Avalonia.CornerRadius",
        "System.Double", "Avalonia.Media.FontWeight", "Avalonia.Media.BoxShadows"
    ];

    /// <summary>
    /// The single descriptor <c>ControlCodeLocalValueVisualWrite</c> that this analyzer reports.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        AtomUIDiagnosticDescriptors.ControlCodeLocalValueVisualWrite
    ];

    /// <summary>
    /// Registers the actions that report ATOMUIGEN047 in the control code of the compilation. Generated code is not
    /// analyzed. Nothing is registered when <c>ControlCodeScope.TryCreate</c> returns <see langword="null" /> or the
    /// compilation does not reference <c>AvaloniaObject</c>, <c>AvaloniaProperty&lt;T&gt;</c>, <c>Control</c> or
    /// <c>BindingPriority</c>.
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            var scope = ControlCodeScope.TryCreate(
                compilationContext.Compilation, compilationContext.Options.AnalyzerConfigOptionsProvider,
                compilationContext.CancellationToken);
            var avaloniaObject = compilationContext.Compilation.GetTypeByMetadataName("Avalonia.AvaloniaObject");
            var avaloniaProperty = compilationContext.Compilation.GetTypeByMetadataName("Avalonia.AvaloniaProperty`1");
            var control = compilationContext.Compilation.GetTypeByMetadataName("Avalonia.Controls.Control");
            var bindingPriority = compilationContext.Compilation.GetTypeByMetadataName("Avalonia.Data.BindingPriority");
            if (scope is null || avaloniaObject is null || avaloniaProperty is null || control is null || bindingPriority is null)
            {
                return;
            }

            var valueTypes = new List<INamedTypeSymbol>();
            foreach (var metadataName in s_valueTypeMetadataNames)
            {
                if (compilationContext.Compilation.GetTypeByMetadataName(metadataName) is { } valueType)
                {
                    valueTypes.Add(valueType);
                }
            }

            var symbols = new Symbols(scope, avaloniaObject, avaloniaProperty, control, bindingPriority, [.. valueTypes]);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, symbols),
                SyntaxKind.InvocationExpression);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeAssignment(nodeContext, symbols),
                SyntaxKind.SimpleAssignmentExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, Symbols symbols)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (!string.Equals(ControlCodeScope.GetInvokedName(invocation), "SetValue", StringComparison.Ordinal) ||
            invocation.ArgumentList.Arguments.Count < 2)
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, symbols.AvaloniaObject))
        {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;
        if (!TryGetVisualProperty(semanticModel.GetSymbolInfo(arguments[0].Expression, context.CancellationToken).Symbol,
                symbols, out var propertyName) ||
            !IsLocalValuePriority(semanticModel, arguments, method, symbols, context.CancellationToken) ||
            IsInsidePropertySetter(invocation) ||
            symbols.Scope.IsInsideTokenDefinition(semanticModel, invocation, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.ControlCodeLocalValueVisualWrite,
            invocation.GetLocation(),
            "SetValue",
            propertyName));
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context, Symbols symbols)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        if (ControlCodeScope.GetRightmostName(assignment.Left) is not { } name ||
            !s_visualPropertyNames.Contains(name) ||
            IsInsideNewObjectInitializer(assignment))
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol is not IPropertySymbol
            {
                IsStatic: false,
                IsIndexer: false
            } property ||
            !IsStyledPropertyWrapper(property, name, symbols) ||
            !IsControlReceiver(semanticModel, assignment.Left, symbols, context.CancellationToken) ||
            IsInsideConstructor(assignment) ||
            symbols.Scope.IsInsideTokenDefinition(semanticModel, assignment, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.ControlCodeLocalValueVisualWrite,
            assignment.GetLocation(),
            "A property assignment",
            name));
    }

    private static bool IsLocalValuePriority(SemanticModel semanticModel, SeparatedSyntaxList<ArgumentSyntax> arguments,
        IMethodSymbol method, Symbols symbols, CancellationToken cancellationToken)
    {
        for (var i = 0; i < arguments.Count; i++)
        {
            var parameterName = arguments[i].NameColon?.Name.Identifier.ValueText ??
                                (i < method.Parameters.Length ? method.Parameters[i].Name : null);
            if (!string.Equals(parameterName, "priority", StringComparison.Ordinal))
            {
                continue;
            }

            // Only the literal enum member is judged; a priority held in a variable is not tracked.
            return semanticModel.GetSymbolInfo(arguments[i].Expression, cancellationToken).Symbol is IFieldSymbol
                   {
                       Name: "LocalValue"
                   } field &&
                   SymbolEqualityComparer.Default.Equals(field.ContainingType, symbols.BindingPriority);
        }

        return true;
    }

    private static bool TryGetVisualProperty(ISymbol? symbol, Symbols symbols, out string propertyName)
    {
        propertyName = string.Empty;
        if (symbol is not IFieldSymbol { IsStatic: true } field ||
            !field.Name.EndsWith("Property", StringComparison.Ordinal))
        {
            return false;
        }

        var name = field.Name.Substring(0, field.Name.Length - "Property".Length);
        if (!s_visualPropertyNames.Contains(name) || !HasVisualValueType(field.Type, symbols))
        {
            return false;
        }

        propertyName = name;
        return true;
    }

    private static bool HasVisualValueType(ITypeSymbol propertyType, Symbols symbols)
    {
        for (var current = propertyType; current is not null; current = current.BaseType)
        {
            if (current is not INamedTypeSymbol { IsGenericType: true } named ||
                !SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, symbols.AvaloniaProperty))
            {
                continue;
            }

            var valueType = named.TypeArguments[0];
            if (valueType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            {
                valueType = nullable.TypeArguments[0];
            }

            foreach (var allowed in symbols.ValueTypes)
            {
                if (SymbolEqualityComparer.Default.Equals(valueType, allowed))
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// The CLR property is the wrapper of a visual StyledProperty field declared on the same type chain.
    /// </summary>
    private static bool IsStyledPropertyWrapper(IPropertySymbol property, string name, Symbols symbols)
    {
        for (var type = property.ContainingType; type is not null; type = type.BaseType)
        {
            foreach (var member in type.GetMembers(name + "Property"))
            {
                if (TryGetVisualProperty(member, symbols, out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsControlReceiver(SemanticModel semanticModel, ExpressionSyntax left, Symbols symbols, CancellationToken cancellationToken)
    {
        ITypeSymbol? receiverType;
        switch (left)
        {
            case MemberAccessExpressionSyntax memberAccess:
                receiverType = semanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken).Type;
                break;
            default:
                var enclosingType = left.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                receiverType = enclosingType is null
                    ? null
                    : semanticModel.GetDeclaredSymbol(enclosingType, cancellationToken);
                break;
        }

        for (var current = receiverType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, symbols.Control))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInsideConstructor(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            switch (current)
            {
                case ConstructorDeclarationSyntax:
                    return true;
                // Closures and local functions run later, outside the construction phase.
                case AnonymousFunctionExpressionSyntax:
                case LocalFunctionStatementSyntax:
                case MemberDeclarationSyntax:
                    return false;
            }
        }

        return false;
    }

    private static bool IsInsidePropertySetter(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            switch (current)
            {
                case AccessorDeclarationSyntax accessor:
                    return accessor.Kind() is SyntaxKind.SetAccessorDeclaration or SyntaxKind.InitAccessorDeclaration;
                case AnonymousFunctionExpressionSyntax:
                case LocalFunctionStatementSyntax:
                case MemberDeclarationSyntax:
                    return false;
            }
        }

        return false;
    }

    private static bool IsInsideNewObjectInitializer(AssignmentExpressionSyntax assignment)
    {
        return assignment.Parent is InitializerExpressionSyntax initializer &&
               initializer.IsKind(SyntaxKind.ObjectInitializerExpression) &&
               initializer.Parent is BaseObjectCreationExpressionSyntax;
    }

    private sealed record Symbols(
        ControlCodeScope Scope,
        INamedTypeSymbol AvaloniaObject,
        INamedTypeSymbol AvaloniaProperty,
        INamedTypeSymbol Control,
        INamedTypeSymbol BindingPriority,
        ImmutableArray<INamedTypeSymbol> ValueTypes);
}
