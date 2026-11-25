using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// OptionButton under the PrimeNG presets. Nora's <c>togglebutton</c> (the SelectButton option) fills the checked
/// option with <c>highlight.background</c> and <c>highlight.color</c> text, so the outline style fills the checked cell
/// the same way (<c>ControlItemBgActive</c> / <c>ControlItemColorActive</c>); with Nora's dark field border a checked
/// border alone is hard to tell from the unchecked ones. Aura and Lara keep the Ant outline (a primary border and text).
/// </summary>
internal static class PrimeNGOptionButtonTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, OptionButtonToken token)
    {
        if (context.Variant.Style != PrimeNGLanguage.Nora)
        {
            return;
        }

        token.ButtonOutlineCheckedBackground  = effectiveGlobal.ControlItemBgActive;
        token.ButtonOutlineCheckedColor       = effectiveGlobal.ControlItemColorActive;
        token.ButtonOutlineCheckedHoverColor  = effectiveGlobal.ControlItemColorActive;
        token.ButtonOutlineCheckedActiveColor = effectiveGlobal.ControlItemColorActive;
    }
}
