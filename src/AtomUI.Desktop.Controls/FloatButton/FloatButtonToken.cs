using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class FloatButtonToken : AbstractControlDesignToken
{

    public FloatButtonToken()

    {
    }
    
    /// <summary>
    /// FloatButton 尺寸
    /// Size of FloatButton
    /// </summary>
    public double FloatButtonSize { get; set; }
    
    /// <summary>
    /// FloatButton 图标尺寸
    /// Icon size of FloatButton
    /// </summary>
    public double FloatButtonIconSize { get; set; }
    
    /// <summary>
    /// 主要按钮文本颜色
    /// </summary>
    public Color PrimaryColor { get; set; }
    
    /// <summary>
    /// 描述文本行高
    /// </summary>
    public double DescriptionLineHeight { get; set; }
    
    /// <summary>
    /// 默认的基于定位的水平偏移值
    /// </summary>
    public double FloatOffsetX { get; set; }
    
    /// <summary>
    /// 默认的基于定位的垂直偏移值
    /// </summary>
    public double FloatOffsetY { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PrimaryColor          = EffectiveGlobalToken.ColorTextLightSolid;
        FloatButtonIconSize   = EffectiveGlobalToken.FontSizeIcon * 1.5;
        FloatButtonSize       = EffectiveGlobalToken.ControlHeightLG;
        DescriptionLineHeight = EffectiveGlobalToken.FontSizeSM * 1.2;
        FloatOffsetX          = EffectiveGlobalToken.UniformlyMargin;
        FloatOffsetY          = EffectiveGlobalToken.UniformlyMargin;
    }
    
}
