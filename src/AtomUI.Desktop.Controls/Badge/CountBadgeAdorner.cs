using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

internal class CountBadgeAdorner : AbstractCountBadgeAdorner
{
    public static BadgeShadowTokenResourceKeys IndicatorShadowTokenResourceKeys { get; } =
        new(CountBadgeTokenKind.ShadowColor, CountBadgeTokenKind.ShadowSize);
}
