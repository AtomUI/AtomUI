using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.ThemeConsumption;

/// <summary>
/// Recognises hard-coded visual values in C# expressions. Shared by ATOMUIGEN044 (visual literals in control code)
/// and the fallback sub-rule of ATOMUIGEN043 (hard-coded value after a failed Token read).
/// Everything is symbol based; only expressions made of literals are considered, so a value that reaches the
/// consuming site through a local variable, a named constant or a computation is never reported.
/// </summary>
internal sealed class VisualValueRecognizer
{
    private static readonly string[] s_colorFactoryNames = ["FromRgb", "FromArgb", "FromUInt32", "Parse"];

    // Exact property or field names that carry visual numeric/brush values.
    private static readonly HashSet<string> s_visualNames = new(StringComparer.Ordinal)
    {
        "Background", "Foreground", "BorderBrush", "Fill", "Stroke",
        "BorderThickness", "CornerRadius", "FontSize", "StrokeThickness", "BoxShadow", "BoxShadows"
    };

    private readonly INamedTypeSymbol? _color;
    private readonly INamedTypeSymbol? _colors;
    private readonly INamedTypeSymbol? _brushes;
    private readonly INamedTypeSymbol? _solidColorBrush;
    private readonly INamedTypeSymbol? _thickness;
    private readonly INamedTypeSymbol? _cornerRadius;
    private readonly INamedTypeSymbol? _pen;
    private readonly INamedTypeSymbol? _boxShadows;
    private readonly INamedTypeSymbol? _timeSpan;

    public VisualValueRecognizer(Compilation compilation)
    {
        _color = compilation.GetTypeByMetadataName("Avalonia.Media.Color");
        _colors = compilation.GetTypeByMetadataName("Avalonia.Media.Colors");
        _brushes = compilation.GetTypeByMetadataName("Avalonia.Media.Brushes");
        _solidColorBrush = compilation.GetTypeByMetadataName("Avalonia.Media.SolidColorBrush");
        _thickness = compilation.GetTypeByMetadataName("Avalonia.Thickness");
        _cornerRadius = compilation.GetTypeByMetadataName("Avalonia.CornerRadius");
        _pen = compilation.GetTypeByMetadataName("Avalonia.Media.Pen");
        _boxShadows = compilation.GetTypeByMetadataName("Avalonia.Media.BoxShadows");
        _timeSpan = compilation.GetTypeByMetadataName("System.TimeSpan");
    }

    /// <summary>
    /// True when <paramref name="name"/> equals a visual member name after stripping one leading <c>_</c> or
    /// <c>s_</c> and upper-casing the first letter (so <c>_borderThickness</c> matches, <c>DefaultFontSize</c> does not).
    /// </summary>
    public static bool IsVisualName(string name)
    {
        return s_visualNames.Contains(NormalizeName(name));
    }

