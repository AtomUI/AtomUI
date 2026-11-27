using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Theme;

/// <summary>
/// The end user's selection: a language, optionally a named theme of it, the appearance (or following the
/// system), the density and the seeds the user picked. Immutable; every <c>With</c> method returns a new selection.
/// </summary>
public sealed record ThemeSelection
{
    private ThemeSelection(string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        Language = language;
    }

    /// <summary>
    /// The design language identity.
    /// </summary>
    public string Language { get; init; }

    /// <summary>
    /// The style, or <see langword="null" /> for the language's default (or the theme's fixed one).
    /// </summary>
    public DesignStyle? Style { get; init; }

    /// <summary>
    /// The named theme, or <see langword="null" /> to use the configured language default.
    /// </summary>
    public string? ThemeId { get; init; }

    internal bool UseImplicitTheme { get; init; }

    /// <summary>
    /// The appearance, or <see langword="null" /> for light (or the theme's fixed one, or the system's).
    /// </summary>
    public ThemeAppearance? Appearance { get; init; }

    /// <summary>
    /// Whether the appearance follows the platform; the manager re-applies when the platform changes.
    /// </summary>
    public bool FollowSystem { get; init; }

    /// <summary>
    /// The density, or <see langword="null" /> for the language's default (or the theme's fixed one).
    /// </summary>
    public DesignDensity? Density { get; init; }

    /// <summary>
    /// Global seeds the user picked, by token name and lexical value.
    /// </summary>
    public IReadOnlyDictionary<string, string> Seeds { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Language seeds the user picked, by language token name and lexical value.
    /// </summary>
    public IReadOnlyDictionary<string, string> LanguageSeeds { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Seeds written through <see cref="WithSeed(Action{DesignToken})" />, captured when the selection is resolved.
    /// </summary>
    internal IReadOnlyList<Action<DesignToken>> SeedWriters { get; init; } = Array.Empty<Action<DesignToken>>();

    /// <summary>
    /// Language seeds written through <see cref="WithLanguageSeed{TToken}" />, captured when resolved.
    /// </summary>
    internal IReadOnlyList<Action<LanguageDesignToken>> LanguageSeedWriters { get; init; } =
        Array.Empty<Action<LanguageDesignToken>>();

    /// <summary>
    /// Selects <typeparamref name="TLanguage" />, optionally in <paramref name="style" />.
    /// </summary>
    public static ThemeSelection Of<TLanguage>(DesignStyle? style = null)
        where TLanguage : DesignLanguage, new() =>
        new(new TLanguage().Id) { Style = style };

    /// <summary>
    /// Selects the language with the identity <paramref name="language" />, optionally in <paramref name="style" />.
    /// </summary>
    public static ThemeSelection Of(string language, DesignStyle? style = null) => new(language) { Style = style };

    /// <summary>
    /// Selects <paramref name="style" />. Resolving the selection throws <see cref="ThemeSelectionException" />
    /// when the language does not declare it.
    /// </summary>
    public ThemeSelection WithStyle(DesignStyle style) => this with { Style = style };

    /// <summary>
    /// Selects the theme with the identity <paramref name="themeId" />. Resolving the selection throws
    /// <see cref="ThemeSelectionException" /> when the theme is not in the catalog or belongs to another
    /// language.
    /// </summary>
    public ThemeSelection WithTheme(string themeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        return this with { ThemeId = themeId, UseImplicitTheme = false };
    }

    /// <summary>
    /// Uses the language's implicit theme even when the application configured a named default.
    /// </summary>
    public ThemeSelection WithoutTheme() => this with { ThemeId = null, UseImplicitTheme = true };

    /// <summary>
    /// Selects <paramref name="appearance" /> and stops following the platform.
    /// </summary>
    public ThemeSelection WithAppearance(ThemeAppearance appearance) => this with { Appearance = appearance, FollowSystem = false };

    /// <summary>
    /// Clears the fixed appearance and follows the platform; the manager applies the theme again when the platform
    /// changes.
    /// </summary>
    public ThemeSelection FollowSystemAppearance() => this with { FollowSystem = true, Appearance = null };

    /// <summary>
    /// Selects <paramref name="density" />. Resolving the selection throws <see cref="ThemeSelectionException" />
    /// when the language does not declare it.
    /// </summary>
    public ThemeSelection WithDensity(DesignDensity density) => this with { Density = density };

    /// <summary>
    /// Sets the global seed named <paramref name="name" /> to the lexical value <paramref name="value" />,
    /// replacing an earlier value for that name.
    /// </summary>
    public ThemeSelection WithSeed(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);
        return this with { Seeds = new Dictionary<string, string>(Seeds, StringComparer.Ordinal) { [name] = value } };
    }

    /// <summary>
    /// Writes global seeds; only the seeds the action changes are taken.
    /// </summary>
    public ThemeSelection WithSeed(Action<DesignToken> seeds)
    {
        ArgumentNullException.ThrowIfNull(seeds);
        return this with { SeedWriters = [.. SeedWriters, seeds] };
    }

    /// <summary>
    /// Sets the language seed named <paramref name="name" /> to the lexical value <paramref name="value" />,
    /// replacing an earlier value for that name.
    /// </summary>
    public ThemeSelection WithLanguageSeed(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);
        return this with
        {
            LanguageSeeds = new Dictionary<string, string>(LanguageSeeds, StringComparer.Ordinal) { [name] = value }
        };
    }

    /// <summary>
    /// Writes language seeds of <typeparamref name="TToken" />; only the seeds the action changes are taken.
    /// </summary>
    public ThemeSelection WithLanguageSeed<TToken>(Action<TToken> seeds)
        where TToken : LanguageDesignToken
    {
        ArgumentNullException.ThrowIfNull(seeds);
        return this with { LanguageSeedWriters = [.. LanguageSeedWriters, token => seeds((TToken)token)] };
    }
}

