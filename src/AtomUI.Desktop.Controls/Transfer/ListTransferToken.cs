using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design token of <see cref="ListTransfer" />. It adds the list width of the large size to the values of
/// <see cref="AbstractTransferToken" />.
/// </summary>
[ControlDesignToken]
public sealed partial class ListTransferToken : AbstractTransferToken
{
    /// <summary>
    /// Width of each list in the large list transfer layout.
    /// </summary>
    public partial double ListWidthLG { get; set; }
}
