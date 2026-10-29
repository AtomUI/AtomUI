using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class CascaderToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 节点标题高度
    /// </summary>
    public double HeaderHeight { get; set; }
    
    /// <summary>
    /// 选择器宽度
    /// Width of Cascader
    /// </summary>
    public double ControlWidth { get; set; }
    
    /// <summary>
    /// 选项宽度
    /// Width of item
    /// </summary>
    public double ControlItemWidth { get; set; }
    
    /// <summary>
    /// 下拉菜单高度
    /// Height of dropdown
    /// </summary>
    public double DropdownHeight { get; set; }
    
    /// <summary>
    /// 选项选中时背景色
    /// Background color of selected item
    /// </summary>
    public Color OptionSelectedBg { get; set; }
    
    /// <summary>
    /// 选项 hover 时背景色
    /// Background color of hover item
    /// </summary>
    public Color OptionHoverBg { get; set; }
    
    /// <summary>
    /// 选项选中时文本颜色
    /// Text color when option is selected
    /// </summary>
    public Color OptionSelectedColor { get; set; }
    
    /// <summary>
    /// 选项选中时字重
    /// Font weight of selected item
    /// </summary>
    public FontWeight OptionSelectedFontWeight { get; set; }
    
    /// <summary>
    /// 选项内间距
    /// Padding of menu item
    /// </summary>
    public Thickness OptionPadding { get; set; }
    
    /// <summary>
    /// 选项菜单（单列）内间距
    /// Padding of menu item (single column)
    /// </summary>
    public Thickness MenuPadding { get; set; }
    
    /// <summary>
    /// 过滤高亮颜色
    /// </summary>
    public Color FilterHighlightColor { get; set; }
    
    /// <summary>
    /// Item 头的元素间距
    /// </summary>
    public double ItemHeaderSpacing { get; set; }

    [ControlTokenDefault(nameof(HeaderHeight))]
    private static double DefaultForHeaderHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(ControlWidth))]
    private static double DefaultForControlWidth(in ControlTokenDefaultContext context) => 184;

    [ControlTokenDefault(nameof(ControlItemWidth))]
    private static double DefaultForControlItemWidth(in ControlTokenDefaultContext context) => 111;

    [ControlTokenDefault(nameof(DropdownHeight))]
    private static double DefaultForDropdownHeight(in ControlTokenDefaultContext context) => 180;

    [ControlTokenDefault(nameof(OptionSelectedBg))]
    private static Color DefaultForOptionSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(OptionHoverBg))]
    private static Color DefaultForOptionHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgHover;

    [ControlTokenDefault(nameof(OptionSelectedColor))]
    private static Color DefaultForOptionSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(OptionSelectedFontWeight))]
    private static FontWeight DefaultForOptionSelectedFontWeight(in ControlTokenDefaultContext context) =>
        context.Global.FontWeightStrong;

    [ControlTokenDefault(nameof(OptionPadding))]
    private static Thickness DefaultForOptionPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingSM,
            Math.Round((context.Global.ControlHeight - context.Global.FontHeight) / 2));

    [ControlTokenDefault(nameof(MenuPadding))]
    private static Thickness DefaultForMenuPadding(in ControlTokenDefaultContext context) =>
        context.Global.PaddingXXS;

    [ControlTokenDefault(nameof(FilterHighlightColor))]
    private static Color DefaultForFilterHighlightColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ItemHeaderSpacing))]
    private static double DefaultForItemHeaderSpacing(in ControlTokenDefaultContext context) =>
        context.Global.SpacingXXS;
    
}
