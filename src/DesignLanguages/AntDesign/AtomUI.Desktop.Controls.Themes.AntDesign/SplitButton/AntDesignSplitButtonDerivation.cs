using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SplitButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignSplitButtonDerivation : IControlDerivation<AntDesignToken, SplitButtonToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<SplitButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.InsetSeparator         = false;
        token.GutterToFlyout         = global.UniformlyMarginXXS;
        token.SolidSeparatorColor    = global.ColorPrimaryHover;
        token.DangerSeparatorColor   = global.ColorErrorHover;
    }
}
