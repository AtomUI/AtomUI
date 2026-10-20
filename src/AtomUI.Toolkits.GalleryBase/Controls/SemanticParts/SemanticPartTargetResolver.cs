using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed class SemanticPartTargetResolution
{
    public SemanticPartTargetResolution(IReadOnlyList<Visual> targets, int totalMatchCount)
    {
        Targets         = targets;
        TotalMatchCount = totalMatchCount;
    }

    public IReadOnlyList<Visual> Targets { get; }

    public int TotalMatchCount { get; }

    public bool IsTruncated => Targets.Count < TotalMatchCount;
}

internal static class SemanticPartTargetResolver
{
    public const int DefaultTargetBudget = 32;

    public static SemanticPartTargetResolution Resolve(
        Control owner,
        SemanticPartDescriptor part,
        SemanticPartRegistry registry,
        IEnumerable<Visual>? additionalRoots = null,
        int targetBudget = DefaultTargetBudget)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetBudget);

        var targets = new List<Visual>(Math.Min(targetBudget, 4));
        var matches = new HashSet<Visual>();
        var totalMatchCount = 0;

        void AddMatch(Visual candidate)
        {
            if (!matches.Add(candidate) || !IsEligible(candidate, part))
            {
                return;
            }

            totalMatchCount++;
            if (targets.Count < targetBudget)
            {
                targets.Add(candidate);
            }
        }

        if (string.Equals(part.Path, "root", StringComparison.Ordinal))
        {
            AddMatch(owner);

            // 弹层承载型控件（如 Tour）的 owner 布局尺寸为零，root 没有可定位的宿主表面。
            // 对齐上游语义（Tour 的 root 即可见的弹层容器）：owner 不可定位时回退到
            // 其 "popup.root" 部件的标记节点（模板弹层根）。回退目标是 popup.root 契约
            // 的实例，不能复用 AddMatch——那会按 root 部件的契约（owner 类型）把它拒绝；
            // 资格校验由回退逻辑按 popup.root 部件执行，此处直接入列。
            if (targets.Count == 0 &&
                TryResolvePopupRootFallback(owner, registry, additionalRoots) is { } popupRoot)
            {
                totalMatchCount++;
                targets.Add(popupRoot);
            }

            return new SemanticPartTargetResolution(targets, totalMatchCount);
        }

        // 模板节点被嵌套控件的内容属性收养时（decorated box 的左右 addon 等），
        // Avalonia 会清空其 TemplatedParent，因此不能按 TemplatedParent 过滤；
        // 同时 scope-aware overlay 会重挂子树（如 ListView 的滚动区域），逐层
        // GetVisualChildren 无法抵达，因此统一用 GetVisualDescendants 全子树枚举，
        // 并以"候选到 owner 的祖先链不跨其他已注册语义 owner"作为隔离边界。
        if (part.RuntimeCreated && HasMarker(owner, part) && MatchesRouteAnchors(owner, owner, part))
        {
            AddMatch(owner);
        }

        foreach (var candidate in owner.GetVisualDescendants())
        {
            if (HasMarker(candidate, part) &&
                MatchesRouteAnchors(candidate, owner, part) &&
                !CrossesNestedOwnerBoundary(candidate, owner, part.CrossNestedOwners, registry))
            {
                AddMatch(candidate);
            }
        }

        if (part.CrossVisualRoot && additionalRoots is not null)
        {
            foreach (var root in additionalRoots)
            {
                if (root is null)
                {
                    continue;
                }

                if (HasMarker(root, part) &&
                    MatchesRouteAnchors(root, owner, part) &&
                    !CrossesNestedOwnerBoundary(root, owner, part.CrossNestedOwners, registry))
                {
                    AddMatch(root);
                }

                foreach (var candidate in root.GetVisualDescendants())
                {
                    if (HasMarker(candidate, part) &&
                        MatchesRouteAnchors(candidate, owner, part) &&
                        !CrossesNestedOwnerBoundary(candidate, owner, part.CrossNestedOwners, registry))
                    {
                        AddMatch(candidate);
                    }
                }
            }
        }

        return new SemanticPartTargetResolution(targets, totalMatchCount);
    }

    /// <summary>
    /// 弹层承载型控件（owner 布局尺寸为零，如 Tour）的 root 回退解析：定位 owner 模板
    /// 弹层中的 "popup.root" 标记节点（该节点的可视范围即上游语义中的控件根容器）。
    /// </summary>
    private static Visual? TryResolvePopupRootFallback(
        Control owner,
        SemanticPartRegistry registry,
        IEnumerable<Visual>? additionalRoots)
    {
        if (!registry.TryGetControl(owner.GetType(), out var descriptor))
        {
            return null;
        }

        SemanticPartDescriptor? popupRootPart = null;
        foreach (var candidate in descriptor.Parts)
        {
            if (string.Equals(candidate.Path, "popup.root", StringComparison.Ordinal))
            {
                popupRootPart = candidate;
                break;
            }
        }

        if (popupRootPart?.SelectorClass is not { } selectorClass)
        {
            return null;
        }

        Visual? MatchInSubtree(Visual root)
        {
            if (IsLocatablePopupRootMarker(root))
            {
                return root;
            }

            return root.GetVisualDescendants().FirstOrDefault(IsLocatablePopupRootMarker);
        }

        // 回退节点是 "popup.root" 契约的实例，资格校验（含契约类型）按 popup.root
        // 部件执行，而不是发起回退的 root 部件（其契约为 owner 类型）。
        bool IsLocatablePopupRootMarker(Visual candidate)
        {
            return HasMarker(candidate, popupRootPart) && IsEligible(candidate, popupRootPart);
        }

        foreach (var popup in owner.GetVisualDescendants().OfType<Popup>())
        {
            if (popup.Child is { } child &&
                MatchInSubtree(child) is { } fromPopup)
            {
                return fromPopup;
            }
        }

        foreach (var root in additionalRoots ?? Array.Empty<Visual>())
        {
            if (root is null)
            {
                continue;
            }

            if (ReferenceEquals(root, owner))
            {
                continue;
            }

            if (MatchInSubtree(root) is { } fromRoot)
            {
                return fromRoot;
            }
        }

        return null;
    }

    private static bool CrossesNestedOwnerBoundary(
        Visual candidate,
        Control owner,
        bool crossNestedOwners,
        SemanticPartRegistry registry)
    {
        // 声明 CrossNestedOwners 的部件（如 Transfer 的条目限定部件）允许穿过
        // 嵌套语义 owner 的视觉子树（scope-aware overlay 重挂后逐层下钻也不可达，
        // 必须全子树枚举）。
        if (crossNestedOwners)
        {
            return false;
        }

        foreach (var ancestor in candidate.GetSelfAndVisualAncestors())
        {
            if (ReferenceEquals(ancestor, owner))
            {
                return false;
            }

            // 候选自身即已注册 owner 时不算边界（正是匹配目标）。
            if (ReferenceEquals(ancestor, candidate))
            {
                continue;
            }

            if (ancestor is Control control &&
                !InOwnerTemplate(control, owner) &&
                registry.TryGetControl(control.GetType(), out _))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断节点是否处于 owner 的模板组合内（TemplatedParent 链可回溯到 owner）。
    /// owner 自身模板内的已注册控件（如 ListView 模板中的 Spin）属于 owner 的
    /// 组合结构，不构成语义隔离边界；独立的内容型嵌套组件才构成边界。
    /// </summary>
    private static bool InOwnerTemplate(Visual control, Control owner)
    {
        for (var current = control as StyledElement;
             current is not null;
             current = current.TemplatedParent as StyledElement)
        {
            if (ReferenceEquals(current, owner))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// 限定部件（如 source.header）与未限定部件共享终端 marker 时，
    /// 依据 SelectorRoute 中的锚点类区分实例：非终端 .class 锚点要求出现在
    /// 候选节点到 owner 的祖先链上，首段自锚点要求 owner 自身携带。
    /// </summary>
    private static bool MatchesRouteAnchors(Visual candidate, Control owner, SemanticPartDescriptor part)
    {
        if (part.SelectorRoute is not { } route)
        {
            return true;
        }

        var tokens = route.Split(' ');
        var index = 0;
        var hasSelfAnchor = false;
        if (tokens.Length > 0 && tokens[0].StartsWith(".", StringComparison.Ordinal))
        {
            if (!owner.Classes.Contains(tokens[0].Substring(1)))
            {
                return false;
            }

            hasSelfAnchor = true;
            index = 1;
        }

        List<string>? anchors = null;
        for (var i = index; i < tokens.Length - 2; i += 2)
        {
            if (tokens[i] is "/template/" or ">" or ">>" &&
                tokens[i + 1].StartsWith(".", StringComparison.Ordinal))
            {
                anchors ??= new List<string>();
                anchors.Add(tokens[i + 1].Substring(1));
            }
        }

        if (hasSelfAnchor && !ReferenceEquals(candidate, owner))
        {
            // 自锚点场景下 owner 类只约束 owner 自身；候选为 owner 模板内节点时同样成立。
            return candidate.Classes.Contains(tokens[0].Substring(1)) ||
                   HasAncestorAnchor(candidate, owner, tokens[0].Substring(1));
        }

        if (anchors is null)
        {
            return true;
        }

        foreach (var anchor in anchors)
        {
            if (!HasAncestorAnchor(candidate, owner, anchor))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasAncestorAnchor(Visual candidate, Visual owner, string anchor)
    {
        foreach (var ancestor in candidate.GetSelfAndVisualAncestors())
        {
            if (ancestor.Classes.Contains(anchor))
            {
                return true;
            }

            if (ReferenceEquals(ancestor, owner))
            {
                return false;
            }
        }

        return false;
    }

    private static bool HasMarker(Visual candidate, SemanticPartDescriptor part)
    {
        return part.SelectorClass is { } selectorClass &&
               part.ContractType.IsInstanceOfType(candidate) &&
               candidate.Classes.Contains(selectorClass);
    }

    private static bool IsEligible(Visual candidate, SemanticPartDescriptor part)
    {
        if (!part.ContractType.IsInstanceOfType(candidate) ||
            !candidate.IsAttachedToVisualTree() ||
            !candidate.IsEffectivelyVisible ||
            candidate.Bounds.Width <= 0 ||
            candidate.Bounds.Height <= 0)
        {
            return false;
        }

        // RestHidden 部件的静止态透明是设计语义（如 ImagePreviewer cover 悬停遮罩），
        // 语义预览必须在不可见状态下仍能定位描边。
        if (part.RestHidden)
        {
            return true;
        }

        return candidate.GetSelfAndVisualAncestors().All(static ancestor => ancestor.Opacity > 0);
    }
}
