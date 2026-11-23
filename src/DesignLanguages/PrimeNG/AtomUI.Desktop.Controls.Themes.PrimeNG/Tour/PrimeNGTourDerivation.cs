using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TourToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTourDerivation : IControlDerivation<PrimeNGToken, TourToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TourToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTourTokens.EvaluateGlobal(in context, global);
        token.CloseBtnSize          = global.FontSize * global.RelativeLineHeight;
        token.PrimaryPrevBtnBg      = global.ColorTextLightSolid.SetAlphaF(0.15);
        token.PrimaryNextBtnHoverBg = ColorUtils.OnBackground(global.ColorBgTextHover, global.ColorWhite);
        token.IndicatorSize         = 6;
        token.TourBorderRadius      = global.BorderRadiusLG;
        token.TourViewMinWidth      = 200;
        token.TourViewMinHeight     = 120;
        token.HeaderColor           = global.ColorTextHeading;
        token.PopupMarginToAnchor   = global.SpacingXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTourTokens.EvaluateOwn(in context, global, token);
    }
}
