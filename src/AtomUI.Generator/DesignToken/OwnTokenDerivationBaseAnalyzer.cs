using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.DesignToken;

/// <summary>
/// ATOMUIGEN048: 在 <c>CalculateTokenValues</c> 中，若 Own Token <c>P</c> 的赋值语句没有被
/// <c>if (!IsExplicit(XxxTokenKind.P))</c> 直接包裹，而 <c>P</c> 在该赋值之后（按源码顺序）又被读取，
/// 则用户显式覆盖会被这次默认赋值抹掉，导致后续派生值读到默认值。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OwnTokenDerivationBaseAnalyzer : DiagnosticAnalyzer
{
    private const string CalculateMethodName = "CalculateTokenValues";
    private const string IsExplicitMethodName = "IsExplicit";
    private const string TokenSuffix = "Token";

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
            var attributeSymbol = compilationContext.Compilation.GetTypeByMetadataName(
                TargetMarkConstants.ControlDesignTokenAttribute);
            var baseTokenSymbol = compilationContext.Compilation.GetTypeByMetadataName(
                TargetMarkConstants.AbstractControlDesignToken);
            if (attributeSymbol is null || baseTokenSymbol is null)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeMethod(nodeContext, attributeSymbol, baseTokenSymbol),
                SyntaxKind.MethodDeclaration);
        });
    }

    private static void AnalyzeMethod(
        SyntaxNodeAnalysisContext context, INamedTypeSymbol attributeSymbol, INamedTypeSymbol baseTokenSymbol)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.Node;
        if (!string.Equals(methodSyntax.Identifier.ValueText, CalculateMethodName, StringComparison.Ordinal))
        {
            return;
        }

        SyntaxNode? body = methodSyntax.Body ?? (SyntaxNode?)methodSyntax.ExpressionBody;
        if (body is null ||
            context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken) is not { IsStatic: false } method)
        {
            return;
        }

        var tokenType = method.ContainingType;
        if (tokenType is null || !HasAttribute(tokenType, attributeSymbol))
        {
            return;
        }

        // 收集方法体内对 Own 属性的赋值中未被守卫的部分。
        var unguarded = new List<(AssignmentExpressionSyntax Assignment, IPropertySymbol Property)>();
        foreach (var assignment in body.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (GetOwnProperty(context, assignment.Left, tokenType) is not { } property ||
                IsGuarded(context, assignment, property, tokenType, baseTokenSymbol))
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

        foreach (var (assignment, property) in unguarded)
        {
            var assignmentEnd = assignment.Span.End;
            var isReadLater = reads.Any(read => read.Position >= assignmentEnd &&
                                                SymbolEqualityComparer.Default.Equals(read.Property, property));
            if (!isReadLater)
            {
                continue;
            }

            var controlName = tokenType.Name.EndsWith(TokenSuffix, StringComparison.Ordinal)
                ? tokenType.Name.Substring(0, tokenType.Name.Length - TokenSuffix.Length)
                : tokenType.Name;
            context.ReportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.OwnTokenDerivationBaseUnguarded,
                assignment.GetLocation(),
                property.Name,
                controlName));
        }
    }

    private static IPropertySymbol? GetOwnProperty(SyntaxNodeAnalysisContext context, ExpressionSyntax left, INamedTypeSymbol tokenType)
    {
        if (left is not (IdentifierNameSyntax or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax }))
        {
            return null;
        }

        if (context.SemanticModel.GetSymbolInfo(left, context.CancellationToken).Symbol is not IPropertySymbol property ||
            property.IsStatic ||
            !IsOwnTokenOfType(property, tokenType))
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
        AssignmentExpressionSyntax assignment,
        IPropertySymbol property,
        INamedTypeSymbol tokenType,
        INamedTypeSymbol baseTokenSymbol)
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

        // 调用必须解析到 AbstractControlDesignToken.IsExplicit<TKind>，同名的其它方法不算守卫。
        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol called ||
            !string.Equals(called.Name, IsExplicitMethodName, StringComparison.Ordinal) ||
            !SymbolEqualityComparer.Default.Equals(called.OriginalDefinition.ContainingType, baseTokenSymbol))
        {
            return false;
        }

        // 实参必须是 <ControlName>TokenKind 枚举中与属性同名的成员。
        if (context.SemanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken).Symbol
            is not IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } kindField)
        {
            return false;
        }

        var controlName = tokenType.Name.EndsWith(TokenSuffix, StringComparison.Ordinal)
            ? tokenType.Name.Substring(0, tokenType.Name.Length - TokenSuffix.Length)
            : tokenType.Name;
        return string.Equals(kindField.Name, property.Name, StringComparison.Ordinal) &&
               string.Equals(kindField.ContainingType.Name, controlName + "TokenKind", StringComparison.Ordinal);
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
