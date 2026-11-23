using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TextAreaToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTextAreaDerivation : IControlDerivation<ShadcnToken, TextAreaToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TextAreaToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var frame = ShadcnInputFamily.DeriveFieldGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.ResizeIndicatorLineColor = global.ColorTextDescription;
        token.ResizeHandleSize         = global.SizeXS;

        var lineWidth    = global.LineWidth;
        token.RightAddOnPadding   = new Thickness(0, 0, global.UniformlyPaddingSM - lineWidth, 0);
        token.RightAddOnPaddingSM = new Thickness(0, 0, global.ControlPaddingHorizontalSM - lineWidth, 0);
        token.RightAddOnPaddingLG = new Thickness(0, 0, global.ControlPaddingHorizontal - lineWidth, 0);
        token.LeftInnerAddOnMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        (token.TransparentOutlineBorder, token.BottomOutlineBorder) = frame;
    }
}
