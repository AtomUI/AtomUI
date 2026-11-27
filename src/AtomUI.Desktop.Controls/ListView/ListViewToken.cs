using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design tokens of <see cref="ListView" /> that extend <see cref="AbstractListViewToken" />: the padding
/// of a group heading, the margin and color of the selected-item indicator, and the margin of the pagers.
/// </summary>
[ControlDesignToken]
public sealed partial class ListViewToken : AbstractListViewToken
{
    /// <summary>
    /// Padding around the label of a list group header.
    /// </summary>
    public partial Thickness GroupHeaderPadding { get; set; }
    /// <summary>
    /// Space around the indicator of a selected list item.
    /// </summary>
    public partial Thickness SelectedIndicatorMargin { get; set; }

    /// <summary>
    /// Color of the check mark on a selected item.
    /// </summary>
    public partial Color ItemSelectedIndicatorColor { get; set; }
    /// <summary>
    /// Space around the pagination controls of the list.
    /// </summary>
    public partial Thickness PaginationMargin { get; set; }
}
