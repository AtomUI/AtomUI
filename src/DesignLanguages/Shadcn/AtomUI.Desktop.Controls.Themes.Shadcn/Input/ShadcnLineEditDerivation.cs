using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="LineEditToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnLineEditDerivation : IControlDerivation<ShadcnToken, LineEditToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<LineEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var frame = ShadcnInputFamily.DeriveFieldGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.InputFontSize = global.FontSize;
        token.InputFontSizeLG = global.FontSizeLG;
        token.InputFontSizeSM = token.InputFontSize;
        (token.TransparentOutlineBorder, token.BottomOutlineBorder) = frame;
    }
}