/// <summary>
/// What resolving a <see cref="ThemeSelection" /> gave: the request to apply and what the theme kept fixed.
/// </summary>
/// <param name="Request">
/// The theme request: the selected theme (or the language default) and the configuration.
/// </param>
/// <param name="IgnoredAxes">
/// The names of the axes (<c>Style</c>, <c>Appearance</c>, <c>Density</c>) whose selected value differs from the
/// value the theme fixes; for the appearance the selected value is the system's when the selection follows it.
/// The theme's fixed value is applied and the selection's value is left out of the request.
/// </param>
/// <param name="IgnoredSeeds">
/// The names of the seeds the selection named through <see cref="ThemeSelection.WithSeed(string, string)" /> or
/// <see cref="ThemeSelection.WithLanguageSeed(string, string)" /> that the theme locks. A locked seed is not replaced,
/// so the value is left out of the request.
/// </param>
public sealed record ThemeSelectionResult(
    ThemeRequest Request,
    IReadOnlyList<string> IgnoredAxes,
    IReadOnlyList<string> IgnoredSeeds);

/// <summary>
/// The selection could not be resolved: the language, theme or an axis value is not available.
/// </summary>
public sealed class ThemeSelectionException : Exception
{
    /// <summary>
    /// Creates the exception with <paramref name="message" />, which states what could not be resolved.
    /// </summary>
    public ThemeSelectionException(string message) : base(message)
    {
    }
}

/// <summary>
/// Resolves a <see cref="ThemeSelection" />. A pure function with three rules and no fallback search: the
/// language must be enabled, the theme must belong to it and the axes must be declared by it; the theme's fixed
/// axes win over the selection; the theme's locked seeds are not replaced.
/// </summary>
public static class ThemeSelectionResolver
{
    /// <summary>
    /// Resolves <paramref name="selection" /> into the request to apply. When the selection follows the system,
    /// <paramref name="systemAppearance" /> is the appearance to use.
    /// </summary>
    /// <exception cref="ThemeSelectionException">
    /// The selected language is not enabled, the theme is not in <paramref name="themes" /> or belongs to another
    /// language, or the language does not declare the selected style or density.
    /// </exception>
    public static ThemeSelectionResult Resolve(
        IReadOnlyList<DesignLanguageInfo> languages,
        IReadOnlyList<ThemeInfo> themes,
        ThemeSelection selection,
        ThemeAppearance systemAppearance = ThemeAppearance.Light)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(themes);
        ArgumentNullException.ThrowIfNull(selection);

        var language = languages.FirstOrDefault(info => string.Equals(info.Id, selection.Language, StringComparison.Ordinal)) ??
                       throw new ThemeSelectionException($"Design language '{selection.Language}' is not enabled.");
        var themeId = selection.ThemeId ?? ThemeInfo.LanguageDefaultId(language.Id);
        var theme = themes.FirstOrDefault(info => string.Equals(info.Id, themeId, StringComparison.Ordinal)) ??
                    throw new ThemeSelectionException($"Theme '{themeId}' is not in the catalog.");
        if (!string.Equals(theme.Language, language.Id, StringComparison.Ordinal))
        {
            throw new ThemeSelectionException(
                $"Theme '{themeId}' belongs to '{theme.Language}', not to the selected language '{language.Id}'.");
        }
        if (selection.Style is { } style && !language.Styles.Contains(style))
        {
            throw new ThemeSelectionException($"Design language '{language.Id}' does not declare the style '{style}'.");
        }
        if (selection.Density is { } density && !language.Densities.Contains(density))
        {
            throw new ThemeSelectionException($"Design language '{language.Id}' does not declare the density '{density}'.");
        }

        var ignoredAxes = new List<string>();
        var builder = new ThemeConfigBuilder().WithLanguage(language.Id);
        if (selection.Style is { } selectedStyle)
        {
            if (theme.Style is { } fixedStyle && fixedStyle != selectedStyle)
            {
                ignoredAxes.Add("Style");
            }
            else if (theme.Style is null)
            {
                builder.WithStyle(selectedStyle);
            }
        }

        var appearance = selection.FollowSystem ? systemAppearance : selection.Appearance;
        if (appearance is { } selectedAppearance)
        {
            if (theme.Appearance is { } fixedAppearance && fixedAppearance != selectedAppearance)
            {
                ignoredAxes.Add("Appearance");
            }
            else if (theme.Appearance is null)
            {
                builder.WithAppearance(selectedAppearance);
            }
        }

        if (selection.Density is { } selectedDensity)
        {
            if (theme.Density is { } fixedDensity && fixedDensity != selectedDensity)
            {
                ignoredAxes.Add("Density");
            }
            else if (theme.Density is null)
            {
                builder.WithDensity(selectedDensity);
            }
        }

        var ignoredSeeds = new List<string>();
        foreach (var (name, value) in selection.Seeds.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            if (theme.LockedSeeds.Contains(name))
            {
                ignoredSeeds.Add(name);
                continue;
            }
            builder.WithToken(name, value);
        }
        foreach (var (name, value) in selection.LanguageSeeds.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            if (theme.LockedSeeds.Contains(name))
            {
                ignoredSeeds.Add(name);
                continue;
            }
            builder.WithLanguageToken(name, value);
        }

        return new ThemeSelectionResult(new ThemeRequest(themeId, builder.Build(), ThemeTransitionReason.UserRequest),
            ignoredAxes, ignoredSeeds);
    }
}
