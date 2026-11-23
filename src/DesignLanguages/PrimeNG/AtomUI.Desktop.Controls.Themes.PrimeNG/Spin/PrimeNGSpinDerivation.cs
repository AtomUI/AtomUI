using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SpinToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGSpinDerivation : IControlDerivation<PrimeNGToken, SpinToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<SpinToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSpinTokens.EvaluateGlobal(in context, global);
        var controlHeightLG = global.ControlHeightLG;
        var controlHeight   = global.ControlHeight;
        var indicatorSize     = controlHeightLG / 2;
        var indicatorSizeSM   = controlHeightLG * 0.35;
        var indicatorSizeLG   = controlHeight;
        token.IndicatorDuration = global.MotionDurationSlow * 4;
        token.DotSize           = ((indicatorSize - global.UniformlyMarginXXS / 2) / 2) * 0.75;
        token.DotSizeLG         = ((indicatorSizeLG - global.UniformlyMarginXXS) / 2) * 0.75;
        token.DotSizeSM         = ((indicatorSizeSM - global.UniformlyMarginXXS / 2) / 2) * 0.75;
        token.IndicatorSize     = indicatorSize + 2;
        token.IndicatorSizeSM   = indicatorSizeSM + 1;
        token.IndicatorSizeLG   = indicatorSizeLG + 4;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSpinTokens.EvaluateOwn(in context, global, token);
    }
}
