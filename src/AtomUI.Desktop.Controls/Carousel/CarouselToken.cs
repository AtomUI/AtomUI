using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CarouselToken : AbstractControlDesignToken
{

    public CarouselToken()

    {
    }
    
    /// <summary>
    /// 指示点宽度
    /// Width of indicator
    /// </summary>
    public partial double IndicatorWidth { get; set; }
    
    /// <summary>
    /// 指示点高度
    /// Height of indicator
    /// </summary>
    public partial double IndicatorHeight { get; set; }
    
    /// <summary>
    /// 指示点之间的间距
    /// gap between indicator
    /// </summary>
    public partial double IndicatorGap { get; set; }
    
    /// <summary>
    /// 指示点距离边缘的距离
    /// dot offset to Carousel edge
    /// </summary>
    public partial double PaginationOffset { get; set; }
    
    /// <summary>
    /// 激活态指示点宽度
    /// Width of active indicator
    /// </summary>
    public partial double IndicatorActiveWidth { get; set; }
    
    /// <summary>
    /// 切换箭头大小
    /// Size of arrows
    /// </summary>
    public partial double ArrowSize { get; set; }
    
    /// <summary>
    /// 切换箭头边距
    /// arrows offset to Carousel edge
    /// </summary>
    public partial double ArrowOffset { get; set; }

    /// <summary>
    /// 切换箭头静止时的不透明度（AtomUI 扩展；antd 固定为 0.4），悬停时完全不透明
    /// Opacity of the resting switch arrows (an AtomUI extension; antd fixes 0.4); hovered arrows are opaque
    /// </summary>
    public partial double ArrowOpacity { get; set; }
    
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        IndicatorActiveWidth = 24;
        ArrowSize            = 16;
        ArrowOffset          = EffectiveGlobalToken.SpacingXS;
        ArrowOpacity         = 0.2;
        IndicatorWidth       = 16;
        IndicatorHeight      = 3;
        IndicatorGap         = EffectiveGlobalToken.UniformlyMarginXS;
        PaginationOffset     = 12;
    }
    
}
