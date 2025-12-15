using AtomUI.Theme.TokenSystem;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class TimePickerToken : AbstractControlDesignToken
{
    public const string ID = "TimePicker";

    public TimePickerToken()
        : base(ID)
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
        ItemWidth                     = 40;
        PeriodHostWidth               = 50;
        ItemHeight                    = SharedToken.ControlHeight - 4;
        ItemPadding                   = new Thickness(0, SharedToken.UniformlyPaddingXXS);
        ButtonsMargin                 = new Thickness(0, SharedToken.UniformlyMarginXS, 0, 0);
        RangePickerArrowMargin        = new Thickness(SharedToken.UniformlyMarginXS, 0);
        RangePickerIndicatorThickness = SharedToken.LineWidthFocus;
        HeaderMargin                  = new Thickness(0, 0, 0, 3);
    }
}