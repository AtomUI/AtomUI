using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.DesignToken;

/// <summary>
/// ATOMUIGEN040: forbids <c>double.IsNaN</c>/<c>float.IsNaN</c> on a token property inside a token calculation
/// method to detect an explicit override; the compiler writes and marks explicit values before evaluation, so
/// <c>IsExplicit(Kind.Field)</c> must be used instead. Scope: <c>CalculateTokenValues</c> on
/// <c>[ControlDesignToken]</c> types (own tokens, <c>XxxTokenKind</c>) and <c>CalculateAliasTokenValues</c> on
/// <c>[GlobalDesignToken]</c> types (<c>[DesignTokenKind(Alias)]</c> properties, <c>SharedTokenKind</c>).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OwnTokenSentinelAnalyzer : DiagnosticAnalyzer
{
    private const string IsNaNMethodName = "IsNaN";

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
            var scope = TokenCalculationScope.Create(compilationContext.Compilation, requireControlBaseToken: false);
            if (scope is null)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, scope),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, TokenCalculationScope scope)
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

        if (GetEnclosingMethod(context, invocation) is not { } enclosingMethod ||
            scope.Match(enclosingMethod) is not { } kind)
        {
            return;
        }

        var tokenType = enclosingMethod.ContainingType;
        var argument  = invocation.ArgumentList.Arguments[0].Expression;
        if (context.SemanticModel.GetSymbolInfo(argument, context.CancellationToken).Symbol is not IPropertySymbol property ||
            !scope.IsCheckedToken(property, tokenType, kind))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.OwnTokenSentinel,
            invocation.GetLocation(),
            property.Name,
            TokenCalculationScope.GetKindEnumName(tokenType, kind),
            enclosingMethod.Name));
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
}
