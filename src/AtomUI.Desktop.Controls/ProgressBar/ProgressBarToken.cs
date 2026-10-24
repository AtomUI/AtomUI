using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ProgressBarToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 进度条默认颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    /// <summary>
    /// 进度条剩余部分颜色
    /// </summary>
    public Color RemainingColor { get; set; }

    /// <summary>
    /// 圆形进度条文字颜色
    /// </summary>
    public Color CircleTextColor { get; set; }

    /// <summary>
    /// 条状进度条圆角
    /// </summary>
    public CornerRadius LineBorderRadius { get; set; }

    /// <summary>
    /// 圆形进度条文本最小大小
    /// </summary>
    public double CircleMinimumTextFontSize { get; set; }

    /// <summary>
    /// 圆形进度条图标最小大小
    /// </summary>
    public double CircleMinimumIconSize { get; set; }

    // 内部属性
    public double ProgressStepMinWidth { get; set; }
    public Thickness ProgressStepMarginInlineEnd { get; set; } // Step 之间的间距
    public double ProgressActiveMotionDuration { get; set; }
    public double LineExtraInfoMargin { get; set; } // 额外信息和 Indicator 之间的间距
    public double LineProgressPadding { get; set; } // 线行进度条的内间距
    public double LineInfoIconSize { get; set; }
    public double LineInfoIconSizeSM { get; set; }

    [ControlTokenDefault(nameof(DefaultColor))]
    private static Color DefaultForDefaultColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorInfo;

    [ControlTokenDefault(nameof(RemainingColor))]
    private static Color DefaultForRemainingColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillSecondary;

    [ControlTokenDefault(nameof(CircleTextColor))]
    private static Color DefaultForCircleTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(LineBorderRadius))]
    private static CornerRadius DefaultForLineBorderRadius(in ControlTokenDefaultContext context) =>
        new CornerRadius(100);

    [ControlTokenDefault(nameof(CircleMinimumTextFontSize))]
    private static double DefaultForCircleMinimumTextFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeSM - 2;

    [ControlTokenDefault(nameof(CircleMinimumIconSize))]
    private static double DefaultForCircleMinimumIconSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeXS;

    [ControlTokenDefault(nameof(ProgressStepMinWidth))]
    private static double DefaultForProgressStepMinWidth(in ControlTokenDefaultContext context) =>
        0;

    [ControlTokenDefault(nameof(ProgressStepMarginInlineEnd))]
    private static Thickness DefaultForProgressStepMarginInlineEnd(in ControlTokenDefaultContext context) =>
        default;

    [ControlTokenDefault(nameof(ProgressActiveMotionDuration))]
    private static double DefaultForProgressActiveMotionDuration(in ControlTokenDefaultContext context) =>
        0;

    [ControlTokenDefault(nameof(LineExtraInfoMargin))]
    private static double DefaultForLineExtraInfoMargin(in ControlTokenDefaultContext context) =>
        context.Global.ControlPaddingHorizontalSM;

    [ControlTokenDefault(nameof(LineProgressPadding))]
    private static double DefaultForLineProgressPadding(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyPaddingXXS / 2;

    [ControlTokenDefault(nameof(LineInfoIconSize))]
    private static double DefaultForLineInfoIconSize(in ControlTokenDefaultContext context) =>
        context.Global.IconSize;

    [ControlTokenDefault(nameof(LineInfoIconSizeSM))]
    private static double DefaultForLineInfoIconSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.IconSizeSM;
}
