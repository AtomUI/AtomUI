using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TextAreaToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTextAreaDerivation : IControlDerivation<PrimeNGToken, TextAreaToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TextAreaToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTextAreaTokens.EvaluateGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.ResizeIndicatorLineColor = global.ColorTextDescription;
        token.ResizeHandleSize         = global.SizeXS;

        var lineWidth    = global.LineWidth;
        token.RightAddOnPadding   = new Thickness(0, 0, global.UniformlyPaddingSM - lineWidth, 0);
        token.RightAddOnPaddingSM = new Thickness(0, 0, global.ControlPaddingHorizontalSM - lineWidth, 0);
        token.RightAddOnPaddingLG = new Thickness(0, 0, global.ControlPaddingHorizontal - lineWidth, 0);
        token.LeftInnerAddOnMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTextAreaTokens.EvaluateOwn(in context, global, token);
    }
}
