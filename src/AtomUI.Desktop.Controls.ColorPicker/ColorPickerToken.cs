using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
public sealed partial class ColorPickerToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// ColorPicker 宽度
    /// Width of ColorPicker
    /// </summary>
    public double ColorPickerWidth { get; set; }

    /// <summary>
    /// 色谱高度
    /// </summary>
    public double ColorSpectrumHeight { get; set; }

    /// <summary>
    /// ColorPicker 内嵌阴影
    /// Inset shadow of ColorPicker
    /// </summary>
    public BoxShadows ColorPickerInsetShadow { get; set; }

    /// <summary>
    /// ColorPicker 处理器尺寸大号
    /// Handler size of ColorPicker
    /// </summary>
    public double ColorPickerHandlerSizeLG { get; set; }

    /// <summary>
    /// ColorPicker 处理器尺寸
    /// Handler size of ColorPicker
    /// </summary>
    public double ColorPickerHandlerSize { get; set; }

    /// <summary>
    /// ColorPicker 小号处理器尺寸
    /// Small handler size of ColorPicker
    /// </summary>
    public double ColorPickerHandlerSizeSM { get; set; }

    /// <summary>
    /// ColorPicker 处理器边框的亮色颜色
    /// </summary>
    public Color ColorPickerHandlerLightColor { get; set; }

    /// <summary>
    /// ColorPicker 处理器边框的暗色颜色
    /// </summary>
    public Color ColorPickerHandlerDarkColor { get; set; }

    /// <summary>
    /// ColorPicker 滑块高度或者宽度
    /// Slider height of ColorPicker
    /// </summary>
    public double ColorPickerSliderSize { get; set; }

    /// <summary>
    /// ColorPicker 轨道的大小高度或者宽度
    /// Slider height of ColorPicker Track
    /// </summary>
    public double ColorPickerSliderTrackSize { get; set; }

    /// <summary>
    /// ColorPicker 轨道 handle 的大小
    /// </summary>
    public double ColorPickerSliderThumbSize { get; set; }

    /// <summary>
    /// ColorPicker 预览尺寸
    /// Preview size of ColorPicker
    /// </summary>
    public double ColorPickerPreviewSize { get; set; }

    /// <summary>
    /// ColorPicker Alpha 输入宽度
    /// Alpha input width of ColorPicker
    /// </summary>
    public double ColorPickerAlphaInputWidth { get; set; }

    /// <summary>
    /// ColorPicker 输入数字处理器宽度
    /// Input number handle width of ColorPicker
    /// </summary>
    public double ColorPickerInputNumberHandleWidth { get; set; }

    /// <summary>
    /// ColorPicker 预设颜色尺寸
    /// Preset color size of ColorPicker
    /// </summary>
    public double ColorPickerPresetColorSize { get; set; }

    /// <summary>
    /// ColorPicker 预设颜色面板宽度
    /// </summary>
    public double ColorPickerPresetPanelWidth { get; set; }

    /// <summary>
    /// ColorPicker 预设颜色选中勾勾的大小
    /// </summary>
    public double CheckedMarkSize { get; set; }

    /// <summary>
    /// ColorPicker 预设颜色序列项内容内间距
    /// </summary>
    public Thickness ColorPickerPresetColorGroupPadding { get; set; }

    /// <summary>
    /// 颜色显示内部的阴影
    /// </summary>
    public BoxShadows ColorBlockInnerShadows { get; set; }

    /// <summary>
    /// 颜色选择器触发控件内边距
    /// </summary>
    public Thickness TriggerPadding { get; set; }

    /// <summary>
    /// 颜色选择器触发控件外边距
    /// </summary>
    public Thickness TriggerTextMargin { get; set; }

    /// <summary>
    /// 颜色滑块外间距
    /// </summary>
    public Thickness SliderContainerMargin { get; set; }

    /// <summary>
    /// 透明时候颜色色块大小
    /// </summary>
    public double TransparentBgSize { get; set; }

    /// <summary>
    /// 禁用状态下颜色色块的不透明度
    /// </summary>
    public double ColorBlockDisabledOpacity { get; set; }

    public ColorPickerToken()

    {
    }

    [ControlTokenDefault(nameof(ColorPickerWidth))]
    private static double DefaultForColorPickerWidth(in ControlTokenDefaultContext context) =>
        234;

    [ControlTokenDefault(nameof(ColorSpectrumHeight))]
    private static double DefaultForColorSpectrumHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 4;

    [ControlTokenDefault(nameof(ColorPickerInsetShadow))]
    private static BoxShadows DefaultForColorPickerInsetShadow(in ControlTokenDefaultContext context) =>
        new(new BoxShadow
        {
            IsInset = true,
            OffsetX = 0,
            OffsetY = 0,
            Blur = 1,
            Spread = 0,
            Color = context.Global.ColorTextQuaternary
        });

    [ControlTokenDefault(nameof(ColorPickerHandlerSizeLG))]
    private static double DefaultForColorPickerHandlerSizeLG(in ControlTokenDefaultContext context) =>
        32; // Touch selector size.

    [ControlTokenDefault(nameof(ColorPickerHandlerSize))]
    private static double DefaultForColorPickerHandlerSize(in ControlTokenDefaultContext context) =>
        16;

    [ControlTokenDefault(nameof(ColorPickerHandlerSizeSM))]
    private static double DefaultForColorPickerHandlerSizeSM(in ControlTokenDefaultContext context) =>
        12;

    [ControlTokenDefault(nameof(ColorPickerHandlerLightColor))]
    private static Color DefaultForColorPickerHandlerLightColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(ColorPickerHandlerDarkColor))]
    private static Color DefaultForColorPickerHandlerDarkColor(in ControlTokenDefaultContext context) =>
        Color.Parse("#22075e");

    [ControlTokenDefault(nameof(ColorPickerSliderSize), nameof(ColorPickerHandlerSizeSM))]
    private static double DefaultForColorPickerSliderSize(in ControlTokenDefaultContext context, double colorPickerHandlerSizeSM) =>
        colorPickerHandlerSizeSM;

    [ControlTokenDefault(nameof(ColorPickerSliderTrackSize))]
    private static double DefaultForColorPickerSliderTrackSize(in ControlTokenDefaultContext context) =>
        8;

    [ControlTokenDefault(nameof(ColorPickerSliderThumbSize), nameof(ColorPickerHandlerSizeSM))]
    private static double DefaultForColorPickerSliderThumbSize(in ControlTokenDefaultContext context, double colorPickerHandlerSizeSM) =>
        colorPickerHandlerSizeSM + context.Global.LineWidth * 2;

    [ControlTokenDefault(nameof(ColorPickerPreviewSize), nameof(ColorPickerSliderTrackSize))]
    private static double DefaultForColorPickerPreviewSize(in ControlTokenDefaultContext context, double colorPickerSliderTrackSize) =>
        colorPickerSliderTrackSize * 2 + context.Global.UniformlyMarginSM;

    [ControlTokenDefault(nameof(ColorPickerAlphaInputWidth))]
    private static double DefaultForColorPickerAlphaInputWidth(in ControlTokenDefaultContext context) =>
        44;

    [ControlTokenDefault(nameof(ColorPickerInputNumberHandleWidth))]
    private static double DefaultForColorPickerInputNumberHandleWidth(in ControlTokenDefaultContext context) =>
        16;

    [ControlTokenDefault(nameof(ColorPickerPresetColorSize))]
    private static double DefaultForColorPickerPresetColorSize(in ControlTokenDefaultContext context) =>
        24;

    [ControlTokenDefault(nameof(ColorPickerPresetPanelWidth))]
    private static double DefaultForColorPickerPresetPanelWidth(in ControlTokenDefaultContext context) =>
        220;

    [ControlTokenDefault(nameof(CheckedMarkSize), nameof(ColorPickerPresetColorSize))]
    private static double DefaultForCheckedMarkSize(in ControlTokenDefaultContext context, double colorPickerPresetColorSize) =>
        colorPickerPresetColorSize * 0.6;

    [ControlTokenDefault(nameof(ColorPickerPresetColorGroupPadding))]
    private static Thickness DefaultForColorPickerPresetColorGroupPadding(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyPaddingXXS);

    [ControlTokenDefault(nameof(ColorBlockInnerShadows))]
    private static BoxShadows DefaultForColorBlockInnerShadows(in ControlTokenDefaultContext context) =>
        new(new BoxShadow
        {
            IsInset = true,
            OffsetX = 0,
            OffsetY = 0,
            Blur = 0,
            Spread = context.Global.LineWidth,
            Color = context.Global.ColorFillSecondary
        });

    [ControlTokenDefault(nameof(TriggerPadding))]
    private static Thickness DefaultForTriggerPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXXS - context.Global.LineWidth);

    [ControlTokenDefault(nameof(TriggerTextMargin))]
    private static Thickness DefaultForTriggerTextMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginXS, 0,
            context.Global.UniformlyMarginXS - context.Global.UniformlyPaddingXXS + context.Global.LineWidth, 0);

    [ControlTokenDefault(nameof(SliderContainerMargin))]
    private static Thickness DefaultForSliderContainerMargin(in ControlTokenDefaultContext context) =>
        new(0, 0, context.Global.UniformlyMarginSM, 0);

    [ControlTokenDefault(nameof(TransparentBgSize))]
    private static double DefaultForTransparentBgSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeXS;

    [ControlTokenDefault(nameof(ColorBlockDisabledOpacity))]
    private static double DefaultForColorBlockDisabledOpacity(in ControlTokenDefaultContext context) =>
        0.4;

}
