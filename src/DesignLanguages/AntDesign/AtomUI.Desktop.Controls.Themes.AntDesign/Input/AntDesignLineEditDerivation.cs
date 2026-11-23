using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="LineEditToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignLineEditDerivation : IControlDerivation<AntDesignToken, LineEditToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<LineEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.InputFontSize = global.FontSize;
        token.InputFontSizeLG = global.FontSizeLG;
        token.InputFontSizeSM = token.InputFontSize;
    }
}
