using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class DatePickerToken : AbstractControlDesignToken
{
    public Thickness PanelContentPadding { get; set; }
    public Thickness ButtonsPanelMargin { get; set; }
    public double RangePanelSpacing { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PanelContentPadding = EffectiveGlobalToken.PaddingSM;
        ButtonsPanelMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
        RangePanelSpacing = 20;
    }
}
