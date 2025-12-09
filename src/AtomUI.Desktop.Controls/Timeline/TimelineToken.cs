using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class TimelineToken : AbstractControlDesignToken
{
    public const string ID = "Timeline";

    public TimelineToken()
        : this(ID)
    {
    }

    protected TimelineToken(string id)
        : base(id)
    {
    }

    /// <summary>
    /// Timeline 轨迹颜色
    /// </summary>
    public Color IndicatorTailColor { get; set; }

    /// <summary>
    /// 轨迹宽度
    /// </summary>
    public double IndicatorTailWidth { get; set; }

    /// <summary>
    /// 时间项内间距
    /// </summary>
    public Thickness ItemPaddingBottom { get; set; }

    /// <summary>
    /// 时间项下大内间距
    /// </summary>
    public Thickness ItemPaddingBottomLG { get; set; }
    
    /// <summary>
    /// 最后一个Item的Content最小高度
    /// </summary>
    public double LastItemContentMinHeight { get; set; }

    /// <summary>
    /// 节点指示器的大小
    /// </summary>
    public double IndicatorSize { get; set; }

    /// <summary>
    /// 节点指示器内置圆形大小
    /// </summary>
    public double IndicatorDotSize { get; set; }
    
    /// <summary>
    /// 指示器在最左边的外边距
    /// </summary>
    public Thickness IndicatorLeftModeMargin { get; set; }
    
    /// <summary>
    /// 指示器在最右边的外边距
    /// </summary>
    public Thickness IndicatorRightModeMargin { get; set; }
    
    /// <summary>
    /// 指示器在中间的外边距
    /// </summary>
    public Thickness IndicatorMiddleModeMargin { get; set; }

    /// <summary>
    /// 节点边框宽度
    /// </summary>
    public double IndicatorDotBorderWidth { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        IndicatorTailColor = SharedToken.ColorSplit;
        IndicatorTailWidth = SharedToken.LineWidthBold;

        IndicatorDotBorderWidth = SharedToken.LineWidth * 3;
        ItemPaddingBottom       = new Thickness(0, 0, 0, SharedToken.UniformlyPadding * 1.25);
        ItemPaddingBottomLG     = ItemPaddingBottom * 2;
        
        IndicatorLeftModeMargin   = new Thickness(0, 0, SharedToken.UniformlyMargin, 0);
        IndicatorRightModeMargin  = new Thickness(SharedToken.UniformlyMargin, 0, 0, 0);
        IndicatorMiddleModeMargin = new Thickness(SharedToken.UniformlyMargin, 0);

        LastItemContentMinHeight = SharedToken.ControlHeightLG * 1.2;
        IndicatorSize            = SharedToken.SizeMS;
        IndicatorDotSize         = 8;
    }
}