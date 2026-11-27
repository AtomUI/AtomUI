using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="TourToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TourTokenRule : IControlTokenRule<ShadcnToken, TourToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TourToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: shadcn has no Tour, so the language has no values of its own.
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
