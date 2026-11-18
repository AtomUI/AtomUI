using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class FloatButtonToken : AbstractControlDesignToken
{

    public FloatButtonToken()

    {
    }
    
    /// <summary>
    /// FloatButton 尺寸
    /// Size of FloatButton
    /// </summary>
    public partial double FloatButtonSize { get; set; }
    
    /// <summary>
    /// FloatButton 图标尺寸
    /// Icon size of FloatButton
    /// </summary>
    public partial double FloatButtonIconSize { get; set; }
    
    /// <summary>
    /// 主要按钮文本颜色
    /// </summary>
    public partial Color PrimaryColor { get; set; }
    
    /// <summary>
    /// 描述文本行高
    /// </summary>
    public partial double DescriptionLineHeight { get; set; }
    
    /// <summary>
    /// 默认的基于定位的水平偏移值
    /// </summary>
    public partial double FloatOffsetX { get; set; }
    
    /// <summary>
    /// 默认的基于定位的垂直偏移值
    /// </summary>
    public partial double FloatOffsetY { get; set; }

    /// <summary>
    /// 悬浮按钮阴影
    /// Shadow of the floating button
    /// </summary>
    public partial BoxShadows FloatButtonShadows { get; set; }

    /// <summary>
    /// 默认类型悬浮按钮底色（BackgroundFrame）
    /// </summary>
    public partial Color DefaultBg { get; set; }

    /// <summary>
    /// 默认类型悬浮按钮悬停底色
    /// </summary>
    public partial Color DefaultHoverBg { get; set; }

    /// <summary>
    /// 默认类型悬浮按钮按下底色
    /// </summary>
    public partial Color DefaultActiveBg { get; set; }

    /// <summary>
    /// 默认类型悬浮按钮前景色
    /// </summary>
    public partial Color DefaultIconColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        FloatButtonShadows    = EffectiveGlobalToken.BoxShadowsSecondary;
        DefaultBg             = EffectiveGlobalToken.ColorBgElevated;
        DefaultHoverBg        = EffectiveGlobalToken.ColorBgTextHover;
        // Ant Design has no pressed reaction for the default (white) type: the frame stays transparent so the
        // elevated layer below shows through, exactly as before this token existed.
        DefaultActiveBg       = Colors.Transparent;
        DefaultIconColor      = EffectiveGlobalToken.ColorText;
        PrimaryColor          = EffectiveGlobalToken.ColorTextLightSolid;
        FloatButtonIconSize   = EffectiveGlobalToken.FontSizeIcon * 1.5;
        FloatButtonSize       = EffectiveGlobalToken.ControlHeightLG;
        DescriptionLineHeight = EffectiveGlobalToken.FontSizeSM * 1.2;
        FloatOffsetX          = EffectiveGlobalToken.UniformlyMargin;
        FloatOffsetY          = EffectiveGlobalToken.UniformlyMargin;
    }
    
}
