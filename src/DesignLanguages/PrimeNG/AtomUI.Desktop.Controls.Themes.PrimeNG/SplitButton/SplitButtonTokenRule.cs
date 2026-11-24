using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SplitButtonToken"/>.</summary>
[ControlTokenRule]
internal sealed class SplitButtonTokenRule : IControlTokenRule<PrimeNGToken, SplitButtonToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SplitButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSplitButtonTokens.EvaluateGlobal(in context, global);
        token.InsetSeparator         = false;
        token.GutterToFlyout         = global.UniformlyMarginXXS;
        token.SolidSeparatorColor    = global.ColorPrimaryHover;
        token.DangerSeparatorColor   = global.ColorErrorHover;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSplitButtonTokens.EvaluateOwn(in context, global, token);
    }
}
