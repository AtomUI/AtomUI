using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.DesignLanguages;

/// <summary>
/// One registered rule: its class, the control token it writes and the language token it reads.
/// </summary>
internal readonly record struct ControlTokenRuleSymbols(INamedTypeSymbol Rule, INamedTypeSymbol Interface, INamedTypeSymbol Token,
    ITypeSymbol Language, Location Location);

/// <summary>
/// The build-time checks of a value package: every token of a derived control is written (completeness), a
/// rule does not read a token of its own before writing it, every token class of a covered control package has a
/// rule (coverage), and a value package holds no controls (dependency direction).
/// </summary>
internal static class ControlTokenRuleAnalysis
{
    private const string ControlTokenBase = "AtomUI.Theme.DesignTokens.AbstractControlDesignToken";
    private const string NotTokenAttribute = "AtomUI.Theme.DesignTokens.NotTokenDefinitionAttribute";
    private const string ControlBase = "Avalonia.Controls.Control";

    internal static void Run(Compilation compilation, IReadOnlyList<ControlTokenRuleSymbols> rules, Action<Diagnostic> report,
        CancellationToken cancellationToken)
    {
        if (rules.Count == 0)
        {
            return;
        }

        var written = CollectWrites(compilation, cancellationToken);
        foreach (var rule in rules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Properties a base token class of another package declares are written by that package's shared rule
            // helpers, which this compilation cannot see; the contract test library checks them at run time.
            foreach (var property in TokenProperties(compilation, rule.Token)
                         .Where(property => SymbolEqualityComparer.Default.Equals(property.ContainingAssembly,
                             rule.Token.ContainingAssembly)))
            {
                if (!written.Contains(property))
                {
                    report(Diagnostic.Create(AtomUIDiagnosticDescriptors.TokenRuleLeavesTokenUnwritten, rule.Location,
                        rule.Rule.ToDisplayString(), property.Name, rule.Token.ToDisplayString()));
                }
            }

            ReportReadsBeforeWrites(compilation, rule, report);
        }

        ReportMissingCoverage(compilation, rules, report);
        ReportControlsInValuePackage(compilation, report, cancellationToken);
    }

    /// <summary>
    /// The token properties of a control token class: its public settable properties up to the control token base.
    /// </summary>
    internal static IEnumerable<IPropertySymbol> TokenProperties(Compilation compilation, INamedTypeSymbol token)
    {
        var tokenBase = compilation.GetTypeByMetadataName(ControlTokenBase);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var type = token; type is not null && !SymbolEqualityComparer.Default.Equals(type, tokenBase); type = type.BaseType)
        {
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic || property.DeclaredAccessibility != Accessibility.Public || property.SetMethod is null ||
                    property.IsIndexer || !seen.Add(property.Name) ||
                    property.GetAttributes().Any(static attribute => attribute.AttributeClass?.ToDisplayString() == NotTokenAttribute))
                {
                    continue;
                }

