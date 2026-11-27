using System.Text.RegularExpressions;
using System.Xml.Linq;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

internal enum OwnTokenState
{
    Active,
    Reserved,
    Unreferenced
}

/// <summary>
/// One Own Token field of a control with its promise state. <see cref="Location" /> is only used to report
/// <c>ATOMUIGEN042</c>; the entry never enters the incremental cache.
/// </summary>
internal sealed class OwnTokenAnalysisEntry
{
    internal OwnTokenAnalysisEntry(
        string catalog,
        string controlId,
        string token,
        OwnTokenState state,
        string? reservedReason,
        Location? location)
    {
        Catalog = catalog;
        ControlId = controlId;
        Token = token;
        State = state;
        ReservedReason = reservedReason;
        Location = location;
    }

    internal string Catalog { get; }
    internal string ControlId { get; }
    internal string Token { get; }
    internal OwnTokenState State { get; }
    internal string? ReservedReason { get; }
    internal Location? Location { get; }
}

/// <summary>
/// A control (or <c>shared-primitive</c>) that reads a global Token from theme AXAML (<c>axaml</c>) or code
/// (<c>code</c>).
/// </summary>
internal readonly record struct GlobalConsumerAnalysisEntry(string Catalog, string ControlId, string GlobalToken, string Source);

internal sealed class OwnTokenReferenceAnalysisResult
{
    internal OwnTokenReferenceAnalysisResult(
        IReadOnlyList<OwnTokenAnalysisEntry> ownTokens,
        IReadOnlyList<GlobalConsumerAnalysisEntry> globalConsumers)
    {
        OwnTokens = ownTokens;
        GlobalConsumers = globalConsumers;
    }

    internal IReadOnlyList<OwnTokenAnalysisEntry> OwnTokens { get; }
    internal IReadOnlyList<GlobalConsumerAnalysisEntry> GlobalConsumers { get; }
}

/// <summary>
/// Classifies every Own Token field of a package as Active (referenced by its theme AXAML or code), Reserved
/// (<c>[ReservedToken]</c>) or Unreferenced (<c>ATOMUIGEN042</c>), and collects which control consumes which global
/// Token. Matching is purely textual/syntactic: AXAML attribute values and elements, and <see
/// cref="MemberAccessExpressionSyntax" /> of the package's own syntax trees. No semantic model is involved.
/// </summary>
internal static class OwnTokenReferenceAnalysis
{
    internal const string SharedPrimitive = "shared-primitive";
    internal const string AxamlSource = "axaml";
    internal const string CodeSource = "code";

    private const string TokenKindSuffix = "TokenKind";
    private const string TokenKeySuffix = "TokenKey";
    private const string TokenSuffix = "Token";
    private const string OwnerKind = "Owner";
    private const string SharedKind = "Shared";

    // `{atom:ButtonTokenResource Height}`, `{atom:ButtonTokenResource Kind=Height}`; may be nested in another extension.
    private static readonly Regex s_tokenResource = new(
        @"\{\s*(?:[\w.-]+:)?(?<kind>\w+?)TokenResource(?:Extension)?\b\s+(?:Kind\s*=\s*)?['""]?(?<name>\w+)",
        RegexOptions.CultureInvariant);

    internal static OwnTokenReferenceAnalysisResult Analyze(
        Compilation compilation,
        string catalog,
        IReadOnlyList<ControlThemeInfo> controls,
        ISet<string> globalTokenNames,
        ISet<string> tokenIdentityControls,
        IEnumerable<XElement> themeRoots,
        CancellationToken cancellationToken)
    {
        var ownByControl = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var control in controls)
        {
            if (control.HasOwnToken)
            {
                ownByControl[control.ControlName] = new HashSet<string>(
                    control.OwnTokens.Select(static token => token.Name), StringComparer.Ordinal);
            }
        }

        var roots = compilation.SyntaxTrees.Select(tree => tree.GetRoot(cancellationToken)).ToList();
        var collector = new Collector(catalog, ownByControl, globalTokenNames, tokenIdentityControls);
        collector.CollectOwnerTransparentControls(roots);
        foreach (var root in themeRoots)
        {
            collector.CollectAxaml(root);
        }

