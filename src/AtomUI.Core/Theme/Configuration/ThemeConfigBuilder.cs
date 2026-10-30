using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

public sealed class ThemeConfigBuilder
{
    private bool _inherit = true;
    private List<ThemeAlgorithm>? _algorithms;
    private readonly Dictionary<string, string> _tokens = new(StringComparer.Ordinal);
    private readonly CanonicalControlTokenMap<ControlThemeConfig> _controls = new();

    public ThemeConfigBuilder WithInherit(bool inherit)
    {
        _inherit = inherit;
        return this;
    }

    public ThemeConfigBuilder WithAlgorithms(params ThemeAlgorithm[] algorithms)
    {
        ArgumentNullException.ThrowIfNull(algorithms);
        _algorithms = new List<ThemeAlgorithm>(algorithms);
        return this;
    }

    public ThemeConfigBuilder WithToken(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        _tokens[name] = value;
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
        return new ThemeConfig(_inherit, _algorithms, _tokens, _controls);
    }
}
