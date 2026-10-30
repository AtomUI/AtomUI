using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

internal static class ThemeConfigMerger
{
    internal static ThemeConfigMergeResult Merge(
        NormalizedThemeConfig defaults,
        NormalizedThemeConfig? parent,
        NormalizedThemeConfig current)
    {
        ArgumentNullException.ThrowIfNull(defaults);
        ArgumentNullException.ThrowIfNull(current);

        var effectiveParent = parent is null
            ? defaults
            : parent;
        var mergeBase = current.Inherit ? effectiveParent : defaults;
        var candidate = Apply(mergeBase, current);
        if (candidate.Equals(effectiveParent))
        {
            return new ThemeConfigMergeResult(effectiveParent, ThemeConfigChangeSet.Empty);
        }

        return new ThemeConfigMergeResult(candidate, Compare(effectiveParent, candidate));
    }

    private static NormalizedThemeConfig Apply(
        NormalizedThemeConfig effectiveBase,
        NormalizedThemeConfig current)
    {
        if (!current.AlgorithmsSpecified &&
            current.GlobalTokens.Count == 0 &&
            current.ControlIdentities.Count == 0)
        {
            return effectiveBase;
        }

        var algorithms = current.AlgorithmsSpecified
            ? ThemeConfigArray.Copy(current.Algorithms)
            : ThemeConfigArray.Copy(effectiveBase.Algorithms);
        var globalTokens = MergeTokens(effectiveBase.GlobalTokens, current.GlobalTokens);
        if (effectiveBase.HasDeferredControls || current.HasDeferredControls)
        {
            var identities = effectiveBase.ControlIdentities.Concat(current.ControlIdentities).Distinct().ToArray();
            return NormalizedThemeConfig.CreateDeferred(false, true, algorithms, globalTokens, identities, identity =>
            {
                effectiveBase.TryGetControl(identity, out var inherited);
                return current.TryGetControl(identity, out var local)
                    ? MergeControl(inherited, local)
                    : inherited is null ? null : ResolveInheritedControl(inherited);
            }, "merge:" + effectiveBase.Fingerprint + ":" + current.Fingerprint,
                (effectiveBase, current));
        }
        var controls = MergeControls(effectiveBase.Controls, current.Controls);
        return NormalizedThemeConfig.CreateCanonical(false, true, algorithms, globalTokens, controls);
    }

    private static NormalizedTokenValue[] MergeTokens(
        IReadOnlyList<NormalizedTokenValue> inherited,
        IReadOnlyList<NormalizedTokenValue> current)
    {
        if (current.Count == 0)
        {
            return ThemeConfigArray.Copy(inherited);
        }

        var result = new NormalizedTokenValue[inherited.Count + current.Count];
        var inheritedIndex = 0;
        var currentIndex = 0;
        var resultIndex = 0;
        while (inheritedIndex < inherited.Count || currentIndex < current.Count)
        {
            if (inheritedIndex == inherited.Count)
            {
                result[resultIndex++] = current[currentIndex++];
                continue;
            }
            if (currentIndex == current.Count)
            {
                result[resultIndex++] = inherited[inheritedIndex++];
                continue;
            }

            var inheritedToken = inherited[inheritedIndex];
            var currentToken = current[currentIndex];
            var comparison = inheritedToken.Descriptor.Slot.CompareTo(currentToken.Descriptor.Slot);
            if (comparison < 0)
            {
                result[resultIndex++] = inheritedToken;
                inheritedIndex++;
            }
            else
            {
                result[resultIndex++] = currentToken;
                currentIndex++;
                if (comparison == 0)
                {
                    inheritedIndex++;
                }
            }
        }

        if (resultIndex != result.Length)
        {
            Array.Resize(ref result, resultIndex);
        }
        return result;
    }

