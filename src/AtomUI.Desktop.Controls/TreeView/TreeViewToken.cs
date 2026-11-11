using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TreeViewToken : AbstractControlDesignToken
{
    
    public TreeViewToken()

    {
    }

    /// <summary>
    /// 节点标题高度
    /// </summary>
    public double HeaderHeight { get; set; }

    /// <summary>
    /// 节点悬浮态背景色
    /// </summary>
    public Color NodeHoverBg { get; set; }

    /// <summary>
    /// 节点选中态背景色
    /// </summary>
    public Color NodeSelectedBg { get; set; }

    /// <summary>
    /// 目录树节点选中文字颜色
    /// </summary>
    public Color DirectoryNodeSelectedColor { get; set; }

    /// <summary>
    /// 目录树节点选中背景色
    /// </summary>
    public Color DirectoryNodeSelectedBg { get; set; }

    /// <summary>
    /// 树节点的外边距
    /// </summary>
    public Thickness TreeItemMargin { get; set; }

    /// <summary>
    /// 树节点标题的内间距
    /// </summary>
    public Thickness TreeItemHeaderPadding { get; set; }

    /// <summary>
    /// 树节点标题的外间距
    /// </summary>
    public Thickness TreeItemHeaderMargin { get; set; }

    /// <summary>
    /// 树节点展开按钮外边距
    /// </summary>
    public Thickness TreeNodeSwitcherMargin { get; set; }

    /// <summary>
    /// 树节点 Icon 边距
    /// </summary>
    public Thickness TreeNodeIconMargin { get; set; }

    /// <summary>
    /// 拖动指示器
    /// </summary>
    public double DragIndicatorLineWidth { get; set; }
    
    /// <summary>
    /// 过滤高亮颜色
    /// </summary>
    public Color FilterHighlightColor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderHeight   = EffectiveGlobalToken.ControlHeightSM;
        NodeHoverBg    = EffectiveGlobalToken.ControlItemBgHover;
        NodeSelectedBg = EffectiveGlobalToken.ControlItemBgActive;

        DirectoryNodeSelectedColor = EffectiveGlobalToken.ColorTextLightSolid;
        DirectoryNodeSelectedBg = EffectiveGlobalToken.ColorPrimary;

        TreeItemMargin         = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyPaddingXXS / 2);
        TreeItemHeaderPadding  = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
        TreeItemHeaderMargin   = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
        TreeNodeSwitcherMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyPaddingXS / 2, 0);
        TreeNodeIconMargin     = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS / 2, 0, 0, 0);
        FilterHighlightColor   = EffectiveGlobalToken.ColorError;
        DragIndicatorLineWidth = EffectiveGlobalToken.LineWidthBold;
    }
    
}
