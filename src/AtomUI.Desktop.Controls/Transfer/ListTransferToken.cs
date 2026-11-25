using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ListTransferToken : AbstractTransferToken
{
    /// <summary>Width of each list in the large list transfer layout.</summary>
    public partial double ListWidthLG { get; set; }
}
