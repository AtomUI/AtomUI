using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="AvatarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class AvatarTokenRule : IControlTokenRule<ShadcnToken, AvatarToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<AvatarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
