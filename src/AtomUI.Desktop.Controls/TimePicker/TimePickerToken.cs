using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TimePickerToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// 时间选择项高度
    /// </summary>
    public double ItemHeight { get; set; }

    /// <summary>
    /// 时间选择项宽度
    /// </summary>
    public double ItemWidth { get; set; }

    /// <summary>
    /// 上下午选择项的宽度
    /// </summary>
    public double PeriodHostWidth { get; set; }

    /// <summary>
    /// 时间选择项内间距
    /// </summary>
    public Thickness ItemPadding { get; set; }

    /// <summary>
    /// 按钮区域对上的外边距
    /// </summary>
    public Thickness ButtonsMargin { get; set; }

    /// <summary>
    /// RangeTimePicker 的明确 Own 箭头间距覆盖；缺省保留 InfoPickerInput 角色。
    /// </summary>
    public Thickness RangePickerArrowMargin { get; set; }

    /// <summary>
    /// RangeTimePicker 的明确 Own 指示器厚度覆盖；缺省保留 InfoPickerInput 角色。
    /// </summary>
    public double RangePickerIndicatorThickness { get; set; }

    /// <summary>
    /// Header 头内间距
    /// </summary>
    public Thickness HeaderMargin { get; set; }

    [ControlTokenDefault(nameof(ItemHeight))]
    private static double DefaultForItemHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight - 4;

    [ControlTokenDefault(nameof(ItemWidth))]
    private static double DefaultForItemWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.4;

    [ControlTokenDefault(nameof(PeriodHostWidth), nameof(ItemWidth))]
    private static double DefaultForPeriodHostWidth(in ControlTokenDefaultContext context, double itemWidth) =>
        itemWidth;

    [ControlTokenDefault(nameof(ItemPadding), nameof(ItemWidth), nameof(ItemHeight))]
    private static Thickness DefaultForItemPadding(in ControlTokenDefaultContext context, double itemWidth, double itemHeight) =>
        new Thickness((itemWidth - itemHeight) / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(ButtonsMargin))]
    private static Thickness DefaultForButtonsMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);

    [ControlTokenDefault(nameof(RangePickerArrowMargin))]
    private static Thickness DefaultForRangePickerArrowMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(RangePickerIndicatorThickness))]
    private static double DefaultForRangePickerIndicatorThickness(in ControlTokenDefaultContext context) =>
        context.Global.LineWidthFocus;

    [ControlTokenDefault(nameof(HeaderMargin))]
    private static Thickness DefaultForHeaderMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, 3);
}
