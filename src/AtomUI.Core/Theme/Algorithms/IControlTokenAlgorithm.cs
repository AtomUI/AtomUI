using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Algorithms;

/// <summary>
/// The optional control token stage of a theme algorithm: one class per control, declared with
/// <see cref="ControlTokenAlgorithmAttribute"/> for every algorithm it serves.
/// </summary>
/// <remarks>
/// <para>
/// <typeparamref name="TToken"/> must be a terminal control token class bound to a control (its generated half
/// implements <see cref="IControlToken"/>). Controls without an own token class cannot take part in the control
/// stage yet. The implementation may live in another assembly than the algorithm, so a control package can supply
/// the values of its own controls under an existing algorithm.
/// </para>
/// <para>
/// The registry keeps one instance per declaration and the compiler may call both methods more than once per
/// compilation. Control tokens compile lazily, so that one instance may be called concurrently from different
/// snapshots and threads. Like every algorithm stage the methods must therefore be pure functions of their inputs:
/// no clocks, environment, static mutable state or instance state, no writes to the inputs they only read, and never
/// a token value computed from that token's own current value.
/// </para>
/// </remarks>
/// <typeparam name="TToken">The control's own token class.</typeparam>
public interface IControlTokenAlgorithm<TToken> where TToken : AbstractControlDesignToken
{
    /// <summary>
    /// Writes global tokens that apply to this control only.
    /// </summary>
    /// <remarks>
    /// When the control uses its scope's global tokens (algorithm mode unspecified or <c>Disabled</c>), the theme's
    /// explicit Map and Alias values win over these writes: after the stage, the compiler writes them back over the
    /// tokens it wrote. An explicit seed is an input the Map stage already consumed and is not restored. In
    /// <c>Global</c> and <c>Custom</c> mode nothing is written back, because those modes re-derive the control's global
    /// tokens without the theme's explicit Map and Alias values. Control-scoped global overrides are applied last and
    /// win over all of these.
    /// </remarks>
    /// <param name="context">The resolved appearance and the effective algorithm chain.</param>
    /// <param name="controlGlobal">The control's own copy of its effective global token.</param>
    void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal);

    /// <summary>
    /// Writes the control's own tokens.
    /// </summary>
    /// <param name="context">The resolved appearance and the effective algorithm chain.</param>
    /// <param name="effectiveGlobal">The control's final effective global token. Read only.</param>
    /// <param name="token">The control's own token after the control's default calculation.</param>
    void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TToken token);
}
