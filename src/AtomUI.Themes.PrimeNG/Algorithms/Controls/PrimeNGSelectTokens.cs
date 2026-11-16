using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Select under the PrimeNG presets: option and panel padding, computed from the control's effective global size
/// tokens by the preset's rule table: PrimeUIX 3.1.1 <c>list.option.padding</c> and <c>list.padding</c> (Aura 10,4 and 4;
/// Lara 14,8 and 0,8; Nora 10,4 and 0,2). PrimeUIX has no compact density; the option padding follows the size ladder.
/// The selected option's text is PrimeUIX <c>highlight.color</c> (<see cref="PrimeNGAppearanceRules.HighlightColor"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSelectTokens : IControlTokenAlgorithm<SelectToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SelectToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.OptionPadding       = controls.SelectOptionPadding(effectiveGlobal);
        token.PopupContentPadding = controls.SelectPopupContentPadding(effectiveGlobal);
        // PrimeUIX highlight.color on the selected option (see PrimeNGHighlightTokens).
        token.OptionSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}
