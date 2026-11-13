using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CardTabStripToken : AbstractCardTabToken
{
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(CardTabStripTokenKind.CardSize))
        {
            CardSize = EffectiveGlobalToken.ControlHeightLG;
        }
        CardPadding = CalculateCardPadding(CardSize);
    }
}
