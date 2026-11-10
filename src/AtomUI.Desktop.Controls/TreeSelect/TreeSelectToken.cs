using Avalonia.Media;
using Avalonia;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TreeSelectToken : AbstractSelectInputToken
{
    
    /// <summary>
    /// 最小的弹窗的宽度
    /// </summary>
    public double HeaderHeight { get; set; }
    public Color NodeHoverBg { get; set; }
    public Color NodeSelectedBg { get; set; }
    public Thickness TreeItemMargin { get; set; }
    public Thickness TreeItemHeaderPadding { get; set; }
    public Thickness TreeItemHeaderMargin { get; set; }
    public Thickness TreeNodeSwitcherMargin { get; set; }
    public Thickness TreeNodeIconMargin { get; set; }
    public Color FilterHighlightColor { get; set; }

    public Thickness PopupContentPadding { get; set; }

    public double MinPopupWidth { get; set; }

    public TreeSelectToken()

    {
    }

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
        FilterHighlightColor = EffectiveGlobalToken.ColorPrimary;
        MinPopupWidth = 300;
        PopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS / 2);
    }
    
}
