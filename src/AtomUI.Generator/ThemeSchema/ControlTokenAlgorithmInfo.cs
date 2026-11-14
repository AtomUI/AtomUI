using AtomUI.Generator.Diagnostics;
using AtomUI.Theme.Build;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

/// <summary>
/// One <c>[ControlTokenAlgorithm]</c> declaration: the algorithm identity and revision, the control token type the
/// class computes and the class itself.
/// </summary>
internal sealed class ControlTokenAlgorithmInfo
{
    internal ControlTokenAlgorithmInfo(string catalog, string id, int revision, string tokenTypeName, string algorithmTypeName)
    {
        Catalog = catalog;
        Id = id;
        Revision = revision;
        TokenTypeName = tokenTypeName;
        AlgorithmTypeName = algorithmTypeName;
    }

    public string Catalog { get; }
    public string Id { get; }
    public int Revision { get; }
    public string TokenTypeName { get; }
    public string AlgorithmTypeName { get; }

    // The runtime registry sorts by algorithm identity and then control; the token and class names make the
    // generated order total and independent of declaration order.
    internal static IEnumerable<ControlTokenAlgorithmInfo> Order(IEnumerable<ControlTokenAlgorithmInfo> algorithms) =>
        algorithms.OrderBy(static algorithm => algorithm.Catalog, StringComparer.Ordinal)
                  .ThenBy(static algorithm => algorithm.Id, StringComparer.Ordinal)
                  .ThenBy(static algorithm => algorithm.TokenTypeName, StringComparer.Ordinal)
                  .ThenBy(static algorithm => algorithm.AlgorithmTypeName, StringComparer.Ordinal);
}

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

    /// <summary>The fully qualified generated <c>&lt;Control&gt;Tokens</c> class that holds the control identity.</summary>
    public string TokensTypeName { get; }

    /// <summary>False when the class already declares <c>IControlToken</c> itself.</summary>
    public bool Emit { get; }
}

/// <summary>
/// Builds the control token stage model: which terminal token classes implement <c>IControlToken</c>, and the
/// validated <c>[ControlTokenAlgorithm]</c> declarations. A generator cannot see what another generator adds, so a
/// token type of this compilation counts as bound when the token model bound it to a control, and a token type of
/// a referenced assembly when it implements <c>IControlToken</c> in metadata.
/// </summary>
internal static class ControlTokenStageModel
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

    internal static IReadOnlyList<ControlTokenAlgorithmInfo> Collect(
        Compilation compilation,
        IEnumerable<string> declarations,
        IReadOnlyList<ControlTokenBinding> bindings,
        Action<Diagnostic> reportDiagnostic,
        CancellationToken cancellationToken)
    {
        var result = new List<ControlTokenAlgorithmInfo>();
        var stageInterface = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlTokenAlgorithmInterface);
        if (stageInterface is null)
        {
            return result;
        }

        var boundLocalTokens = new HashSet<string>(bindings.Select(static binding => binding.MetadataName), StringComparer.Ordinal);
        foreach (var name in declarations.Distinct(StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(name) is not { } type)
            {
                continue;
            }

            var attributes = type.GetAttributes()
                                 .Where(static attribute =>
                                     attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.ControlTokenAlgorithmAttribute)
                                 .ToArray();
            if (attributes.Length == 0)
            {
                continue;
            }

            var typeName = type.ToDisplayString();
            var typeLocation = GetLocation(type, cancellationToken);
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsGenericType ||
                !RegistrationModelBuilder.Accessible(type))
            {
                Report(reportDiagnostic, typeLocation,
                    $"'{typeName}' must be a non-abstract, non-generic class that is accessible within its assembly.");
                continue;
            }

            var stages = type.AllInterfaces
                             .Where(candidate => SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, stageInterface))
                             .ToArray();
            if (stages.Length != 1)
            {
                Report(reportDiagnostic, typeLocation,
                    $"'{typeName}' must implement IControlTokenAlgorithm<TToken> exactly once.");
                continue;
            }

            if (!type.InstanceConstructors.Any(static constructor =>
                    constructor.DeclaredAccessibility == Accessibility.Public && constructor.Parameters.Length == 0))
            {
                Report(reportDiagnostic, typeLocation, $"'{typeName}' must have a public parameterless constructor.");
                continue;
            }

            var tokenType = stages[0].TypeArguments[0];
            if (!IsBoundTerminalToken(compilation, tokenType, boundLocalTokens))
            {
                Report(reportDiagnostic, typeLocation,
                    $"'{typeName}' targets '{tokenType.ToDisplayString()}', which is not bound to a control.");
                continue;
            }

            var tokenTypeName = tokenType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var algorithmTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            foreach (var attribute in attributes)
            {
                if (attribute.ConstructorArguments.Length != 3 ||
                    attribute.ConstructorArguments[0].Value is not string catalog ||
                    attribute.ConstructorArguments[1].Value is not string id ||
                    attribute.ConstructorArguments[2].Value is not int revision ||
                    !ThemeBuildIdentifier.IsValid(catalog) || !ThemeBuildIdentifier.IsValid(id) || revision <= 0)
                {
                    Report(reportDiagnostic,
                        attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? typeLocation,
                        $"'{typeName}' declares a control algorithm with an invalid catalog, id or revision.");
                    continue;
                }

                result.Add(new ControlTokenAlgorithmInfo(catalog, id, revision, tokenTypeName, algorithmTypeName));
            }
        }

        return ControlTokenAlgorithmInfo.Order(result).ToArray();
    }

    private static bool IsBoundTerminalToken(Compilation compilation, ITypeSymbol token, ISet<string> boundLocalTokens)
    {
        if (token is not INamedTypeSymbol named || named.TypeKind == TypeKind.Error)
        {
            return false;
        }

        if (named.AllInterfaces.Any(static type => type.ToDisplayString() == TargetMarkConstants.ControlTokenInterface))
        {
            return true;
        }

        return SymbolEqualityComparer.Default.Equals(named.ContainingAssembly, compilation.Assembly) &&
               named.ContainingType is null &&
               !named.ContainingNamespace.IsGlobalNamespace &&
               boundLocalTokens.Contains(named.ContainingNamespace.ToDisplayString() + "." + named.MetadataName);
    }

    private static bool IsPartial(INamedTypeSymbol type) =>
        type.DeclaringSyntaxReferences.Length != 0 &&
        type.DeclaringSyntaxReferences.All(static reference =>
            reference.GetSyntax() is ClassDeclarationSyntax declaration &&
            declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static Location GetLocation(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        foreach (var reference in type.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax(cancellationToken) is ClassDeclarationSyntax declaration)
            {
                return declaration.Identifier.GetLocation();
            }
        }

        return type.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None;
    }

    private static void Report(Action<Diagnostic> reportDiagnostic, Location location, string message) =>
        reportDiagnostic(Diagnostic.Create(AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed, location, message));
}
