using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SelectToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 多选标签背景色
    /// Background color of multiple tag
    /// </summary>
    public Color MultipleItemBg { get; set; }

    /// <summary>
    /// 多选标签高度
    /// Height of multiple tag
    /// </summary>
    public double MultipleItemHeight { get; set; }

    /// <summary>
    /// 小号多选标签高度
    /// Height of multiple tag with small size
    /// </summary>
    public double MultipleItemHeightSM { get; set; }

    /// <summary>
    /// 大号多选标签高度
    /// Height of multiple tag with large size
    /// </summary>
    public double MultipleItemHeightLG { get; set; }

    /// <summary>
    /// 多选框禁用背景
    /// Background color of multiple selector when disabled
    /// </summary>
    public Color MultipleSelectorBgDisabled { get; set; }

    /// <summary>
    /// 多选标签禁用文本颜色
    /// Text color of multiple tag when disabled
    /// </summary>
    public Color MultipleItemColorDisabled { get; set; }

    /// <summary>
    /// 选项选中时文本颜色
    /// Text color when option is selected
    /// </summary>
    public Color OptionSelectedColor { get; set; }

    /// <summary>
    /// 选项选中时文本字重
    /// Font weight when option is selected
    /// </summary>
    public FontWeight OptionSelectedFontWeight { get; set; }

    /// <summary>
    /// 选项选中时背景色
    /// Font weight when option is selected
    /// </summary>
    public Color OptionSelectedBg { get; set; }

    /// <summary>
    /// 选项激活态时背景色
    /// Background color when option is active
    /// </summary>
    public Color OptionActiveBg { get; set; }

    /// <summary>
    /// 选项内间距
    /// Padding of option
    /// </summary>
    public Thickness OptionPadding { get; set; }

    /// <summary>
    /// 选项字体大小
    /// Font size of option
    /// </summary>
    public double OptionFontSize { get; set; }

    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public double OptionHeight { get; set; }

    /// <summary>
    /// Historical compatibility resource. No built-in affix layout consumer;
    /// retain the default and resource/config contract for external styles.
    /// </summary>
    public Thickness SelectAffixPadding { get; set; }

    /// <summary>
    /// Historical compatibility resource. Built-in tag spacing uses Global.SpacingXXS;
    /// this value remains available without adding implicit tag margins.
    /// </summary>
    public Thickness FixedItemMargin { get; set; }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// 多选模式下的输入框内边距
    /// </summary>
    public Thickness MultiModePadding { get; set; }

    /// <summary>
    /// 多选模式下的小号输入框内边距
    /// </summary>
    public Thickness MultiModePaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public Thickness MultiModePaddingLG { get; set; }

    /// <summary>
    /// 输入框内边距
    /// </summary>
    public Thickness SingleModePadding { get; set; }

    /// <summary>
    /// 小号输入框内边距
    /// </summary>
    public Thickness SingleModePaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public Thickness SingleModePaddingLG { get; set; }

    /// <summary>
    /// 多选模式下前缀（ContentLeftAddOn）的额外左缩进，
    /// 补偿多选内边距与单选水平内边距的差值，使前缀与单选模式左对齐
    /// </summary>
    public Thickness MultiModePrefixIndent { get; set; }

    /// <summary>
    /// 多选模式下的小号前缀额外左缩进
    /// </summary>
    public Thickness MultiModePrefixIndentSM { get; set; }

    /// <summary>
    /// 多选模式下的大号前缀额外左缩进
    /// </summary>
    public Thickness MultiModePrefixIndentLG { get; set; }

    [ControlTokenDefault(nameof(MultipleItemBg))]
    private static Color DefaultForMultipleItemBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillSecondary;

    [ControlTokenDefault(nameof(MultipleItemHeight))]
    private static double DefaultForMultipleItemHeight(in ControlTokenDefaultContext context) =>
        Math.Min(context.Global.ControlHeight - context.Global.UniformlyPaddingXXS * 2,
            context.Global.ControlHeight - context.Global.LineWidth * 2) - 2;

    [ControlTokenDefault(nameof(MultipleItemHeightSM))]
    private static double DefaultForMultipleItemHeightSM(in ControlTokenDefaultContext context) =>
        Math.Min(context.Global.ControlHeightSM - context.Global.UniformlyPaddingXXS * 2,
            context.Global.ControlHeightSM - context.Global.LineWidth * 2) + 4;

    [ControlTokenDefault(nameof(MultipleItemHeightLG))]
    private static double DefaultForMultipleItemHeightLG(in ControlTokenDefaultContext context) =>
        Math.Min(context.Global.ControlHeightLG - context.Global.UniformlyPaddingXXS * 2,
            context.Global.ControlHeightLG - context.Global.LineWidth * 2);

    [ControlTokenDefault(nameof(MultipleSelectorBgDisabled))]
    private static Color DefaultForMultipleSelectorBgDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainerDisabled;

    [ControlTokenDefault(nameof(MultipleItemColorDisabled))]
    private static Color DefaultForMultipleItemColorDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(OptionSelectedColor))]
    private static Color DefaultForOptionSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(OptionSelectedFontWeight))]
    private static FontWeight DefaultForOptionSelectedFontWeight(in ControlTokenDefaultContext context) =>
        context.Global.FontWeightStrong;

    [ControlTokenDefault(nameof(OptionSelectedBg))]
    private static Color DefaultForOptionSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(OptionActiveBg))]
    private static Color DefaultForOptionActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgHover;

    [ControlTokenDefault(nameof(OptionPadding))]
    private static Thickness DefaultForOptionPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.ControlPaddingHorizontal,
            (context.Global.ControlHeight - context.Global.FontHeight) / 2);

    [ControlTokenDefault(nameof(OptionFontSize))]
    private static double DefaultForOptionFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(OptionHeight))]
    private static double DefaultForOptionHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(SelectAffixPadding))]
    private static Thickness DefaultForSelectAffixPadding(in ControlTokenDefaultContext context) =>
        context.Global.PaddingXXS;

    [ControlTokenDefault(nameof(FixedItemMargin))]
    private static Thickness DefaultForFixedItemMargin(in ControlTokenDefaultContext context) =>
        new(Math.Floor(context.Global.UniformlyPaddingXXS / 2));

    [ControlTokenDefault(nameof(PopupContentPadding))]
    private static Thickness DefaultForPopupContentPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXXS / 2);

    [ControlTokenDefault(nameof(MultiModePadding))]
    private static Thickness DefaultForMultiModePadding(in ControlTokenDefaultContext context)
    {
        var vertical = Math.Round((context.Global.ControlHeight - context.Global.FontHeight) / 2 * 10) / 10 -
                       context.Global.LineWidth;
        var right = context.Global.UniformlyPaddingSM - context.Global.LineWidth;
        return new Thickness(vertical, vertical, right, vertical);
    }

    [ControlTokenDefault(nameof(MultiModePaddingSM))]
    private static Thickness DefaultForMultiModePaddingSM(in ControlTokenDefaultContext context)
    {
        var vertical = Math.Round((context.Global.ControlHeightSM - context.Global.FontHeight) / 2 * 10) / 10 -
                       context.Global.LineWidth;
        var right = context.Global.ControlPaddingHorizontalSM - context.Global.LineWidth;
        return new Thickness(vertical, vertical, right, vertical);
    }

    [ControlTokenDefault(nameof(MultiModePaddingLG))]
    private static Thickness DefaultForMultiModePaddingLG(in ControlTokenDefaultContext context)
    {
        var vertical = Math.Ceiling((context.Global.ControlHeightLG - context.Global.FontHeightLG) / 2 * 10) / 10 -
                       context.Global.LineWidth;
        var right = context.Global.ControlPaddingHorizontal - context.Global.LineWidth;
        return new Thickness(vertical, vertical, right, vertical);
    }

    [ControlTokenDefault(nameof(SingleModePadding))]
    private static Thickness DefaultForSingleModePadding(in ControlTokenDefaultContext context)
    {
        var padding = DefaultForMultiModePadding(context);
        return new Thickness(padding.Right, padding.Top);
    }

    [ControlTokenDefault(nameof(SingleModePaddingSM))]
    private static Thickness DefaultForSingleModePaddingSM(in ControlTokenDefaultContext context)
    {
        var padding = DefaultForMultiModePaddingSM(context);
        return new Thickness(padding.Right, padding.Top);
    }

    [ControlTokenDefault(nameof(SingleModePaddingLG))]
    private static Thickness DefaultForSingleModePaddingLG(in ControlTokenDefaultContext context)
    {
        var padding = DefaultForMultiModePaddingLG(context);
        return new Thickness(padding.Right, padding.Top);
    }

    [ControlTokenDefault(nameof(MultiModePrefixIndent))]
    private static Thickness DefaultForMultiModePrefixIndent(in ControlTokenDefaultContext context)
    {
        var padding = DefaultForMultiModePadding(context);
        return new Thickness(Math.Max(0, padding.Right - padding.Left), 0, 0, 0);
    }

    [ControlTokenDefault(nameof(MultiModePrefixIndentSM))]
    private static Thickness DefaultForMultiModePrefixIndentSM(in ControlTokenDefaultContext context)
    {
        var padding = DefaultForMultiModePaddingSM(context);
        return new Thickness(Math.Max(0, padding.Right - padding.Left), 0, 0, 0);
    }

    [ControlTokenDefault(nameof(MultiModePrefixIndentLG))]
    private static Thickness DefaultForMultiModePrefixIndentLG(in ControlTokenDefaultContext context)
    {
        var padding = DefaultForMultiModePaddingLG(context);
        return new Thickness(Math.Max(0, padding.Right - padding.Left), 0, 0, 0);
    }
}
