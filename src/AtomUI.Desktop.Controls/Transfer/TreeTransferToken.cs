using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design token of <see cref="TreeTransfer" />. Its only own value, <see cref="ListWidthLG" />, is reserved;
/// it has no rendering effect.
/// </summary>
[ControlDesignToken]
public sealed partial class TreeTransferToken : AbstractTransferToken
{
    /// <summary>
    /// Width of a tree transfer list in the large size. Reserved: <see cref="TreeTransfer" /> does not support
    /// pagination, so no rendering state reads this value.
    /// </summary>
    [ReservedToken("TreeTransfer does not support pagination; the pagination-only large list width has no reachable rendering state.")]
    public partial double ListWidthLG { get; set; }
}
