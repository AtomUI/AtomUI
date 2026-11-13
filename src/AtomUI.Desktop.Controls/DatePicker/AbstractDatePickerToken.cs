using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractDatePickerToken : AbstractTimeViewToken
{
    public partial Thickness HeaderMargin { get; set; }
    public partial Thickness PanelContentPadding { get; set; }
    public partial Thickness ButtonsPanelMargin { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderMargin = new Thickness(0, 0, 0, 3);
        PanelContentPadding = EffectiveGlobalToken.PaddingSM;
        ButtonsPanelMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
    }
}
