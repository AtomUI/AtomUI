using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TagToken : AbstractTagToken
{
    /// <summary>
    /// 默认背景色
    /// Default background color
    /// </summary>
    public partial Color DefaultBg { get; set; }

    /// <summary>
    /// 默认文字颜色
    /// Default text color
    /// </summary>
    public partial Color DefaultColor { get; set; }

    /// <summary>
    /// 关闭图标尺寸
    /// Size of the close icon
    /// </summary>
    public partial double TagCloseIconSize { get; set; }

    /// <summary>
    /// 默认实心标签的文本色
    /// Default text color for solid tag
    /// </summary>
    public partial Color SolidTextColor { get; set; }

    /// <summary>
    /// 填充型状态标签是否使用实心底色
    /// Whether a Filled status tag uses the solid status fill with the contrast text
    /// </summary>
    public partial bool FilledStatusSolid { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TagCloseIconSize = EffectiveGlobalToken.IconSizeXS;
        DefaultBg        = ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillTertiary, EffectiveGlobalToken.ColorBgContainer);
        DefaultColor     = EffectiveGlobalToken.ColorText;
        FilledStatusSolid = false;
        SolidTextColor   = ColorUtils.IsBright(EffectiveGlobalToken.ColorBgSolid, Colors.White)
            ? Colors.Black
            : Colors.White;
    }
}
