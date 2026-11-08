using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.ThemeConsumption;

/// <summary>
/// Decides where the control-code analyzers (ATOMUIGEN043/044/047) apply. They only run in assemblies that
/// <em>consume</em> <c>AtomUI.Theme.Resources.SharedTokenKind</c> (the control packages); the assembly that defines
/// the Token infrastructure (AtomUI.Core) and any code inside a <c>[ControlDesignToken]</c> type are exempt, because
/// Token definitions legitimately contain concrete values.
/// </summary>
internal sealed class ControlCodeScope
{
    public const string SharedTokenKindMetadataName = "AtomUI.Theme.Resources.SharedTokenKind";

    private ControlCodeScope(INamedTypeSymbol sharedTokenKind, INamedTypeSymbol? controlDesignTokenAttribute)
    {
        SharedTokenKind = sharedTokenKind;
        ControlDesignTokenAttribute = controlDesignTokenAttribute;
    }

    public INamedTypeSymbol SharedTokenKind { get; }

    public INamedTypeSymbol? ControlDesignTokenAttribute { get; }

    /// <summary>Returns <see langword="null"/> when the compilation is not a control package.</summary>
    public static ControlCodeScope? TryCreate(Compilation compilation)
    {
        var sharedTokenKind = compilation.GetTypeByMetadataName(SharedTokenKindMetadataName);
        if (sharedTokenKind is null ||
            SymbolEqualityComparer.Default.Equals(sharedTokenKind.ContainingAssembly, compilation.Assembly))
        {
            return null;
        }

        return new ControlCodeScope(
            sharedTokenKind,
            compilation.GetTypeByMetadataName(TargetMarkConstants.ControlDesignTokenAttribute));
    }

    /// <summary>True when <paramref name="node"/> sits inside a type carrying <c>[ControlDesignToken]</c>.</summary>
    public bool IsInsideTokenDefinition(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
    {
        if (ControlDesignTokenAttribute is null)
        {
            return false;
        }

        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is not TypeDeclarationSyntax typeDeclaration ||
                semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not { } type)
            {
                continue;
            }

            foreach (var attribute in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, ControlDesignTokenAttribute))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsAvaloniaSymbol(ISymbol symbol)
    {
        return symbol.ContainingAssembly?.Name.StartsWith("Avalonia", StringComparison.Ordinal) == true;
    }

    /// <summary>Simple name of an invocation target: <c>M(...)</c>, <c>x.M(...)</c> or <c>x?.M(...)</c>.</summary>
    public static string? GetInvokedName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            GenericNameSyntax generic => generic.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText,
            _ => null,
        };
    }

    /// <summary>Rightmost simple name of an assignment target or argument: <c>X</c>, <c>a.X</c>, <c>this.X</c>.</summary>
    public static string? GetRightmostName(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText,
            PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } postfix =>
                GetRightmostName(postfix.Operand),
            _ => null,
        };
    }
}
