using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Controls;

[ControlDesignToken]
internal sealed class AvatarToken : AbstractControlDesignToken
{
    public double ContainerSize { get; set; }

    public double ContainerSizeLG { get; set; }

    public double ContainerSizeSM { get; set; }

    public double TextFontSize { get; set; }

    public double TextFontSizeLG { get; set; }

    public double TextFontSizeSM { get; set; }

    public Color GroupBorderColor { get; set; }

    public Color AvatarBg { get; set; }

    public Color AvatarColor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
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