        collector.CollectTokenLayers(roots);
        foreach (var root in roots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            collector.CollectCode(root);
        }

        var active = collector.ResolveActiveMembers();
        collector.AddFieldFedConsumers(active);
        var entries = new List<OwnTokenAnalysisEntry>();
        foreach (var control in controls.Where(static control => control.HasOwnToken))
        {
            foreach (var token in control.OwnTokens)
            {
                control.OwnToken!.OwnTokenFacts.TryGetValue(token.Name, out var fact);
                var state = active.Contains((control.ControlName, token.Name))
                    ? OwnTokenState.Active
                    : fact?.ReservedReason is not null ? OwnTokenState.Reserved : OwnTokenState.Unreferenced;
                entries.Add(new OwnTokenAnalysisEntry(
                    catalog,
                    control.ControlName,
                    token.Name,
                    state,
                    state == OwnTokenState.Reserved ? fact!.ReservedReason : null,
                    GetLocation(fact, control.OwnToken!)));
            }
        }

        var consumers = collector.GlobalConsumers.ToList();
        entries.Sort(static (left, right) =>
        {
            var result = string.CompareOrdinal(left.ControlId, right.ControlId);
            return result != 0 ? result : string.CompareOrdinal(left.Token, right.Token);
        });
        consumers.Sort(static (left, right) =>
        {
            var result = string.CompareOrdinal(left.ControlId, right.ControlId);
            if (result != 0)
            {
                return result;
            }

            result = string.CompareOrdinal(left.GlobalToken, right.GlobalToken);
            return result != 0 ? result : string.CompareOrdinal(left.Source, right.Source);
        });
        return new OwnTokenReferenceAnalysisResult(entries, consumers);
    }

    internal static void Report(OwnTokenReferenceAnalysisResult result, Action<Diagnostic> report)
    {
        foreach (var entry in result.OwnTokens.Where(static entry => entry.State == OwnTokenState.Unreferenced))
        {
            report(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.OwnTokenUnreferenced, entry.Location, entry.ControlId, entry.Token));
        }
    }

    // A field inherited from a referenced assembly has no source location; report on the terminal Token class instead.
    private static Location? GetLocation(OwnTokenFact? fact, ControlTokenInfo ownToken) =>
        fact?.Location is { IsInSource: true } location ? location : ownToken.DeclarationLocation;

    private sealed class Collector
    {
        private readonly string _catalog;
        private readonly IReadOnlyDictionary<string, HashSet<string>> _ownByControl;
        private readonly ISet<string> _globals;
        private readonly ISet<string> _identities;
        private readonly Dictionary<XElement, ThemeAssetConsumptionAnalyzer.ThemeScope> _scopes = new();
        private readonly HashSet<GlobalConsumerAnalysisEntry> _consumers = new();

        internal Collector(
            string catalog,
            IReadOnlyDictionary<string, HashSet<string>> ownByControl,
            ISet<string> globals,
            ISet<string> identities)
        {
            _catalog = catalog;
            _ownByControl = ownByControl;
            _globals = globals;
            _identities = identities;
        }

        // Abstract Token layer class name -> the controls whose terminal Token inherits it (directly or through other layers).
        private readonly Dictionary<string, HashSet<string>> _layerControls = new(StringComparer.Ordinal);

        /// <summary>
        /// Maps every abstract <c>[ControlDesignToken]</c> layer to the terminal controls that inherit it, so the
        /// global reads of a layer count for each of those controls. The
        /// chain is followed syntactically by base-type name within the package's own sources.
        /// </summary>
        internal void CollectTokenLayers(IEnumerable<SyntaxNode> roots)
        {
            var baseOf = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var declaration in roots.SelectMany(static root => root.DescendantNodes().OfType<ClassDeclarationSyntax>()))
            {
                var name = declaration.Identifier.ValueText;
                if (!name.EndsWith(TokenSuffix, StringComparison.Ordinal) ||
                    declaration.BaseList?.Types.FirstOrDefault()?.Type is not { } baseType ||
                    GetRightmostTypeName(baseType) is not { } baseName)
                {
                    continue;
                }

                baseOf[name] = baseName;
            }

            foreach (var control in _ownByControl.Keys)
            {
                var visited = new HashSet<string>(StringComparer.Ordinal) { control + TokenSuffix };
                for (var current = control + TokenSuffix; baseOf.TryGetValue(current, out var layer) && visited.Add(layer); current = layer)
                {
                    if (!_layerControls.TryGetValue(layer, out var controls))
                    {
                        _layerControls.Add(layer, controls = new HashSet<string>(StringComparer.Ordinal));
                    }

                    controls.Add(control);
                }
            }
        }

        private static string? GetRightmostTypeName(TypeSyntax type) => type switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
            AliasQualifiedNameSyntax alias => alias.Name.Identifier.ValueText,
            _ => null
        };

        // The controls whose Own Tokens a Token class declares: its own control for a terminal, every inheriting terminal
        // control for an abstract layer.
        private IEnumerable<string> GetTokenClassControls(string className)
        {
            if (className.Length > TokenSuffix.Length && className.EndsWith(TokenSuffix, StringComparison.Ordinal))
            {
                var control = className.Substring(0, className.Length - TokenSuffix.Length);
                if (_ownByControl.ContainsKey(control))
                {
                    return [control];
                }
            }

            return _layerControls.TryGetValue(className, out var controls) ? controls : [];
        }

        // Members of a Token class read by AXAML or by code outside the Token class.
        private readonly HashSet<(string Control, string Member)> _referenced = new();

        /// <summary>
        /// Members that are consumed: referenced by the theme AXAML or by code outside the token class. Token values are
        /// computed in the value packages, so a control package sees no member that feeds another one.
        /// </summary>
        internal HashSet<(string Control, string Member)> ResolveActiveMembers() => new(_referenced);

        internal IEnumerable<GlobalConsumerAnalysisEntry> GlobalConsumers => _consumers;

        // `Field = ... EffectiveGlobalToken.X ...` in an evaluator: X is consumed only when Field is (Active).
        private readonly Dictionary<(string Control, string Member), HashSet<string>> _fieldFedGlobals = new();

        /// <summary>
        /// Adds the global reads that feed an Own field directly once the active fields are known: a global that only
        /// feeds Reserved or Unreferenced fields has no visible consumer and is not part of the control's consumption.
        /// Reads through locals or outside such assignments are recorded unconditionally when collected.
        /// </summary>
        internal void AddFieldFedConsumers(ISet<(string Control, string Member)> active)
        {
            foreach (var pair in _fieldFedGlobals)
            {
                if (!active.Contains(pair.Key))
                {
                    continue;
                }

                foreach (var global in pair.Value)
                {
                    _consumers.Add(new GlobalConsumerAnalysisEntry(_catalog, pair.Key.Control, global, CodeSource));
                }
            }
        }

        private static string? GetAssignedOwnField(MemberAccessExpressionSyntax access, ISet<string> own)
        {
            var assignment = access.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment is null || !assignment.Right.Span.Contains(access.Span))
            {
                return null;
            }

            var target = assignment.Left switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } member => member.Name.Identifier.ValueText,
                _ => null
            };
            return target is not null && own.Contains(target) ? target : null;
        }

        private readonly HashSet<string> _ownerTransparentControls = new(StringComparer.Ordinal);

        internal void CollectOwnerTransparentControls(IEnumerable<SyntaxNode> roots)
        {
            foreach (var declaration in roots.SelectMany(static root => root.DescendantNodes().OfType<ClassDeclarationSyntax>()))
            {
                if (declaration.AttributeLists.SelectMany(static list => list.Attributes)
                               .Any(static attribute => GetRightmostTypeName(attribute.Name) is
                                   "TokenOwnerTransparent" or "TokenOwnerTransparentAttribute"))
                {
                    _ownerTransparentControls.Add(declaration.Identifier.ValueText);
                }
            }
        }

        // ---- AXAML ------------------------------------------------------------------------

        internal void CollectAxaml(XElement root)
        {
            foreach (var element in root.DescendantsAndSelf())
            {
                var localName = element.Name.LocalName;
                if (localName == "TokenTransitions.Duration")
                {
                    AddTransitionDurationReference(element.Value.Trim(), element);
                }

                if (TrySplitTokenResource(localName, out var elementKind))
                {
                    var name = ThemeAssetConsumptionAnalyzer.AttributeValue(element, "Kind")?.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        AddAxamlReference(elementKind, name!, element);
                    }
                }

                foreach (var attribute in element.Attributes())
                {
                    if (attribute.Name.LocalName == "TokenTransitions.Duration")
                    {
                        AddTransitionDurationReference(attribute.Value.Trim(), element);
                    }

                    if (attribute.IsNamespaceDeclaration || attribute.Value.IndexOf("TokenResource", StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    foreach (Match match in s_tokenResource.Matches(attribute.Value))
                    {
                        AddAxamlReference(match.Groups["kind"].Value, match.Groups["name"].Value, element);
                    }
                }
            }
        }

        private void AddTransitionDurationReference(string name, XElement element)
        {
            // The attached marker is consumed only by the TokenTransitions factory. A plain Transitions
            // collection does not read it. Delay is not a supported TokenTransitions attached property.
            if (!_globals.Contains(name) ||
                !element.Ancestors().Any(static ancestor => ancestor.Name.LocalName == "TokenTransitions"))
            {
                return;
            }

            var scope = GetScope(element);
            var owner = scope is { HasIdentity: true, TargetTypeName: not null } &&
                        !_ownerTransparentControls.Contains(scope.TargetTypeName)
                ? scope.TargetTypeName
                : SharedPrimitive;
            _consumers.Add(new GlobalConsumerAnalysisEntry(_catalog, owner, name, AxamlSource));
        }

        private static bool TrySplitTokenResource(string localName, out string kind)
        {
            const string suffix = "TokenResource";
            const string extensionSuffix = "TokenResourceExtension";
            kind = string.Empty;
            if (localName.EndsWith(extensionSuffix, StringComparison.Ordinal))
            {
                kind = localName.Substring(0, localName.Length - extensionSuffix.Length);
            }
            else if (localName.EndsWith(suffix, StringComparison.Ordinal))
            {
                kind = localName.Substring(0, localName.Length - suffix.Length);
            }

            return kind.Length != 0;
        }

        private void AddAxamlReference(string kind, string name, XElement element)
        {
            if (string.Equals(kind, SharedKind, StringComparison.Ordinal))
            {
                return;
            }

            if (string.Equals(kind, OwnerKind, StringComparison.Ordinal))
            {
                if (_globals.Contains(name))
                {
                    var scope = GetScope(element);
                    _consumers.Add(new GlobalConsumerAnalysisEntry(
                        _catalog, scope is { HasIdentity: true, TargetTypeName: not null } ? scope.TargetTypeName : SharedPrimitive, name, AxamlSource));
                }

                return;
            }

            if (_ownByControl.TryGetValue(kind, out var own) && (own.Contains(name) || !_globals.Contains(name)))
            {
                _referenced.Add((kind, name));
            }
            else if (_globals.Contains(name) && _identities.Contains(kind))
            {
                _consumers.Add(new GlobalConsumerAnalysisEntry(_catalog, kind, name, AxamlSource));
            }
        }

        // The TargetType owner of the nearest enclosing ControlTheme; null outside any ControlTheme.
        private ThemeAssetConsumptionAnalyzer.ThemeScope? GetScope(XElement element)
        {
            var theme = element.AncestorsAndSelf().FirstOrDefault(ThemeAssetConsumptionAnalyzer.IsControlTheme);
            if (theme is null)
            {
                return null;
            }

            if (!_scopes.TryGetValue(theme, out var scope))
            {
                scope = ThemeAssetConsumptionAnalyzer.CreateScope(theme, _identities);
                _scopes.Add(theme, scope);
            }

            return scope;
        }

        // ---- C# ---------------------------------------------------------------------------

        internal void CollectCode(SyntaxNode root)
        {
            foreach (var node in root.DescendantNodes())
            {
                if (node is MemberAccessExpressionSyntax access)
                {
                    CollectMemberAccess(access);
                }
            }
        }

        private void CollectMemberAccess(MemberAccessExpressionSyntax access)
        {
            var expressionName = GetRightmostName(access.Expression);
            if (expressionName is null)
            {
                return;
            }

            var name = access.Name.Identifier.ValueText;
            if (string.Equals(expressionName, "SharedTokenKind", StringComparison.Ordinal))
            {
                if (_globals.Contains(name))
                {
                    _consumers.Add(new GlobalConsumerAnalysisEntry(_catalog, GetEnclosingControl(access), name, CodeSource));
                }
            }
            else if (string.Equals(expressionName, "EffectiveGlobalToken", StringComparison.Ordinal))
            {
                if (_globals.Contains(name))
                {
                    foreach (var tokenControl in GetEnclosingTokenControls(access))
                    {
                        if (GetAssignedOwnField(access, _ownByControl[tokenControl]) is { } field)
                        {
                            if (!_fieldFedGlobals.TryGetValue((tokenControl, field), out var globals))
                            {
                                globals = new HashSet<string>(StringComparer.Ordinal);
                                _fieldFedGlobals.Add((tokenControl, field), globals);
                            }

                            globals.Add(name);
                        }
                        else
                        {
                            _consumers.Add(new GlobalConsumerAnalysisEntry(_catalog, tokenControl, name, CodeSource));
                        }
                    }
                }
            }
            else if (expressionName.EndsWith(TokenKindSuffix, StringComparison.Ordinal) ||
                     expressionName.EndsWith(TokenKeySuffix, StringComparison.Ordinal))
            {
                var isKind = expressionName.EndsWith(TokenKindSuffix, StringComparison.Ordinal);
                var control = expressionName.Substring(
                    0, expressionName.Length - (isKind ? TokenKindSuffix.Length : TokenKeySuffix.Length));
                if (_ownByControl.TryGetValue(control, out var own) && (own.Contains(name) || !_globals.Contains(name)))
                {
                    // A field read by its own evaluator is not a reference; derivations are handled separately.
                    if (!IsInsideTokenClass(access, control))
                    {
                        _referenced.Add((control, name));
                    }
                }
                else if (!isKind && _globals.Contains(name) && _identities.Contains(control))
                {
                    _consumers.Add(new GlobalConsumerAnalysisEntry(_catalog, control, name, CodeSource));
                }
            }
        }

        private static string? GetRightmostName(ExpressionSyntax expression) => expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            _ => null
        };

        private bool IsInsideTokenClass(SyntaxNode node, string control)
        {
            return node.Ancestors().OfType<ClassDeclarationSyntax>()
                       .Any(declaration => GetTokenClassControls(declaration.Identifier.ValueText).Contains(control));
        }

        // `ButtonToken` -> `Button` for the innermost enclosing terminal Token class of a control with Own Tokens; an
        // abstract Token layer yields every terminal control that inherits it.
        private IEnumerable<string> GetEnclosingTokenControls(SyntaxNode node)
        {
            foreach (var declaration in node.Ancestors().OfType<ClassDeclarationSyntax>())
            {
                var controls = GetTokenClassControls(declaration.Identifier.ValueText).ToList();
                if (controls.Count != 0)
                {
                    return controls;
                }
            }

            return [];
        }

        // The innermost enclosing type that is a control with a Token identity (or its Token class); otherwise shared.
        private string GetEnclosingControl(SyntaxNode node)
        {
            foreach (var declaration in node.Ancestors().OfType<BaseTypeDeclarationSyntax>())
            {
                var name = declaration.Identifier.ValueText;
                if (_identities.Contains(name))
                {
                    return name;
                }

                if (name.Length > TokenSuffix.Length && name.EndsWith(TokenSuffix, StringComparison.Ordinal))
                {
                    var control = name.Substring(0, name.Length - TokenSuffix.Length);
                    if (_identities.Contains(control))
                    {
                        return control;
                    }
                }
            }

            return SharedPrimitive;
        }
    }
}
