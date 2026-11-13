using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.DesignToken;

/// <summary>
/// ATOMUIGEN048: reports a token assignment <c>P = …</c> in a token calculation method that is not directly
/// wrapped in <c>if (!IsExplicit(Kind.P))</c> while <c>P</c> is read again later in source order; the default
/// assignment would wipe the user's explicit override and the derived values would read the default.
/// Scope: <c>CalculateTokenValues</c> on <c>[ControlDesignToken]</c> types (own tokens, <c>XxxTokenKind</c>) and
/// <c>CalculateAliasTokenValues</c> on <c>[GlobalDesignToken]</c> types (<c>[DesignTokenKind(Alias)]</c> properties,
/// <c>SharedTokenKind</c>).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OwnTokenDerivationBaseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        AtomUIDiagnosticDescriptors.OwnTokenDerivationBaseUnguarded
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            var scope = TokenCalculationScope.Create(compilationContext.Compilation, requireControlBaseToken: true);
            if (scope is null)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeMethod(nodeContext, scope),
                SyntaxKind.MethodDeclaration);
        });
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, TokenCalculationScope scope)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.Node;
        var methodName   = methodSyntax.Identifier.ValueText;
        if (!string.Equals(methodName, TokenCalculationScope.GetMethodName(TokenCalculationKind.Control), StringComparison.Ordinal) &&
            !string.Equals(methodName, TokenCalculationScope.GetMethodName(TokenCalculationKind.Global), StringComparison.Ordinal))
        {
            return;
        }

        SyntaxNode? body = methodSyntax.Body ?? (SyntaxNode?)methodSyntax.ExpressionBody;
        if (body is null ||
            context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken) is not { } method ||
            scope.Match(method) is not { } kind)
        {
            return;
        }

        var tokenType = method.ContainingType;

        // 收集方法体内对受检属性的赋值中未被守卫的部分。
        var unguarded = new List<(AssignmentExpressionSyntax Assignment, IPropertySymbol Property)>();
        foreach (var assignment in body.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (GetCheckedProperty(context, scope, assignment.Left, tokenType, kind) is not { } property ||
                IsGuarded(context, scope, assignment, property, tokenType, kind))
            {
                continue;
            }

            unguarded.Add((assignment, property));
        }

        if (unguarded.Count == 0)
        {
            return;
        }

        // 收集方法体内对这些属性的读取（赋值左侧不算读取）。
        var candidateNames = new HashSet<string>(unguarded.Select(static item => item.Property.Name), StringComparer.Ordinal);
        var reads = new List<(int Position, IPropertySymbol Property)>();
        foreach (var identifier in body.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (!candidateNames.Contains(identifier.Identifier.ValueText) ||
                IsAssignmentTarget(identifier))
            {
                continue;
            }

            if (context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol is IPropertySymbol property &&
                !property.IsStatic)
            {
                reads.Add((identifier.SpanStart, property));
            }
        }

        var kindEnumName = TokenCalculationScope.GetKindEnumName(tokenType, kind);
        foreach (var (assignment, property) in unguarded)
        {
            var assignmentEnd = assignment.Span.End;
            var isReadLater = reads.Any(read => read.Position >= assignmentEnd &&
                                                SymbolEqualityComparer.Default.Equals(read.Property, property));
            if (!isReadLater)
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.OwnTokenDerivationBaseUnguarded,
                assignment.GetLocation(),
                property.Name,
                kindEnumName,
                method.Name));
        }
    }

    private static IPropertySymbol? GetCheckedProperty(
        SyntaxNodeAnalysisContext context,
        TokenCalculationScope scope,
        ExpressionSyntax left,
        INamedTypeSymbol tokenType,
        TokenCalculationKind kind)
    {
        if (left is not (IdentifierNameSyntax or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax }))
        {
            return null;
        }

        if (context.SemanticModel.GetSymbolInfo(left, context.CancellationToken).Symbol is not IPropertySymbol property ||
            !scope.IsCheckedToken(property, tokenType, kind))
        {
            return null;
        }

        return property;
    }

    /// <summary>
    /// 赋值语句是 <c>if (!IsExplicit(Kind.P))</c> 的直接语句体（或其块内唯一语句），且 Kind 成员名等于属性名。
    /// </summary>
    private static bool IsGuarded(
        SyntaxNodeAnalysisContext context,
        TokenCalculationScope scope,
        AssignmentExpressionSyntax assignment,
        IPropertySymbol property,
        INamedTypeSymbol tokenType,
        TokenCalculationKind kind)
    {
        if (assignment.Parent is not ExpressionStatementSyntax statement)
        {
            return false;
        }

        SyntaxNode? container = statement.Parent;
        StatementSyntax guarded = statement;
        if (container is BlockSyntax block)
        {
            if (block.Statements.Count != 1)
            {
                return false;
            }

            guarded = block;
            container = block.Parent;
        }

        if (container is not IfStatementSyntax ifStatement || ifStatement.Statement != guarded)
        {
            return false;
        }

        var condition = ifStatement.Condition;
        while (condition is ParenthesizedExpressionSyntax parenthesized)
        {
            condition = parenthesized.Expression;
        }

        if (condition is not PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression } negation ||
            negation.Operand is not InvocationExpressionSyntax invocation ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        // 控件范围：调用必须解析到 AbstractControlDesignToken.IsExplicit<TKind>；
        // 全局范围：调用必须解析到 Token 类型（或其基类）声明的 IsExplicit。同名的其它方法不算守卫。
        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol called ||
            !scope.IsGuardMethod(called, tokenType, kind))
        {
            return false;
        }

        // 实参必须是该范围的 Kind 枚举中与属性同名的成员。
        if (context.SemanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken).Symbol
            is not IFieldSymbol { ContainingType: { TypeKind: TypeKind.Enum } kindEnum } kindField)
        {
            return false;
        }

        return string.Equals(kindField.Name, property.Name, StringComparison.Ordinal) &&
               TokenCalculationScope.IsKindEnum(kindEnum, tokenType, kind);
    }

    private static bool IsAssignmentTarget(IdentifierNameSyntax identifier)
    {
        SyntaxNode node = identifier;
        if (node.Parent is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess &&
            memberAccess.Name == identifier)
        {
            node = memberAccess;
        }

        return node.Parent is AssignmentExpressionSyntax assignment && assignment.Left == node;
    }
}
