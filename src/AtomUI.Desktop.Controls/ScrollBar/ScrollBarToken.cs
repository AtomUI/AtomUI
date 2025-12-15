using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ScrollBarToken : AbstractControlDesignToken
{
    public const string ID = "ScrollBar";

    /// <summary>
    /// 滚动条滑块的粗细
    /// </summary>
    public double ThumbThickness { get; set; }

    /// <summary>
    /// 滚动条的粗细
    /// </summary>
    public double ScrollBarThickness { get; set; }

    /// <summary>
    /// 滚动条滑块背景颜色
    /// </summary>
    public Color ThumbBg { get; set; }

    /// <summary>
    /// 滚动条滑块鼠标 hover 背景颜色
    /// </summary>
    public Color ThumbHoverBg { get; set; }
    
    /// <summary>
    /// 滚动条滑块的圆角大小
    /// </summary>
    public CornerRadius ThumbCornerRadius { get; set; }
    
    #region 内部 Token 定义
    /// <summary>
    /// 水平滚动条内间距
    /// </summary>
    public Thickness ContentHPadding { get; set; }
    
    /// <summary>
    /// 垂直滚动条内间距
    /// </summary>
    public Thickness ContentVPadding { get; set; }
    #endregion

    public ScrollBarToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ThumbBg            = SharedToken.ColorBorderSecondary;
        ThumbHoverBg       = SharedToken.ColorBorder;
        ThumbThickness     = SharedToken.SizeXS;
        ScrollBarThickness = SharedToken.SizeSM;
        ThumbCornerRadius  = new CornerRadius(SharedToken.SizeXS / 2.0);
        ContentHPadding    = new Thickness(SharedToken.UniformlyPaddingXXS, 0d);
        ContentVPadding    = new Thickness(0d, SharedToken.UniformlyPaddingXXS);
    }
}