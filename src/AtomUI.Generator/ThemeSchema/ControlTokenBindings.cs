using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

/// <summary>
/// A terminal control token class of this compilation that is bound to a control, and whether the generator emits
/// its <c>IControlToken</c> half.
/// </summary>
internal sealed class ControlTokenBinding
{
    internal ControlTokenBinding(string tokenNamespace, string tokenName, string tokensTypeName, bool emit)
    {
        TokenNamespace = tokenNamespace;
        TokenName = tokenName;
        TokensTypeName = tokensTypeName;
        Emit = emit;
    }

    public string TokenNamespace { get; }
    public string TokenName { get; }
    public string MetadataName => TokenNamespace + "." + TokenName;

    /// <summary>
    /// The fully qualified generated <c>&lt;Control&gt;Tokens</c> class that holds the control identity.
    /// </summary>
    public string TokensTypeName { get; }

    /// <summary>
    /// False when the class already declares <c>IControlToken</c> itself.
    /// </summary>
    public bool Emit { get; }
}

/// <summary>
/// Which terminal token classes of this compilation implement <c>IControlToken</c>: the generator emits that half for
/// every terminal control token class bound to a control, unless the class already declares the interface itself.
/// </summary>
internal static class ControlTokenBindings
{
    internal static IReadOnlyList<ControlTokenBinding> GetBindings(Compilation compilation, IEnumerable<ControlThemeInfo> controls)
    {
        var controlToken = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlTokenInterface);
        if (controlToken is null)
        {
            return Array.Empty<ControlTokenBinding>();
        }

        var result = new List<ControlTokenBinding>();
        foreach (var control in controls)
        {
            if (!control.HasDescriptor || control.OwnToken is not { IsTerminal: true } ownToken ||
                string.IsNullOrWhiteSpace(ownToken.TokenNamespace) ||
                compilation.GetTypeByMetadataName(ownToken.GetFullyQualifiedTokenTypeName()) is not { } token ||
                !SymbolEqualityComparer.Default.Equals(token.ContainingAssembly, compilation.Assembly) ||
                token.ContainingType is not null || token.IsGenericType || token.IsAbstract || !IsPartial(token))
            {
                continue;
            }

            var declaresInterface = token.Interfaces.Any(type => SymbolEqualityComparer.Default.Equals(type, controlToken));
            result.Add(new ControlTokenBinding(
                ownToken.TokenNamespace,
                ownToken.TokenName,
                $"global::{control.ControlNamespace}.DesignTokens.{control.TokensType}",
                emit: !declaresInterface));
        }

        return result.OrderBy(static binding => binding.MetadataName, StringComparer.Ordinal).ToArray();
    }

    private static bool IsPartial(INamedTypeSymbol type) =>
        type.DeclaringSyntaxReferences.Length != 0 &&
        type.DeclaringSyntaxReferences.All(static reference =>
            reference.GetSyntax() is ClassDeclarationSyntax declaration &&
            declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

}
