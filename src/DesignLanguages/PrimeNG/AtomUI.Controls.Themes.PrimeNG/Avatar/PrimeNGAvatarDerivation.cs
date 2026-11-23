using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AvatarToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGAvatarDerivation : IControlDerivation<PrimeNGToken, AvatarToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<AvatarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAvatarTokens.EvaluateGlobal(in context, global);
        token.ContainerSize = global.ControlHeight;
        token.ContainerSizeLG = global.ControlHeightLG;
        token.ContainerSizeSM = global.ControlHeightSM;
        token.TextFontSize = Math.Round((global.FontSizeLG + global.FontSizeXL) / 2);
        token.TextFontSizeLG = global.FontSizeHeading3;
        token.TextFontSizeSM = global.FontSize;
        token.GroupBorderColor = global.ColorBorderBg;
        token.AvatarBg = global.ColorTextPlaceholder;
        token.AvatarColor = global.ColorTextLightSolid;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAvatarTokens.EvaluateOwn(in context, global, token);
    }
}
