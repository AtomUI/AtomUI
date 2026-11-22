using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// CheckBox under the PrimeNG presets: PrimeUIX 3.1.1 <c>checkbox.width</c> / <c>checkbox.icon.size</c> (Aura and Nora
/// 18 / 12, Lara 20 / 14). The radius is the global <c>BorderRadiusSM</c> already (PrimeUIX <c>border.radius.sm</c>;
/// Nora <c>border.radius.xs</c>, which its size ladder makes equal). Under a noir primary the check mark is the
/// contrast colour <c>ColorTextLightSolid</c>.
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
        // The check mark is the CheckBox's own ColorWhite. PrimeUIX checkbox.icon.checked.color is
        // primary.contrast.color: under a noir primary it takes the contrast colour, so it stays visible on the dark
        // appearance's near-white primary. A coloured primary keeps the white mark.
        if (PrimeNGNoir.IsActive(context, controlGlobal))
        {
            controlGlobal.ColorWhite = controlGlobal.ColorTextLightSolid;
        }
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CheckBoxToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.CheckIndicatorSize = controls.CheckIndicatorSize;
        token.CheckedMarkSize    = controls.CheckedMarkSize;
        // root.hover.border.color is form.field.hover.border.color.
        token.IndicatorHoverBorderColor = PrimeNGInputStage.HoverBorder(context, effectiveGlobal);
    }
}
