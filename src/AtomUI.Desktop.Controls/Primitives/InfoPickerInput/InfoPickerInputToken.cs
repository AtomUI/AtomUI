using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Primitives;

[ControlDesignToken]
internal sealed class InfoPickerInputToken : AbstractControlDesignToken
{
    
    public InfoPickerInputToken()

    {
    }
    
    /// <summary>
    /// 范围选择箭头外间距
    /// </summary>
    public Thickness RangePickerArrowMargin { get; set; }

    /// <summary>
    /// 选择指示器厚度
    /// </summary>
    public double RangePickerIndicatorThickness { get; set; }
    
    /// <summary>
    /// 范围选择与锚点的间距，在 RangePicker 模式下
    /// </summary>
    public double RangeMarginToAnchor { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        RangePickerArrowMargin        = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0);
        RangePickerIndicatorThickness = EffectiveGlobalToken.LineWidthBold;
        RangeMarginToAnchor           = EffectiveGlobalToken.UniformlyMarginXS;
    }
    
}
