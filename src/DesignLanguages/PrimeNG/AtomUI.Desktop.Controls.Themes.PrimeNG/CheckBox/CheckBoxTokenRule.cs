using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="CheckBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CheckBoxTokenRule : IControlTokenRule<PrimeNGToken, CheckBoxToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CheckBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (ColorWhite), read by the CheckBox theme (CheckedMarkBrush) and
        // not by the formulas below.
        PrimeNGCheckBoxTokens.EvaluateGlobal(in context, global);
        token.CheckIndicatorSize        = global.ControlInteractiveSize;

        token.CheckedMarkSize           = token.CheckIndicatorSize * 0.6;
        token.IndicatorTristateMarkSize = global.FontSizeLG / 2;
        token.IndicatorHoverBorderColor = global.ColorPrimaryHover;
        token.TextMargin                = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCheckBoxTokens.EvaluateOwn(in context, global, token);
    }
}
