using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CarouselToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignCarouselDerivation : IControlDerivation<AntDesignToken, CarouselToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CarouselToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.IndicatorActiveWidth = 24;
        token.ArrowSize            = 16;
        token.ArrowOffset          = global.SpacingXS;
        token.ArrowOpacity         = 0.2;
        token.IndicatorWidth       = 16;
        token.IndicatorHeight      = 3;
        token.IndicatorGap         = global.UniformlyMarginXS;
        token.PaginationOffset     = 12;
    }
}
