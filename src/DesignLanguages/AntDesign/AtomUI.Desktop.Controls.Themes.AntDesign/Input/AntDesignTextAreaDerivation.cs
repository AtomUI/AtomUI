using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TextAreaToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTextAreaDerivation : IControlDerivation<AntDesignToken, TextAreaToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TextAreaToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.ResizeIndicatorLineColor = global.ColorTextDescription;
        token.ResizeHandleSize         = global.SizeXS;

        var lineWidth    = global.LineWidth;
        token.RightAddOnPadding   = new Thickness(0, 0, global.UniformlyPaddingSM - lineWidth, 0);
        token.RightAddOnPaddingSM = new Thickness(0, 0, global.ControlPaddingHorizontalSM - lineWidth, 0);
        token.RightAddOnPaddingLG = new Thickness(0, 0, global.ControlPaddingHorizontal - lineWidth, 0);
        token.LeftInnerAddOnMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
    }
}
