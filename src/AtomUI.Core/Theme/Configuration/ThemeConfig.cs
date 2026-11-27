using System.Collections.ObjectModel;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

/// <summary>
/// An immutable theme configuration layer: the application's initial configuration, a runtime request or a local
/// <c>ThemeConfigProvider</c> scope. Every axis left <see langword="null" /> is inherited; the token maps hold
/// explicit values by token name. See <see cref="Inherit" /> for the case that drops the parent's values.
/// </summary>
public sealed class ThemeConfig
{
    internal static ThemeConfig Empty { get; } = new(
        true,
        null, null, null, null, null,
        new Dictionary<string, string>(StringComparer.Ordinal),
        new Dictionary<string, string>(StringComparer.Ordinal),
        new Dictionary<ControlTokenIdentity, ControlThemeConfig>());

    internal ThemeConfig(
        bool inherit,
        string? language,
        DesignStyle? style,
        ThemeAppearance? appearance,
        DesignDensity? density,
        DesignIdiom? idiom,
        IReadOnlyDictionary<string, string> tokens,
        IReadOnlyDictionary<string, string> languageTokens,
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> controls)
    {
        Inherit        = inherit;
        Language       = language;
        Style          = style;
        Appearance     = appearance;
        Density        = density;
        Idiom          = idiom;
        Tokens         = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(tokens, StringComparer.Ordinal));
        LanguageTokens = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(languageTokens, StringComparer.Ordinal));
        // Builder maps already hold canonical owner evidence; copy their values without rebuilding it.
        var canonicalControls = controls as CanonicalControlTokenMap<ControlThemeConfig>;
        if (canonicalControls is null)
        {
            canonicalControls = new CanonicalControlTokenMap<ControlThemeConfig>();
            foreach (var entry in controls)
            {
                canonicalControls.Set(entry.Key, entry.Value);
            }
        }
        Controls = new ReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>(
            canonicalControls.ToDictionary(static entry => entry.Key, static entry => entry.Value));
    }

    /// <summary>
    /// Whether this layer is merged over its parent scope's values. With <see langword="false" /> the parent's
    /// values are dropped except its language and idiom, and only the defaults lie below what this layer sets.
    /// </summary>
    public bool Inherit { get; }

    /// <summary>
    /// The design language identity, or <see langword="null" /> to inherit it.
    /// </summary>
    public string? Language { get; }

    /// <summary>
    /// The style of the language, or <see langword="null" /> to inherit it.
    /// </summary>
    public DesignStyle? Style { get; }

    /// <summary>
    /// The appearance, or <see langword="null" /> to inherit it.
    /// </summary>
    public ThemeAppearance? Appearance { get; }

    /// <summary>
    /// The density, or <see langword="null" /> to inherit it.
    /// </summary>
    public DesignDensity? Density { get; }

    /// <summary>
    /// The idiom, or <see langword="null" /> to take it from the interface root.
    /// </summary>
    public DesignIdiom? Idiom { get; }

    /// <summary>
    /// Explicit global token values: seeds, Map and Alias tokens.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tokens { get; }

    /// <summary>
    /// Explicit language token values of the current language; ignored under another language.
    /// </summary>
    public IReadOnlyDictionary<string, string> LanguageTokens { get; }

    /// <summary>
    /// The configurations of single controls, by control identity. A control without an entry is derived from
    /// the scope's values alone.
    /// </summary>
    public IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> Controls { get; }

    /// <summary>
    /// This configuration with other global token values.
    /// </summary>
    internal ThemeConfig WithTokens(IReadOnlyDictionary<string, string> tokens) =>
        new(Inherit, Language, Style, Appearance, Density, Idiom, tokens, LanguageTokens, Controls);

    /// <summary>
    /// This configuration without its control entries.
    /// </summary>
    internal ThemeConfig WithoutControls() =>
        new(Inherit, Language, Style, Appearance, Density, Idiom, Tokens, LanguageTokens,
            new Dictionary<ControlTokenIdentity, ControlThemeConfig>());
}
