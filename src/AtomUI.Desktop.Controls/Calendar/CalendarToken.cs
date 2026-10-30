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
    /// Fullscreen 日期范围条默认高度。
    /// </summary>
    public double RangeBarHeight { get; set; }

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
        RangeBarHeight = EffectiveGlobalToken.ControlHeightSM - EffectiveGlobalToken.UniformlyMarginXXS;
    }
}
