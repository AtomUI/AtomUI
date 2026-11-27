using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

/// <summary>
/// Builds a theme configuration layer. Axes left unset are inherited by the layer; see <see cref="WithInherit" />
/// for the case that drops the parent's values.
/// </summary>
public sealed class ThemeConfigBuilder
{
    private bool _inherit = true;
    private string? _language;
    private DesignStyle? _style;
    private ThemeAppearance? _appearance;
    private DesignDensity? _density;
    private DesignIdiom? _idiom;
    private readonly Dictionary<string, string> _tokens = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _languageTokens = new(StringComparer.Ordinal);
    private readonly CanonicalControlTokenMap<ControlThemeConfig> _controls = new();

    /// <summary>
    /// Sets whether the layer inherits its parent scope's values; the default is <see langword="true" />. See
    /// <see cref="ThemeConfig.Inherit" /> for what <see langword="false" /> keeps.
    /// </summary>
    public ThemeConfigBuilder WithInherit(bool inherit)
    {
        _inherit = inherit;
        return this;
    }

    /// <summary>
    /// Selects a design language by identity. A style set earlier is not cleared: the new language must
    /// declare it, or the configuration is rejected when it is normalized.
    /// </summary>
    public ThemeConfigBuilder WithLanguage(string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        _language = language;
        return this;
    }

    /// <summary>
    /// Sets the style of the language. The language must declare it, or the configuration is rejected when it
    /// is normalized.
    /// </summary>
    public ThemeConfigBuilder WithStyle(DesignStyle style)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(style.Id);
        _style = style;
        return this;
    }

    /// <summary>
    /// Sets the appearance, <see cref="ThemeAppearance.Light" /> or <see cref="ThemeAppearance.Dark" />.
    /// </summary>
    public ThemeConfigBuilder WithAppearance(ThemeAppearance appearance)
    {
        _appearance = appearance;
        return this;
    }

    /// <summary>
    /// Sets the density step. The language must declare it, or the configuration is rejected when it is
    /// normalized.
    /// </summary>
    public ThemeConfigBuilder WithDensity(DesignDensity density)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(density.Id);
        _density = density;
        return this;
    }

    /// <summary>
    /// Sets the idiom the interface runs as. The language must declare it, or the configuration is rejected
    /// when it is normalized.
    /// </summary>
    public ThemeConfigBuilder WithIdiom(DesignIdiom idiom)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idiom.Id);
        _idiom = idiom;
        return this;
    }

    /// <summary>
    /// Writes an explicit global token value (a seed, Map or Alias token).
    /// </summary>
    public ThemeConfigBuilder WithToken(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        _tokens[name] = value;
        return this;
    }

    /// <summary>
    /// Writes an explicit language token value of the configured or inherited language.
    /// </summary>
    public ThemeConfigBuilder WithLanguageToken(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        _languageTokens[name] = value;
        return this;
    }

    /// <summary>
    /// Sets the configuration of one control, replacing an earlier configuration of the same identity.
    /// </summary>
    public ThemeConfigBuilder WithControl(ControlTokenIdentity identity, ControlThemeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _controls.Set(identity, config);
        return this;
    }

    /// <summary>
    /// Creates the configuration layer from the values set so far. The values are copied, so the builder can
    /// be changed and built again.
    /// </summary>
    public ThemeConfig Build()
    {
        return new ThemeConfig(_inherit, _language, _style, _appearance, _density, _idiom, _tokens, _languageTokens,
            _controls);
    }
}
