using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// CheckBox under the PrimeNG presets: the indicator and mark sizes of the baseline XML (Aura 20 / 14, which is
/// <c>aura-compat</c> <c>checkbox.width</c> / <c>checkbox.icon.size</c>; Lara 24 / 14, where <c>lara-compat</c> has
/// 24 / 16; Nora 20 / 14). The radius is the global <c>BorderRadiusSM</c> already (PrimeUIX <c>border.radius.sm</c>;
/// Nora <c>border.radius.xs</c>, which its size ladder makes equal).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCheckBoxTokens : IControlTokenAlgorithm<CheckBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CheckBoxToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.CheckIndicatorSize = controls.CheckIndicatorSize;
        token.CheckedMarkSize    = controls.CheckedMarkSize;
    }
}
