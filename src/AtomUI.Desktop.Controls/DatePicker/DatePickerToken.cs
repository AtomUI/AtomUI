using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class DatePickerToken : AbstractControlDesignToken
{
    public const string ID = "DatePicker";

    public DatePickerToken()
        : base(ID)
    {
    }
    
    /// <summary>
    /// 单元格悬浮态背景色
    /// </summary>
    public Color CellHoverBg { get; set; }

    /// <summary>
    /// 选取范围内的单元格背景色
    /// </summary>
    public Color CellActiveWithRangeBg { get; set; }

    /// <summary>
    /// 选取范围内的单元格悬浮态背景色
    /// </summary>
    public Color CellHoverWithRangeBg { get; set; }

    /// <summary>
    /// 单元格禁用态背景色
    /// </summary>
    public Color CellBgDisabled { get; set; }

    /// <summary>
    /// 选取范围时单元格边框色
    /// </summary>
    public Color CellRangeBorderColor { get; set; }

    /// <summary>
    /// 单元格高度
    /// </summary>
    public double CellHeight { get; set; }

    /// <summary>
    /// 单元格宽度
    /// </summary>
    public double CellWidth { get; set; }

    /// <summary>
    /// 单元格外边距
    /// </summary>
    public Thickness CellMargin { get; set; }

    /// <summary>
    /// 日历项最小宽度
    /// </summary>
    public double ItemPanelMinWidth { get; set; }
    
    /// <summary>
    /// 月历最小的宽度
    /// </summary>
    public double MonthViewMinWidth { get; set; }

    /// <summary>
    /// 日历项最小高度
    /// </summary>
    public double ItemPanelMinHeight { get; set; }

    /// <summary>
    /// 单元格文本高度
    /// </summary>
    public double TextHeight { get; set; }

    /// <summary>
    /// 面板内容内边距
    /// </summary>
    public Thickness PanelContentPadding { get; set; }

    /// <summary>
    /// 十年/年/季/月/周单元格高度
    /// </summary>
    public double WithoutTimeCellHeight { get; set; }

    /// <summary>
    /// 星期的高度
    /// </summary>
    public double DayTitleHeight { get; set; }

    /// <summary>
    /// Header 头外间距
    /// </summary>
    public Thickness HeaderMargin { get; set; }
    
    /// <summary>
    /// Header 头内间距
    /// </summary>
    public Thickness HeaderPadding { get; set; }

    /// <summary>
    /// 范围日历间距
    /// </summary>
    public double RangeCalendarSpacing { get; set; }
    
    /// <summary>
    /// 按钮区域面板外间距
    /// </summary>
    public Thickness ButtonsPanelMargin { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        var colorPrimary = SharedToken.ColorPrimary;

        CellHoverBg            = SharedToken.ControlItemBgHover;
        CellActiveWithRangeBg  = SharedToken.ControlItemBgActive;
        CellHoverWithRangeBg   = colorPrimary.Lighten(35);
        CellRangeBorderColor   = colorPrimary.Lighten(20);
        CellBgDisabled         = SharedToken.ColorBgContainerDisabled;
        CellWidth              = SharedToken.ControlHeightSM;
        CellHeight             = SharedToken.ControlHeightSM;
        TextHeight             = SharedToken.ControlHeightLG;
        WithoutTimeCellHeight  = SharedToken.ControlHeightLG * 1.65;
        CellMargin             = SharedToken.MarginXXS;
        PanelContentPadding    = SharedToken.PaddingSM;
        ItemPanelMinWidth      = 225;
        ItemPanelMinHeight     = 270;
        MonthViewMinWidth      = 260;
        DayTitleHeight         = SharedToken.ControlHeightSM;
        HeaderMargin           = new Thickness(0, 0, 0, SharedToken.UniformlyMarginSM);
        HeaderPadding          = new Thickness(0, 0, 0, SharedToken.UniformlyPaddingSM);
        RangeCalendarSpacing   = 20;
        ButtonsPanelMargin     = new Thickness(0, SharedToken.UniformlyMarginXS, 0, 0);
    }
}