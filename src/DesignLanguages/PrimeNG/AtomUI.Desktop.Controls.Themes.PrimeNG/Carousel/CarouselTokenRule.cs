using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="CarouselToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CarouselTokenRule : IControlTokenRule<PrimeNGToken, CarouselToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CarouselToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (MotionDurationSlow), read by the Carousel theme
        // (PageTransitionDuration) and not by the formulas below.
        PrimeNGCarouselTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is: the carousel stage (PrimeNGCarouselTokens) replaces the indicator sizes
        // and the gap (PrimeUIX carousel.indicator); the arrows and the pagination offset keep the Ant formula.
        token.IndicatorActiveWidth = 24;
        token.ArrowSize            = 16;
        token.ArrowOffset          = global.SpacingXS;
        // AtomUI extension: antd fixes the resting arrow at 0.4; the value here stays at the 0.2 default.
        token.ArrowOpacity         = 0.2;
        token.IndicatorWidth       = 16;
        token.IndicatorHeight      = 3;
        token.IndicatorGap         = global.UniformlyMarginXS;
        token.PaginationOffset     = 12;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCarouselTokens.EvaluateOwn(in context, global, token);
    }
}
