using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class CalendarToken : AbstractCalendarToken
{
    public Color ItemActiveBg { get; set; }
    public double YearMonthCellWidth { get; set; }
    public double MiniContentHeight { get; set; }
    public double FullCellMinHeight { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemActiveBg = EffectiveGlobalToken.ControlItemBgActive;
        YearMonthCellWidth = EffectiveGlobalToken.ControlHeightLG * 1.5;
        MiniContentHeight = 256;
        var dateContentHeight = (EffectiveGlobalToken.FontHeightSM + EffectiveGlobalToken.UniformlyMarginXS) * 3 +
                                EffectiveGlobalToken.LineWidth * 2;
        FullCellMinHeight = EffectiveGlobalToken.ControlHeightSM + dateContentHeight +
                            EffectiveGlobalToken.UniformlyPaddingXS / 2 + EffectiveGlobalToken.LineWidthBold;
    }
}
