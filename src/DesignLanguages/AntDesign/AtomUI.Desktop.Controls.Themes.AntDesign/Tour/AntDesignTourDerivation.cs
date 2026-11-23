using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TourToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTourDerivation : IControlDerivation<AntDesignToken, TourToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TourToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.CloseBtnSize          = global.FontSize * global.RelativeLineHeight;
        token.PrimaryPrevBtnBg      = global.ColorTextLightSolid.SetAlphaF(0.15);
        token.PrimaryNextBtnHoverBg = ColorUtils.OnBackground(global.ColorBgTextHover, global.ColorWhite);
        token.IndicatorSize         = 6;
        token.TourBorderRadius      = global.BorderRadiusLG;
        token.TourViewMinWidth      = 200;
        token.TourViewMinHeight     = 120;
        token.HeaderColor           = global.ColorTextHeading;
        token.PopupMarginToAnchor   = global.SpacingXXS;
    }
}
