using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SplitButtonToken : AbstractControlDesignToken
{
    public partial double GutterToFlyout { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GutterToFlyout = EffectiveGlobalToken.UniformlyMarginXXS;
    }
}
