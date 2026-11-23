using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RateToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGRateDerivation : IControlDerivation<PrimeNGToken, RateToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<RateToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
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
