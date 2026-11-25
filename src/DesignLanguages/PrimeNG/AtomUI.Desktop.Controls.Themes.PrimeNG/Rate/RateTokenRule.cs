using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RateToken"/>.</summary>
[ControlTokenRule]
internal sealed class RateTokenRule : IControlTokenRule<PrimeNGToken, RateToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<RateToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGRateTokens.EvaluateGlobal(in context, global);
        token.StarColor      = global.ColorPalettes[PresetPrimaryColor.Yellow].Color6;
        token.StarSize       = global.ControlHeight * 0.625;
        token.StarSizeSM     = global.ControlHeightSM * 0.625;
        token.StarSizeLG     = global.ControlHeightLG * 0.625;
        token.StarHoverScale = 1.2;
        token.StarBg         = global.ColorFillContent;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGRateTokens.EvaluateOwn(in context, global, token);
    }
}
