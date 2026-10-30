using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator;

internal sealed class SemanticPartTemplateValidator
{
    private readonly IReadOnlyList<ThemeAssetInfo> _assets;
    private readonly SemanticPartTypeResolver _typeResolver;
    private readonly Action<Diagnostic> _reportDiagnostic;

    internal SemanticPartTemplateValidator(
        IReadOnlyList<ThemeAssetInfo> assets,
        SemanticPartTypeResolver typeResolver,
        Action<Diagnostic> reportDiagnostic)
    {
        _assets = assets;
        _typeResolver = typeResolver;
        _reportDiagnostic = reportDiagnostic;
    }

    internal bool Validate(
        SemanticControlDeclaration control,
        IReadOnlyList<SemanticPartDeclaration> parts)
    {
        var staticParts = parts.Where(static part => !part.RuntimeCreated).ToArray();
        if (staticParts.Length == 0)
        {
            return true;
        }

        // 显式声明 CrossNestedOwners 且路由越过首个模板边界（">>" 或第二个 "/template/"）
        // 的部件，把目标锚定在嵌套控件（如 AutoComplete 内嵌的输入控件）自己的模板里，
        // 宿主模板中不存在其标记类；这类部件改为跨主题资产校验标记存在性。
        // 未声明 CrossNestedOwners 的既有部件（如 NumericUpDown、LineEdit）继续按宿主模板校验。
        static bool IsCrossNestedOwnerRoute(SemanticPartDeclaration part)
        {
            if (!part.CrossNestedOwners || part.SelectorRoute is null)
            {
                return false;
            }

            if (part.SelectorRoute.Contains(">>", StringComparison.Ordinal))
            {
                return true;
            }

            var templateStepCount = part.SelectorRoute.Split(' ')
                                                     .Count(static token =>
                                                         string.Equals(token, "/template/", StringComparison.Ordinal));
            return templateStepCount > 1;
        }

        var localParts = staticParts
                         .Where(static part => !IsCrossNestedOwnerRoute(part))
                         .ToArray();
        var crossOwnerParts = staticParts
                              .Where(IsCrossNestedOwnerRoute)
                              .ToArray();

        var templates = ResolveTemplates(control.ControlType);
        if (templates.Count == 0)
        {
            _reportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.SemanticPartMissingControlTemplate,
                control.Location,
                control.ControlType.ToDisplayString()));
            return false;
        }

        var declaredClasses = new HashSet<string>(
            localParts.Select(static part => part.SelectorClass!),
            StringComparer.Ordinal);
        var valid = true;
        foreach (var entry in templates)
        {
            var templateName = $"{entry.Asset.AssetPath}#{entry.Template.Index}";
            var relevantMarkers = entry.Template.Markers
                                       .Where(marker => declaredClasses.Contains(marker.SelectorClass))
                                       .ToArray();
            foreach (var marker in relevantMarkers.Where(static marker => !marker.IsStaticallyEnabled))
            {
                _reportDiagnostic(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.SemanticPartTemplateMarkerNotStatic,
                    entry.Asset.CreateLocation(),
                    control.ControlType.ToDisplayString(),
                    templateName,
                    marker.SelectorClass,
                    marker.TypeName));
                valid = false;
            }

            var validMarkers = relevantMarkers.Where(static marker => marker.IsStaticallyEnabled).ToArray();
            foreach (var nodeMarkers in validMarkers.GroupBy(static marker => marker.NodeId))
            {
                var selectorClasses = nodeMarkers.Select(static marker => marker.SelectorClass)
                                                 .Distinct(StringComparer.Ordinal)
                                                 .OrderBy(static selectorClass => selectorClass, StringComparer.Ordinal)
                                                 .ToArray();
                if (selectorClasses.Length <= 1)
                {
                    continue;
                }

                _reportDiagnostic(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.SemanticPartTemplateNodeConflict,
                    entry.Asset.CreateLocation(),
                    control.ControlType.ToDisplayString(),
                    templateName,
                    nodeMarkers.First().TypeName,
                    string.Join(", ", selectorClasses)));
                valid = false;
            }

            foreach (var part in localParts)
            {
                var markers = validMarkers
                              .Where(marker => string.Equals(
                                  marker.SelectorClass,
                                  part.SelectorClass,
                                  StringComparison.Ordinal))
                              .ToArray();
                if (!MatchesCardinality(part.Cardinality, markers.Length))
                {
                    _reportDiagnostic(Diagnostic.Create(
                        AtomUIDiagnosticDescriptors.SemanticPartTemplateCardinalityMismatch,
                        entry.Asset.CreateLocation(),
                        control.ControlType.ToDisplayString(),
                        templateName,
                        markers.Length,
                        part.Name,
                        DescribeCardinality(part.Cardinality)));
                    valid = false;
                }

                if (part.ContractType is not INamedTypeSymbol contractType)
                {
                    continue;
                }

                foreach (var marker in markers)
                {
                    var markerType = _typeResolver.ResolveMarkerType(marker);
                    if (markerType is null ||
                        !SemanticPartTypeResolver.IsAssignableTo(markerType, contractType))
                    {
                        _reportDiagnostic(Diagnostic.Create(
                            AtomUIDiagnosticDescriptors.SemanticPartTemplateContractTypeMismatch,
                            entry.Asset.CreateLocation(),
                            control.ControlType.ToDisplayString(),
                            templateName,
                            part.Name,
                            markerType?.ToDisplayString() ?? marker.TypeName,
                            contractType.ToDisplayString()));
                        valid = false;
                    }
                }
            }
        }

        return ValidateCrossNestedOwnerParts(parts, crossOwnerParts, templates) && valid;
    }

    /// 跨嵌套控件部件：路由中首个边界操作符前的锚点类定位宿主模板里的嵌套控件节点，
    /// 沿其类型继承链找到承载目标模板的主题资产；锚点与目标之间允许经过同控件已声明的
    /// RuntimeCreated 部件（以其 ContractType 为节点类型继续解析主题链），保持与宿主模板
    /// 校验同等级的静态检查能力。
    private bool ValidateCrossNestedOwnerParts(
        IReadOnlyList<SemanticPartDeclaration> allDeclarations,
        IReadOnlyList<SemanticPartDeclaration> parts,
        IReadOnlyList<(ThemeAssetInfo Asset, ThemeAssetSemanticTemplateInfo Template)> ownerTemplates)
    {
        if (parts.Count == 0)
        {
            return true;
        }

        var allThemes = _assets.OrderBy(static asset => asset.AssetPath, StringComparer.Ordinal)
                               .SelectMany(static asset => asset.SemanticThemes)
                               .ToArray();
        var resolvedTargets = new Dictionary<ThemeAssetSemanticThemeInfo, INamedTypeSymbol?>();
        foreach (var theme in allThemes)
        {
            resolvedTargets[theme] = _typeResolver.ResolveTargetType(theme.TargetType);
        }

        var valid = true;
        foreach (var part in parts)
        {
            var targetMarkers = CollectCrossNestedOwnerMarkers(part, allDeclarations, ownerTemplates, allThemes, resolvedTargets);
            var enabledCount = targetMarkers.Count(static marker => marker.IsStaticallyEnabled);
            if (!MatchesCardinality(part.Cardinality, enabledCount))
            {
                _reportDiagnostic(Diagnostic.Create(
                    AtomUIDiagnosticDescriptors.SemanticPartTemplateCardinalityMismatch,
                    part.Location,
                    "(cross-nested-owner)",
                    part.SelectorRoute ?? part.SelectorClass ?? part.Name,
                    enabledCount,
                    part.Name,
                    DescribeCardinality(part.Cardinality)));
                valid = false;
            }

            if (part.ContractType is not INamedTypeSymbol contractType)
            {
                continue;
            }

            foreach (var marker in targetMarkers.Where(static marker => marker.IsStaticallyEnabled))
            {
                var markerType = _typeResolver.ResolveMarkerType(marker);
                if (markerType is null ||
                    !SemanticPartTypeResolver.IsAssignableTo(markerType, contractType))
                {
                    _reportDiagnostic(Diagnostic.Create(
                        AtomUIDiagnosticDescriptors.SemanticPartTemplateContractTypeMismatch,
                        part.Location,
                        "(cross-nested-owner)",
                        part.SelectorRoute ?? part.SelectorClass ?? part.Name,
                        part.Name,
                        markerType?.ToDisplayString() ?? marker.TypeName,
                        contractType.ToDisplayString()));
                    valid = false;
                }
            }
        }

        return valid;
    }

    private IReadOnlyList<ThemeAssetSemanticMarkerInfo> CollectCrossNestedOwnerMarkers(
        SemanticPartDeclaration part,
        IReadOnlyList<SemanticPartDeclaration> allParts,
        IReadOnlyList<(ThemeAssetInfo Asset, ThemeAssetSemanticTemplateInfo Template)> ownerTemplates,
        IReadOnlyList<ThemeAssetSemanticThemeInfo> allThemes,
        Dictionary<ThemeAssetSemanticThemeInfo, INamedTypeSymbol?> resolvedTargets)
    {
        var route = part.SelectorRoute;
        if (route is null)
        {
            return Array.Empty<ThemeAssetSemanticMarkerInfo>();
        }

        var tokens = route.Split(' ');
        var anchorClass = string.Empty;
        for (var index = 1; index < tokens.Length; index++)
        {
            if (tokens[index] is ">>" or "/template/")
            {
                if (tokens[index - 1].StartsWith(".", StringComparison.Ordinal))
                {
                    anchorClass = tokens[index - 1].Substring(1);
                }

                break;
            }
        }

        if (anchorClass.Length == 0)
        {
            return Array.Empty<ThemeAssetSemanticMarkerInfo>();
        }

        var candidateThemes = new List<ThemeAssetSemanticThemeInfo>();
        foreach (var entry in ownerTemplates)
        {
            foreach (var marker in entry.Template.Markers.Where(candidate =>
                         string.Equals(candidate.SelectorClass, anchorClass, StringComparison.Ordinal)))
            {
                var nodeType = _typeResolver.ResolveMarkerType(marker);
                if (nodeType is null)
                {
                    continue;
                }

                candidateThemes.AddRange(allThemes.Where(theme =>
                {
                    var themeTarget = resolvedTargets[theme];
                    return themeTarget is not null &&
                           SemanticPartTypeResolver.IsAssignableTo(nodeType, themeTarget);
                }));
            }
        }

        // 嵌套控件经 StyleKeyOverride 只消费最派生的主题；基类主题（如 SearchEdit 之上的
        // LineEdit）不参与运行时解析，这里同样剔除，避免重复计数。
        var targetThemeSet = DedupeMostDerivedThemes(candidateThemes, resolvedTargets);

        var selectorClass = part.SelectorClass!;
        var targetMarkers = targetThemeSet
                            .SelectMany(static theme => theme.Templates)
                            .SelectMany(static template => template.Markers)
                            .Where(marker => string.Equals(marker.SelectorClass, selectorClass, StringComparison.Ordinal))
                            .ToList();
        if (targetMarkers.Count > 0)
        {
            return targetMarkers;
        }

        // 锚点主题链中不存在目标标记时，允许路由经由中间跳步继续解析：静态标记节点
        // 按其实际类型下沉到对应主题；同控件已声明的 RuntimeCreated 部件（标记由容器
        // 创建路径注入，静态主题中不可见）以 ContractType 承转主题链（如 Cascader 的
        // item 标签容器承转 itemContent / itemRemove）。
        var routeClasses = tokens.Where(static token => token.StartsWith(".", StringComparison.Ordinal))
                                 .Select(static token => token.Substring(1))
                                 .ToArray();
        var hopCount = routeClasses.Length - 2;
        for (var hopIndex = 1; hopIndex <= hopCount && targetMarkers.Count == 0; hopIndex++)
        {
            var hopClass = routeClasses[hopIndex];
            var hopMarkerTypes = targetThemeSet
                                 .SelectMany(static theme => theme.Templates)
                                 .SelectMany(static template => template.Markers)
                                 .Where(marker => string.Equals(
                                     marker.SelectorClass,
                                     hopClass,
                                     StringComparison.Ordinal))
                                 .Select(marker => _typeResolver.ResolveMarkerType(marker))
                                 .Where(static markerType => markerType is not null)
                                 .Cast<INamedTypeSymbol>()
                                 .ToArray();
            if (hopMarkerTypes.Length > 0)
            {
                targetThemeSet = DedupeMostDerivedThemes(
                    hopMarkerTypes.SelectMany(nodeType => allThemes.Where(theme =>
                    {
                        var themeTarget = resolvedTargets[theme];
                        return themeTarget is not null &&
                               SemanticPartTypeResolver.IsAssignableTo(nodeType, themeTarget);
                    })),
                    resolvedTargets);
                continue;
            }

            var sibling = allParts.FirstOrDefault(candidate =>
                !ReferenceEquals(candidate, part) &&
                candidate.RuntimeCreated &&
                string.Equals(candidate.SelectorClass, hopClass, StringComparison.Ordinal));
            if (sibling?.ContractType is not INamedTypeSymbol siblingContractType)
            {
                return Array.Empty<ThemeAssetSemanticMarkerInfo>();
            }

            targetThemeSet = DedupeMostDerivedThemes(
                allThemes.Where(theme =>
                {
                    var themeTarget = resolvedTargets[theme];
                    return themeTarget is not null &&
                           SemanticPartTypeResolver.IsAssignableTo(siblingContractType, themeTarget);
                }),
                resolvedTargets);
        }

        if (targetMarkers.Count == 0)
        {
            targetMarkers = targetThemeSet
                            .SelectMany(static theme => theme.Templates)
                            .SelectMany(static template => template.Markers)
                            .Where(marker => string.Equals(marker.SelectorClass, selectorClass, StringComparison.Ordinal))
                            .ToList();
        }

        return targetMarkers;
    }

    private HashSet<ThemeAssetSemanticThemeInfo> DedupeMostDerivedThemes(
        IEnumerable<ThemeAssetSemanticThemeInfo> candidateThemes,
        Dictionary<ThemeAssetSemanticThemeInfo, INamedTypeSymbol?> resolvedTargets)
    {
        var candidates = candidateThemes.ToList();
        return new HashSet<ThemeAssetSemanticThemeInfo>(candidates.Where(theme =>
        {
            var themeTarget = resolvedTargets[theme]!;
            return !candidates.Any(other =>
                !ReferenceEquals(other, theme) &&
                resolvedTargets[other] is { } otherTarget &&
                !string.Equals(otherTarget.ToDisplayString(),
                    themeTarget.ToDisplayString(),
                    StringComparison.Ordinal) &&
                SemanticPartTypeResolver.IsAssignableTo(otherTarget, themeTarget));
        }));
    }

    private IReadOnlyList<(
        ThemeAssetInfo Asset,
        ThemeAssetSemanticTemplateInfo Template)> ResolveTemplates(INamedTypeSymbol controlType)
    {
        var themes = _assets.OrderBy(static asset => asset.AssetPath, StringComparer.Ordinal)
                            .SelectMany(static asset => asset.SemanticThemes.Select(theme =>
                                (Asset: asset, Theme: theme)))
                            .ToArray();
        var resolvedTargets = themes.ToDictionary(
            static entry => entry.Theme,
            entry => _typeResolver.ResolveTargetType(entry.Theme.TargetType));
        var resolvedClasses = themes.ToDictionary(
            static entry => entry.Theme,
            entry => _typeResolver.ResolveMetadataName(entry.Theme.XmlClass));
        var templates = new List<(
            ThemeAssetInfo Asset,
            ThemeAssetSemanticTemplateInfo Template)>();
        var visited = new HashSet<ThemeAssetSemanticThemeInfo>();

        foreach (var theme in themes.Where(entry =>
                     SymbolEqualityComparer.Default.Equals(resolvedTargets[entry.Theme], controlType)))
        {
            CollectTemplates(theme);
        }
        return templates;

        void CollectTemplates((ThemeAssetInfo Asset, ThemeAssetSemanticThemeInfo Theme) entry)
        {
            if (!visited.Add(entry.Theme))
            {
                return;
            }

            foreach (var template in entry.Theme.Templates)
            {
                templates.Add((entry.Asset, template));
            }

            if (entry.Theme.Templates.Count > 0 ||
                entry.Theme.OverridesBaseTemplate ||
                entry.Theme.BasedOn is null ||
                _typeResolver.ResolveTargetType(entry.Theme.BasedOn) is not { } baseType)
            {
                return;
            }

            foreach (var baseTheme in themes.Where(candidate =>
                         SymbolEqualityComparer.Default.Equals(resolvedTargets[candidate.Theme], baseType) ||
                         (resolvedClasses[candidate.Theme] is { } candidateClass &&
                          SymbolEqualityComparer.Default.Equals(candidateClass, baseType))))
            {
                CollectTemplates(baseTheme);
            }
        }
    }

    private static bool MatchesCardinality(int cardinality, int count)
    {
        return cardinality switch
        {
            1 => count <= 1,
            2 => count >= 1,
            _ => count == 1
        };
    }

    private static string DescribeCardinality(int cardinality)
    {
        return cardinality switch
        {
            1 => "zero or one",
            2 => "one or more",
            _ => "exactly one"
        };
    }
}
