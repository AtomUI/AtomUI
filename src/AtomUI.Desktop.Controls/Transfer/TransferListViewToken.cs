using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design token of the <see cref="TransferListView" /> of a transfer. It adds the margin of the pagination
/// controls to the values of the list view token.
/// </summary>
[ControlDesignToken]
public sealed partial class TransferListViewToken : AbstractListViewToken
{
    /// <summary>
    /// Margin around the pagination controls above or below the transfer list.
    /// </summary>
    public partial Thickness PaginationMargin { get; set; }
}
