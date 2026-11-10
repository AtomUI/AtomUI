using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class CalendarToken : AbstractCalendarToken
{
    public double YearMonthCellWidth { get; set; }
    public double MiniContentHeight { get; set; }
    public double FullCellMinHeight { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        YearMonthCellWidth = EffectiveGlobalToken.ControlHeightLG * 1.5;
        MiniContentHeight = 256;
        var dateContentHeight = (EffectiveGlobalToken.FontHeightSM + EffectiveGlobalToken.UniformlyMarginXS) * 3 +
                                EffectiveGlobalToken.LineWidth * 2;
        FullCellMinHeight = EffectiveGlobalToken.ControlHeightSM + dateContentHeight +
                            EffectiveGlobalToken.UniformlyPaddingXS / 2 + EffectiveGlobalToken.LineWidthBold;
    }
}
