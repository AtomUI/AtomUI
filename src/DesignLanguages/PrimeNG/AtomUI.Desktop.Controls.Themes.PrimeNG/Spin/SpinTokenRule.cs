using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SpinToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SpinTokenRule : IControlTokenRule<PrimeNGToken, SpinToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SpinToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGSpinTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is; the PrimeNG stage replaces only IndicatorDuration, and only under Nora (its
        // zero durations would stop the spinner). The sizes are the Ant formula over the PrimeNG global tokens.
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
