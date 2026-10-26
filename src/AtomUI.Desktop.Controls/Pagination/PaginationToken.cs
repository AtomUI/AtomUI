using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class PaginationToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 页码选项背景色
    /// Background color of Pagination item
    /// </summary>
    public Color ItemBg { get; set; }
    
    /// <summary>
    /// 页码尺寸
    /// Size of Pagination item
    /// </summary>
    public double ItemSize { get; set; }
        
    /// <summary>
    /// 页码激活态背景色
    /// Background color of active Pagination item
    /// </summary>
    public Color ItemActiveBg { get; set; }
    
    /// <summary>
    /// 小号页码尺寸
    /// Size of small Pagination item
    /// </summary>
    public double ItemSizeSM { get; set; }
    
    /// <summary>
    /// 页码链接背景色
    /// Background color of Pagination item link
    /// 兼容保留：默认主题未消费，当前未提供 bordered 导航变体；保留资源与配置往返。
    /// </summary>
    public Color ItemLinkBg { get; set; }
    
    /// <summary>
    /// 页码激活态禁用状态背景色
    /// Background color of disabled active Pagination item
    /// </summary>
    public Color ItemActiveBgDisabled { get; set; }
    
    /// <summary>
    /// 页码激活态禁用状态文字颜色
    /// Text color of disabled active Pagination item
    /// </summary>
    public Color ItemActiveColorDisabled { get; set; }
    
    /// <summary>
    /// 输入框背景色
    /// Background color of input
    /// 用于简洁分页编辑输入的启用 Outlined 绘制帧。
    /// </summary>
    public Color ItemInputBg { get; set; }
    
    /// <summary>
    /// 输入框轮廓偏移量
    /// Outline offset of input
    /// 兼容保留：未定义桌面 Thickness 偏移语义，焦点阴影继续使用有效 Global。
    /// </summary>
    public Thickness InputOutlineOffset { get; set; }
    
    /// <summary>
    /// 分页布局间距
    /// Horizontal layout spacing
    /// </summary>
    public double PaginationLayoutSpacing { get; set; }
    
    /// <summary>
    /// 分页布局迷你间距
    /// Horizontal layout mini spacing
    /// </summary>
    public double PaginationLayoutMiniSpacing { get; set; }
    
    /// <summary>
    /// 快速跳转输入框宽度
    /// Width of quick jumper input
    /// </summary>
    public double PaginationQuickJumperInputWidth { get; set; }
    
    /// <summary>
    /// 迷你快速跳转输入框宽度
    /// Width of mini quick jumper input
    /// </summary>
    public double PaginationMiniQuickJumperInputWidth { get; set; }
    
    /// <summary>
    /// 页码横向内边距
    /// Horizontal padding of Pagination item
    /// </summary>
    public Thickness PaginationItemPaddingInline { get; set; }
    
    public PaginationToken()

    {
    }
    
    [ControlTokenDefault(nameof(ItemBg))]
    private static Color DefaultForItemBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ItemSize))]
    private static double DefaultForItemSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(ItemSizeSM))]
    private static double DefaultForItemSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(ItemActiveBg))]
    private static Color DefaultForItemActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ItemLinkBg))]
    private static Color DefaultForItemLinkBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ItemActiveColorDisabled))]
    private static Color DefaultForItemActiveColorDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(ItemActiveBgDisabled))]
    private static Color DefaultForItemActiveBgDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActiveDisabled;

    [ControlTokenDefault(nameof(ItemInputBg))]
    private static Color DefaultForItemInputBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(InputOutlineOffset))]
    private static Thickness DefaultForInputOutlineOffset(in ControlTokenDefaultContext context) => new(0);

    [ControlTokenDefault(nameof(PaginationLayoutSpacing))]
    private static double DefaultForPaginationLayoutSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(PaginationLayoutMiniSpacing))]
    private static double DefaultForPaginationLayoutMiniSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXXS / 2;

    [ControlTokenDefault(nameof(PaginationQuickJumperInputWidth))]
    private static double DefaultForPaginationQuickJumperInputWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.25;

    [ControlTokenDefault(nameof(PaginationMiniQuickJumperInputWidth))]
    private static double DefaultForPaginationMiniQuickJumperInputWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.1;

    [ControlTokenDefault(nameof(PaginationItemPaddingInline))]
    private static Thickness DefaultForPaginationItemPaddingInline(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginXXS * 1.5);
    
}
