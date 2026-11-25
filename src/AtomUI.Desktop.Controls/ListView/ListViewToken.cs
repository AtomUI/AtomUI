using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ListViewToken : AbstractListViewToken
{
    /// <summary>Padding around the label of a list group header.</summary>
    public partial Thickness GroupHeaderPadding { get; set; }
    /// <summary>Space around the indicator of a selected list item.</summary>
    public partial Thickness SelectedIndicatorMargin { get; set; }

    /// <summary>
    /// Colour of the check mark on a selected item.
    /// </summary>
    public partial Color ItemSelectedIndicatorColor { get; set; }
    /// <summary>Space around the pagination controls of the list.</summary>
    public partial Thickness PaginationMargin { get; set; }
}
