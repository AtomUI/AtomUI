using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ScrollViewerToken : AbstractControlDesignToken
{
    public const string ID = "ScrollViewer";
    
    /// <summary>
    /// 指示器模式下，滚动条滑块的粗细
    /// </summary>
    public double IndicatorModeThumbThickness { get; set; }
    
    /// <summary>
    /// 正常模式下，滚动条滑块的粗细
    /// </summary>
    public double NormalModeThumbThickness { get; set; }
    
    /// <summary>
    /// 指示器模式下，滚动条滑块的圆角大小
    /// </summary>
    public CornerRadius IndicatorModeThumbCornerRadius { get; set; }
    
    /// <summary>
    /// 正常模式下，滚动条滑块的圆角大小
    /// </summary>
    public CornerRadius NormalModeThumbCornerRadius { get; set; }

    /// <summary>
    /// 滚动条滑块背景颜色
    /// </summary>
    public Color ThumbBg { get; set; }

    /// <summary>
    /// 滚动条滑块鼠标 hover 背景颜色
    /// </summary>
    public Color ThumbHoverBg { get; set; }
    
    #region 内部 Token 定义
    /// <summary>
    /// 水平滚动条内间距
    /// </summary>
    public Thickness ScrollBarContentHPadding { get; set; }
    
    /// <summary>
    /// 垂直滚动条内间距
    /// </summary>
    public Thickness ScrollBarContentVPadding { get; set; }
    #endregion
    
    public ScrollViewerToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        IndicatorModeThumbThickness = SharedToken.LineWidthBold;
        NormalModeThumbThickness    = SharedToken.SizeXS;
        
        ThumbBg                     = SharedToken.ColorBorderSecondary;
        ThumbHoverBg                = SharedToken.ColorBorder;
        NormalModeThumbCornerRadius = new CornerRadius(SharedToken.SizeXS / 2.0);
        ScrollBarContentHPadding    = new Thickness(SharedToken.UniformlyPaddingXXS, 0d);
        ScrollBarContentVPadding    = new Thickness(0d, SharedToken.UniformlyPaddingXXS);
    }
}