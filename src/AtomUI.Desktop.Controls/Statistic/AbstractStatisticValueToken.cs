using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractStatisticValueToken : AbstractControlDesignToken
{
    public double ContentFontSize { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ContentFontSize = EffectiveGlobalToken.FontSizeHeading3;
    }
}