    /// <summary>A literal expression: numeric/string/char literals, possibly parenthesised, signed, cast or combined.</summary>
    public static bool IsLiteral(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax literal:
                return literal.Kind() is SyntaxKind.NumericLiteralExpression
                    or SyntaxKind.StringLiteralExpression
                    or SyntaxKind.CharacterLiteralExpression;
            case ParenthesizedExpressionSyntax parenthesized:
                return IsLiteral(parenthesized.Expression);
            case PrefixUnaryExpressionSyntax prefix when prefix.Kind() is SyntaxKind.UnaryMinusExpression
                or SyntaxKind.UnaryPlusExpression or SyntaxKind.BitwiseNotExpression:
                return IsLiteral(prefix.Operand);
            case CastExpressionSyntax cast:
                return IsLiteral(cast.Expression);
            case BinaryExpressionSyntax binary when binary.Kind() is SyntaxKind.AddExpression
                or SyntaxKind.SubtractExpression or SyntaxKind.MultiplyExpression or SyntaxKind.DivideExpression
                or SyntaxKind.LeftShiftExpression or SyntaxKind.RightShiftExpression or SyntaxKind.BitwiseOrExpression
                or SyntaxKind.BitwiseAndExpression or SyntaxKind.ExclusiveOrExpression:
                return IsLiteral(binary.Left) && IsLiteral(binary.Right);
            default:
                return false;
        }
    }

    /// <summary>A numeric literal expression whose constant value is not zero.</summary>
    public static bool IsNonZeroNumericLiteral(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        if (!IsLiteral(expression))
        {
            return false;
        }

        var constant = semanticModel.GetConstantValue(expression, cancellationToken);
        if (!constant.HasValue || constant.Value is null or string or char or bool)
        {
            return false;
        }

        try
        {
            return Convert.ToDouble(constant.Value, System.Globalization.CultureInfo.InvariantCulture) != 0d;
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException)
        {
            return false;
        }
    }

    /// <summary><c>Color.FromRgb/FromArgb/FromUInt32/Parse</c> whose arguments are all literals.</summary>
    public bool IsColorLiteralCall(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        if (_color is null ||
            invocation.ArgumentList.Arguments.Count == 0 ||
            !IsOneOf(ControlCodeScope.GetInvokedName(invocation), s_colorFactoryNames) ||
            semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol { IsStatic: true } method ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, _color))
        {
            return false;
        }

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (!IsLiteral(argument.Expression))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary><c>BoxShadows.Parse("...")</c> with a literal argument.</summary>
    public bool IsBoxShadowLiteralCall(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        if (_boxShadows is null ||
            invocation.ArgumentList.Arguments.Count == 0 ||
            !string.Equals(ControlCodeScope.GetInvokedName(invocation), "Parse", StringComparison.Ordinal) ||
            semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol { IsStatic: true } method ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, _boxShadows))
        {
            return false;
        }

        return invocation.ArgumentList.Arguments.All(static argument => IsLiteral(argument.Expression));
    }

    /// <summary><c>Brushes.X</c> / <c>Colors.X</c> other than <c>Transparent</c>.</summary>
    public bool IsPaletteMember(SemanticModel semanticModel, MemberAccessExpressionSyntax memberAccess, CancellationToken cancellationToken)
    {
        if (string.Equals(memberAccess.Name.Identifier.ValueText, "Transparent", StringComparison.Ordinal) ||
            (_brushes is null && _colors is null))
        {
            return false;
        }

        var symbol = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
        if (symbol is not (IPropertySymbol { IsStatic: true } or IFieldSymbol { IsStatic: true }))
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(symbol.ContainingType, _brushes) ||
               SymbolEqualityComparer.Default.Equals(symbol.ContainingType, _colors);
    }

    /// <summary><c>new SolidColorBrush(&lt;literal&gt;[, ...])</c>.</summary>
    public bool IsSolidColorBrushLiteral(SemanticModel semanticModel, BaseObjectCreationExpressionSyntax creation, CancellationToken cancellationToken)
    {
        if (_solidColorBrush is null ||
            creation.ArgumentList is not { Arguments.Count: > 0 } arguments ||
            !IsLiteral(arguments.Arguments[0].Expression))
        {
            return false;
        }

        return IsCreationOf(semanticModel, creation, _solidColorBrush, cancellationToken);
    }

    /// <summary><c>new Pen(brush, &lt;literal thickness&gt;, ...)</c> with a non-zero literal thickness.</summary>
    public bool IsPenWithLiteralThickness(SemanticModel semanticModel, BaseObjectCreationExpressionSyntax creation,
        CancellationToken cancellationToken, out ExpressionSyntax? thickness)
    {
        thickness = null;
        if (_pen is null || creation.ArgumentList is not { Arguments.Count: > 0 } arguments ||
            semanticModel.GetSymbolInfo(creation, cancellationToken).Symbol is not IMethodSymbol constructor ||
            !SymbolEqualityComparer.Default.Equals(constructor.ContainingType, _pen))
        {
            return false;
        }

        for (var i = 0; i < arguments.Arguments.Count; i++)
        {
            var argument = arguments.Arguments[i];
            var parameterName = argument.NameColon?.Name.Identifier.ValueText ??
                                (i < constructor.Parameters.Length ? constructor.Parameters[i].Name : null);
            if (string.Equals(parameterName, "thickness", StringComparison.Ordinal) &&
                IsNonZeroNumericLiteral(semanticModel, argument.Expression, cancellationToken))
            {
                thickness = argument.Expression;
                return true;
            }
        }

        return false;
    }

    /// <summary><c>new Thickness(...)</c> / <c>new CornerRadius(...)</c> whose arguments are literals and not all zero.</summary>
    public bool IsGeometryLiteral(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        if (expression is not BaseObjectCreationExpressionSyntax { ArgumentList: { Arguments.Count: > 0 } arguments } creation ||
            !IsCreationOf(semanticModel, creation, _thickness, cancellationToken) &&
            !IsCreationOf(semanticModel, creation, _cornerRadius, cancellationToken))
        {
            return false;
        }

        var anyNonZero = false;
        foreach (var argument in arguments.Arguments)
        {
            if (!IsLiteral(argument.Expression))
            {
                return false;
            }

            anyNonZero |= IsNonZeroNumericLiteral(semanticModel, argument.Expression, cancellationToken);
        }

        return anyNonZero;
    }

    /// <summary>
    /// A hard-coded visual value for the 043 fallback sub-rule: non-zero numeric literal, colour/brush/geometry
    /// literal, or a <c>TimeSpan.FromXxx(&lt;literal&gt;)</c> motion duration.
    /// </summary>
    public bool IsFallbackValue(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax invocation:
                return IsColorLiteralCall(semanticModel, invocation, cancellationToken) ||
                       IsBoxShadowLiteralCall(semanticModel, invocation, cancellationToken) ||
                       IsTimeSpanLiteralCall(semanticModel, invocation, cancellationToken);
            case MemberAccessExpressionSyntax memberAccess:
                return IsPaletteMember(semanticModel, memberAccess, cancellationToken);
            case BaseObjectCreationExpressionSyntax creation:
                return IsSolidColorBrushLiteral(semanticModel, creation, cancellationToken) ||
                       IsGeometryLiteral(semanticModel, creation, cancellationToken);
            default:
                return IsNonZeroNumericLiteral(semanticModel, expression, cancellationToken);
        }
    }

    private bool IsTimeSpanLiteralCall(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        if (_timeSpan is null ||
            invocation.ArgumentList.Arguments.Count != 1 ||
            ControlCodeScope.GetInvokedName(invocation) is not { } name ||
            !name.StartsWith("From", StringComparison.Ordinal) ||
            semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol { IsStatic: true } method ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, _timeSpan))
        {
            return false;
        }

        return IsNonZeroNumericLiteral(semanticModel, invocation.ArgumentList.Arguments[0].Expression, cancellationToken);
    }

    private static bool IsCreationOf(SemanticModel semanticModel, BaseObjectCreationExpressionSyntax creation,
        INamedTypeSymbol? type, CancellationToken cancellationToken)
    {
        return type is not null &&
               semanticModel.GetTypeInfo(creation, cancellationToken).Type is { } created &&
               SymbolEqualityComparer.Default.Equals(created, type);
    }

    private static bool IsOneOf(string? name, string[] names)
    {
        return name is not null && Array.IndexOf(names, name) >= 0;
    }

    private static string NormalizeName(string name)
    {
        var trimmed = name.StartsWith("s_", StringComparison.Ordinal)
            ? name.Substring(2)
            : name.StartsWith("_", StringComparison.Ordinal) ? name.Substring(1) : name;
        return trimmed.Length == 0 ? trimmed : char.ToUpperInvariant(trimmed[0]) + trimmed.Substring(1);
    }
}
