using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// vertical: part   (水平时，垂直方向命名为 part)
// horizontal: full (水平时，水平方向命名为 full)
[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SliderToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
/// <summary>
    /// 滑动条控件的高度
    /// </summary>
    public double SliderTrackSize { get; set; }

    /// <summary>
    /// 轨道高度
    /// </summary>
    public double RailSize { get; set; }

    /// <summary>
    /// Mark 的大小
    /// </summary>
    public double MarkSize { get; set; }

    /// <summary>
    /// Thumb 推荐的大小，方便风格使用，是最大需要的大小
    /// </summary>
    public double ThumbSize { get; set; }

    /// <summary>
    /// 滑块尺寸
    /// </summary>
    public double ThumbCircleSize { get; set; }

    /// <summary>
    /// 滑块尺寸（悬浮态）
    /// </summary>
    public double ThumbCircleSizeHover { get; set; }

    /// <summary>
    /// 滑块边框宽度
    /// </summary>
    public Thickness ThumbCircleBorderThickness { get; set; }

    /// <summary>
    /// 滑块边框宽度（悬浮态）
    /// </summary>
    public Thickness ThumbCircleBorderThicknessHover { get; set; }

    /// <summary>
    /// 轨道背景色
    /// </summary>
    public Color RailBg { get; set; }

    /// <summary>
    /// 轨道背景色（悬浮态）
    /// </summary>
    public Color RailHoverBg { get; set; }

    /// <summary>
    /// 轨道已覆盖部分背景色
    /// </summary>
    public Color TrackBg { get; set; }

    /// <summary>
    /// 轨道已覆盖部分背景色（悬浮态）
    /// </summary>
    public Color TrackHoverBg { get; set; }

    /// <summary>
    /// Mark 的背景颜色
    /// </summary>
    public Color MarkBorderColor { get; set; }

    /// <summary>
    /// Mark 的背景 hover 效果
    /// </summary>
    public Color MarkBorderColorHover { get; set; }

    /// <summary>
    /// Mark 的背景激活时候效果
    /// </summary>
    public Color MarkBorderColorActive { get; set; }

    /// <summary>
    /// 滑块颜色
    /// </summary>
    public Color ThumbCircleBorderColor { get; set; }

    /// <summary>
    /// 滑块颜色 hover
    /// </summary>
    public Color ThumbCircleBorderHoverColor { get; set; }

    /// <summary>
    /// 滑块激活态颜色
    /// </summary>
    public Color ThumbCircleBorderActiveColor { get; set; }

    /// <summary>
    /// 滑块禁用颜色
    /// </summary>
    public Color ThumbCircleBorderColorDisabled { get; set; }

    /// <summary>
    /// 滑块的 outline 环的颜色
    /// </summary>
    public Color ThumbOutlineColor { get; set; }

    /// <summary>
    /// 滑块的 outline 环的厚度
    /// </summary>
    public Thickness ThumbOutlineThickness { get; set; }

    /// <summary>
    /// 轨道禁用态背景色
    /// </summary>
    public Color TrackBgDisabled { get; set; }

    public Thickness SliderPaddingHorizontal { get; set; }
    public Thickness SliderPaddingVertical { get; set; }
    public Thickness MarginPartWithMark { get; set; }

    [ControlTokenDefault(nameof(SliderTrackSize))]
    private static double DefaultForSliderTrackSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM / 2;

    [ControlTokenDefault(nameof(RailSize))]
    private static double DefaultForRailSize(in ControlTokenDefaultContext context) =>
        4;

    [ControlTokenDefault(nameof(MarkSize))]
    private static double DefaultForMarkSize(in ControlTokenDefaultContext context) =>
        8;

    [ControlTokenDefault(nameof(ThumbCircleSize))]
    private static double DefaultForThumbCircleSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG / 4;

    [ControlTokenDefault(nameof(ThumbCircleSizeHover))]
    private static double DefaultForThumbCircleSizeHover(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM / 2;

    [ControlTokenDefault(nameof(ThumbCircleBorderThickness))]
    private static Thickness DefaultForThumbCircleBorderThickness(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.LineWidth + 1d);

    [ControlTokenDefault(nameof(ThumbCircleBorderThicknessHover))]
    private static Thickness DefaultForThumbCircleBorderThicknessHover(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.LineWidth + 1.5);

    [ControlTokenDefault(nameof(RailBg))]
    private static Color DefaultForRailBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillTertiary;

    [ControlTokenDefault(nameof(RailHoverBg))]
    private static Color DefaultForRailHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillSecondary;

    [ControlTokenDefault(nameof(TrackBg))]
    private static Color DefaultForTrackBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryBorder;

    [ControlTokenDefault(nameof(TrackHoverBg))]
    private static Color DefaultForTrackHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryBorderHover;

    [ControlTokenDefault(nameof(MarkBorderColor))]
    private static Color DefaultForMarkBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorderSecondary;

    [ControlTokenDefault(nameof(MarkBorderColorHover))]
    private static Color DefaultForMarkBorderColorHover(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillContentHover;

    [ControlTokenDefault(nameof(MarkBorderColorActive))]
    private static Color DefaultForMarkBorderColorActive(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryBorder;

    [ControlTokenDefault(nameof(ThumbCircleBorderColor))]
    private static Color DefaultForThumbCircleBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryBorder;

    [ControlTokenDefault(nameof(ThumbCircleBorderHoverColor))]
    private static Color DefaultForThumbCircleBorderHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryBorderHover;

    [ControlTokenDefault(nameof(ThumbCircleBorderActiveColor))]
    private static Color DefaultForThumbCircleBorderActiveColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ThumbCircleBorderColorDisabled))]
    private static Color DefaultForThumbCircleBorderColorDisabled(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorTextDisabled, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(TrackBgDisabled))]
    private static Color DefaultForTrackBgDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainerDisabled;

    [ControlTokenDefault(nameof(SliderPaddingHorizontal), nameof(SliderTrackSize))]
    private static Thickness DefaultForSliderPaddingHorizontal(in ControlTokenDefaultContext context, double sliderTrackSize) =>
        new Thickness(sliderTrackSize / 2, (context.Global.ControlHeight - sliderTrackSize) / 2);

    [ControlTokenDefault(nameof(SliderPaddingVertical), nameof(SliderTrackSize))]
    private static Thickness DefaultForSliderPaddingVertical(in ControlTokenDefaultContext context, double sliderTrackSize) =>
        new Thickness((context.Global.ControlHeight - sliderTrackSize) / 2, sliderTrackSize / 2);

    [ControlTokenDefault(nameof(MarginPartWithMark), nameof(SliderTrackSize))]
    private static Thickness DefaultForMarginPartWithMark(in ControlTokenDefaultContext context, double sliderTrackSize) =>
        new Thickness(0, 0, 0, context.Global.ControlHeightLG - sliderTrackSize);

    [ControlTokenDefault(nameof(ThumbOutlineColor), nameof(ThumbCircleBorderActiveColor))]
    private static Color DefaultForThumbOutlineColor(in ControlTokenDefaultContext context, Color thumbCircleBorderActiveColor) =>
        ColorUtils.FromRgbF(0.2, thumbCircleBorderActiveColor.GetRedF(), thumbCircleBorderActiveColor.GetGreenF(), thumbCircleBorderActiveColor.GetBlueF());

    [ControlTokenDefault(nameof(ThumbOutlineThickness))]
    private static Thickness DefaultForThumbOutlineThickness(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.WaveAnimationRange);

    [ControlTokenDefault(nameof(ThumbSize), nameof(ThumbCircleSizeHover), nameof(ThumbCircleBorderThicknessHover), nameof(ThumbOutlineThickness))]
    private static double DefaultForThumbSize(in ControlTokenDefaultContext context, double thumbCircleSizeHover, Thickness thumbCircleBorderThicknessHover, Thickness thumbOutlineThickness) =>
        thumbCircleSizeHover + thumbCircleBorderThicknessHover.Left * 2 + thumbOutlineThickness.Left * 2;

}
