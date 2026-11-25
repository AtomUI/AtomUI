using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="LineEditToken"/>.</summary>
[ControlTokenRule]
internal sealed class LineEditTokenRule : IControlTokenRule<PrimeNGToken, LineEditToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<LineEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGLineEditTokens.EvaluateGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.InputFontSize = global.FontSize;
        token.InputFontSizeLG = global.FontSizeLG;
        token.InputFontSizeSM = token.InputFontSize;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGLineEditTokens.EvaluateOwn(in context, global, token);
    }
}
