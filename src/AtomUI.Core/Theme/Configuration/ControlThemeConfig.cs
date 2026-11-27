using System.Collections.ObjectModel;
using AtomUI.Theme.Languages;

namespace AtomUI.Theme.Configuration;

/// <summary>
/// The configuration of one kind of control: the axes it is fixed to and its token values, which are its control
/// tokens and any global token it sees. Overriding a seed or fixing an axis makes the control its own evaluation
/// unit; overriding a Map, Alias or control token only replaces that value.
/// </summary>
public sealed class ControlThemeConfig
{
    internal ControlThemeConfig(
        ThemeAppearance? appearance,
        DesignDensity? density,
        IReadOnlyDictionary<string, string> tokens)
    {
        Appearance = appearance;
        Density    = density;
        Tokens     = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(tokens, StringComparer.Ordinal));
    }

    /// <summary>
    /// The appearance this control always uses, or <see langword="null" /> to use its scope's.
    /// </summary>
    public ThemeAppearance? Appearance { get; }

    /// <summary>
    /// The density this control always uses, or <see langword="null" /> to use its scope's.
    /// </summary>
    public DesignDensity? Density { get; }

    /// <summary>
    /// Explicit token values by token name: the control's own tokens and any global token it sees.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tokens { get; }
}
