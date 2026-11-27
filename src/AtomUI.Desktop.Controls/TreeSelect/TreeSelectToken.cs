using Avalonia.Media;
using Avalonia;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="TreeSelect" /> drop-down tree: node size and spacing, node hover and
/// selected colors, the filter highlight color, and the padding and minimum width of the drop-down frame.
/// Tokens of the selection input come from the base class.
/// </summary>
[ControlDesignToken]
public sealed partial class TreeSelectToken : AbstractSelectInputToken
{
    
    /// <summary>
    /// Minimum height of a tree item header in the drop-down. Also sets the size of the expand and collapse
    /// switcher.
    /// </summary>
    public partial double HeaderHeight { get; set; }
    /// <summary>
    /// Background color of a hovered tree node.
    /// </summary>
    public partial Color NodeHoverBg { get; set; }
    /// <summary>
    /// Background color of a selected tree node.
    /// </summary>
    public partial Color NodeSelectedBg { get; set; }

    /// <summary>
    /// The text color of a selected node in the dropdown tree, drawn on <see cref="NodeSelectedBg" />. Defaults to
    /// the global <c>ControlItemColorActive</c>.
    /// </summary>
    public partial Color NodeSelectedColor { get; set; }
    /// <summary>
    /// Margin around a tree item.
    /// </summary>
    public partial Thickness TreeItemMargin { get; set; }
    /// <summary>
    /// Padding inside a tree item header.
    /// </summary>
    public partial Thickness TreeItemHeaderPadding { get; set; }
    /// <summary>
    /// Margin around a tree item header.
    /// </summary>
    public partial Thickness TreeItemHeaderMargin { get; set; }
    /// <summary>
    /// Margin around the tree node's expand and collapse switcher.
    /// </summary>
    public partial Thickness TreeNodeSwitcherMargin { get; set; }
    /// <summary>
    /// Margin around the tree node icon.
    /// </summary>
    public partial Thickness TreeNodeIconMargin { get; set; }
    /// <summary>
    /// Foreground color of text that matches the filter.
    /// </summary>
    public partial Color FilterHighlightColor { get; set; }

    /// <summary>
    /// Padding inside the drop-down content area.
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// Minimum width of the drop-down.
    /// </summary>
    public partial double MinPopupWidth { get; set; }
    
}
