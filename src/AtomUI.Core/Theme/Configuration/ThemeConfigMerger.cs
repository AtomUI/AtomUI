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
        // Inherit = false: keep the parent's language and idiom, drop everything else the parent and the selected
        // definition give; the axes the defaults themselves write (a root definition's fixed axes) remain.
        var mergeBase = current.Inherit ? effectiveParent : KeepLanguage(defaults, effectiveParent);
        var candidate = Apply(mergeBase, current);
        if (candidate.Equals(effectiveParent))
        {
            return new ThemeConfigMergeResult(effectiveParent, ThemeConfigChangeSet.Empty);
        }

        return new ThemeConfigMergeResult(candidate, Compare(effectiveParent, candidate));
    }

    private static NormalizedThemeConfig KeepLanguage(NormalizedThemeConfig defaults, NormalizedThemeConfig parent)
    {
        var axes = new ThemeAxes(
            defaults.Axes.Language ?? parent.Axes.Language,
            defaults.Axes.Style,
            defaults.Axes.Appearance,
            defaults.Axes.Density,
            defaults.Axes.Idiom ?? parent.Axes.Idiom);
        if (axes.Equals(defaults.Axes))
        {
            return defaults;
        }

        return NormalizedThemeConfig.CreateCanonical(defaults.Inherit, axes, ThemeConfigArray.Copy(defaults.GlobalTokens),
            ThemeConfigArray.Copy(defaults.LanguageTokens), ThemeConfigArray.Copy(defaults.Controls), defaults.Definition);
    }

    private static NormalizedThemeConfig Apply(
        NormalizedThemeConfig effectiveBase,
        NormalizedThemeConfig current)
    {
        if (current.Axes.IsEmpty &&
            current.GlobalTokens.Count == 0 &&
            current.LanguageTokens.Count == 0 &&
            current.ControlIdentities.Count == 0)
        {
            return effectiveBase;
        }

        var axes = current.Axes.Over(effectiveBase.Axes);
        var globalTokens = MergeTokens(effectiveBase.GlobalTokens, current.GlobalTokens);
        // Language tokens belong to the language they were written for: a layer that switches the language drops the
        // lower layers' ones.
        var languageChanged = current.Axes.Language is not null && effectiveBase.Axes.Language is not null &&
                              !ReferenceEquals(current.Axes.Language, effectiveBase.Axes.Language);
        var languageTokens = MergeLanguageTokens(
            languageChanged ? Array.Empty<LanguageTokenValue>() : effectiveBase.LanguageTokens, current.LanguageTokens);
        var controls = MergeControls(effectiveBase.Controls, current.Controls);
        // The definition layer stays the base's: a layer cannot select another definition (the request's theme does).
        return NormalizedThemeConfig.CreateCanonical(false, axes, globalTokens, languageTokens, controls,
            effectiveBase.Definition);
    }

    private static LanguageTokenValue[] MergeLanguageTokens(
        IReadOnlyList<LanguageTokenValue> inherited,
        IReadOnlyList<LanguageTokenValue> current)
    {
        if (current.Count == 0)
        {
            return ThemeConfigArray.Copy(inherited);
        }

        var result = new Dictionary<string, LanguageTokenValue>(StringComparer.Ordinal);
        foreach (var token in inherited)
        {
            result[token.Name] = token;
        }
        foreach (var token in current)
        {
            result[token.Name] = token;
        }
        var array = result.Values.ToArray();
        Array.Sort(array, static (left, right) => string.CompareOrdinal(left.Name, right.Name));
        return array;
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
                result[resultIndex++] = inherited[inheritedIndex++];
                continue;
            }

            var parent = inherited[inheritedIndex];
            var local = current[currentIndex];
            var comparison = CompareIdentity(parent.Identity, local.Identity);
            if (comparison < 0)
            {
                result[resultIndex++] = parent;
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
        return NormalizedControlThemeConfig.CreateCanonical(
            parent is null ? local.Identity : ControlTokenIdentityCanonicalizer.Merge(parent.Identity, local.Identity),
            local.Appearance ?? parent?.Appearance,
            local.Density ?? parent?.Density,
            MergeTokens(parent?.GlobalTokens ?? Array.Empty<NormalizedTokenValue>(), local.GlobalTokens),
            MergeTokens(parent?.OwnTokens ?? Array.Empty<NormalizedTokenValue>(), local.OwnTokens));
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

        var axesChanged = !previous.Axes.Equals(current.Axes) ||
                          !previous.LanguageTokens.SequenceEqual(current.LanguageTokens);
        var globalTokensChanged = !TokenSequenceEqual(previous.GlobalTokens, current.GlobalTokens);
        var changedControls = FindChangedControls(previous.Controls, current.Controls);
        if (!ReferenceEquals(previous.Definition, current.Definition))
        {
            // Another definition layer may configure any control: every control of either definition changed.
            changedControls = changedControls
                .Concat(previous.Definition?.ControlIdentities ?? Array.Empty<ControlTokenIdentity>())
                .Concat(current.Definition?.ControlIdentities ?? Array.Empty<ControlTokenIdentity>())
                .Distinct().ToArray();
            axesChanged = true;
        }
        return new ThemeConfigChangeSet(axesChanged, globalTokensChanged, changedControls);
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
        bool languageInputsChanged,
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
        LanguageInputsChanged = languageInputsChanged;
        GlobalTokensChanged = globalTokensChanged;
        ChangedControls     = Array.AsReadOnly(changedControlArray);
    }

    /// <summary>
    /// Whether the language, an axis or a language token changed: the global tokens must be derived again.
    /// </summary>
    public bool LanguageInputsChanged { get; }
    public bool GlobalTokensChanged { get; }
    public IReadOnlyList<ControlTokenIdentity> ChangedControls { get; }
    public bool IsEmpty => !LanguageInputsChanged && !GlobalTokensChanged && ChangedControls.Count == 0;
}
