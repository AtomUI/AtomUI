using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class RadioButtonToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 单选框大小，除去文字部分的
    /// </summary>
    public double RadioSize { get; set; }

    /// <summary>
    /// 单选框圆点大小
    /// </summary>
    public double DotSize { get; set; }

    /// <summary>
    /// 单选框圆点禁用颜色
    /// </summary>
    public Color DotColorDisabled { get; set; }

    /// 内部使用
    public Color RadioColor { get; set; }

    public Color RadioBgColor { get; set; }

    public double DotPadding { get; set; }

    public Thickness TextMargin { get; set; }

    [ControlTokenDefault(nameof(RadioSize))]
    private static double DefaultForRadioSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(DotSize), nameof(RadioSize), nameof(DotPadding))]
    private static double DefaultForDotSize(in ControlTokenDefaultContext context, double radioSize, double dotPadding) =>
        radioSize - (dotPadding + context.Global.LineWidth) * 2;

    [ControlTokenDefault(nameof(DotColorDisabled))]
    private static Color DefaultForDotColorDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(RadioColor))]
    private static Color DefaultForRadioColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorWhite;

    [ControlTokenDefault(nameof(RadioBgColor))]
    private static Color DefaultForRadioBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(DotPadding))]
    private static double DefaultForDotPadding(in ControlTokenDefaultContext context) => 4;

    [ControlTokenDefault(nameof(TextMargin))]
    private static Thickness DefaultForTextMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginXS, 0, context.Global.UniformlyMarginXS, 0);
}
