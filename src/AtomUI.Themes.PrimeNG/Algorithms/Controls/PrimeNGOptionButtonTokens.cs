using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// OptionButton under the PrimeNG presets. Nora's <c>togglebutton</c> (the SelectButton option) fills the checked
/// option with <c>highlight.background</c> and <c>highlight.color</c> text, so the outline style fills the checked cell
/// the same way (<c>ControlItemBgActive</c> / <c>ControlItemColorActive</c>); with Nora's dark field border a checked
/// border alone is hard to tell from the unchecked ones. Aura and Lara keep the Ant outline (a primary border and text).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGOptionButtonTokens : IControlTokenAlgorithm<OptionButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, OptionButtonToken token)
    {
        if (PrimeNGPresets.KindOf(context) != PrimeNGPresetKind.Nora)
        {
            return;
        }

        token.ButtonOutlineCheckedBackground  = effectiveGlobal.ControlItemBgActive;
        token.ButtonOutlineCheckedColor       = effectiveGlobal.ControlItemColorActive;
        token.ButtonOutlineCheckedHoverColor  = effectiveGlobal.ControlItemColorActive;
        token.ButtonOutlineCheckedActiveColor = effectiveGlobal.ControlItemColorActive;
    }
}
