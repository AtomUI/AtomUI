using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Calendar 控件的 Design Token，承载产品视觉语义。
/// </summary>
[ControlDesignToken]
internal sealed class CalendarToken : AbstractControlDesignToken
{

    public CalendarToken()

    {
    }

    /// <summary>
    /// 完整 Calendar 背景，派生自容器背景。
    /// </summary>
    public Color FullBg { get; set; }

    /// <summary>
    /// 完整 Calendar Panel 背景，派生自容器背景。
    /// </summary>
    public Color FullPanelBg { get; set; }

    /// <summary>
    /// 完整模式选中日期/月单元背景，派生自 active item 背景。
    /// </summary>
    public Color ItemActiveBg { get; set; }

    /// <summary>
    /// Year Select 最小宽度。
    /// </summary>
    public double YearControlWidth { get; set; }

    /// <summary>
    /// Month Select 最小宽度。
    /// </summary>
    public double MonthControlWidth { get; set; }

    /// <summary>
    /// Year 模式月份单元的内容宽度。
    /// </summary>
    public double YearMonthCellWidth { get; set; }

    /// <summary>
    /// Mini 内容高度。
    /// </summary>
    public double MiniContentHeight { get; set; }

    /// <summary>
    /// Fullscreen 周标题的总高度和文字内边距。
    /// </summary>
    public double WeekHeaderHeight { get; set; }
    public Thickness WeekHeaderPadding { get; set; }

    /// <summary>
    /// Fullscreen 日期/月单元最小高度。
    /// </summary>
    public double FullCellMinHeight { get; set; }

    /// <summary>
    /// Fullscreen 日期/月单元的水平外边距（antd 全屏单元 margin: 0 marginXS/2）。
    /// </summary>
    public Thickness FullCellMargin { get; set; }

    /// <summary>
    /// Fullscreen 日期/月单元的内边距（antd 全屏单元 padding: paddingXS/2 paddingXS 0）。
    /// </summary>
    public Thickness FullCellPadding { get; set; }

    /// <summary>
    /// Fullscreen 日期/月单元顶部分隔线的线宽（antd 全屏单元 borderTop: lineWidthBold）。
    /// </summary>
    public Thickness FullCellBorderThickness { get; set; }

    /// <summary>
    /// Fullscreen 日期范围条默认高度。
    /// </summary>
    public double RangeBarHeight { get; set; }

    /// <summary>
    /// Fullscreen 同一单元内上下堆叠的日期范围条之间的间距。
    /// </summary>
    public double RangeBarGap { get; set; }

    /// <summary>
    /// Fullscreen 日期范围条标签的水平内边距。
    /// </summary>
    public Thickness RangeBarLabelPadding { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        var dateContentHeight = (EffectiveGlobalToken.FontHeightSM + EffectiveGlobalToken.UniformlyMarginXS) * 3 +
                                EffectiveGlobalToken.LineWidth * 2;

        FullBg = EffectiveGlobalToken.ColorBgContainer;
        FullPanelBg = EffectiveGlobalToken.ColorBgContainer;
        ItemActiveBg = EffectiveGlobalToken.ControlItemBgActive;
        YearControlWidth = 80;
        MonthControlWidth = 70;
        YearMonthCellWidth = EffectiveGlobalToken.ControlHeightLG * 1.5;
        MiniContentHeight = 256;
        WeekHeaderHeight = EffectiveGlobalToken.ControlHeightSM * 0.75 +
                           EffectiveGlobalToken.UniformlyPaddingXXS;
        WeekHeaderPadding = new Thickness(0, 0, EffectiveGlobalToken.UniformlyPaddingSM,
            EffectiveGlobalToken.UniformlyPaddingXXS);
        FullCellMinHeight = EffectiveGlobalToken.ControlHeightSM +
                            dateContentHeight +
                            EffectiveGlobalToken.UniformlyPaddingXS / 2 +
                            EffectiveGlobalToken.LineWidthBold;
        FullCellMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXS / 2, 0);
        FullCellPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, EffectiveGlobalToken.UniformlyPaddingXS / 2,
            EffectiveGlobalToken.UniformlyPaddingXS, 0);
        FullCellBorderThickness = new Thickness(0, EffectiveGlobalToken.LineWidthBold, 0, 0);
        RangeBarHeight = EffectiveGlobalToken.ControlHeightSM - EffectiveGlobalToken.UniformlyMarginXXS;
        RangeBarGap = EffectiveGlobalToken.UniformlyMarginXXS / 2;
        RangeBarLabelPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
    }
}
