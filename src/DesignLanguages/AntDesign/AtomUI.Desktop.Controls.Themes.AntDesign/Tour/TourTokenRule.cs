using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="TourToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TourTokenRule : IControlTokenRule<AntDesignToken, TourToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TourToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
