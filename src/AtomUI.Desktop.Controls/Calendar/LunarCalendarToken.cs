using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class LunarCalendarToken : AbstractControlDesignToken
{
    public double MiniContentHeight { get; set; }
    public double MiniDateCellSize { get; set; }
    public double MiniMonthCellWidth { get; set; }
    public double SecondaryTextFontSize { get; set; }
    public double SecondaryTextLineHeight { get; set; }
    public Color SecondaryTextColor { get; set; }
    public Color WeekendTextColor { get; set; }
    public Color HolidayMarkerColor { get; set; }
    public Color WorkdayMarkerColor { get; set; }
    public double FullCellMinHeight { get; set; }
    public double RangeBarTopOffset { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
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
