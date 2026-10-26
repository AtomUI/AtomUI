using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TreeViewToken : AbstractControlDesignToken, IControlTokenDefaults
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
    /// 兼容保留：当前默认主题未消费，未实现目录树专用交互；保持资源与配置往返。
    /// </summary>
    public Color DirectoryNodeSelectedColor { get; set; }

    /// <summary>
    /// 目录树节点选中背景色
    /// 兼容保留：当前默认主题未消费，未实现目录树专用交互；保持资源与配置往返。
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

    [ControlTokenDefault(nameof(HeaderHeight))]
    private static double DefaultForHeaderHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(NodeHoverBg))]
    private static Color DefaultForNodeHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgHover;

    [ControlTokenDefault(nameof(NodeSelectedBg))]
    private static Color DefaultForNodeSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(DirectoryNodeSelectedColor))]
    private static Color DefaultForDirectoryNodeSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(DirectoryNodeSelectedBg))]
    private static Color DefaultForDirectoryNodeSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(TreeItemMargin))]
    private static Thickness DefaultForTreeItemMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyPaddingXXS / 2);

    [ControlTokenDefault(nameof(TreeItemHeaderPadding))]
    private static Thickness DefaultForTreeItemHeaderPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(TreeItemHeaderMargin))]
    private static Thickness DefaultForTreeItemHeaderMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXXS, 0, 0, 0);

    [ControlTokenDefault(nameof(TreeNodeSwitcherMargin))]
    private static Thickness DefaultForTreeNodeSwitcherMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyPaddingXS / 2, 0);

    [ControlTokenDefault(nameof(TreeNodeIconMargin))]
    private static Thickness DefaultForTreeNodeIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXS / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(FilterHighlightColor))]
    private static Color DefaultForFilterHighlightColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DragIndicatorLineWidth))]
    private static double DefaultForDragIndicatorLineWidth(in ControlTokenDefaultContext context) =>
        context.Global.LineWidthFocus;
}
