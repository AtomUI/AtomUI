using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

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

    public ThemeConfigBuilder WithInherit(bool inherit)
    {
        _inherit = inherit;
        return this;
    }

    /// <summary>Selects a design language by identity; a style set earlier stays only if it is set again.</summary>
    public ThemeConfigBuilder WithLanguage(string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        _language = language;
        return this;
    }

    public ThemeConfigBuilder WithStyle(DesignStyle style)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(style.Id);
        _style = style;
        return this;
    }

    public ThemeConfigBuilder WithAppearance(ThemeAppearance appearance)
    {
        _appearance = appearance;
        return this;
    }

    public ThemeConfigBuilder WithDensity(DesignDensity density)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(density.Id);
        _density = density;
        return this;
    }

    public ThemeConfigBuilder WithIdiom(DesignIdiom idiom)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idiom.Id);
        _idiom = idiom;
        return this;
    }

    /// <summary>Writes an explicit global token value (a seed, Map or Alias token).</summary>
    public ThemeConfigBuilder WithToken(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        _tokens[name] = value;
        return this;
    }

    /// <summary>Writes an explicit language token value of the configured or inherited language.</summary>
    public ThemeConfigBuilder WithLanguageToken(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        _languageTokens[name] = value;
        return this;
    }

    public ThemeConfigBuilder WithControl(ControlTokenIdentity identity, ControlThemeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _controls.Set(identity, config);
        return this;
    }

    public ThemeConfig Build()
    {
        return new ThemeConfig(_inherit, _language, _style, _appearance, _density, _idiom, _tokens, _languageTokens,
            _controls);
    }
}
