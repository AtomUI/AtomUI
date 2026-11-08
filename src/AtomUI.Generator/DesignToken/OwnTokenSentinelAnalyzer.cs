using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.DesignToken;

/// <summary>
/// ATOMUIGEN040: 禁止在 <c>CalculateTokenValues</c> 中用 <c>double.IsNaN</c>/<c>float.IsNaN</c> 判断 Own Token 是否被显式设置，
/// 编译器会在求值前写入显式覆盖并标记，应使用 <c>IsExplicit(XxxTokenKind.Field)</c>。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OwnTokenSentinelAnalyzer : DiagnosticAnalyzer
{
    private const string CalculateMethodName = "CalculateTokenValues";
    private const string IsNaNMethodName = "IsNaN";
    private const string TokenSuffix = "Token";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        AtomUIDiagnosticDescriptors.OwnTokenSentinel
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            var attributeSymbol = compilationContext.Compilation.GetTypeByMetadataName(
                TargetMarkConstants.ControlDesignTokenAttribute);
            if (attributeSymbol is null)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, attributeSymbol),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol attributeSymbol)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method ||
            !IsFloatingPointIsNaN(method))
        {
            return;
        }

        var enclosingMethod = GetEnclosingMethod(context, invocation);
        if (enclosingMethod is null ||
            !string.Equals(enclosingMethod.Name, CalculateMethodName, StringComparison.Ordinal) ||
            enclosingMethod.IsStatic)
        {
            return;
        }

        var tokenType = enclosingMethod.ContainingType;
        if (tokenType is null || !HasAttribute(tokenType, attributeSymbol))
        {
            return;
        }

        var argument = invocation.ArgumentList.Arguments[0].Expression;
        if (context.SemanticModel.GetSymbolInfo(argument, context.CancellationToken).Symbol is not IPropertySymbol property ||
            property.IsStatic ||
            !IsOwnTokenOfType(property, tokenType))
        {
            return;
        }

        var controlName = tokenType.Name.EndsWith(TokenSuffix, StringComparison.Ordinal)
            ? tokenType.Name.Substring(0, tokenType.Name.Length - TokenSuffix.Length)
            : tokenType.Name;
        context.ReportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.OwnTokenSentinel,
            invocation.GetLocation(),
            property.Name,
            controlName));
    }

    private static bool IsFloatingPointIsNaN(IMethodSymbol method)
    {
        return string.Equals(method.Name, IsNaNMethodName, StringComparison.Ordinal) &&
               method.ContainingType.SpecialType is SpecialType.System_Double or SpecialType.System_Single;
    }

    private static IMethodSymbol? GetEnclosingMethod(SyntaxNodeAnalysisContext context, SyntaxNode node)
    {
        var methodSyntax = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        return methodSyntax is null
            ? null
            : context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken);
    }

    private static bool HasAttribute(INamedTypeSymbol type, INamedTypeSymbol attributeSymbol)
    {
        foreach (var attribute in type.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsOwnTokenOfType(IPropertySymbol property, INamedTypeSymbol tokenType)
    {
        for (var current = tokenType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, property.ContainingType))
            {
                return true;
            }
        }

        return false;
    }
}
