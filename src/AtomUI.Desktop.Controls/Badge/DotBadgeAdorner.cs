using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

internal class DotBadgeAdorner : AbstractDotBadgeAdorner
{
    public static BadgeShadowTokenResourceKeys IndicatorShadowTokenResourceKeys { get; } =
        new(DotBadgeTokenKind.ShadowColor, DotBadgeTokenKind.ShadowSize);
}
