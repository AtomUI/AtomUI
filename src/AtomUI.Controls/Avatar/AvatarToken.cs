using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Controls;

[ControlDesignToken]
public sealed partial class AvatarToken : AbstractControlDesignToken
{
    public partial double ContainerSize { get; set; }

    public partial double ContainerSizeLG { get; set; }

    public partial double ContainerSizeSM { get; set; }

    public partial double TextFontSize { get; set; }

    public partial double TextFontSizeLG { get; set; }

    public partial double TextFontSizeSM { get; set; }

    public partial Color GroupBorderColor { get; set; }

    public partial Color AvatarBg { get; set; }

    public partial Color AvatarColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ContainerSize = EffectiveGlobalToken.ControlHeight;
        ContainerSizeLG = EffectiveGlobalToken.ControlHeightLG;
        ContainerSizeSM = EffectiveGlobalToken.ControlHeightSM;
        TextFontSize = Math.Round((EffectiveGlobalToken.FontSizeLG + EffectiveGlobalToken.FontSizeXL) / 2);
        TextFontSizeLG = EffectiveGlobalToken.FontSizeHeading3;
        TextFontSizeSM = EffectiveGlobalToken.FontSize;
        GroupBorderColor = EffectiveGlobalToken.ColorBorderBg;
        AvatarBg = EffectiveGlobalToken.ColorTextPlaceholder;
        AvatarColor = EffectiveGlobalToken.ColorTextLightSolid;
    }
}
