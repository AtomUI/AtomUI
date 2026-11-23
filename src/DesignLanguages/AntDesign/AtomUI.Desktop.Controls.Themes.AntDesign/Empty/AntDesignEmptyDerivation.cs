using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="EmptyToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignEmptyDerivation : IControlDerivation<AntDesignToken, EmptyToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<EmptyToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var controlHeightLG = global.ControlHeightLG;
        token.EmptyImgHeight      = controlHeightLG * 2.5;
        token.EmptyImgHeightMD    = controlHeightLG * 1.85;
        token.EmptyImgHeightSM    = controlHeightLG * 0.875;
        token.DescriptionMargin   = new Thickness(0, global.UniformlyMarginSM, 0, 0);
        token.DescriptionMarginSM = new Thickness(0, global.UniformlyMarginXS, 0, 0);
        token.FooterMargin        = new Thickness(0, global.UniformlyMargin, 0, 0);
    }
}
