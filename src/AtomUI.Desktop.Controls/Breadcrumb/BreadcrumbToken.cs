using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class BreadcrumbToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 面包屑项文字颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 最后一项文字颜色
    /// </summary>
    public Color LastItemColor { get; set; }

    /// <summary>
    /// 链接文字颜色
    /// </summary>
    public Color LinkColor { get; set; }

    /// <summary>
    /// 链接文字悬浮颜色
    /// </summary>
    public Color LinkHoverColor { get; set; }

    /// <summary>
    /// 链接文字悬浮背景颜色
    /// </summary>
    public Color LinkHoverBgColor { get; set; }

    /// <summary>
    /// BreadcrumbItem外面Border的Padding
    /// </summary>
    public Thickness BreadcrumbItemContentPadding { get; set; }

    /// <summary>
    /// 分隔符颜色
    /// </summary>
    public Color SeparatorColor { get; set; }

    /// <summary>
    /// 分隔符外间距
    /// </summary>
    public Thickness SeparatorMargin { get; set; }

    public BreadcrumbToken()

    {
    }

    [ControlTokenDefault(nameof(ItemColor))]
    private static Color DefaultForItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(LastItemColor))]
    private static Color DefaultForLastItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(LinkColor))]
    private static Color DefaultForLinkColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(LinkHoverColor))]
    private static Color DefaultForLinkHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(LinkHoverBgColor))]
    private static Color DefaultForLinkHoverBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgTextHover;

    [ControlTokenDefault(nameof(SeparatorColor))]
    private static Color DefaultForSeparatorColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(SeparatorMargin))]
    private static Thickness DefaultForSeparatorMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginXXS, 0);

    [ControlTokenDefault(nameof(BreadcrumbItemContentPadding))]
    private static Thickness DefaultForBreadcrumbItemContentPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXXS);
    
}
