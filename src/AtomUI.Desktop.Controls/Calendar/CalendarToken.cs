using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CalendarToken : AbstractCalendarToken
{
    public partial double YearMonthCellWidth { get; set; }
    public partial double MiniContentHeight { get; set; }
    public partial double FullCellMinHeight { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
