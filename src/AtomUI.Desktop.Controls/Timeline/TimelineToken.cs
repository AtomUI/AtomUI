using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TimelineToken : AbstractControlDesignToken
{
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
    /// 节点指示器的大小
    /// </summary>
    public double IndicatorSize { get; set; }

    /// <summary>
    /// 节点指示器内置圆形大小
    /// </summary>
    public double IndicatorDotSize { get; set; }
    
    /// <summary>
    /// 指示器在 Start 模式下的外边距
    /// </summary>
    public Thickness IndicatorStartModeMargin { get; set; }
    
    /// <summary>
    /// 指示器在 End 模式下的外边距
    /// </summary>
    public Thickness IndicatorEndModeMargin { get; set; }
    
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

        IndicatorTailColor = EffectiveGlobalToken.ColorSplit;
        IndicatorTailWidth = EffectiveGlobalToken.LineWidthBold;

        IndicatorDotBorderWidth = EffectiveGlobalToken.LineWidth * 3;

        if (!IsExplicit(TimelineTokenKind.ItemPaddingBottom))
        {
            ItemPaddingBottom       = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyPadding * 1.25);
        }

        ItemPaddingBottomLG     = ItemPaddingBottom * 2;
        
        IndicatorStartModeMargin  = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMargin, 0);
        IndicatorEndModeMargin    = new Thickness(EffectiveGlobalToken.UniformlyMargin, 0, 0, 0);
        IndicatorMiddleModeMargin = new Thickness(EffectiveGlobalToken.UniformlyMargin, 0);

        IndicatorSize            = EffectiveGlobalToken.SizeMS;
        IndicatorDotSize         = 8;
    }
    
}
