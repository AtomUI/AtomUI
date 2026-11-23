using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CarouselToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCarouselDerivation : IControlDerivation<PrimeNGToken, CarouselToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CarouselToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGCarouselTokens.EvaluateGlobal(in context, global);
        token.IndicatorActiveWidth = 24;
        token.ArrowSize            = 16;
        token.ArrowOffset          = global.SpacingXS;
        token.ArrowOpacity         = 0.2;
        token.IndicatorWidth       = 16;
        token.IndicatorHeight      = 3;
        token.IndicatorGap         = global.UniformlyMarginXS;
        token.PaginationOffset     = 12;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCarouselTokens.EvaluateOwn(in context, global, token);
    }
}
