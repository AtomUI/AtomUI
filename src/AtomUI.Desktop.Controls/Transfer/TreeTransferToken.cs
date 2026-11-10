using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TreeTransferToken : AbstractControlDesignToken
{
    public double ListWidth { get; set; }

    [ReservedToken("TreeTransfer does not support pagination; the pagination-only large list width has no reachable rendering state.")]
    public double ListWidthLG { get; set; }

    public double ListHeight { get; set; }

    public double HeaderHeight { get; set; }

    public Thickness HeaderPadding { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        ListWidth    = 180;
        ListWidthLG  = 250;
        ListHeight   = 200;
        HeaderHeight = EffectiveGlobalToken.ControlHeightLG;
        HeaderPadding = new Thickness(
            EffectiveGlobalToken.UniformlyPaddingSM,
            Math.Ceiling((EffectiveGlobalToken.ControlHeightLG -
                          EffectiveGlobalToken.LineWidth -
                          EffectiveGlobalToken.FontHeight) / 2));
    }
}
