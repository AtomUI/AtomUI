using Avalonia.Media;
using Avalonia;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TreeSelectToken : AbstractSelectInputToken
{
    
    /// <summary>
    /// 最小的弹窗的宽度
    /// </summary>
    public partial double HeaderHeight { get; set; }
    public partial Color NodeHoverBg { get; set; }
    public partial Color NodeSelectedBg { get; set; }

    /// <summary>
    /// The text colour of a selected node in the dropdown tree, drawn on <see cref="NodeSelectedBg"/>. Defaults to
    /// the body text colour.
    /// </summary>
    public partial Color NodeSelectedColor { get; set; }
    public partial Thickness TreeItemMargin { get; set; }
    public partial Thickness TreeItemHeaderPadding { get; set; }
    public partial Thickness TreeItemHeaderMargin { get; set; }
    public partial Thickness TreeNodeSwitcherMargin { get; set; }
    public partial Thickness TreeNodeIconMargin { get; set; }
    public partial Color FilterHighlightColor { get; set; }

    public partial Thickness PopupContentPadding { get; set; }

    public partial double MinPopupWidth { get; set; }

    public TreeSelectToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var presentation = TreePresentationTokenValues.Calculate(EffectiveGlobalToken);
        HeaderHeight = presentation.HeaderHeight;
        NodeHoverBg = presentation.NodeHoverBg;
        NodeSelectedBg = presentation.NodeSelectedBg;
        NodeSelectedColor = presentation.NodeSelectedColor;
        TreeItemMargin = presentation.TreeItemMargin;
        TreeItemHeaderPadding = presentation.TreeItemHeaderPadding;
        TreeItemHeaderMargin = presentation.TreeItemHeaderMargin;
        TreeNodeSwitcherMargin = presentation.TreeNodeSwitcherMargin;
        TreeNodeIconMargin = presentation.TreeNodeIconMargin;
        FilterHighlightColor = EffectiveGlobalToken.ColorPrimary;
        MinPopupWidth = 300;
        PopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS / 2);
    }
    
}
