using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TreeTransferToken : AbstractTransferToken
{
    [ReservedToken("TreeTransfer does not support pagination; the pagination-only large list width has no reachable rendering state.")]
    public double ListWidthLG { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ListWidthLG = 250;
    }
}
