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

            // The owner of a popup-hosted control (such as Tour) has a zero layout size, so its root has no host
            // surface that can be located. To match the upstream semantics (the root of Tour is the visible popup
            // container), the root falls back to the marker node of the "popup.root" part (the popup root of the
            // template) when the owner is not locatable. That node is an instance of the popup.root contract, so
            // AddMatch cannot be reused: it would reject the node under the contract of the root part (the owner
            // type). The fallback logic has already checked eligibility against the popup.root part, so the node
            // is added directly.
            if (targets.Count == 0 &&
                TryResolvePopupRootFallback(owner, registry, additionalRoots) is { } popupRoot)
            {
                totalMatchCount++;
                targets.Add(popupRoot);
            }

            return new SemanticPartTargetResolution(targets, totalMatchCount);
        }

        // When a template node is adopted by the content property of a nested control (such as the left and
        // right addons of a decorated box), Avalonia clears its TemplatedParent, so candidates cannot be
        // filtered by TemplatedParent. A scope-aware overlay also re-parents subtrees (such as the scrolling
        // area of ListView), which a level-by-level walk over GetVisualChildren cannot reach, so the whole
        // subtree is enumerated with GetVisualDescendants instead. Isolation is then guaranteed by requiring
        // that the ancestor chain from a candidate up to the owner does not cross another registered semantic
        // owner.
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
    /// Resolves the root fallback of a popup-hosted control, whose owner has a zero layout size (such as
    /// Tour): locates the marker node of the "popup.root" part in the popups of the owner's template and
    /// then in the additional roots. The visual bounds of that node are the root container of the control
    /// in the upstream semantics.
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

        // The fallback node is an instance of the "popup.root" contract, so the eligibility check (including
        // the contract type) runs against the popup.root part, not against the root part that started the
        // fallback (its contract is the owner type).
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
        // A part that declares CrossNestedOwners (such as the item-scoped parts of Transfer) is allowed to
        // cross the visual subtree of a nested semantic owner (after a scope-aware overlay has re-parented the
        // subtree, a level-by-level descent cannot reach it either, so the whole subtree must be enumerated).
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

            // A candidate that is itself a registered owner is not a boundary: it is the match target.
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
    /// Determines whether a node lies inside the template composition of the owner (its TemplatedParent
    /// chain leads back to the owner). A registered control inside the owner's own template (such as Spin
    /// in the ListView template) belongs to the composition of the owner and forms no semantic isolation
    /// boundary; only a standalone nested component that comes from content forms one.
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
    /// Tells instances apart by the anchor classes of SelectorRoute when a qualified part (such as
    /// source.header) shares its terminal marker with an unqualified part: a non-terminal .class anchor
    /// must occur on the ancestor chain from the candidate node up to the owner, and a leading self anchor
    /// must be carried by the owner itself.
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
            // With a self anchor, the owner class constrains the owner itself only; the same holds when the
            // candidate is a node inside the template of the owner.
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

        // A RestHidden part is transparent at rest by design (such as the hover mask over the cover of
        // ImagePreviewer), so the semantic preview must still be able to locate the part for its outline while
        // the part is invisible.
        if (part.RestHidden)
        {
            return true;
        }

        return candidate.GetSelfAndVisualAncestors().All(static ancestor => ancestor.Opacity > 0);
    }
}
