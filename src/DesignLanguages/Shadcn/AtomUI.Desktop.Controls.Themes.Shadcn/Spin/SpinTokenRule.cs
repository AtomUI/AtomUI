using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SpinToken"/>.</summary>
[ControlTokenRule]
internal sealed class SpinTokenRule : IControlTokenRule<ShadcnToken, SpinToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SpinToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
    }
}
