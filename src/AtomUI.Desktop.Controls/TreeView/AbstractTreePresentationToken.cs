using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractTreePresentationToken : AbstractControlDesignToken
{
    public double HeaderHeight { get; set; }
    public Color NodeHoverBg { get; set; }
    public Color NodeSelectedBg { get; set; }
    public Thickness TreeItemMargin { get; set; }
    public Thickness TreeItemHeaderPadding { get; set; }
    public Thickness TreeItemHeaderMargin { get; set; }
    public Thickness TreeNodeSwitcherMargin { get; set; }
    public Thickness TreeNodeIconMargin { get; set; }
    public Color FilterHighlightColor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
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
