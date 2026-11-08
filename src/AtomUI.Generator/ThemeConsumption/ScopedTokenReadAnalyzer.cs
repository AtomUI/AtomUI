using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.ThemeConsumption;

/// <summary>
/// ATOMUIGEN043: control code must read Tokens only through <c>ControlTokenAccessor</c>, <c>TokenResourceBinder</c> or
/// an AXAML TokenResource (control-token-guidelines rule 2).
/// <para>
/// Reported: a call to the Avalonia <c>TryGetResource</c>/<c>TryFindResource</c>/<c>FindResource</c> family
/// (<c>Application.Current</c>, any <c>IResourceHost</c>, the <c>ResourceNodeExtensions</c> extensions) whose key
/// argument is a <c>SharedTokenKind</c>, an enum named <c>*TokenKey</c>/<c>*TokenKind</c> (generated global and own
/// Token keys), or a <c>ControlTokenResourceKey.Global/Own(...)</c> call; such reads bypass local
/// <c>ThemeConfigProvider</c> scopes. Also reported, as a second diagnostic on the fallback expression: a hard-coded
/// visual value used when that read fails, in the forms <c>if (!read) { x = 12; }</c>,
/// <c>if (read) { return v; } return 200;</c> (the true branch must exit), <c>read ? v : 14</c> and
/// <c>read ?? Brushes.Red</c>. A <c>TimeSpan.FromXxx(literal)</c> counts as a visual value here because motion
/// durations are Tokens.
/// </para>
/// <para>
/// Not detected: reads whose key is a string, a <c>Type</c> (ControlTheme lookups) or a forwarded <c>object</c>
/// variable; a Token key stored in an <c>object</c> local before the call; custom resource hosts declared in the same
/// compilation; fallbacks that are not literals (named constants, computed values) or that sit outside the shapes
/// above. Assemblies that do not consume <c>SharedTokenKind</c>, the assembly defining it and
/// <c>[ControlDesignToken]</c> types are exempt.
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ScopedTokenReadAnalyzer : DiagnosticAnalyzer
{
    private const string ControlTokenResourceKeyMetadataName = "AtomUI.Theme.Resources.ControlTokenResourceKey";

    private static readonly string[] s_readMethodNames = ["TryGetResource", "TryFindResource", "FindResource"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        AtomUIDiagnosticDescriptors.ControlCodeScopedTokenRead
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            var scope = ControlCodeScope.TryCreate(compilationContext.Compilation);
            if (scope is null)
            {
                return;
            }

            var keyType = compilationContext.Compilation.GetTypeByMetadataName(ControlTokenResourceKeyMetadataName);
            var recognizer = new VisualValueRecognizer(compilationContext.Compilation);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, scope, keyType, recognizer),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, ControlCodeScope scope,
        INamedTypeSymbol? controlTokenResourceKey, VisualValueRecognizer recognizer)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var name = ControlCodeScope.GetInvokedName(invocation);
        if (name is null || Array.IndexOf(s_readMethodNames, name) < 0)
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method ||
            !ControlCodeScope.IsAvaloniaSymbol(method) ||
            GetKeyArgument(invocation) is not { } keyArgument ||
            !IsTokenKey(semanticModel, keyArgument, scope, controlTokenResourceKey, context.CancellationToken) ||
            scope.IsInsideTokenDefinition(semanticModel, invocation, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.ControlCodeScopedTokenRead,
            invocation.GetLocation(),
            $"{method.ContainingType.Name}.{method.Name}",
            "the read bypasses local ThemeConfigProvider scopes; read Tokens with ControlTokenAccessor, TokenResourceBinder or an AXAML TokenResource"));

        foreach (var fallback in FindFallbackValues(semanticModel, invocation, recognizer, context.CancellationToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.ControlCodeScopedTokenRead,
                fallback.GetLocation(),
                fallback.ToString(),
                $"hard-coded fallback for the failed Token read '{method.Name}'; a Token always resolves, remove the fallback and read it with ControlTokenAccessor or TokenResourceBinder"));
        }
    }

    private static ExpressionSyntax? GetKeyArgument(InvocationExpressionSyntax invocation)
    {
        var arguments = invocation.ArgumentList.Arguments;
        foreach (var argument in arguments)
        {
            if (string.Equals(argument.NameColon?.Name.Identifier.ValueText, "key", StringComparison.Ordinal))
            {
                return argument.Expression;
            }
        }

        return arguments.Count > 0 && arguments[0].NameColon is null ? arguments[0].Expression : null;
    }

    private static bool IsTokenKey(SemanticModel semanticModel, ExpressionSyntax expression, ControlCodeScope scope,
        INamedTypeSymbol? controlTokenResourceKey, CancellationToken cancellationToken)
    {
        expression = Unwrap(expression);

        if (expression is InvocationExpressionSyntax invocation &&
            controlTokenResourceKey is not null &&
            semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol keyFactory &&
            SymbolEqualityComparer.Default.Equals(keyFactory.ContainingType, controlTokenResourceKey))
        {
            return true;
        }

        var type = semanticModel.GetTypeInfo(expression, cancellationToken).Type;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        if (type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(enumType, scope.SharedTokenKind) ||
               enumType.Name.EndsWith("TokenKey", StringComparison.Ordinal) ||
               enumType.Name.EndsWith("TokenKind", StringComparison.Ordinal);
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (true)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    expression = parenthesized.Expression;
                    continue;
                case CastExpressionSyntax cast:
                    expression = cast.Expression;
                    continue;
                default:
                    return expression;
            }
        }
    }

    private static IEnumerable<ExpressionSyntax> FindFallbackValues(SemanticModel semanticModel,
        InvocationExpressionSyntax read, VisualValueRecognizer recognizer, CancellationToken cancellationToken)
    {
        var values = new List<ExpressionSyntax>();
        var negated = false;
        SyntaxNode current = read;
        while (current.Parent is { } parent)
        {
            switch (parent)
            {
                case PrefixUnaryExpressionSyntax prefix when prefix.IsKind(SyntaxKind.LogicalNotExpression):
                    negated = !negated;
                    break;
                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression):
                    if (binary.Left == current)
                    {
                        Collect(binary.Right);
                    }

                    return values;
                case BinaryExpressionSyntax binary when binary.Kind() is SyntaxKind.EqualsExpression or SyntaxKind.NotEqualsExpression:
                    var other = binary.Left == current ? binary.Right : binary.Left;
                    if (other.Kind() is SyntaxKind.TrueLiteralExpression or SyntaxKind.FalseLiteralExpression)
                    {
                        var comparesToTrue = other.IsKind(SyntaxKind.TrueLiteralExpression);
                        if (comparesToTrue != binary.IsKind(SyntaxKind.EqualsExpression))
                        {
                            negated = !negated;
                        }
                    }

                    break;
                case BinaryExpressionSyntax:
                case ParenthesizedExpressionSyntax:
                case CastExpressionSyntax:
                case ConditionalAccessExpressionSyntax:
                case IsPatternExpressionSyntax:
                    break;
                case ConditionalExpressionSyntax conditional when conditional.Condition == current:
                    Collect(negated ? conditional.WhenTrue : conditional.WhenFalse);
                    return values;
                case IfStatementSyntax ifStatement when ifStatement.Condition == current:
                    CollectFromIf(ifStatement, negated);
                    return values;
                default:
                    return values;
            }

            current = parent;
        }

        return values;

        void CollectFromIf(IfStatementSyntax ifStatement, bool readFailsInThen)
        {
            if (readFailsInThen)
            {
                CollectFromStatement(ifStatement.Statement);
                return;
            }

            if (ifStatement.Else is { } elseClause)
            {
                CollectFromStatement(elseClause.Statement);
            }

            if (Exits(ifStatement.Statement) && ifStatement.Parent is BlockSyntax block)
            {
                var seen = false;
                foreach (var statement in block.Statements)
                {
                    if (seen)
                    {
                        CollectFromStatement(statement);
                    }
                    else if (statement == ifStatement)
                    {
                        seen = true;
                    }
                }
            }
        }

        void CollectFromStatement(SyntaxNode statement)
        {
            foreach (var node in statement.DescendantNodesAndSelf())
            {
                switch (node)
                {
                    case ReturnStatementSyntax { Expression: { } returned }:
                        Collect(returned);
                        break;
                    case AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression):
                        Collect(assignment.Right);
                        break;
                    case VariableDeclaratorSyntax { Initializer: { } initializer }:
                        Collect(initializer.Value);
                        break;
                }
            }
        }

        void Collect(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    Collect(parenthesized.Expression);
                    break;
                case ConditionalExpressionSyntax conditional:
                    Collect(conditional.WhenTrue);
                    Collect(conditional.WhenFalse);
                    break;
                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression):
                    Collect(binary.Left);
                    Collect(binary.Right);
                    break;
                default:
                    if (recognizer.IsFallbackValue(semanticModel, expression, cancellationToken))
                    {
                        values.Add(expression);
                    }

                    break;
            }
        }
    }

    private static bool Exits(StatementSyntax statement)
    {
        return statement switch
        {
            ReturnStatementSyntax or ThrowStatementSyntax or ContinueStatementSyntax or BreakStatementSyntax => true,
            BlockSyntax block => block.Statements.Count > 0 && Exits(block.Statements[block.Statements.Count - 1]),
            _ => false,
        };
    }
}
