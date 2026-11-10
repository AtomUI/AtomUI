using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractDatePickerToken : AbstractTimeViewToken
{
    public Thickness HeaderMargin { get; set; }
    public Thickness PanelContentPadding { get; set; }
    public Thickness ButtonsPanelMargin { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderMargin = new Thickness(0, 0, 0, 3);
        PanelContentPadding = EffectiveGlobalToken.PaddingSM;
        ButtonsPanelMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
    }
}
