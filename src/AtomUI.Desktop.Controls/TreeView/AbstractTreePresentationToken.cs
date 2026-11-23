using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTreePresentationToken : AbstractControlDesignToken
{
    /// <summary>Minimum height of a node header and the size of its expand and collapse button.</summary>
    public partial double HeaderHeight { get; set; }
    /// <summary>Background color of a hovered tree node.</summary>
    public partial Color NodeHoverBg { get; set; }
    /// <summary>Background color behind <see cref="NodeSelectedColor"/> for a selected tree node.</summary>
    public partial Color NodeSelectedBg { get; set; }

    /// <summary>
    /// The text colour of a selected node, drawn on <see cref="NodeSelectedBg"/>. Defaults to the global
    /// <c>ControlItemColorActive</c>, which is the body text colour unless a theme sets the contrast colour of a solid
    /// selected background (antd Tree <c>nodeSelectedColor</c>).
    /// </summary>
    public partial Color NodeSelectedColor { get; set; }
    /// <summary>Margin around the tree item header.</summary>
    public partial Thickness TreeItemMargin { get; set; }
    /// <summary>Padding inside the tree item header's content frame.</summary>
    public partial Thickness TreeItemHeaderPadding { get; set; }
    /// <summary>Margin around the tree item header's content frame.</summary>
    public partial Thickness TreeItemHeaderMargin { get; set; }
    /// <summary>Margin around a tree node's expand and collapse button.</summary>
    public partial Thickness TreeNodeSwitcherMargin { get; set; }
    /// <summary>Margin around the icon next to a tree node's label.</summary>
    public partial Thickness TreeNodeIconMargin { get; set; }
    /// <summary>Foreground color used to highlight matching text when the tree is filtered.</summary>
    public partial Color FilterHighlightColor { get; set; }
}
