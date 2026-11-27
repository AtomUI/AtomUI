using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TourToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TourTokenRule : IControlTokenRule<PrimeNGToken, TourToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TourToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGTourTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is: the PrimeNG tour stage at the end of the method replaces only
        // TourBorderRadius; the sizes and colors are the Ant formula over the PrimeNG global tokens.
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
