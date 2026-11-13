using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractStatisticValueToken : AbstractControlDesignToken
{
    public partial double ContentFontSize { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ContentFontSize = EffectiveGlobalToken.FontSizeHeading3;
    }
}
