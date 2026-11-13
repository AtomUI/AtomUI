using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RadioButtonToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 单选框大小，除去文字部分的
    /// </summary>
    public partial double RadioSize { get; set; }

    /// <summary>
    /// 单选框圆点大小
    /// </summary>
    public partial double DotSize { get; set; }

    /// <summary>
    /// 单选框圆点禁用颜色
    /// </summary>
    public partial Color DotColorDisabled { get; set; }

    /// <summary>
    /// 选中时圆点颜色（内部使用）
    /// Dot color of a checked radio (internal)
    /// </summary>
    public partial Color RadioColor { get; set; }

    /// <summary>
    /// 选中时单选框背景色（内部使用）
    /// Background of a checked radio (internal)
    /// </summary>
    public partial Color RadioBgColor { get; set; }

    public partial double DotPadding { get; set; }

    public partial Thickness TextMargin { get; set; }

    public RadioButtonToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var lineWidth        = EffectiveGlobalToken.LineWidth;
        var fontSizeLG       = EffectiveGlobalToken.FontSizeLG;
        var colorPrimary     = EffectiveGlobalToken.ColorPrimary;
        var colorWhite       = EffectiveGlobalToken.ColorWhite;

        var dotPadding = 4; // 魔术值，需要看有没有好办法消除
        var radioSize  = fontSizeLG;

        var radioDotSize = radioSize - (dotPadding + lineWidth) * 2;

        DotPadding       = dotPadding;
        RadioSize        = radioSize;
        DotSize          = radioDotSize;
        DotColorDisabled = EffectiveGlobalToken.ColorTextDisabled;

        // internal
        RadioColor   = colorWhite;
        RadioBgColor = colorPrimary;
        TextMargin   = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
    }
    
}
