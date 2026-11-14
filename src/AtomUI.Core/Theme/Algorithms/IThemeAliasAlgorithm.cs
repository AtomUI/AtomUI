using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Algorithms;

/// <summary>
/// The optional Alias stage of a theme algorithm, implemented by the same class as its Map stage.
/// </summary>
/// <remarks>
/// The compiler calls <see cref="EvaluateAlias"/> on the instance that ran the Map stage, after the explicit Map
/// values are written and the Ant Design Alias formulas have run. The stage may write only Alias-stage global
/// tokens; a value it writes takes priority over the Ant Design formula, drives the Alias tokens derived from it,
/// and is still overridden by a value the theme sets explicitly. The stage must be a pure function of its inputs
/// and may run more than once per compilation, so a token's new value must never be computed from that token's
/// own current value.
/// </remarks>
public interface IThemeAliasAlgorithm : IThemeAlgorithm
{
    /// <summary>
    /// Writes the Alias-stage global tokens this algorithm defines.
    /// </summary>
    /// <param name="context">The resolved appearance and the effective algorithm chain.</param>
    /// <param name="token">
    /// The global token after the Map stage, the explicit Map values and the Ant Design Alias formulas.
    /// </param>
    void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token);
}