    private static NormalizedControlThemeConfig[] MergeControls(
        IReadOnlyList<NormalizedControlThemeConfig> inherited,
        IReadOnlyList<NormalizedControlThemeConfig> current)
    {
        var result = new NormalizedControlThemeConfig[inherited.Count + current.Count];
        var inheritedIndex = 0;
        var currentIndex = 0;
        var resultIndex = 0;
        while (inheritedIndex < inherited.Count || currentIndex < current.Count)
        {
            if (inheritedIndex == inherited.Count)
            {
                result[resultIndex++] = MergeControl(null, current[currentIndex++]);
                continue;
            }
            if (currentIndex == current.Count)
            {
                result[resultIndex++] = ResolveInheritedControl(inherited[inheritedIndex++]);
                continue;
            }

            var parent = inherited[inheritedIndex];
            var local = current[currentIndex];
            var comparison = CompareIdentity(parent.Identity, local.Identity);
            if (comparison < 0)
            {
                result[resultIndex++] = ResolveInheritedControl(parent);
                inheritedIndex++;
            }
            else if (comparison > 0)
            {
                result[resultIndex++] = MergeControl(null, local);
                currentIndex++;
            }
            else
            {
                result[resultIndex++] = MergeControl(parent, local);
                inheritedIndex++;
                currentIndex++;
            }
        }

        if (resultIndex != result.Length)
        {
            Array.Resize(ref result, resultIndex);
        }
        return result;
    }

    private static NormalizedControlThemeConfig MergeControl(
        NormalizedControlThemeConfig? parent,
        NormalizedControlThemeConfig local)
    {
        var mode = local.AlgorithmMode == ControlAlgorithmMode.Unspecified
            ? parent?.AlgorithmMode ?? ControlAlgorithmMode.Disabled
            : local.AlgorithmMode;
        ThemeAlgorithmDescriptor[] algorithms;
        if (local.AlgorithmMode == ControlAlgorithmMode.Unspecified)
        {
            algorithms = mode == ControlAlgorithmMode.Custom && parent is not null
                ? ThemeConfigArray.Copy(parent.Algorithms)
                : Array.Empty<ThemeAlgorithmDescriptor>();
        }
        else
        {
            algorithms = mode == ControlAlgorithmMode.Custom
                ? ThemeConfigArray.Copy(local.Algorithms)
                : Array.Empty<ThemeAlgorithmDescriptor>();
        }

        return NormalizedControlThemeConfig.CreateCanonical(
            parent is null ? local.Identity : ControlTokenIdentityCanonicalizer.Merge(parent.Identity, local.Identity),
            mode,
            algorithms,
            MergeTokens(parent?.GlobalTokens ?? Array.Empty<NormalizedTokenValue>(), local.GlobalTokens),
            MergeTokens(parent?.OwnTokens ?? Array.Empty<NormalizedTokenValue>(), local.OwnTokens));
    }

    private static NormalizedControlThemeConfig ResolveInheritedControl(NormalizedControlThemeConfig control)
    {
        if (control.AlgorithmMode != ControlAlgorithmMode.Unspecified)
        {
            return control;
        }

        return new NormalizedControlThemeConfig(
            control.Identity,
            ControlAlgorithmMode.Disabled,
            Array.Empty<ThemeAlgorithmDescriptor>(),
            control.GlobalTokens,
            control.OwnTokens);
    }

    internal static ThemeConfigChangeSet Compare(
        NormalizedThemeConfig previous,
        NormalizedThemeConfig current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);
        if (ReferenceEquals(previous, current))
        {
            return ThemeConfigChangeSet.Empty;
        }

