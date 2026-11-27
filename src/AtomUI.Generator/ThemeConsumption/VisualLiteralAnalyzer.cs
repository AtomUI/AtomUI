using System.Collections.Immutable;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.ThemeConsumption;

/// <summary>
/// ATOMUIGEN044: control code must not hard-code visual values.
/// <para>
/// Reported: <c>Color.FromRgb/FromArgb/FromUInt32/Parse</c> with literal arguments, <c>Brushes.X</c> and
/// <c>Colors.X</c> (except <c>Transparent</c>), <c>new SolidColorBrush(&lt;literal&gt;)</c>,
/// <c>BoxShadows.Parse("...")</c>, <c>new Pen(brush, &lt;literal thickness&gt;)</c>, and non-zero numeric literals or
/// <c>new Thickness/CornerRadius(&lt;literals&gt;)</c> assigned to a property, field or constant named
/// Background/Foreground/BorderBrush/Fill/Stroke/BorderThickness/CornerRadius/FontSize/StrokeThickness/BoxShadow
/// (exact names; one leading <c>_</c> or <c>s_</c> and the first letter's case are ignored, so <c>_borderThickness</c>
/// matches and <c>DefaultLabelFontSize</c> does not; the target must resolve to a field or property, never a local or
/// parameter), or passed as the value of <c>SetValue/SetCurrentValue(XxxProperty, ...)</c>
/// for those names.
/// </para>
/// <para>
/// Not detected: values that reach the consuming site through a local variable, a named constant or a computation;
/// <c>Width/Height/Padding/Margin</c>; default values passed to <c>AvaloniaProperty.Register</c>;
/// <c>using static</c> access to <c>Brushes</c>/<c>Colors</c>; <c>Color.TryParse</c> and non-literal arguments.
/// Applications and libraries that are not control packages (no <c>SharedTokenKind</c>, no <c>[ControlDesignToken]</c> type and no <c>AtomUIThemeConsumptionAnalysis=true</c> opt-in), the assembly defining <c>SharedTokenKind</c> and <c>[ControlDesignToken]</c>
/// types are exempt.
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VisualLiteralAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The single descriptor <c>ControlCodeVisualLiteral</c> that this analyzer reports.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        AtomUIDiagnosticDescriptors.ControlCodeVisualLiteral
    ];

    /// <summary>
    /// Registers the actions that report ATOMUIGEN044 in the control code of the compilation: the analyses of
    /// invocations, object creations and member accesses. Generated code is not analyzed. Nothing is registered when
    /// <c>ControlCodeScope.TryCreate</c> returns <see langword="null" />.
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
            if (scope is null)
            {
                return;
            }

            var recognizer = new VisualValueRecognizer(compilationContext.Compilation);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, scope, recognizer),
                SyntaxKind.InvocationExpression);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeCreation(nodeContext, scope, recognizer),
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ImplicitObjectCreationExpression);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeMemberAccess(nodeContext, scope, recognizer),
                SyntaxKind.SimpleMemberAccessExpression);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeAssignment(nodeContext, scope, recognizer),
                SyntaxKind.SimpleAssignmentExpression);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeFieldOrProperty(nodeContext, scope, recognizer),
                SyntaxKind.VariableDeclarator,
                SyntaxKind.PropertyDeclaration);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, ControlCodeScope scope, VisualValueRecognizer recognizer)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var name = ControlCodeScope.GetInvokedName(invocation);
        if (name is null)
        {
            return;
        }

        if (string.Equals(name, "SetValue", StringComparison.Ordinal) ||
            string.Equals(name, "SetCurrentValue", StringComparison.Ordinal))
        {
            AnalyzePropertyWrite(context, scope, recognizer, invocation);
            return;
        }

        var isLiteralCall = recognizer.IsColorLiteralCall(context.SemanticModel, invocation, context.CancellationToken) ||
                            recognizer.IsBoxShadowLiteralCall(context.SemanticModel, invocation, context.CancellationToken);
        if (isLiteralCall)
        {
            Report(context, scope, invocation, invocation);
        }
    }

    private static void AnalyzePropertyWrite(SyntaxNodeAnalysisContext context, ControlCodeScope scope,
        VisualValueRecognizer recognizer, InvocationExpressionSyntax invocation)
    {
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count < 2 ||
            ControlCodeScope.GetRightmostName(arguments[0].Expression) is not { } propertyFieldName ||
            !propertyFieldName.EndsWith("Property", StringComparison.Ordinal) ||
            !VisualValueRecognizer.IsVisualName(propertyFieldName.Substring(0, propertyFieldName.Length - "Property".Length)))
        {
            return;
        }

        var value = arguments[1].Expression;
        if (IsVisualValueForVisualName(context, recognizer, value))
        {
            Report(context, scope, value, value);
        }
    }

    private static void AnalyzeCreation(SyntaxNodeAnalysisContext context, ControlCodeScope scope, VisualValueRecognizer recognizer)
    {
        var creation = (BaseObjectCreationExpressionSyntax)context.Node;
        if (creation.ArgumentList is null)
        {
            return;
        }

        if (recognizer.IsSolidColorBrushLiteral(context.SemanticModel, creation, context.CancellationToken))
        {
            Report(context, scope, creation, creation);
            return;
        }

        if (recognizer.IsPenWithLiteralThickness(context.SemanticModel, creation, context.CancellationToken, out var thickness) &&
            thickness is not null)
        {
            Report(context, scope, thickness, thickness);
        }
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context, ControlCodeScope scope, VisualValueRecognizer recognizer)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;
        if (recognizer.IsPaletteMember(context.SemanticModel, memberAccess, context.CancellationToken))
        {
            Report(context, scope, memberAccess, memberAccess);
        }
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context, ControlCodeScope scope, VisualValueRecognizer recognizer)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        if (ControlCodeScope.GetRightmostName(assignment.Left) is not { } name ||
            !VisualValueRecognizer.IsVisualName(name) ||
            context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol is not (IFieldSymbol or IPropertySymbol))
        {
            // Locals and parameters are never visual members, whatever their name.
            return;
        }

        if (IsVisualValueForVisualName(context, recognizer, assignment.Right))
        {
            Report(context, scope, assignment.Right, assignment.Right);
        }
    }

    private static void AnalyzeFieldOrProperty(SyntaxNodeAnalysisContext context, ControlCodeScope scope, VisualValueRecognizer recognizer)
    {
        ExpressionSyntax? value;
        string name;
        switch (context.Node)
        {
            case VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax, Initializer: { } initializer } declarator:
                name = declarator.Identifier.ValueText;
                value = initializer.Value;
                break;
            case PropertyDeclarationSyntax property:
                name = property.Identifier.ValueText;
                value = property.Initializer?.Value ?? property.ExpressionBody?.Expression;
                break;
            default:
                return;
        }

        if (value is null || !VisualValueRecognizer.IsVisualName(name))
        {
            return;
        }

        if (IsVisualValueForVisualName(context, recognizer, value))
        {
            Report(context, scope, value, value);
        }
    }

    /// <summary>
    /// Non-zero numeric literal, <c>new Thickness/CornerRadius(literals)</c> or a box-shadow literal.
    /// </summary>
    private static bool IsVisualValueForVisualName(SyntaxNodeAnalysisContext context, VisualValueRecognizer recognizer, ExpressionSyntax value)
    {
        return VisualValueRecognizer.IsNonZeroNumericLiteral(context.SemanticModel, value, context.CancellationToken) ||
               recognizer.IsGeometryLiteral(context.SemanticModel, value, context.CancellationToken);
    }

    private static void Report(SyntaxNodeAnalysisContext context, ControlCodeScope scope, SyntaxNode location, SyntaxNode shown)
    {
        if (scope.IsInsideTokenDefinition(context.SemanticModel, location, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AtomUIDiagnosticDescriptors.ControlCodeVisualLiteral,
            location.GetLocation(),
            shown.ToString()));
    }
}
