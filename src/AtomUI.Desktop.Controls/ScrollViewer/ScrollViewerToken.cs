using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ScrollViewerToken : AbstractControlDesignToken
{
    public const string ID = "ScrollViewer";
    
    /// <summary>
    /// 极简模式下，滚动条滑块的粗细
    /// </summary>
    public double LiteModeThumbThickness { get; set; }
    
    /// <summary>
    /// 正常模式下，滚动条滑块的粗细
    /// </summary>
    public double NormalModeThumbThickness { get; set; }
    
    /// <summary>
    /// 滚动条滑块的圆角大小
    /// </summary>
    public CornerRadius ThumbCornerRadius { get; set; }

    /// <summary>
    /// 滚动条滑块背景颜色
    /// </summary>
    public Color ThumbBg { get; set; }

    /// <summary>
    /// 滚动条滑块鼠标 hover 背景颜色
    /// </summary>
    public Color ThumbHoverBg { get; set; }
    
    /// <summary>
    /// 滚动条滑块鼠标按下的背景颜色
    /// </summary>
    public Color ThumbActiveBg { get; set; }
    
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
        LiteModeThumbThickness   = SharedToken.LineWidthBold;
        NormalModeThumbThickness = SharedToken.SizeXS;

        if (isDarkMode)
        {
            ThumbBg       = SharedToken.ColorBorder;
            ThumbHoverBg  = ThumbBg.Lighten();
            ThumbActiveBg = ThumbHoverBg.Lighten();
        }
        else
        {
            ThumbBg       = SharedToken.ColorBorder;
            ThumbHoverBg  = ThumbBg.Darken();
            ThumbActiveBg = ThumbHoverBg.Darken();
        }
        ThumbCornerRadius        = new CornerRadius(NormalModeThumbThickness / 2);
        ScrollBarContentHPadding = new Thickness(SharedToken.UniformlyPaddingXXS, 0d);
        ScrollBarContentVPadding = new Thickness(0d, SharedToken.UniformlyPaddingXXS);
    }
}