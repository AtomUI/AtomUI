using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TagToken : AbstractTagToken
{
    /// <summary>
    /// 默认背景色
    /// Default background color
    /// </summary>
    public Color DefaultBg { get; set; }

    /// <summary>
    /// 默认文字颜色
    /// Default text color
    /// </summary>
    public Color DefaultColor { get; set; }

    /// <summary>
    /// 关闭图标尺寸
    /// Size of the close icon
    /// </summary>
    public double TagCloseIconSize { get; set; }

    /// <summary>
    /// 默认实心标签的文本色
    /// Default text color for solid tag
    /// </summary>
    public Color SolidTextColor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TagCloseIconSize = EffectiveGlobalToken.IconSizeXS;
        DefaultBg        = ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillTertiary, EffectiveGlobalToken.ColorBgContainer);
        DefaultColor     = EffectiveGlobalToken.ColorText;
        SolidTextColor   = ColorUtils.IsBright(EffectiveGlobalToken.ColorBgSolid, Colors.White)
            ? Colors.Black
            : Colors.White;
    }
}
