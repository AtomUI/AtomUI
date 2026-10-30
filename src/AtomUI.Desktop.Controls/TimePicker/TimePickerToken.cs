using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TimePickerToken : AbstractControlDesignToken
{
    
    public TimePickerToken()
    {
    }

    /// <summary>
    /// 时间选择项高度
    /// </summary>
    public double ItemHeight { get; set; }
    
    /// <summary>
    /// 时间选择项宽度
    /// </summary>
    public double ItemWidth { get; set; }
    
    /// <summary>
    /// 上下午选择项的宽度
    /// </summary>
    public double PeriodHostWidth { get; set; }

    /// <summary>
    /// 时间选择项内间距
    /// </summary>
    public Thickness ItemPadding { get; set; }

    /// <summary>
    /// 按钮区域对上的外边距
    /// </summary> 
    public Thickness ButtonsMargin { get; set; }

    /// <summary>
    /// 范围选择箭头外间距
    /// </summary>
    public Thickness RangePickerArrowMargin { get; set; }

    /// <summary>
    /// 选择指示器厚度
    /// </summary>
    public double RangePickerIndicatorThickness { get; set; }

    /// <summary>
    /// Header 头内间距
    /// </summary>
    public Thickness HeaderMargin { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemWidth                     = EffectiveGlobalToken.ControlHeightLG * 1.4;
        PeriodHostWidth               = ItemWidth;
        ItemHeight                    = EffectiveGlobalToken.ControlHeight - 4;
        ItemPadding                   = new Thickness((ItemWidth - ItemHeight) / 2, 0, 0, 0);
        ButtonsMargin                 = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
        RangePickerArrowMargin        = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0);
        RangePickerIndicatorThickness = EffectiveGlobalToken.LineWidthFocus;
        HeaderMargin                  = new Thickness(0, 0, 0, 3);
    }
    
}
