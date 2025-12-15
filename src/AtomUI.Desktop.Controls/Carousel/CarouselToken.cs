using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class CarouselToken : AbstractControlDesignToken
{
    public const string ID = "Carousel";

    public CarouselToken()
        : base(ID)
    {
    }
    
    /// <summary>
    /// 指示点宽度
    /// Width of indicator
    /// </summary>
    public double IndicatorWidth { get; set; }
    
    /// <summary>
    /// 指示点高度
    /// Height of indicator
    /// </summary>
    public double IndicatorHeight { get; set; }
    
    /// <summary>
    /// 指示点之间的间距
    /// gap between indicator
    /// </summary>
    public double IndicatorGap { get; set; }
    
    /// <summary>
    /// 指示点距离边缘的距离
    /// dot offset to Carousel edge
    /// </summary>
    public double PaginationOffset { get; set; }
    
    /// <summary>
    /// 激活态指示点宽度
    /// Width of active indicator
    /// </summary>
    public double IndicatorActiveWidth { get; set; }
    
    /// <summary>
    /// 切换箭头大小
    /// Size of arrows
    /// </summary>
    public double ArrowSize { get; set; }
    
    /// <summary>
    /// 切换箭头边距
    /// arrows offset to Carousel edge
    /// </summary>
    public double ArrowOffset { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        IndicatorActiveWidth = 24;
        ArrowSize            = 16;
        ArrowOffset          = SharedToken.SpacingXS;
        IndicatorWidth       = 16;
        IndicatorHeight      = 3;
        IndicatorGap         = SharedToken.UniformlyMarginXS;
        PaginationOffset     = 12;
    }
}