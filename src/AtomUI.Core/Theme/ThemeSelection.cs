using AtomUI.Theme.Algorithms;
using Avalonia.Media;

namespace AtomUI.Theme;

/// <summary>
/// A user's theme selection: the design language, appearance, density and an optional brand color.
/// </summary>
/// <param name="DesignLanguage">The design language identifier, as listed by <see cref="ThemeSelectionResolver.DesignLanguages"/>.</param>
/// <param name="Appearance">The requested appearance.</param>
/// <param name="Density">The requested density.</param>
/// <param name="BrandColor">The chosen brand color, or <see langword="null"/> to keep the definition's own.</param>
public sealed record ThemeSelection(
    string DesignLanguage,
    ThemeAppearance Appearance,
    ThemeDensity Density,
    Color? BrandColor);

/// <summary>
/// The outcome of resolving a <see cref="ThemeSelection"/> against a theme catalog.
/// </summary>
/// <param name="ThemeId">The id of the selected theme definition.</param>
/// <param name="Algorithms">
/// The algorithm chain to request, or <see langword="null"/> to keep the definition's own chain.
/// </param>
/// <param name="BrandSeed">
/// The seed to write into <c>ColorPrimary</c>, or <see langword="null"/> when the language is fixed or no color was chosen.
/// </param>
/// <param name="BrandColorIgnored">A brand color was chosen but the design language does not accept one.</param>
public sealed record ThemeSelectionResult(
    string ThemeId,
    IReadOnlyList<ThemeAlgorithm>? Algorithms,
    Color? BrandSeed,
    bool BrandColorIgnored);

/// <summary>
/// Turns a <see cref="ThemeSelection"/> into the theme id, algorithm chain and brand seed to apply.
/// The resolver is a pure function and reads or writes no state.
/// </summary>
public static class ThemeSelectionResolver
{
    /// <summary>
    /// Resolves a selection against the catalog.
    /// </summary>
    /// <remarks>
    /// An exact (appearance, density) variant of the language is used as is. Otherwise the language's light, normal
    /// definition is the base and its chain gets the built-in compact and dark algorithms appended.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// The language is unknown, or has neither an exact variant nor a light, normal base definition.
    /// </exception>
    public static ThemeSelectionResult Resolve(IReadOnlyList<ThemeInfo> catalog, ThemeSelection selection)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(selection);

        ThemeInfo? first = null;
        ThemeInfo? exact = null;
        ThemeInfo? baseInfo = null;
        foreach (var info in catalog)
        {
            if (!string.Equals(info.DesignLanguage, selection.DesignLanguage, StringComparison.Ordinal))
            {
                continue;
            }

            first ??= info;
            if (info.Appearance == selection.Appearance && info.Density == selection.Density)
            {
                exact ??= info;
            }

            if (info.Appearance == ThemeAppearance.Light && info.Density == ThemeDensity.Normal)
            {
                baseInfo ??= info;
            }
        }

        if (first is null)
        {
            throw new ArgumentException(
                $"Design language '{selection.DesignLanguage}' is not present in the theme catalog.",
                nameof(selection));
        }

        string themeId;
        IReadOnlyList<ThemeAlgorithm>? algorithms;
        if (exact is not null)
        {
            themeId    = exact.Id;
            algorithms = null;
        }
        else if (baseInfo is not null)
        {
            themeId = baseInfo.Id;
            var chain = new List<ThemeAlgorithm>(baseInfo.Algorithms.Count + 2);
            foreach (var algorithm in baseInfo.Algorithms)
            {
                if (algorithm != ThemeAlgorithm.Dark && algorithm != ThemeAlgorithm.Compact)
                {
                    chain.Add(algorithm);
                }
            }

            if (selection.Density == ThemeDensity.Compact)
            {
                chain.Add(ThemeAlgorithm.Compact);
            }

            if (selection.Appearance == ThemeAppearance.Dark)
            {
                chain.Add(ThemeAlgorithm.Dark);
            }

            algorithms = chain;
        }
        else
        {
            throw new ArgumentException(
                $"Design language '{selection.DesignLanguage}' has no variant for appearance '{selection.Appearance}' " +
                $"and density '{selection.Density}' and no light, normal base definition to fall back to.",
                nameof(selection));
        }

        Color? seed = null;
        var ignored = false;
        if (selection.BrandColor is not null)
        {
            if (first.BrandColor == ThemeBrandColorSupport.Seed)
            {
                seed = selection.BrandColor;
            }
            else
            {
                ignored = true;
            }
        }

        return new ThemeSelectionResult(themeId, algorithms, seed, ignored);
    }

    /// <summary>
    /// Lists the design languages of the catalog in catalog order, without duplicates. Themes that declare none are skipped.
    /// </summary>
    public static IReadOnlyList<string> DesignLanguages(IReadOnlyList<ThemeInfo> catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var result = new List<string>();
        foreach (var info in catalog)
        {
            if (info.DesignLanguage is { } language && !result.Contains(language))
            {
                result.Add(language);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets whether the design language accepts a replaceable brand color.
    /// </summary>
    /// <exception cref="ArgumentException">The language is not present in the catalog.</exception>
    public static ThemeBrandColorSupport BrandColorSupport(IReadOnlyList<ThemeInfo> catalog, string designLanguage)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(designLanguage);

        foreach (var info in catalog)
        {
            if (string.Equals(info.DesignLanguage, designLanguage, StringComparison.Ordinal))
            {
                return info.BrandColor;
            }
        }

        throw new ArgumentException(
            $"Design language '{designLanguage}' is not present in the theme catalog.", nameof(designLanguage));
    }
}
