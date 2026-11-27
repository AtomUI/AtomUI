using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.DataMemberAccessors;

/// <summary>
/// Checks that the data member paths used for sorting and filtering are covered by generated accessors for native
/// AOT. It analyzes the first argument of <c>FromPath</c> on
/// <c>AtomUI.Controls.Data.ListSortDescription</c> and <c>AtomUI.Desktop.Controls.Data.DataGridSortDescription</c>,
/// and the value assigned to <c>SortMemberPath</c> or <c>FilterMemberPath</c> of a
/// <c>AtomUI.Desktop.Controls.DataGridColumn</c> or a type derived from it.
/// </summary>
/// <remarks>
/// A path written as <c>nameof</c> of a property is checked against the property's declaring type, its base types
/// and its interfaces. It passes when that type is a non-generic class, struct or interface and one of them is
/// marked with <c>GenerateDataMemberAccessors</c> and exposes an accessible instance property of the path's name.
/// <see cref="AtomUIDiagnosticDescriptors.AotMissingGeneratedAccessor" /> (ATOMUIAOT001) is reported when none is
/// marked, and <see cref="AtomUIDiagnosticDescriptors.AotMissingGeneratedPath" /> (ATOMUIAOT002) when a marked type
/// exists but lacks the property.
/// <see cref="AtomUIDiagnosticDescriptors.AotUnverifiableDataMemberPath" /> (ATOMUIAOT003) is reported for a
/// non-empty constant that is not such a <c>nameof</c> expression; write <c>nameof(Type.Property)</c> instead.
/// All three are warnings. Other expressions, including an <c>IDataMemberAccessorDescriptor</c> passed in place of
/// the path, are not checked.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AotDataMemberPathAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        AtomUIDiagnosticDescriptors.AotMissingGeneratedAccessor,
        AtomUIDiagnosticDescriptors.AotMissingGeneratedPath,
        AtomUIDiagnosticDescriptors.AotUnverifiableDataMemberPath
    ];

    /// <summary>
    /// Registers the analysis of invocations and of simple assignments. Generated code is not analyzed.
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method ||
            !IsAotSensitiveFromPathMethod(method))
        {
            return;
        }

        var pathExpression = invocation.ArgumentList.Arguments[0].Expression;
        AnalyzePathExpression(context, pathExpression);
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        if (!IsAotSensitiveDataGridPathAssignment(context, assignment.Left))
        {
            return;
        }

        AnalyzePathExpression(context, assignment.Right);
    }

    private static bool IsAotSensitiveFromPathMethod(IMethodSymbol method)
    {
        if (!string.Equals(method.Name, "FromPath", StringComparison.Ordinal))
        {
            return false;
        }

        var containingType = method.ContainingType?.ToDisplayString();
        return containingType is "AtomUI.Controls.Data.ListSortDescription" or
                                 "AtomUI.Desktop.Controls.Data.DataGridSortDescription";
    }

    private static bool IsAotSensitiveDataGridPathAssignment(SyntaxNodeAnalysisContext context, ExpressionSyntax left)
    {
        if (context.SemanticModel.GetSymbolInfo(left, context.CancellationToken).Symbol is not IPropertySymbol property)
        {
            return false;
        }

        if (!string.Equals(property.Name, "SortMemberPath", StringComparison.Ordinal) &&
            !string.Equals(property.Name, "FilterMemberPath", StringComparison.Ordinal))
        {
            return false;
        }

        return IsOrInheritsFrom(property.ContainingType, "AtomUI.Desktop.Controls.DataGridColumn");
    }

    private static bool IsOrInheritsFrom(INamedTypeSymbol? typeSymbol, string metadataName)
    {
        for (var current = typeSymbol; current != null; current = current.BaseType)
        {
            if (string.Equals(current.ToDisplayString(), metadataName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void AnalyzePathExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax pathExpression)
    {
        if (!TryGetNameofPropertyPath(context, pathExpression, out var property, out var diagnosticLocation))
        {
            ReportUnverifiableConstantPath(context, pathExpression);
            return;
        }

        var modelType = property.ContainingType;
        var hasGeneratedAccessor = false;
        var hasCompatibleAccessor = CanGenerateAccessorsFor(modelType) &&
                                    HasCompatibleGeneratedAccessor(modelType, property.Name, out hasGeneratedAccessor);
        if (!hasCompatibleAccessor)
        {
            var rule = hasGeneratedAccessor
                ? AtomUIDiagnosticDescriptors.AotMissingGeneratedPath
                : AtomUIDiagnosticDescriptors.AotMissingGeneratedAccessor;
            context.ReportDiagnostic(Diagnostic.Create(
                rule,
                diagnosticLocation,
                property.Name,
                modelType.ToDisplayString()));
        }
    }

    private static void ReportUnverifiableConstantPath(SyntaxNodeAnalysisContext context, ExpressionSyntax pathExpression)
    {
        var constantValue = context.SemanticModel.GetConstantValue(pathExpression, context.CancellationToken);
        if (!constantValue.HasValue || constantValue.Value is not string path || string.IsNullOrEmpty(path))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.AotUnverifiableDataMemberPath,
            pathExpression.GetLocation(),
            path));
    }

    private static bool TryGetNameofPropertyPath(SyntaxNodeAnalysisContext context,
                                                 ExpressionSyntax expression,
                                                 out IPropertySymbol property,
                                                 out Location location)
    {
        property = null!;
        location = expression.GetLocation();

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not IdentifierNameSyntax { Identifier.ValueText: "nameof" } ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        var targetExpression = invocation.ArgumentList.Arguments[0].Expression;
        if (context.SemanticModel.GetSymbolInfo(targetExpression, context.CancellationToken).Symbol is IPropertySymbol targetProperty)
        {
            property = targetProperty;
            location = targetExpression.GetLocation();
            return true;
        }

        return false;
    }

    private static bool CanGenerateAccessorsFor(INamedTypeSymbol typeSymbol)
    {
        return (typeSymbol.TypeKind == TypeKind.Class ||
                typeSymbol.TypeKind == TypeKind.Struct ||
                typeSymbol.TypeKind == TypeKind.Interface) &&
               !typeSymbol.IsGenericType;
    }

    private static bool HasCompatibleGeneratedAccessor(INamedTypeSymbol modelType,
                                                       string path,
                                                       out bool hasGeneratedAccessor)
    {
        hasGeneratedAccessor = false;

        foreach (var candidate in GetCompatibleAccessorTypes(modelType))
        {
            if (!HasGenerateDataMemberAccessorsAttribute(candidate))
            {
                continue;
            }

            hasGeneratedAccessor = true;
            if (HasGeneratedProperty(candidate, path))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetCompatibleAccessorTypes(INamedTypeSymbol modelType)
    {
        for (INamedTypeSymbol? current = modelType; current != null; current = current.BaseType)
        {
            yield return current;
        }

        foreach (var interfaceType in modelType.AllInterfaces)
        {
            yield return interfaceType;
        }
    }

    private static bool HasGenerateDataMemberAccessorsAttribute(INamedTypeSymbol typeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (string.Equals(attribute.AttributeClass?.ToDisplayString(),
                              TargetMarkConstants.GenerateDataMemberAccessorsAttribute,
                              StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasGeneratedProperty(INamedTypeSymbol typeSymbol, string path)
    {
        foreach (var property in GetAccessibleInstanceProperties(typeSymbol))
        {
            if (string.Equals(property.Name, path, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<IPropertySymbol> GetAccessibleInstanceProperties(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Interface)
        {
            foreach (var property in GetDeclaredAccessibleInstanceProperties(typeSymbol))
            {
                yield return property;
            }

            foreach (var interfaceType in typeSymbol.AllInterfaces)
            {
                foreach (var property in GetDeclaredAccessibleInstanceProperties(interfaceType))
                {
                    yield return property;
                }
            }

            yield break;
        }

        for (INamedTypeSymbol? current = typeSymbol; current != null; current = current.BaseType)
        {
            foreach (var property in GetDeclaredAccessibleInstanceProperties(current))
            {
                yield return property;
            }
        }
    }

    private static IEnumerable<IPropertySymbol> GetDeclaredAccessibleInstanceProperties(INamedTypeSymbol typeSymbol)
    {
        foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (property.IsStatic ||
                property.GetMethod is null ||
                !IsAccessibleFromGeneratedCode(property.GetMethod.DeclaredAccessibility))
            {
                continue;
            }

            yield return property;
        }
    }

    private static bool IsAccessibleFromGeneratedCode(Accessibility accessibility)
    {
        return accessibility is Accessibility.Public or Accessibility.Internal;
    }
}
