using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ComboBoxToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }

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
    /// 列表项内间距
    /// </summary>
    public Thickness ItemPadding { get; set; }

    /// <summary>
    /// 列表项外边距
    /// </summary>
    public Thickness ItemMargin { get; set; }

    /// <summary>
    /// 下拉手柄悬浮颜色。
    /// </summary>
    public Color HandleHoverColor { get; set; }

    [ControlTokenDefault(nameof(PopupContentPadding))]
    private static Thickness DefaultForPopupContentPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXXS, context.Global.BorderRadiusLG.TopLeft / 2);

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
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(ItemHoverBgColor))]
    private static Color DefaultForItemHoverBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgTextHover;

    [ControlTokenDefault(nameof(ItemSelectedBgColor))]
    private static Color DefaultForItemSelectedBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(ItemPadding))]
    private static Thickness DefaultForItemPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingSM, 0);

    [ControlTokenDefault(nameof(ItemMargin))]
    private static Thickness DefaultForItemMargin(in ControlTokenDefaultContext context) =>
        new(0, 0.5);

    [ControlTokenDefault(nameof(HandleHoverColor))]
    private static Color DefaultForHandleHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;
}
