using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Button under the PrimeNG presets: the PrimeUIX <c>button.label.font.weight</c>. The radius is the global
/// <c>BorderRadius</c> already (PrimeUIX <c>button.border.radius</c> is <c>border.radius.md</c>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
internal sealed class PrimeNGButtonTokens : IControlTokenAlgorithm<ButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ButtonToken token)
    {
        token.FontWeight = PrimeNGPresets.Resolve(context).Controls.ButtonFontWeight;
    }
}
