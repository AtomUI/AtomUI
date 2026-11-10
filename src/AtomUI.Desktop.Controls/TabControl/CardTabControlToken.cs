using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class CardTabControlToken : AbstractCardTabToken
{
    public double TabAndContentGutter { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(CardTabControlTokenKind.CardSize))
        {
            CardSize = EffectiveGlobalToken.ControlHeightLG;
        }
        CardPadding = CalculateCardPadding(CardSize);
        TabAndContentGutter = EffectiveGlobalToken.UniformlyMarginSM;
    }
}
