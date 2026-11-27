using AtomUI.Theme.Languages;

namespace AtomUI.Theme.Configuration;

/// <summary>
/// Builds the configuration of one kind of control for a theme configuration layer.
/// </summary>
public sealed class ControlThemeConfigBuilder
{
    private ThemeAppearance? _appearance;
    private DesignDensity? _density;
    private readonly Dictionary<string, string> _tokens = new(StringComparer.Ordinal);

    /// <summary>
    /// Fixes the appearance of this control; the control becomes its own evaluation unit.
    /// </summary>
    public ControlThemeConfigBuilder WithAppearance(ThemeAppearance appearance)
    {
        _appearance = appearance;
        return this;
    }

    /// <summary>
    /// Fixes the density of this control; the control becomes its own evaluation unit.
    /// </summary>
    public ControlThemeConfigBuilder WithDensity(DesignDensity density)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(density.Id);
        _density = density;
        return this;
    }

    /// <summary>
    /// Writes a control token or a global token this control sees.
    /// </summary>
    public ControlThemeConfigBuilder WithToken(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);
        _tokens[name] = value;
        return this;
    }

    /// <summary>
    /// Creates the control configuration from the values set so far. The values are copied, so the builder
    /// can be changed and built again.
    /// </summary>
    public ControlThemeConfig Build()
    {
        return new ControlThemeConfig(_appearance, _density, _tokens);
    }
}
