using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TreeTransferToken : AbstractTransferToken
{
    [ReservedToken("TreeTransfer does not support pagination; the pagination-only large list width has no reachable rendering state.")]
    public partial double ListWidthLG { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ListWidthLG = 250;
    }
}
