using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TagToken : AbstractControlDesignToken
{
    /// <summary>
    /// 默认背景色
    /// </summary>
    public Color DefaultBg { get; set; }

    /// <summary>
    /// 默认文字颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    public double TagFontSize { get; set; }
    public double TagLineHeight { get; set; }
    public double TagIconSize { get; set; }
    public double TagCloseIconSize { get; set; }
    public Thickness TagPadding { get; set; }
    public Thickness TagTextPaddingInline { get; set; }
    public Color SolidTextColor { get; set; }

    public TagToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TagFontSize      = EffectiveGlobalToken.FontSizeSM;
        TagLineHeight    = EffectiveGlobalToken.FontHeightSM;
        TagCloseIconSize = EffectiveGlobalToken.IconSizeXS;
        TagIconSize      = EffectiveGlobalToken.FontSizeIcon;
        TagPadding       = new Thickness(EffectiveGlobalToken.SizeXS - 1, 0);
        DefaultBg            = ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillTertiary, EffectiveGlobalToken.ColorBgContainer);
        DefaultColor         = EffectiveGlobalToken.ColorText;
        TagTextPaddingInline = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0);
        SolidTextColor       = ColorUtils.IsBright(EffectiveGlobalToken.ColorBgSolid, Colors.White)
            ? Colors.Black
            : Colors.White;
    }
    
}
