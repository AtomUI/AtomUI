using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class LunarCalendarToken : AbstractCalendarToken
{
    public partial double MiniContentHeight { get; set; }
    public partial double MiniDateCellSize { get; set; }
    public partial double MiniMonthCellWidth { get; set; }
    public partial double SecondaryTextFontSize { get; set; }
    public partial double SecondaryTextLineHeight { get; set; }
    public partial Color SecondaryTextColor { get; set; }
    public partial Color WeekendTextColor { get; set; }
    public partial Color HolidayMarkerColor { get; set; }
    public partial Color WorkdayMarkerColor { get; set; }

    /// <summary>
    /// 节假日/调休标记圆点的直径。
    /// </summary>
    public partial double MarkerSize { get; set; }

    /// <summary>
    /// 节假日/调休标记圆点与次要文字之间的外边距。
    /// </summary>
    public partial Thickness MarkerMargin { get; set; }

    /// <summary>
    /// 节假日/调休标记圆点的圆角，默认为直径的一半。
    /// </summary>
    public partial CornerRadius MarkerCornerRadius { get; set; }
    public partial double FullCellMinHeight { get; set; }
    public partial double RangeBarTopOffset { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        if (!IsExplicit(LunarCalendarTokenKind.MiniDateCellSize))
        {
            MiniDateCellSize = EffectiveGlobalToken.ControlHeightLG;
        }

        MiniContentHeight = EffectiveGlobalToken.FontHeightSM +
                            (MiniDateCellSize + EffectiveGlobalToken.UniformlyMarginXS) * 6;
        MiniMonthCellWidth = EffectiveGlobalToken.ControlHeightLG * 2;
        SecondaryTextFontSize = EffectiveGlobalToken.FontSizeSM;

        if (!IsExplicit(LunarCalendarTokenKind.SecondaryTextLineHeight))
        {
            SecondaryTextLineHeight = EffectiveGlobalToken.FontHeightSM;
        }

        SecondaryTextColor = EffectiveGlobalToken.ColorTextTertiary;
        WeekendTextColor = EffectiveGlobalToken.ColorError;
        HolidayMarkerColor = EffectiveGlobalToken.ColorError;
        WorkdayMarkerColor = EffectiveGlobalToken.ColorTextTertiary;
        if (!IsExplicit(LunarCalendarTokenKind.MarkerSize))
        {
            MarkerSize = EffectiveGlobalToken.UniformlyMarginXXS;
        }

        MarkerMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXXS, 0);
        MarkerCornerRadius = new CornerRadius(MarkerSize / 2);

        var calendarContentHeight =
            (EffectiveGlobalToken.FontHeightSM + EffectiveGlobalToken.UniformlyMarginXS) * 3 +
            EffectiveGlobalToken.LineWidth * 2;
        FullCellMinHeight = EffectiveGlobalToken.ControlHeightSM +
                            calendarContentHeight +
                            EffectiveGlobalToken.UniformlyPaddingXS / 2 +
                            EffectiveGlobalToken.LineWidthBold +
                            SecondaryTextLineHeight +
                            EffectiveGlobalToken.UniformlyMarginXXS;
        RangeBarTopOffset = EffectiveGlobalToken.ControlHeightSM +
                            SecondaryTextLineHeight +
                            EffectiveGlobalToken.UniformlyMarginXXS;
    }
}
