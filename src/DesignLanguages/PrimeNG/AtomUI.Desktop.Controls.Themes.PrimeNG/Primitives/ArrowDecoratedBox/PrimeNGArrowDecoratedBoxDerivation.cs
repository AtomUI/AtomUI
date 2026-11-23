using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ArrowDecoratedBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGArrowDecoratedBoxDerivation : IControlDerivation<PrimeNGToken, ArrowDecoratedBoxToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ArrowDecoratedBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGArrowDecoratedBoxTokens.EvaluateGlobal(in context, global);
        // antd genRoundedArrow: sizePopupArrow wide, half as high, borderRadiusXS at the tip and borderRadiusOuter
        // where the sides meet the box.
        token.ArrowSize            = global.SizePopupArrow;
        token.ArrowTipRadius       = global.BorderRadiusXS.TopLeft;
        token.ArrowBaseRadius      = global.BorderRadiusOuter.TopLeft;
        token.ContentPadding       = global.PaddingXS;
        token.ArrowStrokeColor     = ColorUtils.FromRgbF(0.07, 0, 0, 0);
        token.ArrowStrokeThickness = 1;
        token.ContentBorderColor     = global.ColorBorderSecondary;
        token.ContentBorderThickness = default;
        token.ContentMinHeight       = global.ControlHeight;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGArrowDecoratedBoxTokens.EvaluateOwn(in context, global, token);
    }
}
