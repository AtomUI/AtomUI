using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTimePickerToken : AbstractTimeViewToken
{
    public partial Thickness ButtonsMargin { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ButtonsMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
    }
}
