using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AtomUI.Generator.ThemeConsumption;

/// <summary>
/// Decides where the control-code analyzers (ATOMUIGEN043/044/047) apply. They only run in control packages
/// (assemblies that consume <c>AtomUI.Theme.Resources.SharedTokenKind</c> and either declare a Token type or build
/// theme assets); the assembly that defines
/// the Token infrastructure (AtomUI.Core) and any code inside a <c>[ControlDesignToken]</c> type are exempt, because
/// Token definitions legitimately contain concrete values.
/// </summary>
internal sealed class ControlCodeScope
{
    public const string ThemeAssetProjectDirectoryOption = "build_property.AtomUIThemeAssetProjectDirectory";
    public const string SharedTokenKindMetadataName = "AtomUI.Theme.Resources.SharedTokenKind";

    private ControlCodeScope(INamedTypeSymbol sharedTokenKind, INamedTypeSymbol? controlDesignTokenAttribute)
    {
        SharedTokenKind = sharedTokenKind;
        ControlDesignTokenAttribute = controlDesignTokenAttribute;
    }

    public INamedTypeSymbol SharedTokenKind { get; }

    public INamedTypeSymbol? ControlDesignTokenAttribute { get; }

    /// <summary>
    /// Returns <see langword="null"/> when the compilation is not a control package: <c>SharedTokenKind</c> must be
    /// resolvable and not defined by the compilation itself, and the compilation must either declare a
    /// <c>[ControlDesignToken]</c> type or carry the <c>AtomUIThemeAssetProjectDirectory</c> build property (set by
    /// the theme-asset build targets of control packages). Plain applications referencing AtomUI.Core match neither.
    /// </summary>
    public static ControlCodeScope? TryCreate(CompilationStartAnalysisContext context)
    {
        var compilation = context.Compilation;
        var sharedTokenKind = compilation.GetTypeByMetadataName(SharedTokenKindMetadataName);
        if (sharedTokenKind is null ||
            SymbolEqualityComparer.Default.Equals(sharedTokenKind.ContainingAssembly, compilation.Assembly))
        {
            return null;
        }

        var attribute = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlDesignTokenAttribute);
        var isControlPackage =
            (context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(
                 ThemeAssetProjectDirectoryOption, out var directory) &&
             !string.IsNullOrWhiteSpace(directory)) ||
            (attribute is not null && DeclaresAttributedType(compilation.Assembly.GlobalNamespace, attribute, context.CancellationToken));
        return isControlPackage ? new ControlCodeScope(sharedTokenKind, attribute) : null;
    }

    private static bool DeclaresAttributedType(INamespaceOrTypeSymbol container, INamedTypeSymbol attribute, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol or INamedTypeSymbol { TypeKind: TypeKind.Class }:
                    if (member is INamedTypeSymbol type)
                    {
                        foreach (var data in type.GetAttributes())
                        {
                            if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute))
                            {
                                return true;
                            }
                        }
                    }

                    if (DeclaresAttributedType((INamespaceOrTypeSymbol)member, attribute, cancellationToken))
                    {
                        return true;
                    }

                    break;
            }
        }

        return false;
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
