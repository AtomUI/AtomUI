using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTreePresentationToken : AbstractControlDesignToken
{
    public partial double HeaderHeight { get; set; }
    public partial Color NodeHoverBg { get; set; }
    public partial Color NodeSelectedBg { get; set; }
    public partial Thickness TreeItemMargin { get; set; }
    public partial Thickness TreeItemHeaderPadding { get; set; }
    public partial Thickness TreeItemHeaderMargin { get; set; }
    public partial Thickness TreeNodeSwitcherMargin { get; set; }
    public partial Thickness TreeNodeIconMargin { get; set; }
    public partial Color FilterHighlightColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var presentation = TreePresentationTokenValues.Calculate(EffectiveGlobalToken);
        HeaderHeight = presentation.HeaderHeight;
        NodeHoverBg = presentation.NodeHoverBg;
        NodeSelectedBg = presentation.NodeSelectedBg;
        TreeItemMargin = presentation.TreeItemMargin;
        TreeItemHeaderPadding = presentation.TreeItemHeaderPadding;
        TreeItemHeaderMargin = presentation.TreeItemHeaderMargin;
        TreeNodeSwitcherMargin = presentation.TreeNodeSwitcherMargin;
        TreeNodeIconMargin = presentation.TreeNodeIconMargin;
        FilterHighlightColor = presentation.FilterHighlightColor;
    }
}
