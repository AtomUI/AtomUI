using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractTimePickerToken : AbstractTimeViewToken
{
    public Thickness ButtonsMargin { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ButtonsMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
    }
}
