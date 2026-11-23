using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AvatarToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignAvatarDerivation : IControlDerivation<AntDesignToken, AvatarToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<AvatarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.ContainerSize = global.ControlHeight;
        token.ContainerSizeLG = global.ControlHeightLG;
        token.ContainerSizeSM = global.ControlHeightSM;
        token.TextFontSize = Math.Round((global.FontSizeLG + global.FontSizeXL) / 2);
        token.TextFontSizeLG = global.FontSizeHeading3;
        token.TextFontSizeSM = global.FontSize;
        token.GroupBorderColor = global.ColorBorderBg;
        token.AvatarBg = global.ColorTextPlaceholder;
        token.AvatarColor = global.ColorTextLightSolid;
    }
}
