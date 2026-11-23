using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="RateToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignRateDerivation : IControlDerivation<AntDesignToken, RateToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<RateToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.StarColor      = global.ColorPalettes[PresetPrimaryColor.Yellow].Color6;
        token.StarSize       = global.ControlHeight * 0.625;
        token.StarSizeSM     = global.ControlHeightSM * 0.625;
        token.StarSizeLG     = global.ControlHeightLG * 0.625;
        token.StarHoverScale = 1.2;
        token.StarBg         = global.ColorFillContent;
    }
}
