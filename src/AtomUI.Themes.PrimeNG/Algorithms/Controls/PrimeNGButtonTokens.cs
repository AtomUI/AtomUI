using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Button under the PrimeNG presets: the PrimeUIX <c>button.label.font.weight</c> and, where the preset sets one, the
/// button's own <c>ControlPaddingHorizontal</c> (Lara 16, PrimeUIX <c>lara-compat</c> <c>button.padding.x</c>). The
/// radius is the global <c>BorderRadius</c> already (PrimeUIX <c>button.border.radius</c> is
/// <c>border.radius.md</c>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
internal sealed class PrimeNGButtonTokens : IControlTokenAlgorithm<ButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        if (PrimeNGPresets.Resolve(context).Controls.ButtonPaddingHorizontal is { } paddingHorizontal)
        {
            controlGlobal.ControlPaddingHorizontal = paddingHorizontal;
        }
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ButtonToken token)
    {
        token.FontWeight = PrimeNGPresets.Resolve(context).Controls.ButtonFontWeight;
    }
}