        var algorithmsChanged = !AlgorithmSequenceEqual(previous.Algorithms, current.Algorithms);
        var globalTokensChanged = !TokenSequenceEqual(previous.GlobalTokens, current.GlobalTokens);
        var changedControls = previous.HasDeferredControls || current.HasDeferredControls
            ? previous.ControlRevision == current.ControlRevision && Equals(previous.ControlBindingIdentity, current.ControlBindingIdentity)
                ? Array.Empty<ControlTokenIdentity>()
                : previous.ControlIdentities.Concat(current.ControlIdentities).Distinct().ToArray()
            : FindChangedControls(previous.Controls, current.Controls);
        return new ThemeConfigChangeSet(algorithmsChanged, globalTokensChanged, changedControls);
    }

    private static IReadOnlyList<ControlTokenIdentity> FindChangedControls(
        IReadOnlyList<NormalizedControlThemeConfig> previous,
        IReadOnlyList<NormalizedControlThemeConfig> current)
    {
        var result = new List<ControlTokenIdentity>();
        var previousIndex = 0;
        var currentIndex = 0;
        while (previousIndex < previous.Count || currentIndex < current.Count)
        {
            if (previousIndex == previous.Count)
            {
                result.Add(current[currentIndex++].Identity);
                continue;
            }
            if (currentIndex == current.Count)
            {
                result.Add(previous[previousIndex++].Identity);
                continue;
            }

            var previousControl = previous[previousIndex];
            var currentControl = current[currentIndex];
            var comparison = CompareIdentity(previousControl.Identity, currentControl.Identity);
            if (comparison < 0)
            {
                result.Add(previousControl.Identity);
                previousIndex++;
            }
            else if (comparison > 0)
            {
                result.Add(currentControl.Identity);
                currentIndex++;
            }
            else
            {
                if (!previousControl.Equals(currentControl))
                {
                    result.Add(currentControl.Identity);
                }
                previousIndex++;
                currentIndex++;
            }
        }

        return result;
    }

    private static bool AlgorithmSequenceEqual(
        IReadOnlyList<ThemeAlgorithmDescriptor> left,
        IReadOnlyList<ThemeAlgorithmDescriptor> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }
        for (var index = 0; index < left.Count; index++)
        {
            if (!ReferenceEquals(left[index], right[index]))
            {
                return false;
            }
        }
        return true;
    }

    private static bool TokenSequenceEqual(
        IReadOnlyList<NormalizedTokenValue> left,
        IReadOnlyList<NormalizedTokenValue> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }
        for (var index = 0; index < left.Count; index++)
        {
            if (!left[index].Equals(right[index]))
            {
                return false;
            }
        }
        return true;
    }

    private static int CompareIdentity(ControlTokenIdentity left, ControlTokenIdentity right)
    {
        var catalog = string.Compare(left.Catalog, right.Catalog, StringComparison.Ordinal);
        return catalog != 0 ? catalog : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }
}

internal sealed class ThemeConfigMergeResult
{
    internal ThemeConfigMergeResult(NormalizedThemeConfig effectiveConfig, ThemeConfigChangeSet changeSet)
    {
        EffectiveConfig = effectiveConfig;
        ChangeSet       = changeSet;
    }

    public NormalizedThemeConfig EffectiveConfig { get; }
    public ThemeConfigChangeSet ChangeSet { get; }
}

internal sealed class ThemeConfigChangeSet
{
    internal static ThemeConfigChangeSet Empty { get; } = new(
        false,
        false,
        Array.Empty<ControlTokenIdentity>());

    internal ThemeConfigChangeSet(
        bool algorithmsChanged,
        bool globalTokensChanged,
        IEnumerable<ControlTokenIdentity> changedControls)
    {
        ArgumentNullException.ThrowIfNull(changedControls);

        var changedControlArray = ThemeConfigArray.Copy(changedControls);
        Array.Sort(changedControlArray, static (left, right) =>
        {
            var catalog = string.Compare(left.Catalog, right.Catalog, StringComparison.Ordinal);
            return catalog != 0
                ? catalog
                : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
        });
        AlgorithmsChanged  = algorithmsChanged;
        GlobalTokensChanged = globalTokensChanged;
        ChangedControls     = Array.AsReadOnly(changedControlArray);
    }

    public bool AlgorithmsChanged { get; }
    public bool GlobalTokensChanged { get; }
    public IReadOnlyList<ControlTokenIdentity> ChangedControls { get; }
    public bool IsEmpty => !AlgorithmsChanged && !GlobalTokensChanged && ChangedControls.Count == 0;
}
