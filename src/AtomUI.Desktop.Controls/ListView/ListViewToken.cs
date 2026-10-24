using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ListViewToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// List 内边距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 列表项文字颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 列表项文字悬浮颜色
    /// </summary>
    public Color ItemHoverColor { get; set; }

    /// <summary>
    /// 列表项文字选中颜色
    /// </summary>
    public Color ItemSelectedColor { get; set; }

    /// <summary>
    /// 列表项文字禁用颜色
    /// </summary>
    public Color ItemDisabledColor { get; set; }

    /// <summary>
    /// 列表项背景色
    /// </summary>
    public Color ItemBgColor { get; set; }

    /// <summary>
    /// 列表项悬浮态背景色
    /// </summary>
    public Color ItemHoverBgColor { get; set; }

    /// <summary>
    /// 列表项选中背景色
    /// </summary>
    public Color ItemSelectedBgColor { get; set; }

    /// <summary>
    /// 列表项小号内间距
    /// </summary>
    public Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// 列表项内间距
    /// </summary>
    public Thickness ItemPadding { get; set; }

    /// <summary>
    /// 列表项大号内间距
    /// </summary>
    public Thickness ItemPaddingLG { get; set; }

    /// <summary>
    /// 列表项外边距
    /// </summary>
    public Thickness ItemMargin { get; set; }

    /// <summary>
    /// 分页器的外边距
    /// </summary>
    public Thickness PaginationMargin { get; set; }
    
    /// <summary>
    /// 分组标题的颜色
    /// </summary>
    public Color GroupHeaderColor { get; set; }

    /// <summary>
    /// 分组标题的内间距
    /// </summary>
    public Thickness GroupHeaderPadding { get; set; }

    /// <summary>
    /// 列表项选中标记的外间距
    /// </summary>
    public Thickness SelectedIndicatorMargin { get; set; }

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0);

    [ControlTokenDefault(nameof(ItemColor))]
    private static Color DefaultForItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextSecondary;

    [ControlTokenDefault(nameof(ItemHoverColor))]
    private static Color DefaultForItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextSecondary;

    [ControlTokenDefault(nameof(ItemSelectedColor))]
    private static Color DefaultForItemSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(ItemDisabledColor))]
    private static Color DefaultForItemDisabledColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(ItemBgColor))]
    private static Color DefaultForItemBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTransparent;

    [ControlTokenDefault(nameof(ItemHoverBgColor))]
    private static Color DefaultForItemHoverBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgTextHover;

    [ControlTokenDefault(nameof(ItemSelectedBgColor))]
    private static Color DefaultForItemSelectedBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(ItemPaddingSM))]
    private static Thickness DefaultForItemPaddingSM(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(ItemPadding))]
    private static Thickness DefaultForItemPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingSM, 0);

    [ControlTokenDefault(nameof(ItemPaddingLG))]
    private static Thickness DefaultForItemPaddingLG(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPadding, 0);

    [ControlTokenDefault(nameof(ItemMargin))]
    private static Thickness DefaultForItemMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0);

    [ControlTokenDefault(nameof(PaginationMargin))]
    private static Thickness DefaultForPaginationMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(GroupHeaderColor))]
    private static Color DefaultForGroupHeaderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(GroupHeaderPadding))]
    private static Thickness DefaultForGroupHeaderPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingSM, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(SelectedIndicatorMargin))]
    private static Thickness DefaultForSelectedIndicatorMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXXS, 0, 0, 0);
}
