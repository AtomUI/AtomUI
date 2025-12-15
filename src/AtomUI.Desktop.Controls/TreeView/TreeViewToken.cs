using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class TreeViewToken : AbstractControlDesignToken
{
    public const string ID = "TreeView";

    public TreeViewToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 节点标题高度
    /// </summary>
    public double TitleHeight { get; set; }

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

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TitleHeight = SharedToken.ControlHeightSM;
        NodeHoverBg = SharedToken.ControlItemBgHover;
        NodeSelectedBg = SharedToken.ControlItemBgActive;

        DirectoryNodeSelectedColor = SharedToken.ColorTextLightSolid;
        DirectoryNodeSelectedBg = SharedToken.ColorPrimary;

        TreeItemMargin         = new Thickness(0, 0, 0, SharedToken.UniformlyPaddingXXS / 2);
        TreeItemHeaderPadding  = new Thickness(SharedToken.UniformlyPaddingXS, 0);
        TreeItemHeaderMargin   = new Thickness(SharedToken.UniformlyMarginXXS, 0, 0, 0);
        TreeNodeSwitcherMargin = new Thickness(0, 0, SharedToken.UniformlyPaddingXS / 2, 0);
        TreeNodeIconMargin     = new Thickness(SharedToken.UniformlyPaddingXS / 2, 0, 0, 0);

        DragIndicatorLineWidth = SharedToken.LineWidthFocus;
    }
}