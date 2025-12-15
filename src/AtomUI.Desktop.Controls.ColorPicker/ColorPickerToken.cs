using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public class ColorPickerToken : AbstractControlDesignToken
{
    public const string ID = "ColorPicker";
    
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
    
    public ColorPickerToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ColorPickerWidth                  = 234;
        ColorPickerHandlerSizeLG          = 32; // 用在触摸屏上
        ColorPickerHandlerSize            = 16;
        ColorPickerHandlerSizeSM          = 12;
        ColorPickerAlphaInputWidth        = 44;
        ColorPickerInputNumberHandleWidth = 16;
        ColorPickerPresetColorSize        = 24;
        CheckedMarkSize                   = ColorPickerPresetColorSize * 0.6;
        ColorPickerPresetPanelWidth       = 220;
        ColorPickerSliderSize             = ColorPickerHandlerSizeSM;
        ColorPickerSliderTrackSize        = 8;
        ColorPickerSliderThumbSize        = ColorPickerHandlerSizeSM + SharedToken.LineWidth * 2;
        ColorPickerPreviewSize            = ColorPickerSliderTrackSize * 2 + SharedToken.UniformlyMarginSM;
        ColorPickerInsetShadow            = new BoxShadows(new BoxShadow()
        {
            IsInset = true,
            OffsetX = 0,
            OffsetY = 0,
            Blur = 1,
            Spread = 0,
            Color = SharedToken.ColorTextQuaternary
        });

        ColorBlockInnerShadows = new BoxShadows(new BoxShadow()
        {
            IsInset = true,
            OffsetX = 0,
            OffsetY = 0,
            Blur    = 0,
            Spread  = SharedToken.LineWidth,
            Color   = SharedToken.ColorFillSecondary
        });
        TriggerPadding    = new Thickness(SharedToken.UniformlyPaddingXXS - SharedToken.LineWidth);
        TriggerTextMargin = new Thickness(SharedToken.UniformlyMarginXS, 0, 
            SharedToken.UniformlyMarginXS - SharedToken.UniformlyPaddingXXS + SharedToken.LineWidth, 0);

        ColorSpectrumHeight                = SharedToken.ControlHeightLG * 4;
        ColorPickerHandlerLightColor       = SharedToken.ColorBgElevated;
        ColorPickerHandlerDarkColor        = Color.Parse("#22075e");
        SliderContainerMargin              = new Thickness(0, 0, SharedToken.UniformlyMarginSM, 0);
        TransparentBgSize                  = SharedToken.SizeXS;
        ColorPickerPresetColorGroupPadding = new Thickness(0, SharedToken.UniformlyPaddingXXS);
    }
}
