using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TransferListViewToken : AbstractListViewToken
{
    /// <summary>Margin around the pagination controls above or below the transfer list.</summary>
    public partial Thickness PaginationMargin { get; set; }
}
