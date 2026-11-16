using AtomUI.Theme.Algorithms;
using Avalonia.Media;

namespace AtomUI.Theme;

/// <summary>
/// A user's theme selection: the design language, appearance, density and optional brand and neutral colors.
/// </summary>
/// <param name="DesignLanguage">The design language identifier, as listed by <see cref="ThemeSelectionResolver.DesignLanguages"/>.</param>
/// <param name="Appearance">The requested appearance.</param>
/// <param name="Density">The requested density.</param>
/// <param name="BrandColor">The chosen brand color, or <see langword="null"/> to keep the definition's own.</param>
/// <param name="NeutralColor">The chosen neutral color, or <see langword="null"/> to keep the definition's own.</param>
public sealed record ThemeSelection(
    string DesignLanguage,
    ThemeAppearance Appearance,
    ThemeDensity Density,
    Color? BrandColor,
    Color? NeutralColor = null);

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
/// <param name="NeutralSeed">
/// The seed to write into <c>ColorNeutral</c>, or <see langword="null"/> when the language is fixed or no color was chosen.
/// </param>
/// <param name="NeutralColorIgnored">A neutral color was chosen but the design language does not accept one.</param>
public sealed record ThemeSelectionResult(
    string ThemeId,
    IReadOnlyList<ThemeAlgorithm>? Algorithms,
    Color? BrandSeed,
    bool BrandColorIgnored,
    Color? NeutralSeed = null,
    bool NeutralColorIgnored = false);

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
    /// <para>
    /// An exact (appearance, density) variant of the language is used as is, with its own algorithm chain.
    /// </para>
    /// <para>
    /// Otherwise a fallback definition of the language is chosen and its chain, without the built-in dark and compact
    /// algorithms, gets the built-in compact and dark algorithms the selection asks for appended. The fallback is the
    /// language's light, normal definition (its base). A language without a base falls back to its nearest declared
    /// definition: the first one in catalog order with the requested appearance, else the first one with the requested
    /// density, else the language's first definition. A language therefore never needs a light, normal definition to
    /// be selectable in every appearance and density.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">The language is not present in the catalog.</exception>
    public static ThemeSelectionResult Resolve(IReadOnlyList<ThemeInfo> catalog, ThemeSelection selection)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(selection);

        ThemeInfo? first = null;
        ThemeInfo? exact = null;
        ThemeInfo? baseInfo = null;
        ThemeInfo? sameAppearance = null;
        ThemeInfo? sameDensity = null;
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

            if (info.Appearance == selection.Appearance)
            {
                sameAppearance ??= info;
            }

            if (info.Density == selection.Density)
            {
                sameDensity ??= info;
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
        else
        {
            var fallback = baseInfo ?? sameAppearance ?? sameDensity ?? first;
            themeId = fallback.Id;
            var chain = new List<ThemeAlgorithm>(fallback.Algorithms.Count + 2);
            foreach (var algorithm in fallback.Algorithms)
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

        Color? neutralSeed = null;
        var neutralIgnored = false;
        if (selection.NeutralColor is not null)
        {
            if (first.NeutralColor == ThemeNeutralColorSupport.Seed)
            {
                neutralSeed = selection.NeutralColor;
            }
            else
            {
                neutralIgnored = true;
            }
        }

        return new ThemeSelectionResult(themeId, algorithms, seed, ignored, neutralSeed, neutralIgnored);
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

    /// <summary>
    /// Gets whether the design language accepts a replaceable neutral color family.
    /// </summary>
    /// <exception cref="ArgumentException">The language is not present in the catalog.</exception>
    public static ThemeNeutralColorSupport NeutralColorSupport(IReadOnlyList<ThemeInfo> catalog, string designLanguage)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(designLanguage);

        foreach (var info in catalog)
        {
            if (string.Equals(info.DesignLanguage, designLanguage, StringComparison.Ordinal))
            {
                return info.NeutralColor;
            }
        }

        throw new ArgumentException(
            $"Design language '{designLanguage}' is not present in the theme catalog.", nameof(designLanguage));
    }
}