                yield return property.OriginalDefinition;
            }
        }
    }

    // Every property written anywhere in the compilation: a rule may delegate to shared helpers.
    private static HashSet<IPropertySymbol> CollectWrites(Compilation compilation, CancellationToken cancellationToken)
    {
        var result = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        foreach (var tree in compilation.SyntaxTrees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var model = compilation.GetSemanticModel(tree);
            foreach (var assignment in tree.GetRoot(cancellationToken).DescendantNodes().OfType<AssignmentExpressionSyntax>())
            {
                if (model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol is IPropertySymbol property)
                {
                    AddWithOverridden(result, property);
                }
            }
        }

        return result;
    }

    private static void AddWithOverridden(HashSet<IPropertySymbol> result, IPropertySymbol property)
    {
        for (var current = property; current is not null; current = current.OverriddenProperty)
        {
            result.Add(current.OriginalDefinition);
        }
    }

    // Within the Apply body: a read of a token property that the same body writes only later, with no call before the read
    // that could have written it.
    private static void ReportReadsBeforeWrites(Compilation compilation, ControlTokenRuleSymbols rule, Action<Diagnostic> report)
    {
        var apply = rule.Interface.GetMembers("Apply").OfType<IMethodSymbol>().FirstOrDefault();
        var method = apply is null ? null : rule.Rule.FindImplementationForInterfaceMember(apply) as IMethodSymbol;
        if (method?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not MethodDeclarationSyntax syntax)
        {
            return;
        }

        var body = (SyntaxNode?)syntax.Body ?? syntax.ExpressionBody;
        if (body is null)
        {
            return;
        }

        var model = compilation.GetSemanticModel(syntax.SyntaxTree);
        var properties = new HashSet<IPropertySymbol>(TokenProperties(compilation, rule.Token), SymbolEqualityComparer.Default);
        var writes = new Dictionary<IPropertySymbol, int>(SymbolEqualityComparer.Default);
        var firstCall = int.MaxValue;
        foreach (var node in body.DescendantNodes())
        {
            if (node is AssignmentExpressionSyntax assignment &&
                model.GetSymbolInfo(assignment.Left).Symbol is IPropertySymbol written &&
                properties.Contains(written.OriginalDefinition) && !writes.ContainsKey(written.OriginalDefinition))
            {
                writes[written.OriginalDefinition] = assignment.Left.SpanStart;
            }
            else if (node is InvocationExpressionSyntax invocation && invocation.ArgumentList.Arguments.Count != 0)
            {
                firstCall = Math.Min(firstCall, invocation.SpanStart);
            }
        }

        var reported = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        foreach (var access in body.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            if (access.Parent is AssignmentExpressionSyntax parent && parent.Left == access &&
                parent.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                continue;
            }

            if (model.GetSymbolInfo(access).Symbol is IPropertySymbol read &&
                properties.Contains(read.OriginalDefinition) &&
                writes.TryGetValue(read.OriginalDefinition, out var writePosition) &&
                access.SpanStart < writePosition && access.SpanStart < firstCall &&
                reported.Add(read.OriginalDefinition))
            {
                report(Diagnostic.Create(AtomUIDiagnosticDescriptors.TokenRuleReadsUnwrittenToken, access.GetLocation(),
                    rule.Rule.ToDisplayString(), read.Name));
            }
        }
    }

    // Coverage: for each control package this one derives, every bound token class has a rule per language.
    private static void ReportMissingCoverage(Compilation compilation, IReadOnlyList<ControlTokenRuleSymbols> rules,
        Action<Diagnostic> report)
    {
        var controlToken = compilation.GetTypeByMetadataName(TargetMarkConstants.ControlTokenInterface);
        var covered = new HashSet<(string Language, string Token)>(
            rules.Select(static rule => (rule.Language.ToDisplayString(), rule.Token.ToDisplayString())));
        foreach (var group in rules.Where(rule =>
                                                    !SymbolEqualityComparer.Default.Equals(rule.Token.ContainingAssembly, compilation.Assembly))
                                         .GroupBy(static rule => rule.Token.ContainingAssembly, SymbolEqualityComparer.Default))
        {
            var assembly = (IAssemblySymbol)group.Key!;
            var tokens = AllTypes(assembly.GlobalNamespace)
                .Where(type => !type.IsAbstract && type.DeclaredAccessibility == Accessibility.Public &&
                               type.Interfaces.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, controlToken)))
                .ToArray();
            // A derived language writes only the controls it changes; the rest resolve to its base language's value
            // packages along the language token chain, which the startup coverage check verifies.
            foreach (var language in group.Where(rule => !IsDerivedLanguage(compilation, rule.Language))
                                          .Select(static rule => rule.Language.ToDisplayString()).Distinct())
            {
                foreach (var token in tokens)
                {
                    if (!covered.Contains((language, token.ToDisplayString())))
                    {
                        report(Diagnostic.Create(AtomUIDiagnosticDescriptors.ValuePackageMissesTokenRule, Location.None,
                            assembly.Name, language, token.ToDisplayString()));
                    }
                }
            }
        }
    }

    // A language token class whose base is another language's token class (not LanguageDesignToken itself).
    private static bool IsDerivedLanguage(Compilation compilation, ITypeSymbol language)
    {
        var root = compilation.GetTypeByMetadataName(TargetMarkConstants.LanguageDesignToken);
        return root is not null && language.BaseType is { } baseType &&
               !SymbolEqualityComparer.Default.Equals(baseType, root) &&
               !SymbolEqualityComparer.Default.Equals(language, root);
    }

    // A value package holds no controls; a compilation that declares its own control tokens is a control package that
    // also carries values for them (GalleryBase) and is exempt.
    private static void ReportControlsInValuePackage(Compilation compilation, Action<Diagnostic> report, CancellationToken cancellationToken)
    {
        var tokenBase = compilation.GetTypeByMetadataName(ControlTokenBase);
        var controlBase = compilation.GetTypeByMetadataName(ControlBase);
        if (controlBase is null || AllTypes(compilation.Assembly.GlobalNamespace).Any(type => DerivesFrom(type, tokenBase)))
        {
            return;
        }

        foreach (var type in AllTypes(compilation.Assembly.GlobalNamespace))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (DerivesFrom(type, controlBase))
            {
                report(Diagnostic.Create(AtomUIDiagnosticDescriptors.PackageDependencyDirection,
                    type.Locations.FirstOrDefault() ?? Location.None,
                    $"A value package holds no controls; '{type.ToDisplayString()}' derives from a control."));
            }
        }
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol? baseType)
    {
        for (var current = type.BaseType; current is not null && baseType is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
        }
        return false;
    }

    internal static IEnumerable<INamedTypeSymbol> AllTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            yield return type;
            foreach (var nested in Nested(type))
            {
                yield return nested;
            }
        }
        foreach (var child in ns.GetNamespaceMembers())
        {
            foreach (var type in AllTypes(child))
            {
                yield return type;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> Nested(INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;
            foreach (var deeper in Nested(nested))
            {
                yield return deeper;
            }
        }
    }
}
